// bsn GoldParser .NET Engine
// --------------------------
// 
// Copyright 2009, 2010 by Ars�ne von Wyss - avw@gmx.ch
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

namespace bsn.GoldParser.Grammar {
	/// <summary>
	/// The advance mode for a group in the EGT grammar
	/// </summary>
	public enum GroupAdvanceMode: int {
		/// <summary>
		/// The group will advance a token at a time.
		/// </summary>
		Token = 0,

		/// <summary>
		/// The group will advance by just one character at a time.
		/// </summary>
		Character = 1
	}
}