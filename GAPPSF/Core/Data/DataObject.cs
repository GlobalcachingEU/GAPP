using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        private int _updateCounter = 0;
        private bool IsDataChanged = false;
        protected Storage.RecordInfo RecordInfo = null;

        public long[] CachePropertyPositions { get; set; }
        public Hashtable CachedPropertyValues { get; private set; }

        public DataObject(Storage.RecordInfo recordInfo)
        {
            RecordInfo = recordInfo;
            CachedPropertyValues = new Hashtable();
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
                else
                {
                    IsDataChanged = false;
                }
            }
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
                    this.RecordInfo.Database.BinaryWriter.Write(((DateTime)value).ToFileTime());
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
