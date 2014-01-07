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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Parser {
	/// <summary>
	/// A reduction token, which contains the child tokens reduced with the <see cref="ParentRule"/>.
	/// </summary>
	public class Reduction: Token {
		private readonly Rule rule;
		private readonly ReadOnlyCollection<Token> tokens;

		internal Reduction(Rule rule, IList<Token> tokens): base() {
			if (rule == null) {
				throw new ArgumentNullException("rule");
			}
			if (tokens == null) {
				throw new ArgumentNullException("tokens");
			}
			this.rule = rule;
			Token[] tokenArray = new Token[tokens.Count];
			tokens.CopyTo(tokenArray, 0);
			this.tokens = Array.AsReadOnly(tokenArray);
		}

		public ReadOnlyCollection<Token> Children {
			[DebuggerStepThrough]
			get {
				return tokens;
			}
		}

		public override sealed LineInfo Position {
			[DebuggerStepThrough]
			get {
				return tokens.Count > 0 ? tokens[0].Position : default(LineInfo);
			}
		}

		public override sealed Symbol Symbol {
			[DebuggerStepThrough]
			get {
				return rule.RuleSymbol;
			}
		}

		public Rule Rule {
			get {
				return rule;
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			foreach (Token token in tokens) {
				if (sb.Length > 0) {
					sb.Append(' ');
				}
				sb.Append(token);
			}
			return sb.ToString();
		}
	}
}