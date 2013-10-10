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
	/// A generic interface for tokenizers.
	/// </summary>
	/// <remarks>
	/// The <see cref="LalrProcessor{T}"/> instances accept any tokenizer implementing this interface, but usually a tokenizer derived from the default implementation <see cref="Tokenizer{T}"/> is used.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public interface ITokenizer<T> {
		/// <summary>
		/// Gets the grammar used by the tokenizer.
		/// </summary>
		/// <value>The compiled grammar.</value>
		CompiledGrammar Grammar {
			get;
		}

		/// <summary>
		/// Tries to read and tokenize the next token.
		/// </summary>
		/// <param name="token">The new token.</param>
		/// <returns>A parsing result.</returns>
		ParseMessage NextToken(out T token);
	}
}