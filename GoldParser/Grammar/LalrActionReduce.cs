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

using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Grammar {
	internal sealed class LalrActionReduce: LalrAction {
		private readonly Rule reduceRule;

		public LalrActionReduce(int index, Symbol symbol, Rule reduceRule): base(index, symbol) {
			if (reduceRule == null) {
				throw new ArgumentNullException("reduceRule");
			}
			this.reduceRule = reduceRule;
		}

		public override LalrActionType ActionType {
			get {
				return LalrActionType.Reduce;
			}
		}

		public override object Target {
			get {
				return reduceRule;
			}
		}

		internal override TokenParseResult Execute<T>(IParser<T> parser, T token) {
			bool trim = reduceRule.ContainsOneNonterminal && parser.CanTrim(reduceRule);
			T head = trim ? parser.PopToken() : parser.CreateReduction(reduceRule);
			LalrActionGoto gotoAction = parser.TopState.GetActionBySymbol(reduceRule.RuleSymbol) as LalrActionGoto;
			if (gotoAction == null) {
				Debug.Fail("Internal table error.");
				return TokenParseResult.InternalError;
			}
			parser.PushTokenAndState(head, gotoAction.State);
			return trim ? TokenParseResult.ReduceEliminated : TokenParseResult.ReduceNormal;
		}
	}
}