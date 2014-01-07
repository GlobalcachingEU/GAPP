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
using System.Diagnostics;
using System.IO;

namespace bsn.GoldParser.Parser {
	public class TextBuffer {
		private const int BufferSize = 2048;

		private readonly TextReader reader;
		private char[] buffer = new char[BufferSize];
		private int bufferLength;
		private int bufferOffset;
		private long bufferPosition;
		private int column = 1;
		private bool eof;
		private int line = 1;
		private char previous;
		private int rollbackBufferOffset;
		private long rollbackBufferPosition = -1;
		private int rollbackColumn;
		private int rollbackLine;
		private char rollbackPrevious;

		public TextBuffer(TextReader reader) {
			if (reader == null) {
				throw new ArgumentNullException("reader");
			}
			this.reader = reader;
		}

		public int Column {
			get {
				return column;
			}
		}

		public int Line {
			get {
				return line;
			}
		}

		public long Position {
			get {
				return bufferPosition+bufferOffset;
			}
		}

		public TextReader TextReader {
			get {
				return reader;
			}
		}

		public string Read(int count, out LineInfo position) {
			position = new LineInfo(bufferPosition+bufferOffset, line, column);
			if (count == 0) {
				return string.Empty;
			}
			if (!EnsureBuffer(count-1)) {
				throw new ArgumentOutOfRangeException("count");
			}
			rollbackBufferOffset = bufferOffset;
			rollbackBufferPosition = bufferPosition;
			rollbackColumn = column;
			rollbackLine = line;
			rollbackPrevious = previous;
			var result = new string(buffer, bufferOffset, count);
			for (int i = 0; i < count; i++) {
				char current = buffer[bufferOffset++];
				switch (current) {
				case '\r':
					HandleNewline(current, '\n');
					break;
				case '\n':
					HandleNewline(current, '\r');
					break;
				default:
					column++;
					previous = current;
					break;
				}
			}
			return result;
		}

		public void Rollback() {
			if (rollbackBufferPosition != bufferPosition) {
				throw new InvalidOperationException("The buffer was flushed and cannot be rolled back");
			}
			bufferOffset = rollbackBufferOffset;
			column = rollbackColumn;
			line = rollbackLine;
			previous = rollbackPrevious;
			rollbackBufferPosition = -1;
		}

		private void HandleNewline(char current, char skipChar) {
			if (previous != skipChar) {
				line++;
				column = 1;
				previous = current;
			} else {
				previous = default(char);
			}
		}

		public bool TryLookahead(ref int offset, out char ch) {
			if (TryLookahead(offset, out ch)) {
				offset ++;
				return true;
			}
			return false;
		}

		public bool TryLookahead(int offset, out char ch) {
			if (EnsureBuffer(offset)) {
				ch = buffer[bufferOffset+offset];
				return true;
			}
			ch = default(char);
			return false;
		}

		private bool EnsureBuffer(int offset) {
			Debug.Assert(offset >= 0);
			if (offset < 0) {
				return false;
			}
			if (bufferOffset+offset < bufferLength) {
				return true;
			}
			if (eof) {
				return false;
			}
			// get rid of unused buffered data to free buffer space
			if (bufferOffset > 0) {
				Array.Copy(buffer, bufferOffset, buffer, 0, bufferLength-bufferOffset);
				bufferPosition += bufferOffset;
				bufferLength -= bufferOffset;
				bufferOffset = 0;
			}
			// if the offset is above the buffer length, enhance the buffer size to match
			if (offset >= buffer.Length) {
				Array.Resize(ref buffer, Math.Max(buffer.Length*2, (offset-(offset%BufferSize)))+BufferSize);
			}
			// now try to fill the buffer with data
			int bufferEmptyCount = buffer.Length-bufferLength;
			Debug.Assert(bufferEmptyCount > 0);
			int length = reader.ReadBlock(buffer, bufferLength, bufferEmptyCount);
			if (length == 0) {
				eof = true;
			} else {
				bufferLength += length;
			}
			return offset < bufferLength;
		}
	}
}
