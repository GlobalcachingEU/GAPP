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

namespace bsn.GoldParser.Grammar {
	/// <summary>
	/// Action in a LR State. 
	/// </summary>
	public abstract class LalrAction: GrammarObject<LalrAction> {
		private readonly Symbol symbol;

		/// <summary>
		/// Creats a new instance of the <c>LalrAction</c> class.
		/// </summary>
		/// <param name="index">Index of the LR state action.</param>
		/// <param name="symbol">Symbol associated with the action.</param>
		internal LalrAction(int index, Symbol symbol): base(symbol.Owner, index) {
			this.symbol = symbol;
		}

		/// <summary>
		/// Gets action type.
		/// </summary>
		public abstract LalrActionType ActionType {
			get;
		}

		/// <summary>
		/// Gets symbol associated with the LR state action.
		/// </summary>
		public Symbol Symbol {
			get {
				return symbol;
			}
		}

		public virtual object Target {
			get {
				return null;
			}
		}

		public override string ToString() {
			return string.Format("Action {0}: {1}, Symbol {2}, Target: {3}", Index, ActionType, Symbol, Target);
		}

		internal abstract TokenParseResult Execute<T>(IParser<T> parser, T token) where T: IToken;
	}
}