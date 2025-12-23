namespace ClaimsManagement.Business.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime AddBusinessDays(DateTime startDate, int businessDays)
        {
            var currentDate = startDate;
            var addedDays = 0;

            while (addedDays < businessDays)
            {
                currentDate = currentDate.AddDays(1);
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    addedDays++;
                }
            }

            return currentDate;
        }

        public static int GetBusinessDaysBetween(DateTime startDate, DateTime endDate)
        {
            var businessDays = 0;
            var currentDate = startDate;

            while (currentDate < endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    businessDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return businessDays;
        }

        public static bool IsBusinessDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        public static DateTime GetNextBusinessDay(DateTime date)
        {
            var nextDay = date.AddDays(1);
            while (!IsBusinessDay(nextDay))
            {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }
    }
}