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
	/// Available parse messages.
	/// </summary>
	public enum ParseMessage {
		/// <summary>
		/// Nothing
		/// </summary>
		None = 0,

		/// <summary>
		/// Each time a token is read, this message is generated.
		/// </summary>
		TokenRead = 1,

		/// <summary>
		/// When the engine is able to reduce a rule,
		/// this message is returned. The rule that was
		/// reduced is set in the GOLDParser's ReduceRule property.
		/// The tokens that are reduced and correspond the
		/// rule's definition are stored in the Tokens() property.
		/// </summary>
		Reduction = 2,

		/// <summary>
		/// The engine will returns this message when the source
		/// text has been accepted as both complete and correct.
		/// In other words, the source text was successfully analyzed.
		/// </summary>
		Accept = 3,

		/// <summary>
		/// The tokenizer will generate this message when
		/// it is unable to recognize a series of characters
		/// as a valid token. To recover, pop the invalid
		/// token from the input queue.
		/// </summary>
		LexicalError = 5,

		/// <summary>
		/// Often the parser will read a token that is not expected
		/// in the grammar. When this happens, the Tokens() property
		/// is filled with tokens the parsing engine expected to read.
		/// To recover: push one of the expected tokens on the input queue.
		/// </summary>
		SyntaxError = 6,

		/// <summary>
		/// The parser reached the end of the file while reading a comment.
		/// This is caused when the source text contains a "run-away"
		/// comment, or in other words, a block comment that lacks the
		/// delimiter.
		/// </summary>
		BlockError = 7,
		[Obsolete("Renamed to GroupError for EGT files")]
		CommentError = 7,

		/// <summary>
		/// Something is wrong, very wrong.
		/// </summary>
		InternalError = 8,

		/// <summary>
		/// A block comment is complete.
		/// When this message is returned, the content of the CurrentComment
		/// property is set to the comment text. The text includes starting and ending
		/// block comment characters.
		/// </summary>
		BlockRead = 9,
		[Obsolete("Renamed to BlockRead for EGT files")]
		CommentBlockRead = 9,

		/// <summary>
		/// Line comment is read.
		/// When this message is returned, the content of the CurrentComment
		/// property is set to the comment text. The text includes starting 
		/// line comment characters.
		/// </summary>
		[Obsolete("EGT files always return BlockRead instead of CommentLineRead")]
		CommentLineRead = 10
	}
}