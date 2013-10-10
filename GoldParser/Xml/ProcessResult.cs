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

using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Xml {
	/// <summary>
	/// A structure to return the parsing result of a <see cref="GrammarXmlProcessor"/>
	/// </summary>
	public struct ProcessResult {
		private readonly string errorMessage;
		private readonly LineInfo lineInfo;

		internal ProcessResult(LineInfo lineInfo, string errorMessage) {
			this.lineInfo = lineInfo;
			this.errorMessage = errorMessage;
		}

		/// <summary>
		/// Gets the text column.
		/// </summary>
		/// <value>The text column.</value>
		public int Column {
			get {
				return lineInfo.Column;
			}
		}

		/// <summary>
		/// Gets the text line.
		/// </summary>
		/// <value>The text line.</value>
		public int Line {
			get {
				return lineInfo.Line;
			}
		}

		/// <summary>
		/// Gets the parser error message.
		/// </summary>
		/// <value>The parser message.</value>
		public string Message {
			get {
				return errorMessage ?? string.Empty;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the processing was terminated successfully.
		/// </summary>
		/// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
		public bool Success {
			get {
				return string.IsNullOrEmpty(errorMessage);
			}
		}

		public override string ToString() {
			if (Success) {
				return "Parsed successfully";
			}
			return string.Format("({0},{1}): {2}", Line, Column, errorMessage);
		}
	}
}