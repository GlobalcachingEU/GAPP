using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace GAPPSF.UIControls
{
    public class TimeEntryConverter : TypeConverter
    {
        public TimeEntryConverter() { }
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            return sourceType == Type.GetType("System.String");
        }
        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return destinationType == Type.GetType("System.String");
        }
        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, System.Globalization.CultureInfo cultureInfo, object source)
        {
            if (source == null)
                throw new System.ArgumentNullException();

            String[] strArr = ((String)source).Split(',');

            if (strArr.Count() != 3)
                throw new System.ArgumentException();

            int[] HMS = new int[3];

            for (int i = 0; i < strArr.Count(); i++)
            {
                if (!int.TryParse(strArr[i], out HMS[i]))
                    throw new System.ArgumentException();
            }
            if (HMS[0] >= 24 || HMS[1] >= 60 || HMS[2] >= 60)
                throw new System.ArgumentException();

            return new TimeEntry(HMS[0], HMS[1], HMS[2]);
        }
        public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, System.Globalization.CultureInfo cultureInfo, object value, Type destinationType)
        {
            if (value == null)
                throw new System.ArgumentNullException();
            if (!(value is TimeEntry))
                throw new System.ArgumentException();

            TimeEntry te = (TimeEntry)(value);

            return te.ToString();
        }
    }
    [TypeConverter(typeof(TimeEntryConverter))]
    public struct TimeEntry : IEquatable<TimeEntry>
    {
        private int hour, minute, second;

        public TimeEntry(int Hour, int Minute, int Second)
        {
            this.hour = Hour;
            this.minute = Minute;
            this.second = Second;
        }
        public int Hour { get { return hour; } set { hour = value; } }
        public int Minute { get { return minute; } set { minute = value; } }
        public int Second { get { return second; } set { second = value; } }

        public static bool operator !=(TimeEntry te1, TimeEntry te2)
        {
            return te1.Hour != te2.Hour || te1.Minute != te2.Minute || te1.Second != te2.Second;
        }
        public static bool operator ==(TimeEntry te1, TimeEntry te2)
        {
            return te1.Hour == te2.Hour && te1.Minute == te2.Minute && te1.Second != te2.Second;
        }
        public bool Equals(TimeEntry te)
        {
            return this == te;
        }
        public override bool Equals(object obj)
        {
            if (obj is TimeEntry)
                return this == (TimeEntry)obj;
            else
                return false;
        }
        public override int GetHashCode()
        {
            return Hour << 12 | Minute << 6 | Second;
        }
        public override string ToString()
        {
            return Hour.ToString() + "," + Minute.ToString() + "," + Second.ToString();
        }
        private static bool Comparer(int val1, int val2, ref bool IsLessEqual)
        {
            bool Compared = true;

            if (val1 > val2)
                IsLessEqual = true;
            else if (val1 < val2)
                IsLessEqual = false;
            else
                Compared = false;

            return Compared;
        }
        public static bool operator >= (DateTime dt, TimeEntry te)
        {
            bool IsLessEqual = true;

            if (!Comparer(dt.Hour, te.Hour, ref IsLessEqual) && !Comparer(dt.Minute, te.Minute, ref IsLessEqual))
                return (dt.Second >= te.Second);

            return IsLessEqual;
        }
        public static bool operator <= (DateTime dt, TimeEntry te)
        {
            bool IsGreaterEqual = true;

            if (!Comparer(te.Hour, dt.Hour, ref IsGreaterEqual) && !Comparer(te.Minute, dt.Minute, ref IsGreaterEqual))
                return (te.Second >= dt.Second);

            return IsGreaterEqual;
        }
    }
    public class ValidTimeItem : DependencyObject
    {
        private static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(TimeEntry), typeof(ValidTimeItem));
        private static readonly DependencyProperty EndTimeProperty =
            DependencyProperty.Register("EndTime", typeof(TimeEntry), typeof(ValidTimeItem));
        public ValidTimeItem()
        {
        }
        public ValidTimeItem(TimeEntry BeginTime, TimeEntry EndTime)
        {
            this.BeginTime = BeginTime;
            this.EndTime = EndTime;
        }
        public TimeEntry BeginTime
        {
            get { return (TimeEntry)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }
        public TimeEntry EndTime
        {
            get { return (TimeEntry)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }
    }
}
