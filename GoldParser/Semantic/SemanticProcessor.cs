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
using System.Diagnostics;
using System.IO;

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	public class SemanticProcessor<T>: LalrProcessor<T> where T: SemanticToken {
		private readonly SemanticActions<T> actions;

		public SemanticProcessor(TextReader reader, SemanticActions<T> actions): this(actions.CreateTokenizer(reader), actions) {}

		public SemanticProcessor(ITokenizer<T> tokenizer, SemanticActions<T> actions): base(tokenizer) {
			if (actions == null) {
				throw new ArgumentNullException("actions");
			}
			if (tokenizer.Grammar != actions.Grammar) {
				throw new ArgumentException("Mismatch of tokenizer and action grammars");
			}
			this.actions = actions;
		}

		protected override bool CanTrim(Rule rule) {
			return false;
		}

		protected override T CreateReduction(Rule rule, IList<T> children) {
			SemanticNonterminalFactory<T> factory;
			if (actions.TryGetNonterminalFactory(rule, out factory)) {
				Debug.Assert(factory != null);
				return factory.CreateAndInitialize(rule, children);
			}
			throw new InvalidOperationException(string.Format("Missing a token type for the rule {0}", rule.Definition));
		}
	}
}