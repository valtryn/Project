using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// Documentation: this is just a helper functions for converting unix to mm/dd/yy string format and vice versa
namespace ExpenseTracker
{
    public class Utils
    {
        public static DateTime UnixTimeToMonthYear(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToUniversalTime();
        }

        public static string UnixTimeToDateTimeString(long unixTime)
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            return dateTime.ToString("MM/dd/yyyy");
        }

        public static long UnixCurrentTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long StringTimeToUnixTime(string str)
        {
            DateTime parsedDate;

            // Parse the date string using the specified format
            if (DateTime.TryParseExact(str, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                // Convert the parsed DateTime to Unix time (seconds since January 1, 1970)
                long unixTime = ((DateTimeOffset)parsedDate).ToUnixTimeSeconds();
            }
            else
            {
                //TODO: handle popup error
                Console.WriteLine("Invalid date format.");
            }

            return ((DateTimeOffset)parsedDate).ToUnixTimeSeconds(); ;
        }
    }
}
