#pragma warning disable IDE0022
namespace Playground.Types
{
    public class Time
    {
        public Time(
            ulong totalSeconds = 0,
            ulong secondsPerMinute = 60,
            ulong minutesPerHour = 60,
            ulong hoursPerDay = 24,
            ulong daysPerWeek = 7,
            ulong weeksPerMonth = 4,
            ulong monthsPerYear = 12)
        {
            if (secondsPerMinute == 0 ||
                minutesPerHour == 0 ||
                hoursPerDay == 0 ||
                daysPerWeek == 0 ||
                weeksPerMonth == 0 ||
                monthsPerYear == 0)
            {
                throw new ArgumentException("Time units cannot be zero.");
            }

            TotalSeconds = totalSeconds;
            SecondsPerMinute = secondsPerMinute;
            MinutesPerHour = minutesPerHour;
            HoursPerDay = hoursPerDay;
            DaysPerWeek = daysPerWeek;
            WeeksPerMonth = weeksPerMonth;
            MonthsPerYear = monthsPerYear;
        }

        public ulong SecondsPerMinute { get; }
        public ulong MinutesPerHour { get; }
        public ulong HoursPerDay { get; }
        public ulong DaysPerWeek { get; }
        public ulong WeeksPerMonth { get; }
        public ulong MonthsPerYear { get; }

        private ulong SecondsPerHour => SecondsPerMinute * MinutesPerHour;
        private ulong SecondsPerDay => SecondsPerHour * HoursPerDay;
        private ulong SecondsPerWeek => SecondsPerDay * DaysPerWeek;
        private ulong SecondsPerMonth => SecondsPerWeek * WeeksPerMonth;
        private ulong SecondsPerYear => SecondsPerMonth * MonthsPerYear;

        public ulong TotalSeconds { get; private set; }
        public ulong TotalMinutes => TotalSeconds / SecondsPerMinute;
        public ulong TotalHours => TotalSeconds / SecondsPerHour;
        public ulong TotalDays => TotalSeconds / SecondsPerDay;
        public ulong TotalWeeks => TotalSeconds / SecondsPerWeek;
        public ulong TotalMonths => TotalSeconds / SecondsPerMonth;
        public ulong TotalYears => TotalSeconds / SecondsPerYear;

        public ulong Second => TotalSeconds % SecondsPerMinute;
        public ulong Minute => TotalMinutes % MinutesPerHour;
        public ulong Hour => TotalHours % HoursPerDay;
        public ulong Day => TotalDays % DaysPerWeek;
        public ulong Week => TotalWeeks % WeeksPerMonth;
        public ulong Month => TotalMonths % MonthsPerYear;
        public ulong Year => TotalYears;

        public void Add(ulong seconds = 0, ulong minutes = 0, ulong hours = 0, ulong days = 0, ulong weeks = 0, ulong months = 0, ulong years = 0)
        {
            TotalSeconds += seconds +
                (minutes * SecondsPerMinute) +
                (hours * SecondsPerHour) +
                (days * SecondsPerDay) +
                (weeks * SecondsPerWeek) +
                (months * SecondsPerMonth) +
                (years * SecondsPerYear);
        }

        public void AddSeconds(ulong seconds) => Add(seconds: seconds);
        public void AddMinutes(ulong minutes) => Add(minutes: minutes);
        public void AddHours(ulong hours) => Add(hours: hours);
        public void AddDays(ulong days) => Add(days: days);
        public void AddWeeks(ulong weeks) => Add(weeks: weeks);
        public void AddMonths(ulong months) => Add(months: months);
        public void AddYears(ulong years) => Add(years: years);

        public void Set(ulong seconds = 0, ulong minutes = 0, ulong hours = 0, ulong days = 0, ulong weeks = 0, ulong months = 0, ulong years = 0)
        {
            TotalSeconds = seconds +
                (minutes * SecondsPerMinute) +
                (hours * SecondsPerHour) +
                (days * SecondsPerDay) +
                (weeks * SecondsPerWeek) +
                (months * SecondsPerMonth) +
                (years * SecondsPerYear);
        }

        public override string ToString()
        {
            return $"{Year}y {Month}m {Week}w {Day}d {Hour}h {Minute}min {Second}s";
        }
    }
}
