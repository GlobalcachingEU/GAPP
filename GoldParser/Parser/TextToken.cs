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

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Parser {
	/// <summary>
	/// Represents data about current token.
	/// </summary>
	public sealed class TextToken: Token {
		private readonly LineInfo position; // Token source line number.
		private readonly Symbol symbol; // Token symbol.
		private readonly string text; // Token text.

		internal TextToken(Symbol symbol, LineInfo position, string text) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			if (text == null) {
				throw new ArgumentNullException("text");
			}
			this.symbol = symbol;
			this.position = position;
			this.text = this.symbol.Name.Equals(text, StringComparison.Ordinal) ? this.symbol.Name : text; // "intern" short strings which are equal to the terminal name
		}

		public override LineInfo Position {
			get {
				return position;
			}
		}

		public override Symbol Symbol {
			get {
				return symbol;
			}
		}

		public override string Text {
			get {
				return text;
			}
		}

		public override string ToString() {
			return text;
		}
	}
}