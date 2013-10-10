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
using System.Text.RegularExpressions;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Semantic {
	/// <summary>
	/// This class is used to decorate constructors which accept exactly one string for the terminal value
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class TerminalAttribute: Attribute {
		private static readonly Regex rxSpecialToken = new Regex(@"^\(.*\)$");
		private readonly Type[] genericTypes;

		private readonly string symbolName;

		public TerminalAttribute(string symbolName): this(symbolName, null) {}

		public TerminalAttribute(string symbolName, params Type[] genericTypes) {
			if (string.IsNullOrEmpty(symbolName)) {
				throw new ArgumentNullException("symbolName");
			}
			this.symbolName = rxSpecialToken.IsMatch(symbolName) ? symbolName : Symbol.FormatTerminalSymbol(symbolName);
			this.genericTypes = genericTypes ?? Type.EmptyTypes;
		}

		public Type[] GenericTypes {
			get {
				return genericTypes;
			}
		}

		public bool IsGeneric {
			get {
				return genericTypes.Length > 0;
			}
		}

		public string SymbolName {
			get {
				return symbolName;
			}
		}

		public Symbol Bind(CompiledGrammar grammar) {
			if (grammar == null) {
				throw new ArgumentNullException("grammar");
			}
			Symbol result;
			grammar.TryGetSymbol(symbolName, out result);
			return result;
		}
	}
}