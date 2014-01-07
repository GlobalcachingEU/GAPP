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

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	/// <summary>
	/// The abstract nongeneric case class for semantic terminal tokens. This class is for internal use only.
	/// </summary>
	/// <typeparam name="TBase">The base type of the semantic token.</typeparam>
	public abstract class SemanticTerminalFactory<TBase>: SemanticTokenFactory<TBase> where TBase: SemanticToken {
		internal SemanticTerminalFactory() {}

		public abstract TBase CreateAndInitialize(Symbol symbol, LineInfo position, string text);
	}

	/// <summary>
	/// The abstract generic case class for semantic terminal tokens. This class is usually not directly inherited.
	/// </summary>
	/// <typeparam name="TBase">The base type of the semantic token.</typeparam>
	/// <typeparam name="TOutput">The type of the terminal token.</typeparam>
	public abstract class SemanticTerminalFactory<TBase, TOutput>: SemanticTerminalFactory<TBase> where TBase: SemanticToken where TOutput: TBase {
		public override sealed Type OutputType {
			get {
				return typeof(TOutput);
			}
		}

		public override sealed TBase CreateAndInitialize(Symbol symbol, LineInfo position, string text) {
			Debug.Assert(symbol != null);
			TOutput result = Create(text);
			Debug.Assert(result != null);
			result.Initialize(symbol, position);
			return result;
		}

		protected abstract TOutput Create(string text);
	}
}