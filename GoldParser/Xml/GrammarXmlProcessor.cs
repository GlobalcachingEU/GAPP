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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Xml {
	public class GrammarXmlProcessor {
		public static GrammarXmlProcessor Create(Type type, string grammarName, string transformName) {
			if (type == null) {
				throw new ArgumentNullException("type");
			}
			return Create(type, grammarName, type, transformName);
		}

		public static GrammarXmlProcessor Create(Type grammarType, string grammarName, Type transformType, string transformName) {
			if (grammarType == null) {
				throw new ArgumentNullException("grammarType");
			}
			if (grammarName == null) {
				throw new ArgumentNullException("grammarName");
			}
			if (transformType == null) {
				throw new ArgumentNullException("transformType");
			}
			if (transformName == null) {
				throw new ArgumentNullException("transformName");
			}
			CompiledGrammar grammar = CompiledGrammar.Load(grammarType, grammarName);
			using (Stream stream = transformType.Assembly.GetManifestResourceStream(transformType, transformName)) {
				if (stream == null) {
					throw new InvalidOperationException("The transform embedded resource was not found: "+transformName);
				}
				using (XmlReader reader = XmlReader.Create(stream)) {
					return new GrammarXmlProcessor(grammar, reader);
				}
			}
		}

		private readonly CompiledGrammar grammar;
		private readonly XmlNameTable nametable;
		private readonly XslCompiledTransform transform;

		protected GrammarXmlProcessor(CompiledGrammar grammar, XmlReader transform) {
			if (grammar == null) {
				throw new ArgumentNullException("grammar");
			}
			this.grammar = grammar;
			if (transform == null) {
				throw new ArgumentNullException("stylesheet");
			}
			this.transform = new XslCompiledTransform(false);
			this.transform.Load(transform, new XsltSettings(false, false), null);
			nametable = transform.NameTable;
		}

		public ProcessResult TryProcess(TextReader input, XmlWriter output) {
			return TryProcess(input, output, null);
		}

		public ProcessResult TryProcess(TextReader input, XmlWriter output, XmlResolver resolver) {
			if (input == null) {
				throw new ArgumentNullException("input");
			}
			if (output == null) {
				throw new ArgumentNullException("output");
			}
			LalrProcessor parser = new LalrProcessor(new Tokenizer(input, grammar));
			ParseMessage message = parser.Parse();
			while (CompiledGrammar.CanContinueParsing(message)) {
				message = parser.Parse();
			}
			LineInfo position = default(LineInfo);
			if (parser.CurrentToken != null) {
				position = parser.CurrentToken.Position;
			}
			switch (message) {
			case ParseMessage.Accept:
				break;
			case ParseMessage.LexicalError:
				return new ProcessResult(position, string.Format("Lexical error: {0}", parser.CurrentToken));
			case ParseMessage.SyntaxError:
				StringBuilder result = new StringBuilder("Syntax error:");
				foreach (Symbol expectedSymbol in parser.GetExpectedTokens()) {
					result.Append(' ');
					result.Append(expectedSymbol.Name);
				}
				result.Append(" expected");
				return new ProcessResult(position, result.ToString());
			case ParseMessage.CommentError:
				return new ProcessResult(position, "End of block comment not found");
			default: // includes InternalError
				return new ProcessResult(position, "Internal error");
			}
			StringBuilder xsltError = new StringBuilder();
			XsltArgumentList arguments = new XsltArgumentList();
			arguments.XsltMessageEncountered += ((sender, args) => xsltError.Append(args.Message));
			using (XmlReader reader = new TokenXmlReader(nametable, parser.CurrentToken)) {
				transform.Transform(reader, arguments, output, resolver);
			}
			if (xsltError.Length > 0) {
				return new ProcessResult(default(LineInfo), xsltError.ToString());
			}
			return new ProcessResult();
		}
	}
}