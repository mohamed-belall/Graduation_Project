namespace Graduation_Project.Core.Common
{
    public static class DateHelper
    {
        private const string EgyptTimeZoneId = "Egypt Standard Time";

        public static DateOnly GetTodayInEgypt()
        {
            var egyptTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, EgyptTimeZoneId);
            return DateOnly.FromDateTime(egyptTime);
        }

        public static DateTime GetNowInEgypt()
        {
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, EgyptTimeZoneId);
        }
    }
}
