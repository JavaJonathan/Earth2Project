using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Models
{
    public class ValidationHelper
    {
        public static string ValidateReferralCode(string referralCode)
        {
            if (string.IsNullOrWhiteSpace(referralCode))
            {
                return "Error. Activation Code Cannot Be Null or White Space.";
            }

            if (referralCode.Trim().Length != 10 && !referralCode.All(char.IsLetterOrDigit))
            {
                return "Error. Invalid Activation Code.";
            }

            return "true";
        }
    }
}
