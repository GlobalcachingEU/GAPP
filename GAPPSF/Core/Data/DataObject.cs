using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Core.Data
{
    public class DataObject
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EventArgs> DataChanged;

        private static byte[] _buffer = new byte[10000000];
        private static byte[] _checkbuffer = new byte[5000000];
        private static MemoryStream _ms = new MemoryStream(_checkbuffer);
        private static BinaryWriter _bw = new BinaryWriter(_ms);

        private int _updateCounter = 0;
        private bool IsDataChanged = false;

        protected Storage.RecordInfo RecordInfo = null;
        protected byte[] DataBuffer { get { return _buffer; } }

        public long[] _cachePropertyPositions = null;
        public long[] CachePropertyPositions 
        {
            get { return _cachePropertyPositions; } 
            set
            {
                if (value!=_cachePropertyPositions)
                {
                    _cachePropertyPositions = value;
                    if (_cachePropertyPositions!=null && _cachePropertyPositions.Length>0)
                    {
                        if (CachedPropertyValues==null)
                        {
                            CachedPropertyValues = new Hashtable();
                        }
                    }
                    else
                    {
                        CachedPropertyValues = null;
                    }
                }
            }
        }
        public Hashtable CachedPropertyValues { get; private set; }

        public DataObject(Storage.RecordInfo recordInfo)
        {
            RecordInfo = recordInfo;
            CachedPropertyValues = null;
        }

        public void DeleteRecord()
        {
            if (RecordInfo != null && RecordInfo.Database != null)
            {
                RecordInfo.Database.DeleteRecord(RecordInfo);
            }
        }

        public void BeginUpdate()
        {
            if (_updateCounter == 0)
            {
                IsDataChanged = false;
            }
            _updateCounter++;
        }
        public void EndUpdate()
        {
            _updateCounter--;
            if (_updateCounter <= 0)
            {
                if (IsDataChanged)
                {
                    OnDataChanged();
                }
            }
        }

        public void OnDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this, EventArgs.Empty);
            }
            IsDataChanged = false;
        }

        protected void SetProperty<T>(long pos, ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                IsDataChanged = true;
                field = value;
                StoreProperty(pos, name, value);
                if (_updateCounter == 0)
                {
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(name));
                    }
                }
            }
        }

        protected void SetStringProperty(long pos, long nextpos, ref string field, string value, [CallerMemberName] string name = "")
        {
            string rvalue = GetSafeString(pos, nextpos, value);
            if (!EqualityComparer<string>.Default.Equals(field, rvalue))
            {
                IsDataChanged = true;
                field = rvalue;
                StoreProperty(pos, name, rvalue);
                if (_updateCounter == 0)
                {
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs(name));
                    }
                }
            }
        }

        protected string GetSafeString(long pos, long nextpos, string value)
        {
            string result = value;
            if (!string.IsNullOrEmpty(value))
            {
                while (!checkStringFits(result, pos, nextpos))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        protected bool checkStringFits(string s, long startPos)
        {
            return checkStringFits(s, startPos, RecordInfo.Length);
        }
        protected bool checkStringFits(string s, long startPos, long nextPos)
        {
            bool result;
            _ms.Position = 0;
            _bw.Write(s);
            result = (_ms.Position <= (nextPos - startPos));
            return result;
        }

        protected string readString(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                string s = this.RecordInfo.Database.BinaryReader.ReadString();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return o as string;
            }
        }
        protected long readLong(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                long s = this.RecordInfo.Database.BinaryReader.ReadInt64();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return (long)o;
            }
        }
        protected double readDouble(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                double s = this.RecordInfo.Database.BinaryReader.ReadDouble();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return (double)o;
            }
        }
        protected int readInt(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                int s = this.RecordInfo.Database.BinaryReader.ReadInt32();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return (int)o;
            }
        }
        protected byte readByte(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                byte s = this.RecordInfo.Database.BinaryReader.ReadByte();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return (byte)o;
            }
        }
        protected bool readBool(long pos)
        {
            object o = CachedPropertyValues == null ? null : CachedPropertyValues[pos];
            if (o == null)
            {
                this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                bool s = this.RecordInfo.Database.BinaryReader.ReadBoolean();
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = s;
                }
                return s;
            }
            else
            {
                return (bool)o;
            }
        }

        protected virtual void StoreProperty(long pos, string name, object value)
        {
            if (pos>=0 && value != null)
            {
                if (CachePropertyPositions != null && CachePropertyPositions.Contains(pos))
                {
                    CachedPropertyValues[pos] = value;
                }

                if (value.GetType() == typeof(string))
                {
                    this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                    this.RecordInfo.Database.BinaryWriter.Write(value as string);
                }
                else if (value.GetType() == typeof(DateTime))
                {
                    this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                    this.RecordInfo.Database.BinaryWriter.Write(Utils.Conversion.DateTimeToLong((DateTime)value));
                }
                else if (value.GetType() == typeof(bool))
                {
                    this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                    this.RecordInfo.Database.BinaryWriter.Write((bool)value);
                }
                else if (value.GetType() == typeof(int))
                {
                    this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                    this.RecordInfo.Database.BinaryWriter.Write((int)value);
                }
                else if (value.GetType() == typeof(double))
                {
                    this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
                    this.RecordInfo.Database.BinaryWriter.Write((double)value);
                }
            }
        }
    }
}
