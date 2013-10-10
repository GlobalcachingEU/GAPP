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
	internal class CgtReader {
		private readonly BinaryReader reader;
		private int entryCount; // Number of entries left

		public CgtReader(BinaryReader reader) {
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}
			this.reader = reader;
		}

		public int EntryCount {
			get {
				return entryCount;
			}
		}

		public void CopyEntry(CgtWriter writer) {
			if (writer == null) {
				throw new ArgumentNullException("writer");
			}
			switch (ReadEntryType()) {
			case CgtEntryType.Boolean:
				writer.WriteBoolEntry(reader.ReadBoolean());
				break;
			case CgtEntryType.Byte:
				writer.WriteByteEntry(reader.ReadByte());
				break;
			case CgtEntryType.Empty:
				writer.WriteEmptyEntry();
				break;
			case CgtEntryType.Integer:
				writer.WriteIntegerEntry(reader.ReadUInt16());
				break;
			case CgtEntryType.String:
				writer.WriteStringEntry(ReadString());
				break;
			default:
				throw new InvalidOperationException("Unknown entry type");
			}
		}

		public bool HasMoreData() {
			return reader.PeekChar() != -1;
		}

		public bool ReadBoolEntry() {
			ReadAndCheckEntryType(CgtEntryType.Boolean);
			return reader.ReadBoolean();
		}

		public byte ReadByteEntry() {
			ReadAndCheckEntryType(CgtEntryType.Byte);
			return reader.ReadByte();
		}

		public void ReadEmptyEntry() {
			ReadAndCheckEntryType(CgtEntryType.Empty);
		}

		public string ReadHeaderString() {
			if (entryCount != 0) {
				throw new FileLoadException("Header expected");
			}
			return ReadString();
		}

		public int ReadIntegerEntry() {
			ReadAndCheckEntryType(CgtEntryType.Integer);
			return reader.ReadUInt16();
		}

		public CgtRecordType ReadNextRecord() {
			var recordType = (char)reader.ReadByte();
			//Structure below is ready for future expansion
			switch (recordType) {
			case 'M':
				//Read the number of entry's
				entryCount = reader.ReadUInt16();
				return (CgtRecordType)ReadByteEntry();
			default:
				throw new FileLoadException("Invalid record header");
			}
		}

		public string ReadStringEntry() {
			ReadAndCheckEntryType(CgtEntryType.String);
			return ReadString();
		}

		private void ReadAndCheckEntryType(CgtEntryType expectedEntryType) {
			CgtEntryType entryType = ReadEntryType();
			if (entryType != expectedEntryType) {
				throw new FileLoadException(string.Format("{0} entry expected, but {1} entry found", expectedEntryType, entryType));
			}
		}

		private CgtEntryType ReadEntryType() {
			if (entryCount <= 0) {
				throw new FileLoadException("No entry found");
			}
			entryCount--;
			return (CgtEntryType)reader.ReadByte();
		}

		private string ReadString() {
			var result = new StringBuilder();
			var unicodeChar = (char)reader.ReadUInt16();
			while (unicodeChar != (char)0) {
				result.Append(unicodeChar);
				unicodeChar = (char)reader.ReadUInt16();
			}
			return result.ToString();
		}
	}
}
