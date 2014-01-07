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
	/// LR parser action type.
	/// </summary>
	public enum LalrActionType {
		/// <summary>
		/// No action. Not used.
		/// </summary>
		None = 0,

		/// <summary>
		/// Shift a symbol and go to a state
		/// </summary>
		Shift = 1,

		/// <summary>
		/// Reduce by a specified rule
		/// </summary>
		Reduce = 2,

		/// <summary>
		/// Goto to a state on reduction
		/// </summary>
		Goto = 3,

		/// <summary>
		/// Input successfully parsed
		/// </summary>
		Accept = 4,

		/// <summary>
		/// Error
		/// </summary>
		Error = 5
	}
}