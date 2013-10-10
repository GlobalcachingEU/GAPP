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

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	/// <summary>
	/// The abstract nongeneric case class for semantic nonterminal tokens. This class is for internal use only.
	/// </summary>
	/// <typeparam name="TBase">The base type of the semantic token.</typeparam>
	public abstract class SemanticNonterminalFactory<TBase>: SemanticTokenFactory<TBase> where TBase: SemanticToken {
		public abstract ReadOnlyCollection<Type> InputTypes {
			get;
		}

		public abstract TBase CreateAndInitialize(Rule rule, IList<TBase> tokens);
		protected internal abstract IEnumerable<Symbol> GetInputSymbols(Rule rule);
	}

	/// <summary>
	/// The abstract generic case class for semantic nonterminal tokens. This class is usually not directly inherited.
	/// </summary>
	/// <typeparam name="TBase">The base type of the semantic token.</typeparam>
	/// <typeparam name="TOutput">The type of the nonterminal token.</typeparam>
	public abstract class SemanticNonterminalFactory<TBase, TOutput>: SemanticNonterminalFactory<TBase> where TBase: SemanticToken where TOutput: TBase {
		public override sealed Type OutputType {
			get {
				return typeof(TOutput);
			}
		}

		public abstract TOutput Create(Rule rule, IList<TBase> tokens);

		public override sealed TBase CreateAndInitialize(Rule rule, IList<TBase> tokens) {
			Debug.Assert(rule != null);
			TOutput result = Create(rule, tokens);
			Debug.Assert(result != null);
			LineInfo position = default(LineInfo);
			for (int i = 0; i < tokens.Count; i++) {
				IToken token = tokens[i];
				if (token.Position.Index > 0) {
					position = token.Position;
					break;
				}
			}
			result.Initialize(rule.RuleSymbol, position);
			return result;
		}

		protected internal override IEnumerable<Symbol> GetInputSymbols(Rule rule) {
			if (rule == null) {
				throw new ArgumentNullException("rule");
			}
			return rule;
		}
	}
}