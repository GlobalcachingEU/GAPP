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
	/// The base class for the grammar objects <see cref="DfaState"/>, <see cref="LalrAction"/>, <see cref="LalrState"/>, <see cref="Rule"/> and <see cref="Symbol"/>.
	/// </summary>
	/// <typeparam name="TSelf">The type of the grammar object implemented (self).</typeparam>
	public abstract class GrammarObject<TSelf>: IEquatable<TSelf> where TSelf: GrammarObject<TSelf> {
		private readonly int index;
		private readonly CompiledGrammar owner;

		/// <summary>
		/// Initializes a new instance of the <see cref="GrammarObject&lt;TSelf&gt;"/> class.
		/// </summary>
		/// <param name="owner">The owner grammar.</param>
		/// <param name="index">The index.</param>
		protected GrammarObject(CompiledGrammar owner, int index) {
			this.owner = owner;
			this.index = index;
		}

		/// <summary>
		/// Gets index of grammar object in the <see cref="CompiledGrammar"/>.
		/// </summary>
		public int Index {
			get {
				return index;
			}
		}

		/// <summary>
		/// Gets the owner.
		/// </summary>
		/// <value>The owner.</value>
		public CompiledGrammar Owner {
			get {
				return owner;
			}
		}

		public override sealed bool Equals(object other) {
			return ReferenceEquals(this, other);
		}

		public override sealed int GetHashCode() {
			return base.GetHashCode();
		}

		public bool Equals(TSelf other) {
			return ReferenceEquals(this, other);
		}
	}
}
