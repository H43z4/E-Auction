using System.Text.RegularExpressions;

namespace eauction.Infrastructure
{
    public static class CustomRegex
    {
        private static Regex digitsOnly = new Regex(@"[^\d]");

        public static string Clean(string CNIC)
        {
            return digitsOnly.Replace(CNIC, "");
        }
    }

}
