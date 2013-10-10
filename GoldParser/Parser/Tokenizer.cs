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

using System;
using System.Diagnostics;
using System.IO;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Parser {
	///<summary>
	/// The tokenizer reads an input character stream and outputs the tokens read.
	///</summary>
	/// <remarks>
	/// A pull-model is used for the tokenizer.
	/// </remarks>
	public abstract class Tokenizer<T>: ITokenizer<T> where T: class, IToken {
		private enum ParseMode {
			SingleSymbol,
			MergeLexicalErrors
		}

		private readonly TextBuffer buffer; // Buffer to keep current characters.
		private readonly CompiledGrammar grammar;
		private bool mergeLexicalErrors;

		/// <summary>
		/// Initializes new instance of Parser class.
		/// </summary>
		/// <param name="textReader"><see cref="TextReader"/> instance to read data from.</param>
		/// <param name="grammar">The grammar used for the DFA states</param>
		protected Tokenizer(TextReader textReader, CompiledGrammar grammar): this(new TextBuffer(textReader), grammar) {}

		/// <summary>
		/// Initializes new instance of Parser class.
		/// </summary>
		/// <param name="textBuffer"><see cref="TextBuffer"/> instance to read data from.</param>
		/// <param name="grammar">The grammar used for the DFA states</param>
		protected Tokenizer(TextBuffer textBuffer, CompiledGrammar grammar) {
			this.grammar = grammar;
			if (textBuffer == null) {
				throw new ArgumentNullException("textBuffer");
			}
			if (grammar == null) {
				throw new ArgumentNullException("grammar");
			}
			buffer = textBuffer;
		}

		/// <summary>
		/// Gets the index of the input.
		/// </summary>
		/// <value>The index of the input.</value>
		public long InputIndex {
			get {
				return buffer.Position;
			}
		}

		/// <summary>
		/// Gets current char position in the current source line. It is 1-based.
		/// </summary>
		public int LineColumn {
			get {
				return buffer.Column;
			}
		}

		/// <summary>
		/// Gets current line number. It is 1-based.
		/// </summary>
		public int LineNumber {
			get {
				return buffer.Line;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether lexical errors are merged (so that they contain more than one character).
		/// </summary>
		/// <value><c>true</c> if lexical errors are to be merged; otherwise, <c>false</c>.</value>
		public bool MergeLexicalErrors {
			get {
				return mergeLexicalErrors;
			}
			set {
				mergeLexicalErrors = value;
			}
		}

		/// <summary>
		/// Gets source of parsed data.
		/// </summary>
		public TextReader TextReader {
			get {
				return buffer.TextReader;
			}
		}

		/// <summary>
		/// Gets the text buffer.
		/// </summary>
		/// <value>
		/// The text buffer.
		/// </value>
		protected TextBuffer Buffer {
			get {
				return buffer;
			}
		}

		protected abstract T CreateToken(Symbol tokenSymbol, LineInfo tokenPosition, string text);

		private ParseMessage NextSymbol(IGroup parent, ParseMode mode, out Symbol tokenSymbol, ref int length) {
			DfaState state = grammar.DfaInitialState;
			char ch;
			tokenSymbol = null;
			int offset = length;
			while (buffer.TryLookahead(ref offset, out ch)) {
				state = state.GetTransition(ch);
				// This block-if statement checks whether an edge was found from the current state.
				// If so, the state and current position advance. Otherwise it is time to exit the main loop
				// and report the token found (if there was it fact one). If the LastAcceptState is -1,
				// then we never found a match and the Error Token is created. Otherwise, a new token
				// is created using the Symbol in the Accept State and all the characters that
				// comprise it.
				if (state == null) {
					// end has been reached
					if (tokenSymbol == null) {
						//Tokenizer cannot recognize symbol
						offset = ++length;
						if (mode == ParseMode.MergeLexicalErrors) {
							while (NextSymbol(parent, ParseMode.SingleSymbol, out tokenSymbol, ref offset) == ParseMessage.LexicalError) {
								length = offset;
							}
						}
						tokenSymbol = grammar.ErrorSymbol;
					}
					break;
				}
				if (parent.IsAllowedDfaState(state)) {
					// This code checks whether the target state accepts a token. If so, it sets the
					// appropriate variables so when the algorithm in done, it can return the proper
					// token and number of characters.
					if (state.AcceptSymbol != null) {
						tokenSymbol = state.AcceptSymbol;
						length = offset;
					}
				} else {
					break;
				}
			}
			if (tokenSymbol == null) {
				if (offset == length) {
					tokenSymbol = grammar.EndSymbol;
				} else {
					tokenSymbol = grammar.ErrorSymbol;
					length++;
				}
			}
			switch (tokenSymbol.Kind) {
#pragma warning disable 612,618
			case SymbolKind.CommentLine:
				Debug.Assert(grammar.FileVersion == CgtVersion.V1_0);
				while (buffer.TryLookahead(length, out ch) && (ch != '\r') && (ch != '\n')) {
					length++;
				}
				return ParseMessage.CommentLineRead;
#pragma warning restore 612,618
			case SymbolKind.BlockStart:
				Group group = grammar.GetGroupByStartSymbol(tokenSymbol);
				if (!parent.IsNestingAllowed(group)) {
					return ParseMessage.LexicalError; // nesting is not allowed -> error
				}
				for (;;) {
					Symbol blockTokenSymbol;
					int oldLength = length;
					NextSymbol(group, ParseMode.SingleSymbol, out blockTokenSymbol, ref length);
					if (blockTokenSymbol.Kind == SymbolKind.End) {
						if (@group.EndingMode == GroupEndingMode.Open) {
							tokenSymbol = @group.ContainerSymbol; // special case: a block that doesn't consume its end (such as a line comment), if we reach EOF we take this as a valid end symbol
							return ParseMessage.BlockRead;
						}
						return ParseMessage.BlockError;
					}
					if (blockTokenSymbol == @group.EndSymbol) {
						if (@group.EndingMode == GroupEndingMode.Open) {
							length = oldLength;
						}
						tokenSymbol = @group.ContainerSymbol;
						return ParseMessage.BlockRead;
					}
				}
			case SymbolKind.Error:
				return ParseMessage.LexicalError;
			}
			return ParseMessage.TokenRead;
		}

		/// <summary>
		/// Gets the grammar.
		/// </summary>
		/// <value>The grammar.</value>
		public CompiledGrammar Grammar {
			get {
				return grammar;
			}
		}

		/// <summary>
		/// Reads next token from the input stream.
		/// </summary>
		/// <returns>Token symbol which was read.</returns>
		public virtual ParseMessage NextToken(out T token) {
			Symbol tokenSymbol;
			int offset = 0;
			ParseMessage result = NextSymbol(DummyGroup.Default, MergeLexicalErrors ? ParseMode.MergeLexicalErrors : ParseMode.SingleSymbol, out tokenSymbol, ref offset);
			LineInfo position;
			string text = buffer.Read(offset, out position);
			token = CreateToken(tokenSymbol, position, text);
			return result;
		}
	}

	/// <summary>
	/// A concrete tokenizer creating the normal <see cref="TextToken"/> as tokens.
	/// </summary>
	public class Tokenizer: Tokenizer<Token> {
		/// <summary>
		/// Initializes a new instance of the <see cref="Tokenizer"/> class.
		/// </summary>
		/// <param name="textReader"><see cref="TextReader"/> instance to read data from.</param>
		/// <param name="grammar">The grammar used for the DFA states</param>
		public Tokenizer(TextReader textReader, CompiledGrammar grammar): base(textReader, grammar) {}

		protected override Token CreateToken(Symbol tokenSymbol, LineInfo tokenPosition, string text) {
			return new TextToken(tokenSymbol, tokenPosition, text);
		}
	}
}
