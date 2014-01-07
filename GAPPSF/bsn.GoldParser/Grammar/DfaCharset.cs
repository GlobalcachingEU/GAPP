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

namespace bsn.GoldParser.Grammar {
	/// <summary>
	/// A grammar charset representation
	/// </summary>
	public sealed class DfaCharset: GrammarObject<DfaCharset> {
		private readonly char[] charsetExcludingSequence;
		private readonly char[] charsetIncludingSequence;
		private readonly char sequenceEnd;
		private readonly char sequenceStart;

		internal DfaCharset(CompiledGrammar owner, int index, string charset): base(owner, index) {
			if (string.IsNullOrEmpty(charset)) {
				throw new ArgumentException("Empty charsets are not supported", "charset");
			}
			charsetIncludingSequence = charset.ToCharArray();
			Array.Sort(charsetIncludingSequence);
			char currentStartChar = charsetIncludingSequence[0];
			char currentChar = currentStartChar;
			sequenceStart = currentStartChar;
			sequenceEnd = currentStartChar;
			int sequenceLength = 1;
			int currentLength = 1;
			for (int i = 1; i < charsetIncludingSequence.Length; i++) {
				currentChar ++;
				if (charsetIncludingSequence[i] == currentChar) {
					currentLength++;
					if (currentLength > sequenceLength) {
						sequenceLength = currentLength;
						sequenceStart = currentStartChar;
						sequenceEnd = currentChar;
					}
				} else {
					currentLength = 1;
					currentStartChar = charsetIncludingSequence[i];
					currentChar = currentStartChar;
				}
			}
			charsetExcludingSequence = new char[charsetIncludingSequence.Length-sequenceLength];
			int charsetIndex = 0;
			foreach (char c in charsetIncludingSequence) {
				if ((c < sequenceStart) || (c > sequenceEnd)) {
					charsetExcludingSequence[charsetIndex++] = c;
				}
			}
			Debug.Assert(charsetIndex == charsetExcludingSequence.Length);
		}

		public char[] CharactersExcludingSequence {
			get {
				return charsetExcludingSequence;
			}
		}

		public char[] CharactersIncludingSequence {
			get {
				return charsetIncludingSequence;
			}
		}

		public char SequenceEnd {
			get {
				return sequenceEnd;
			}
		}

		public int SequenceLength {
			get {
				return (sequenceEnd-sequenceStart)+1;
			}
		}

		public char SequenceStart {
			get {
				return sequenceStart;
			}
		}
	}
}
