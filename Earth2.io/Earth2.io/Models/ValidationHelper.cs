using Earth2.io.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Earth2.io.Models
{
    public class ValidationHelper
    {
        public static bool ValidateReferralCode(string referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode))
            {
                return false;
            }

            if (referralCode.Trim().Length != 10 && !referralCode.All(char.IsLetterOrDigit))
            {
                return false;
            }

            return true;
        }

        public void BanUserIfNeeded(string referralCode)
        {
            ReportRepository reportRepository = new ReportRepository();
            UserRepository userRepository = new UserRepository();

            var timesReported = reportRepository.GetNumberOfReportsByUser(referralCode);
            if (timesReported == "Error Occurred.") { return; }
            if (int.Parse(timesReported) > 3)
            {
                reportRepository.InsertBanRecord(referralCode);
                userRepository.InsertTrackingRecord(referralCode, "User Banned", $"{referralCode} has been banned.");
            }
        }

        //public static bool CanUserBeReportedYet()
        //{
        //    return false;
        //}
    }
}
