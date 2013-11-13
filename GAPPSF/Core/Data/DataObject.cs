using System;
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

        public DataObject(Storage.RecordInfo recordInfo)
        {
            RecordInfo = recordInfo;
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
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadString();
        }
        protected long readLong(long pos)
        {
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadInt64();
        }
        protected double readDouble(long pos)
        {
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadDouble();
        }
        protected int readInt(long pos)
        {
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadInt32();
        }
        protected byte readByte(long pos)
        {
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadByte();
        }
        protected bool readBool(long pos)
        {
            this.RecordInfo.Database.FileStream.Position = this.RecordInfo.Offset + pos;
            return this.RecordInfo.Database.BinaryReader.ReadBoolean();
        }

        protected virtual void StoreProperty(long pos, string name, object value)
        {
            if (pos>=0 && value != null)
            {
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
