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
using System.Collections.Generic;
using System.Xml;

using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Xml {
	/// <summary>
	/// Exposes the grammar tree previously parsed into a <see cref="Token"/> as XML tree, using
	/// elements with the same name as their symbol in the grammar and namespaces 
	/// (<c>urn:nonterminal</c> and <c>urn:terminal</c>) to represent all nodes.
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Only terminals have line information attached to them.</item>
	/// <item>All insignificant data, such as comments and whitespace, is not included in the 
	/// XML output.</item>
	/// <item>Symbol names are converted to XML compatible names by removing all invalid 
	/// characters from the name. If the resulting name is empty, a <c>_</c> (underscore) 
	/// character is used as name. Note that XML names may therefore be ambiguous compared to 
	/// the original names, the symbols <c>A B</c> and <c>AB</c> for instance would get the 
	/// same name (<c>AB</c>), as well as <c>==</c> and <c>.</c> etc. would (<c>_</c>).</item>
	/// </list>
	/// </remarks>
	public class TokenXmlReader: XmlReader {
		private enum ActiveAttribute {
			None,
			Line,
			Column
		}

		private enum ElementPosition {
			Start,
			Text,
			End
		}

		/// <summary>
		/// The XML namespace used for nonterminal symbol token elements.
		/// </summary>
		public const string NonterminalNS = "urn:nonterminal";

		/// <summary>
		/// The XML namespace used for terminal symbol token elements.
		/// </summary>
		public const string TerminalNS = "urn:terminal";

		private readonly string columnAttribute;
		private readonly string lineAttribute;
		private readonly string nPrefix;
		private readonly XmlNameTable nametable;
		private readonly string nonterminalNs;
		private readonly Stack<KeyValuePair<Token, int>> stack = new Stack<KeyValuePair<Token, int>>();
		private readonly string tPrefix;
		private readonly string terminalNs;
		private ActiveAttribute activeAttribute;
		private Token current;
		private ElementPosition elementPosition;
		private bool onAttributeValue;
		private ReadState readState;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenXmlReader"/> class.
		/// </summary>
		/// <param name="nametable">The nametable (optional, may be <c>null</c>) to be used.</param>
		/// <param name="root">The root (if <c>null</c>, no nodes are returned from the reader).</param>
		public TokenXmlReader(XmlNameTable nametable, Token root) {
			this.nametable = nametable ?? new NameTable();
			terminalNs = this.nametable.Add(TerminalNS);
			nonterminalNs = this.nametable.Add(NonterminalNS);
			tPrefix = this.nametable.Add("t");
			nPrefix = this.nametable.Add("n");
			lineAttribute = this.nametable.Add("line");
			columnAttribute = this.nametable.Add("column");
			readState = ReadState.Initial;
			if ((root != null) && (root.Symbol != null)) {
				current = root;
			}
		}

		/// <summary>
		/// Gets the number of attributes on the current node.
		/// </summary>
		/// <value></value>
		/// <returns>The number of attributes on the current node.</returns>
		public override int AttributeCount {
			get {
				return IsTerminal ? 2 : 0;
			}
		}

		/// <summary>
		/// Gets the base URI of the current node, which is empty by default.
		/// </summary>
		/// <value></value>
		/// <returns>The base URI of the current node.</returns>
		public override string BaseURI {
			get {
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the depth of the current node in the XML document.
		/// </summary>
		/// <value></value>
		/// <returns>The depth of the current node in the XML document.</returns>
		public override int Depth {
			get {
				int result = stack.Count;
				if (current != null) {
					result++;
					if (activeAttribute != ActiveAttribute.None) {
						result++;
						if (onAttributeValue) {
							result++;
						}
					} else if (elementPosition == ElementPosition.Text) {
						result++;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the reader is positioned at the end of the stream.
		/// </summary>
		/// <value></value>
		/// <returns>true if the reader is positioned at the end of the stream; otherwise, false.</returns>
		public override bool EOF {
			get {
				return (ReadState == ReadState.EndOfFile);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node can have a <see cref="P:System.Xml.XmlReader.Value"/>.
		/// </summary>
		/// <value></value>
		/// <returns>true if the node on which the reader is currently positioned can have a Value; otherwise, false. If false, the node has a value of String.Empty.</returns>
		public override bool HasValue {
			get {
				return (activeAttribute != ActiveAttribute.None) || IsTerminal;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).
		/// </summary>
		/// <value></value>
		/// <returns>true if the current node is an element (<see cref="P:System.Xml.XmlReader.NodeType"/> equals XmlNodeType.Element) that ends with /&gt;; otherwise, false.</returns>
		public override bool IsEmptyElement {
			get {
				return false;
			}
		}

		/// <summary>
		/// Gets the local name of the current node.
		/// </summary>
		/// <value></value>
		/// <returns>The name of the current node with the prefix removed. For example, LocalName is book for the element &lt;bk:book&gt;.For node types that do not have a name (like Text, Comment, and so on), this property returns String.Empty.</returns>
		public override string LocalName {
			get {
				switch (activeAttribute) {
				case ActiveAttribute.Line:
					if (!onAttributeValue) {
						return lineAttribute;
					}
					break;
				case ActiveAttribute.Column:
					if (!onAttributeValue) {
						return columnAttribute;
					}
					break;
				default:
					if ((current != null) && (elementPosition != ElementPosition.Text)) {
						return nametable.Add(current.Symbol.XmlName);
					}
					break;
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the <see cref="T:System.Xml.XmlNameTable"/> associated with this implementation.
		/// </summary>
		/// <value></value>
		/// <returns>The XmlNameTable enabling you to get the atomized version of a string within the node.</returns>
		public override XmlNameTable NameTable {
			get {
				return nametable;
			}
		}

		/// <summary>
		/// Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
		/// </summary>
		/// <value></value>
		/// <returns>The namespace URI of the current node; otherwise an empty string.</returns>
		public override string NamespaceURI {
			get {
				if ((activeAttribute == ActiveAttribute.None) && (current != null) && (elementPosition != ElementPosition.Text)) {
					return IsTerminal ? terminalNs : nonterminalNs;
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the type of the current node.
		/// </summary>
		/// <value></value>
		/// <returns>One of the <see cref="T:System.Xml.XmlNodeType"/> values representing the type of the current node.</returns>
		public override XmlNodeType NodeType {
			get {
				if (onAttributeValue) {
					return XmlNodeType.Text;
				}
				if (activeAttribute != ActiveAttribute.None) {
					return XmlNodeType.Attribute;
				}
				if (current != null) {
					switch (elementPosition) {
					case ElementPosition.Start:
						return XmlNodeType.Element;
					case ElementPosition.Text:
						return XmlNodeType.Text;
					case ElementPosition.End:
						return XmlNodeType.EndElement;
					}
				}
				return XmlNodeType.None;
			}
		}

		/// <summary>
		/// Gets the namespace prefix associated with the current node.
		/// </summary>
		/// <value></value>
		/// <returns>The namespace prefix associated with the current node.</returns>
		public override string Prefix {
			get {
				if ((activeAttribute == ActiveAttribute.None) && (current != null) && (elementPosition != ElementPosition.Text)) {
					return IsTerminal ? tPrefix : nPrefix;
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets the state of the reader.
		/// </summary>
		/// <value></value>
		/// <returns>One of the <see cref="T:System.Xml.ReadState"/> values.</returns>
		public override ReadState ReadState {
			get {
				return readState;
			}
		}

		/// <summary>
		/// Gets the text value of the current node.
		/// </summary>
		/// <value></value>
		/// <returns>The value returned depends on the <see cref="P:System.Xml.XmlReader.NodeType"/> of the node. The following table lists node types that have a value to return. All other node types return String.Empty.Node type Value AttributeThe value of the attribute. CDATAThe content of the CDATA section. CommentThe content of the comment. DocumentTypeThe internal subset. ProcessingInstructionThe entire content, excluding the target. SignificantWhitespaceThe white space between markup in a mixed content model. TextThe content of the text node. WhitespaceThe white space between markup. XmlDeclarationThe content of the declaration. </returns>
		public override string Value {
			get {
				switch (activeAttribute) {
				case ActiveAttribute.Line:
					return XmlConvert.ToString(current.Position.Line);
				case ActiveAttribute.Column:
					return XmlConvert.ToString(current.Position.Column);
				default:
					if (IsTerminal) {
						return current.ToString();
					}
					break;
				}
				return string.Empty;
			}
		}

		private bool IsTerminal {
			get {
				return current is TextToken;
			}
		}

		/// <summary>
		/// Changes the <see cref="P:System.Xml.XmlReader.ReadState"/> to Closed.
		/// </summary>
		public override void Close() {
			readState = ReadState.Closed;
		}

		/// <summary>
		/// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.Name"/>.
		/// </summary>
		/// <param name="name">The qualified name of the attribute.</param>
		/// <returns>
		/// The value of the specified attribute. If the attribute is not found, null is returned.
		/// </returns>
		public override string GetAttribute(string name) {
			switch (name) {
			case "line":
				if (IsTerminal) {
					return XmlConvert.ToString(current.Position.Line);
				}
				break;
			case "column":
				if (IsTerminal) {
					return XmlConvert.ToString(current.Position.Column);
				}
				break;
			}
			return string.Empty;
		}

		/// <summary>
		/// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName"/> and <see cref="P:System.Xml.XmlReader.NamespaceURI"/>.
		/// </summary>
		/// <param name="name">The local name of the attribute.</param>
		/// <param name="namespaceURI">The namespace URI of the attribute.</param>
		/// <returns>
		/// The value of the specified attribute. If the attribute is not found, null is returned. This method does not move the reader.
		/// </returns>
		public override string GetAttribute(string name, string namespaceURI) {
			if (string.IsNullOrEmpty(namespaceURI)) {
				return GetAttribute(name);
			}
			return string.Empty;
		}

		/// <summary>
		/// Gets the value of the attribute with the specified index.
		/// </summary>
		/// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.)</param>
		/// <returns>
		/// The value of the specified attribute. This method does not move the reader.
		/// </returns>
		public override string GetAttribute(int i) {
			if (IsTerminal) {
				switch (i) {
				case 0:
					return XmlConvert.ToString(current.Position.Line);
				case 1:
					return XmlConvert.ToString(current.Position.Column);
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Resolves a namespace prefix in the current element's scope.
		/// </summary>
		/// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string.</param>
		/// <returns>
		/// The namespace URI to which the prefix maps or null if no matching prefix is found.
		/// </returns>
		public override string LookupNamespace(string prefix) {
			switch (prefix) {
			case "t":
				return terminalNs;
			case "n":
				return nonterminalNs;
			}
			return string.Empty;
		}

		/// <summary>
		/// Moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.Name"/>.
		/// </summary>
		/// <param name="name">The qualified name of the attribute.</param>
		/// <returns>
		/// true if the attribute is found; otherwise, false. If false, the reader's position does not change.
		/// </returns>
		public override bool MoveToAttribute(string name) {
			if (IsTerminal) {
				switch (name) {
				case "line":
					activeAttribute = ActiveAttribute.Line;
					onAttributeValue = false;
					return true;
				case "column":
					activeAttribute = ActiveAttribute.Column;
					onAttributeValue = false;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName"/> and <see cref="P:System.Xml.XmlReader.NamespaceURI"/>.
		/// </summary>
		/// <param name="name">The local name of the attribute.</param>
		/// <param name="ns">The namespace URI of the attribute.</param>
		/// <returns>
		/// true if the attribute is found; otherwise, false. If false, the reader's position does not change.
		/// </returns>
		public override bool MoveToAttribute(string name, string ns) {
			if (string.IsNullOrEmpty(ns)) {
				return MoveToAttribute(name);
			}
			return false;
		}

		/// <summary>
		/// Moves to the element that contains the current attribute node.
		/// </summary>
		/// <returns>
		/// true if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); false if the reader is not positioned on an attribute (the position of the reader does not change).
		/// </returns>
		public override bool MoveToElement() {
			if (activeAttribute != ActiveAttribute.None) {
				activeAttribute = ActiveAttribute.None;
				onAttributeValue = false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Moves to the first attribute.
		/// </summary>
		/// <returns>
		/// true if an attribute exists (the reader moves to the first attribute); otherwise, false (the position of the reader does not change).
		/// </returns>
		public override bool MoveToFirstAttribute() {
			if (IsTerminal) {
				activeAttribute = ActiveAttribute.Line;
				onAttributeValue = false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Moves to the next attribute.
		/// </summary>
		/// <returns>
		/// true if there is a next attribute; false if there are no more attributes.
		/// </returns>
		public override bool MoveToNextAttribute() {
			switch (activeAttribute) {
			case ActiveAttribute.None:
				return MoveToFirstAttribute();
			case ActiveAttribute.Line:
				activeAttribute = ActiveAttribute.Column;
				onAttributeValue = false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Reads the next node from the stream.
		/// </summary>
		/// <returns>
		/// true if the next node was read successfully; false if there are no more nodes to read.
		/// </returns>
		/// <exception cref="T:System.Xml.XmlException">An error occurred while parsing the XML. </exception>
		public override bool Read() {
			switch (readState) {
			case ReadState.Initial:
				if (current != null) {
					readState = ReadState.Interactive;
					return true;
				}
				readState = ReadState.EndOfFile;
				return false;
			case ReadState.Interactive:
				MoveToElement();
				switch (elementPosition) {
				case ElementPosition.Start:
					if (IsTerminal) {
						elementPosition = ElementPosition.Text;
						return true;
					} else {
						Reduction reduction = current as Reduction;
						if (reduction != null) {
							if (reduction.Children.Count > 0) {
								stack.Push(new KeyValuePair<Token, int>(reduction, 0));
								current = reduction.Children[0];
								return true;
							}
						}
					}
					elementPosition = ElementPosition.End;
					return true;
				case ElementPosition.Text:
					elementPosition = ElementPosition.End;
					return true;
				case ElementPosition.End:
					if (stack.Count > 0) {
						KeyValuePair<Token, int> pair = stack.Pop();
						current = pair.Key;
						Reduction reduction = (Reduction)current;
						if (pair.Value < (reduction.Children.Count-1)) {
							current = reduction.Children[pair.Value+1];
							stack.Push(new KeyValuePair<Token, int>(pair.Key, pair.Value+1));
							elementPosition = ElementPosition.Start;
						}
						return true;
					}
					readState = ReadState.EndOfFile;
					break;
				}
				break;
			}
			return false;
		}

		/// <summary>
		/// Parses the attribute value into one or more Text, EntityReference, or EndEntity nodes.
		/// </summary>
		/// <returns>
		/// <c>true</c> if there are nodes to return. <c>false</c> if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read.An empty attribute, such as, misc="", returns <c>true</c> with a single node with a value of String.Empty.
		/// </returns>
		public override bool ReadAttributeValue() {
			if (onAttributeValue) {
				return false;
			}
			onAttributeValue = (activeAttribute != ActiveAttribute.None);
			return onAttributeValue;
		}

		/// <summary>
		/// Resolves the entity reference for EntityReference nodes.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">The reader is not positioned on an EntityReference node; this implementation of the reader cannot resolve entities (<see cref="P:System.Xml.XmlReader.CanResolveEntity"/> returns <c>false</c>). </exception>
		public override void ResolveEntity() {
			throw new InvalidOperationException();
		}
	}
}