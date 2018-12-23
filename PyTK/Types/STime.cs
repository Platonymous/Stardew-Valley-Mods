using StardewValley;
using System;

namespace PyTK.Types
{
    public class STime : IComparable<STime>
    {
        public int timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                setTimeFromTimestamp();
            }
        }
        public int season
        {
            get => _season;
            set
            {
                _season = value;
                setTimestamp();
            }
        }
        public int day
        {
            get => _day;
            set
            {
                _day = value;
                setTimestamp();
            }
        }
        public int hour
        {
            get => _hour;
            set
            {
                _hour = value;
                setTimestamp();
            }
        }
        public int minute
        {
            get => _minute;
            set
            {
                _minute = value;
                setTimestamp();
            }
        }
        public int year
        {
            get => _year;
            set
            {
                _year = value;
                setTimestamp();
            }
        }

        private int _timestamp;
        private int _season;
        private int _day;
        private int _hour;
        private int _minute;
        private int _year;

        private const int sMinute = 1;
        private const int sHour = sMinute * 60; //60
        private const int sDay = sHour * 24; //1440
        private const int sSeason = sDay * 28; // 40320
        private const int sYear = sSeason * 4; // 161280

        public STime()
        {

        }

        public STime (int year, int season, int day, int timeOfDay)
            :this(year,season,day, (int)Math.Floor((decimal)timeOfDay / 100), timeOfDay - ((int)Math.Floor((decimal)timeOfDay / 100) * 100))
        {

        }

        public STime(int year, int season, int day, int hour, int minute)
        {
            _year = year;
            _day = day;
            _season = season;
            _hour = hour;
            _minute = minute;
            setTimestamp();
        }

        public STime(int timestamp)
        {
            _timestamp = timestamp;
            setTimeFromTimestamp();
        }

        public static STime CURRENT 
        {
            get => new STime(Game1.year, Utility.getSeasonNumber(Game1.currentSeason), Game1.dayOfMonth, Game1.timeOfDay);
        }

        public static STime ZERO
        {
            get => new STime(0);
        }

        public static STime YEAR
        {
            get => new STime(sYear);
        }

        public static STime SEASON
        {
            get => new STime(sSeason);
        }

        public static STime DAY
        {
            get => new STime(sDay);
        }

        public static STime HOUR
        {
            get => new STime(sHour);
        }

        public static STime MINUTE
        {
            get => new STime(sMinute);
        }

        private void setTimestamp()
        {
            _timestamp = (year * sYear) + (season * sSeason) + (day * sDay) + (hour * sHour) + (minute * sMinute);
        }

        private void setTimeFromTimestamp()
        { 
            _year = (int) Math.Floor((decimal)timestamp / sYear);
            _season = (int)Math.Floor((decimal)timestamp / sSeason) % 4; 
            _day = (int)Math.Floor((decimal)timestamp / sDay) % 28;
            _hour = (int)Math.Floor((decimal)timestamp / sHour) % 24;
            _minute = (int)Math.Floor((decimal)timestamp / sMinute) % 60;
        }

        public int CompareTo(STime other)
        {
            return timestamp.CompareTo(other.timestamp);
        }

        public override string ToString()
        {
            return $"STime[Year:{year} Season:{season} Day:{day} Hour:{hour} Minute:{minute}";
        }

        public override bool Equals(object obj)
        {
            if (obj is STime s)
                return this == s;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return timestamp;
        }

        public static STime operator +(STime value1, STime value2)
        {
            return new STime(value1.timestamp + value2.timestamp);
        }

        public static STime operator +(STime value1, int value2)
        {
            return new STime(value1.timestamp + value2);
        }

        public static STime operator -(STime value1, STime value2)
        {
            return new STime(value1.timestamp - value2.timestamp);
        }

        public static STime operator *(STime value1, STime value2)
        {
            return new STime(value1.timestamp * value2.timestamp);
        }

        public static STime operator *(STime value1, int value2)
        {
            return new STime(value1.timestamp * value2);
        }

        public static float operator /(STime value1, STime value2)
        {
            return value1.timestamp / (float)value2.timestamp;
        }

        public static STime operator %(STime value1, STime value2)
        {
            return new STime(value1.timestamp % value2.timestamp);
        }

        public static STime operator /(STime value1, int value2)
        {
            return new STime(value1.timestamp / value2);
        }

        public static STime operator -(STime value1, int value2)
        {
            return new STime(value1.timestamp - value2);
        }

        public static bool operator >(STime value1, STime value2)
        {
            return (value1.timestamp > value2.timestamp);
        }

        public static bool operator <(STime value1, STime value2)
        {
            return (value1.timestamp < value2.timestamp);
        }

        public static bool operator >=(STime value1, STime value2)
        {
            return (value1.timestamp >= value2.timestamp);
        }

        public static bool operator <=(STime value1, STime value2)
        {
            return (value1.timestamp <= value2.timestamp);
        }

        public static bool operator ==(STime value1, STime value2)
        {
            if (!(value1 is STime))
                value1 = ZERO;

            if (!(value2 is STime))
                value2 = ZERO;

            return (value1.timestamp == value2.timestamp);
        }

        public static bool operator !=(STime value1, STime value2)
        {
            if (!(value1 is STime))
                value1 = ZERO;

            if (!(value2 is STime))
                value2 = ZERO;

            return (value1.timestamp != value2.timestamp);
        }
    }
}
