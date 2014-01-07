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

namespace bsn.GoldParser.Parser {
	/// <summary>
	/// The common interface for all tokens, which carries the (grammar) symbol as well as their position in the input text.
	/// </summary>
	public interface IToken {
		/// <summary>
		/// Gets the line number where this token begins.
		/// </summary>
		/// <value>The line number and position.</value>
		LineInfo Position {
			get;
		}

		/// <summary>
		/// Gets the symbol associated with this token.
		/// </summary>
		/// <value>The parent symbol.</value>
		Symbol Symbol {
			get;
		}

		/// <summary>
		/// Checks if the symbol name is the given name.
		/// </summary>
		/// <param name="name">The name to check.</param>
		/// <returns><c>true</c> if the name match, false otherwise</returns>
		bool NameIs(string name);
	}
}