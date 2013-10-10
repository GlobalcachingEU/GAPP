﻿// bsn GoldParser .NET Engine
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
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	public abstract class SemanticToken: IToken {
		private LineInfo position;
		private Symbol symbol;

		protected internal virtual void Initialize(Symbol symbol, LineInfo position) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			this.symbol = symbol;
			this.position = position;
		}

		LineInfo IToken.Position {
			get {
				return position;
			}
		}

		bool IToken.NameIs(string name) {
			if (name == null) {
				throw new ArgumentNullException("name");
			}
			return (symbol != null) && name.Equals(symbol.Name, StringComparison.Ordinal);
		}

		Symbol IToken.Symbol {
			get {
				return symbol;
			}
		}
	}
}