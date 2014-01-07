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
using System.Globalization;

namespace bsn.GoldParser.Parser {
	/// <summary>
	/// A structure holding information about the text position of a specific token.
	/// </summary>
	public struct LineInfo: IEquatable<LineInfo>, IComparable<LineInfo> {
		private readonly int column;
		private readonly long index;
		private readonly int line;

		/// <summary>
		/// Initializes a new instance of the <see cref="LineInfo"/> struct.
		/// </summary>
		/// <param name="index">The character index.</param>
		/// <param name="line">The line.</param>
		/// <param name="column">The column.</param>
		public LineInfo(long index, int line, int column) {
			this.line = line;
			this.index = index;
			this.column = column;
		}

		/// <summary>
		/// Gets the column.
		/// </summary>
		/// <value>The column.</value>
		public int Column {
			get {
				return column;
			}
		}

		/// <summary>
		/// Gets the character index.
		/// </summary>
		/// <value>The character index.</value>
		public long Index {
			get {
				return index;
			}
		}

		/// <summary>
		/// Gets the line.
		/// </summary>
		/// <value>The line.</value>
		public int Line {
			get {
				return line;
			}
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) {
				return false;
			}
			if (obj.GetType() != typeof(LineInfo)) {
				return false;
			}
			return Equals((LineInfo)obj);
		}

		public override int GetHashCode() {
			unchecked {
				return (line*397)^(column*31)^index.GetHashCode();
			}
		}

		public override string ToString() {
			return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", line, column);
		}

		public int CompareTo(LineInfo other) {
			return Math.Sign(index-other.index);
		}

		public bool Equals(LineInfo other) {
			return (other.line == line) && (other.column == column) && (other.index == index);
		}
	}
}