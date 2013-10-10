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

namespace bsn.GoldParser.Grammar {
	/// <summary>
	/// State of LR parser.
	/// </summary>
	public class LalrState: GrammarObject<LalrState> {
		private LalrAction[] actions;
		private LalrAction[] transitionVector;

		/// <summary>
		/// Creates a new instance of the <c>LalrState</c> class
		/// </summary>
		/// <param name="index">Index of the LR state in the LR state table.</param>
		internal LalrState(CompiledGrammar owner, int index): base(owner, index) {
			if (owner == null) {
				throw new ArgumentNullException("owner");
			}
		}

		/// <summary>
		/// Gets LR state action count.
		/// </summary>
		public int ActionCount {
			get {
				return actions.Length;
			}
		}

		/// <summary>
		/// Returns state action by its index.
		/// </summary>
		/// <param name="index">State action index.</param>
		/// <returns>LR state action for the given index.</returns>
		public LalrAction GetAction(int index) {
			return actions[index];
		}

		/// <summary>
		/// Returns LR state action by symbol index.
		/// </summary>
		/// <param name="symbol">The symbol.</param>
		/// <returns>LR state action object.</returns>
		public LalrAction GetActionBySymbol(Symbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			return transitionVector[symbol.Index];
		}

		public override string ToString() {
			return string.Format("LALR State {0}", Index);
		}

		/// <summary>
		/// Initializes the specified actions.
		/// </summary>
		/// <param name="actions">List of all available LR actions in this state.</param>
		/// <param name="transitionVector">Transition vector which has symbol index as an index.</param>
		internal void Initialize(LalrAction[] actions, LalrAction[] transitionVector) {
			if (actions == null) {
				throw new ArgumentNullException("actions");
			}
			if (transitionVector == null) {
				throw new ArgumentNullException("transitionVector");
			}
			if ((this.actions != null) || (this.transitionVector != null)) {
				throw new InvalidOperationException("The LALR state is already initialized.");
			}
			this.actions = actions;
			this.transitionVector = transitionVector;
		}
	}
}
