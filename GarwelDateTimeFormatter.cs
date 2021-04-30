using KSP.Localization;
using System;

namespace SpaceAge
{
    public class GarwelDateTimeFormatter : IDateTimeFormatter
    {
        public static readonly GarwelDateTimeFormatter Instance = new GarwelDateTimeFormatter();

        public int Minute => KSPUtil.dateTimeFormatter.Minute;

        public int Hour => KSPUtil.dateTimeFormatter.Hour;

        public int Day => KSPUtil.dateTimeFormatter.Day;

        public int Year => KSPUtil.dateTimeFormatter.Year;

        /// <summary>
        /// Parses UT into a string (e.g. "Y23 D045 1:23:45")
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <param name="showSeconds">If false, seconds will be displayed only if time is less than 1 minute; otherwise always</param>
        /// <returns></returns>
        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            if (time < 0)
                return "—";
            ParseTime((long)time, out int y, out int d, out int h, out int m, out int s, false, true);
            return includeTime
                ? includeSeconds
                    ? Localizer.Format("#SpaceAge_DateTime_Sec", y, d.ToString("D3"), h, m.ToString("D2"), s.ToString("D2"))
                    : Localizer.Format("#SpaceAge_DateTime_NoSec", y, d.ToString("D3"), h, m.ToString("D2"))
                : Localizer.Format("#SpaceAge_Date", y, d.ToString("D3"));
        }

        /// <summary>
        /// Translates number of seconds into a string of [T+][[ddd:]hh:]mm:ss
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            string res;
            if (explicitPositive)
                if (time < 0)
                    res = "T-";
                else res = "T+";
            else res = "";
            time = Math.Abs(time);
            ParseTime((long)time, out int y, out int d, out int h, out int m, out int s, true, false);
            d += y * Year / Day;
            if (d > 0)
                res += $"{d:D3}:";
            if (h > 0 || d > 0)
            {
                if (h < 10 && Day > Hour * 10)
                    res += "0";
                res += $"{h}:";
            }
            res += $"{m:D2}:{s:D2}";
            return res;
        }

        /// <summary>
        /// Translates number of seconds into a string of [[[yy:]ddd:]hh:]mm:ss
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            string res = "";
            ParseTime(Math.Abs((long)time), out int y, out int d, out int h, out int m, out int s, true, years);
            if (years)
            {
                res += $"{y:D2}:";
                days = true;
            }
            if (days)
                res += $"{d:D3}:";
            else h += d * Day / Hour;
            if (days || h > 0)
            {
                if (h < 10 && Day > Hour * 10)
                    res += "0";
                res += $"{h}:";
            }
            res += $"{m:D2}:{s:D2}";
            return res;
        }

        public string PrintTimeLong(double time) => KSPUtil.PrintTimeLong(time);

        public string PrintTimeStamp(double time, bool days = false, bool years = false) => KSPUtil.PrintTimeStamp(time, days, years);

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive) => throw new NotImplementedException();

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish) => throw new NotImplementedException();

        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs) => throw new NotImplementedException();

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs) => throw new NotImplementedException();

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false) => throw new NotImplementedException();

        public string PrintDateNew(double time, bool includeTime) => throw new NotImplementedException();

        void ParseTime(long time, out int y, out int d, out int h, out int m, out int s, bool interval, bool parseYears)
        {
            if (parseYears)
            {
                y = (int)(time / Year);
                time -= y * Year;
                if (!interval)
                    y++;
            }
            else y = 0;
            d = (int)time / Day;
            time -= d * Day;
            h = (int)time / 3600;
            time -= h * 3600;
            m = (int)time / 60;
            s = (int)time - m * 60;
        }
    }
}
