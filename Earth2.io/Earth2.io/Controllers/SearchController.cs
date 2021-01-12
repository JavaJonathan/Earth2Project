using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Earth2.io.Data;
using Earth2.io.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Earth2.io.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        // POST api/values
        [HttpPost("Start")]
        public string StartSearch([FromBody] string jsonUserObject)
        {
            var userObject = JObject.Parse(jsonUserObject);

            var referralCode = userObject["referralCode"]?.ToString();
            var userName = userObject["userName"]?.ToString();

            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return "Invalid Referral Code"; }

            var isUserBanned = ValidationHelper.CheckIfUserIsBanned(referralCode);

            switch (isUserBanned)
            {
                case "true": return "User is banned.";
                case "Error Occurred.": return "Error Occurred.";
            }

            userName = HtmlEncoder.Create().Encode(userName);

            if (!bool.Parse(UserRepository.CheckIfUserExists(referralCode)))
            {
                UserRepository.AddNewUser(referralCode, userName);
            }

            if (bool.Parse(SearchRepository.CheckIfUserIsAlreadySearching(referralCode)))
            {
                return "Error. User is already searching.";
            }

            SearchRepository.InsertSearchingRecord(referralCode);
            UserRepository.InsertTrackingRecord(referralCode, "Started Searching", $"{referralCode} has started searching.");

            return "User is successfully searching.";
        }

        [HttpPost("StopSearch")]
        public string StopSearch([FromBody] string referralCode)
        {
            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return "Invalid Referral Code"; }

            if (!bool.Parse(UserRepository.CheckIfUserExists(referralCode)))
            {
                return "Error. User does not exist.";
            }

            var removedSuccessfully = SearchRepository.RemoveSearchingRecord(referralCode);

            if (!removedSuccessfully)
            {
                return "Error Occurred.";
            }

            UserRepository.InsertTrackingRecord(referralCode, "User Stopped Searching", $"{referralCode} has cancelled search.");

            return "Successfully Removed.";
        }

        [HttpPost("FindMatch")]
        public string[] FindMatch([FromBody] string referralCode)
        {
            var userFound = new string[2];
            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return new string[] { "Invalid Referral Code" }; }

            if (!bool.Parse(UserRepository.CheckIfUserExists(referralCode)))
            {
                return new[] { "Error. User does not exist." };
            }

            //we will to write a function to ensure they are already searching just in case

            userFound = SearchRepository.CheckIfUserAlreadyMatched(referralCode);

            //checkifuseralreadymatched will return an array either with the referralCode and username or a reason why a that value was returned
            if (userFound.Length == 2)
            {
                return userFound;
            }
            //we weill need to write some else if blocks or log the errors in the DB

            userFound = SearchRepository.FindAnotherUserSearching(referralCode);
            var matchedUserReferralCode = userFound[0];

            //findAnotherUserSearching returns an array of emtpy strings if no user if found
            if (matchedUserReferralCode == "")
            {
                return new[] { "No match found yet." };
            }

            //we will need to handle to the error strings that this function returns
            SearchRepository.RemoveBothSearchingRecords(referralCode, matchedUserReferralCode);

            //we will need to handle to the error strings that this function returns
            SearchRepository.InsertUserMatchedRecord(referralCode, matchedUserReferralCode);
            UserRepository.InsertTrackingRecord(referralCode, "Found Match", $"{referralCode} has matched with {userFound[0]}.");

            return userFound;
        }

        [HttpGet("GetNumberOfUsers")]
        public string GetNumberOfUsersSearching()
        {
            return SearchRepository.GetNumberOfUsersSearching();
        }
    }
}