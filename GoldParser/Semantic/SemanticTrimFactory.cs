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

namespace bsn.GoldParser.Semantic {
	public sealed class SemanticTrimFactory<TBase>: SemanticNonterminalFactory<TBase> where TBase: SemanticToken {
		private readonly int handleIndex;
		private readonly SemanticActions<TBase> owner;
		private readonly Rule rule;

		internal SemanticTrimFactory(SemanticActions<TBase> owner, Rule rule, int handleIndex) {
			if (owner == null) {
				throw new ArgumentNullException("owner");
			}
			if (rule == null) {
				throw new ArgumentNullException("rule");
			}
			if ((handleIndex < 0) || (handleIndex >= rule.SymbolCount)) {
				throw new ArgumentOutOfRangeException("handleIndex");
			}
			this.owner = owner;
			this.handleIndex = handleIndex;
			this.rule = rule;
		}

		public override ReadOnlyCollection<Type> InputTypes {
			get {
				return Array.AsReadOnly(new[] {GetRuleType()});
			}
		}

		public override Type OutputType {
			get {
				return GetRuleType();
			}
		}

		protected internal override Symbol RedirectForOutputType {
			get {
				return GetTrimSymbol();
			}
		}

		public override TBase CreateAndInitialize(Rule rule, IList<TBase> tokens) {
			Debug.Assert(this.rule == rule);
			TBase result = tokens[handleIndex];
			Debug.Assert(OutputType.IsAssignableFrom(result.GetType()));
			return result;
		}

		protected internal override IEnumerable<Symbol> GetInputSymbols(Rule rule) {
			yield return GetTrimSymbol();
		}

		private Type GetRuleType() {
			if (owner.Initialized) {
				return owner.GetSymbolOutputType(GetTrimSymbol());
			}
			return typeof(TBase);
		}

		private Symbol GetTrimSymbol() {
			return rule[handleIndex];
		}
	}
}