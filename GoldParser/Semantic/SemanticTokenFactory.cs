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

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Semantic {
	/// <summary>
	/// The abstract base class for all seamntic token factories.
	/// </summary>
	public abstract class SemanticTokenFactory<TBase> where TBase: SemanticToken {
		/// <summary>
		/// Gets a value indicating whether the type created by this factory can vary or not. Typically, all factories but trim factories will return a static output type.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is static output type; otherwise, <c>false</c>.
		/// </value>
		public bool IsStaticOutputType {
			get {
				return RedirectForOutputType == null;
			}
		}

		/// <summary>
		/// Gets the type of the instances created by this factory.
		/// </summary>
		/// <value>The type of the output.</value>
		public abstract Type OutputType {
			get;
		}

		protected internal virtual Symbol RedirectForOutputType {
			get {
				return null;
			}
		}
	}
}