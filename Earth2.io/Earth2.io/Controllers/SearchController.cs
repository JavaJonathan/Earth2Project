using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Earth2.io.Data;
using Earth2.io.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Earth2.io.Controllers
{
    [DisableCors]
    [Route("api/")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        SearchRepository searchRepository;
        ErrorRepository errorRepository;
        ReportRepository reportRepository;
        UserRepository userRepository;

        // POST api/values
        [HttpPost("Start")]
        public string StartSearch([FromBody] JObject jsonUserObject)
        {
            var ip = HttpContext.Connection.RemoteIpAddress;

            searchRepository = new SearchRepository();
            errorRepository = new ErrorRepository();
            reportRepository = new ReportRepository();
            userRepository = new UserRepository();

            errorRepository.LogIpAndCheckForDDOS(ip.ToString());

            var referralCode = jsonUserObject["referralCode"]?.ToString();
            var userName = jsonUserObject["userName"]?.ToString();

            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return "Invalid Referral Code"; }

            var userBanned = reportRepository.IsUserBanned(referralCode);

            if (userBanned) { return "User is banned."; }

            if (!bool.Parse(userRepository.CheckIfUserExists(referralCode)))
            {
                userRepository.AddNewUser(referralCode, userName);
            }

            if (bool.Parse(searchRepository.CheckIfUserIsAlreadySearching(referralCode)))
            {
                return "Error. User is already searching.";
            }

            searchRepository.InsertSearchingRecord(referralCode);
            userRepository.InsertTrackingRecord(referralCode, "Started Searching", $"{referralCode} has started searching.");

            return "User is successfully searching.";
        }

        [HttpPost("StopSearch")]
        public string StopSearch([FromBody] JObject JsonReferralCode)
        {
            var referralCode = JsonReferralCode["referralCode"]?.ToString();
            var ip = HttpContext.Connection.RemoteIpAddress;

            searchRepository = new SearchRepository();
            errorRepository = new ErrorRepository();
            userRepository = new UserRepository();


            errorRepository.LogIpAndCheckForDDOS(ip.ToString());

            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return "Invalid Referral Code"; }

            if (!bool.Parse(userRepository.CheckIfUserExists(referralCode)))
            {
                return "Error. User does not exist.";
            }

            var removedSuccessfully = searchRepository.RemoveSearchingRecord(referralCode);

            if (!removedSuccessfully)
            {
                return "Error Occurred.";
            }

            userRepository.InsertTrackingRecord(referralCode, "User Stopped Searching", $"{referralCode} has cancelled search.");

            return "Successfully Removed.";
        }

        [HttpPost("FindMatch")]
        public string[] FindMatch([FromBody] JObject JsonReferralCode)
        {
            var referralCode = JsonReferralCode["referralCode"]?.ToString();
            var ip = HttpContext.Connection.RemoteIpAddress;

            searchRepository = new SearchRepository();
            errorRepository = new ErrorRepository();
            userRepository = new UserRepository();

            errorRepository.LogIpAndCheckForDDOS(ip.ToString());

            var userFound = new string[2];

            var validationPass = ValidationHelper.ValidateReferralCode(referralCode);

            if (!validationPass) { return new string[] { "Invalid Referral Code" }; }

            if (!bool.Parse(userRepository.CheckIfUserExists(referralCode)))
            {
                return new[] { "Error. User does not exist." };
            }

            //we will to write a function to ensure they are already searching just in case

            userFound = searchRepository.CheckIfUserAlreadyMatched(referralCode);

            //checkifuseralreadymatched will return an array either with the referralCode and username or a reason why that value was not returned
            if (userFound.Length == 2)
            {
                return userFound;
            }

            userFound = searchRepository.FindAnotherUserSearching(referralCode);
            var matchedUserReferralCode = userFound[0];

            //findAnotherUserSearching returns an array of empty strings if no user if found
            if (matchedUserReferralCode == "" || matchedUserReferralCode == "Error Occurred.")
            {
                return new[] { "No match found yet." };
            }

            //we will need to handle to the error strings that this function returns
            searchRepository.RemoveBothSearchingRecords(referralCode, matchedUserReferralCode);

            //we will need to handle to the error strings that this function returns
            searchRepository.InsertUserMatchedRecord(referralCode, matchedUserReferralCode);
            userRepository.InsertTrackingRecord(referralCode, "Found Match", $"{referralCode} has matched with {userFound[0]}.");

            return userFound;
        }

        [HttpGet("GetNumberOfUsers")]
        public string GetNumberOfUsersSearching()
        {
            var ip = HttpContext.Connection.RemoteIpAddress;

            searchRepository = new SearchRepository();
            errorRepository = new ErrorRepository();

            errorRepository.LogIpAndCheckForDDOS(ip.ToString());

            return searchRepository.GetNumberOfUsersSearching();
        }
    }
}