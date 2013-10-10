using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Cachebox
{
    public class DLong
    {
        private ulong low;
        private ulong high;

        /// <summary>
        /// Constructor for create with lower and higher Long
        /// </summary>
        /// <param name="High">higher Long</param>
        /// <param name="Low">lower Long</param>
        public DLong(ulong High, ulong Low)
        {
            low = Low;
            high = High;
        }

        public ulong getLow()
        {
            return low;
        }

        public ulong getHigh()
        {
            return high;
        }

        public void setLow(ulong value)
        {
            low = value;
        }

        public void setHigh(ulong value)
        {
            high = value;
        }


        public static DLong shift(int value)
        {
            ulong low = 0;
            ulong high = 0;

            if (value > 62)
            {
                high = 1UL << (value - 63);
            }
            else
            {
                low = 1UL << value;
            }

            return new DLong(high, low);
        }

        public DLong bitAdd(DLong value)
        {
            low = this.low + value.getLow();
            high = this.high + value.getHigh();

            return this;
        }

        public DLong BitAnd(DLong value)
        {
            low = this.low & value.getLow();
            high = this.high & value.getHigh();

            return this;
        }

        public DLong BitOr(DLong value)
        {
            low = this.low | value.getLow();
            high = this.high | value.getHigh();

            return this;
        }

        public bool BitAndBiggerNull(DLong value)
        {
            bool bLow = (this.low & value.getLow()) > 0;
            bool bHigh = (this.high & value.getHigh()) > 0;

            return (bLow || bHigh) ? true : false;
        }
        /*
        public override string ToString()
        {
            StringBuilder Sb = new StringBuilder();

            Sb.AppendLine("low =" + this.low.ToString());
            Sb.AppendLine("high=" + this.high.ToString());
            Sb.AppendLine("high:" + getUInt64BitString(high) + "  low:" + getUInt64BitString(low));
            Sb.AppendLine("True Bits[]=" + getTrueArray(low) + getTrueArray(high, 64));

            return Sb.ToString();
        }


        private String getUInt64BitString(ulong value)
        {
            byte[] storearr = BitConverter.GetBytes(value);
            BitArray ba = new BitArray(storearr);

            StringBuilder Sb = new StringBuilder();
            for (int i = ba.Count - 1; i > -1; i--)
            {
                bool bit = ba.Get(i);
                Sb.Append((bit ? 1 : 0) + ",");
            }

            return Sb.ToString();
        }
        */

        private String getTrueArray(ulong value, int add = 0)
        {
            StringBuilder Sb = new StringBuilder();

            for (int i = 0; i < 64; i++)
            {
                ulong mask = 1UL << i;
                if ((mask & value) > 0)
                {
                    Sb.Append("[" + (add + i).ToString() + "],");
                }

            }
            return Sb.ToString();
        }

    }
}
