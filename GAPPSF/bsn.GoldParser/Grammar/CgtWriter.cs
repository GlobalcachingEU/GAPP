// bsn GoldParser .NET Engine
// --------------------------
// 
// Copyright 2009, 2010 by Arsène von Wyss - avw@gmx.ch
// 
// Development has been supported by Sirius Technologies AG, Basel
// 
// Source:
// 
// https://bsn-goldparser.googlecode.com/hg/
// 
// License:
// 
// The library is distributed under the GNU Lesser General Public License:
// http://www.gnu.org/licenses/lgpl.html
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
using System;
using System.IO;
using System.Text;

namespace bsn.GoldParser.Grammar {
	internal class CgtWriter {
		private readonly BinaryWriter writer;
		private int entryCount;

		public CgtWriter(BinaryWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			this.writer = writer;
		}

		public void WriteBoolEntry(bool value) {
			WriteEntryType(CgtEntryType.Boolean);
			entryCount--;
			writer.Write(value);
		}

		public void WriteByteEntry(byte value) {
			WriteEntryType(CgtEntryType.Byte);
			entryCount--;
			writer.Write(value);
		}

		public void WriteEmptyEntry() {
			WriteEntryType(CgtEntryType.Empty);
			entryCount--;
		}

		public void WriteHeaderString(string header) {
			if (entryCount != 0) {
				throw new FileLoadException("Header expected");
			}
			WriteString(header);
		}

		public void WriteIntegerEntry(int value) {
			WriteEntryType(CgtEntryType.Integer);
			entryCount--;
			writer.Write(checked((UInt16)value));
		}

		public void WriteNextRecord(CgtRecordType recordType, int entries) {
			if (entryCount != 0) {
				throw new InvalidOperationException("There are entries missing before starting a new record");
			}
			entryCount = entries+1;
			writer.Write((byte)'M');
			writer.Write(checked((UInt16)entryCount));
			WriteByteEntry((byte)recordType);
		}

		public void WriteStringEntry(string value) {
			WriteEntryType(CgtEntryType.String);
			entryCount--;
			WriteString(value);
		}

		private void WriteEntryType(CgtEntryType entryType) {
			if (entryCount <= 0) {
				throw new FileLoadException("No entry pending in this record");
			}
			writer.Write((byte)entryType);
		}

		private void WriteString(string data) {
			if (data == null) {
				throw new ArgumentNullException("data");
			}
			writer.Write(Encoding.Unicode.GetBytes(data));
			writer.Write((UInt16)0);
		}
	}
}
