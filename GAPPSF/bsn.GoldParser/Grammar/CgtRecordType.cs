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
namespace bsn.GoldParser.Grammar {
	/// <summary>
	/// Record type byte in the binary grammar file.
	/// </summary>
	internal enum CgtRecordType {
		Parameters = (int)'P', // 80
		Property = (int)'p', // 112
		Groups = (int)'g', // 103
		TableCounts = (int)'T', // 84
		TableCountsEnhanced = (int)'t', // 116
		Initial = (int)'I', // 73
		Symbols = (int)'S', // 83
		Charsets = (int)'C', // 67
		PackedCharsets = (int)'c', // 99
		Rules = (int)'R', // 82
		DfaStates = (int)'D', // 68
		LRStates = (int)'L', // 76
		Comment = (int)'!' // 33
	}
}
