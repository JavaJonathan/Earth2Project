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

        public static string CheckIfUserIsBanned(string referralCode)
        {
            var timesReported = ReportRepository.GetNumberOfReportsByUser(referralCode);
            if (timesReported == "Error Occurred.") { return "Error Occurred."; }
            if(int.Parse(timesReported) > 3) return "true";

            return "false";
        }

        public static bool IsDDOSAttack(IPAddress ip)
        {

        }
    }
}
