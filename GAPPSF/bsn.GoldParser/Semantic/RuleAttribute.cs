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

namespace bsn.GoldParser.Semantic {
	/// <summary>
	/// <para>Specifies the binding of a specific rule to a constructor (therefore implying the type to be constructed).</para>
	/// </summary>
	/// <example>
	/// <code><![CDATA[
	/// public class MyRule<T>: SemanticToken {
	///   [Rule("<X> ::= Y <X>)]
	///   public MyRule(MyTerminal y, MyRule x) { ... }
	/// } ]]></code>
	/// </example>
	/// <remarks>
	/// <para>You can apply this attribute to constructors only.</para>
	/// <para>The class which defines the constructor must inherit from the type used as generic type parameter of the <see cref="SemanticTypeActions{T}"/> and be located in the same assembly in order to be found and associated with the grammar rule.</para>
	/// <para>Use the <see cref="ConstructorParameterMapping" /> to map rule handles arbitrarily to constructor parameters.</para>
	/// <para>If the class containing the constructor is generic, you can specify the generic type(s) to use following the rule string.</para>
	/// </remarks>
	/// <seealso cref="TerminalAttribute"/>
	/// <seealso cref="RuleTrimAttribute"/>
	[AttributeUsage(AttributeTargets.Constructor|AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public sealed class RuleAttribute: RuleAttributeBase, IEquatable<RuleAttribute> {
		private readonly Type[] genericTypeParameters;
		private bool allowTruncationForConstructor;
		private int[] constructorParameterMapping;
		private bool? strictlyMatchParameters;

		/// <summary>
		/// Define that the constructor where the attribute is applied shall be invoked on the closed generic type for the given reduction rule.
		/// </summary>
		/// <param name="rule">The rule (in the same form as in the grammar file, such as <c>&lt;List&gt; ::= Item ',' &lt;List&gt;</c>).</param>
		/// <param name="genericTypeParameters">The type parameters to use for closing the generic type.</param>
		[CLSCompliant(false)]
		public RuleAttribute(string rule, params Type[] genericTypeParameters)
				: base(rule) {
			this.genericTypeParameters = genericTypeParameters;
		}

		/// <summary>
		/// Define that the constructor where the attribute is applied shall be invoked on the closed generic type for the given reduction rule.
		/// </summary>
		/// <param name="rule">The rule (in the same form as in the grammar file, such as <c>&lt;List&gt; ::= Item ',' &lt;List&gt;</c>).</param>
		/// <param name="genericTypeParameters">The type parameters to use for closing the generic type.</param>
		public RuleAttribute(string rule, Type genericTypeParam1)
				: this(rule, new[] {genericTypeParam1}) {}

		/// <summary>
		/// Define that the constructor where the attribute is applied shall be invoked on the closed generic type for the given reduction rule.
		/// </summary>
		/// <param name="rule">The rule (in the same form as in the grammar file, such as <c>&lt;List&gt; ::= Item ',' &lt;List&gt;</c>).</param>
		/// <param name="genericTypeParameters">The type parameters to use for closing the generic type.</param>
		public RuleAttribute(string rule, Type genericTypeParam1, Type genericTypeParam2)
				: this(rule, new[] {genericTypeParam1, genericTypeParam2}) {}

		/// <summary>
		/// Define that the constructor where the attribute is applied shall be invoked on the closed generic type for the given reduction rule.
		/// </summary>
		/// <param name="rule">The rule (in the same form as in the grammar file, such as <c>&lt;List&gt; ::= Item ',' &lt;List&gt;</c>).</param>
		/// <param name="genericTypeParameters">The type parameters to use for closing the generic type.</param>
		public RuleAttribute(string rule, Type genericTypeParam1, Type genericTypeParam2, Type genericTypeParam3)
				: this(rule, new[] {genericTypeParam1, genericTypeParam2, genericTypeParam3}) {}

		/// <summary>
		/// Gets or sets a value indicating whether the list of symbols may be truncated when invoking the constructor.
		/// </summary>
		/// <remarks>
		/// <list type="bullet">
		/// <item>The same functionality can be achieved with an explicit <see cref="ConstructorParameterMapping"/>.</item>
		/// <item>If a <see cref="ConstructorParameterMapping"/> is defined, this property has no function.</item>
		/// </list>
		/// </remarks>
		/// <example>
		/// <para>Rule: <c>&lt;List&gt; ::= Item ';'</c></para>
		/// <para>Constructor: <c>MyList(MyItem item)</c></para>
		/// <para>Assuming that there is no explicit <see cref="ConstructorParameterMapping"/> defined, <see cref="AllowTruncationForConstructor"/> must be <c>true</c> to pass the consistency check in this example, since the <c>';'</c> symbol is to be truncated.</para>
		/// </example>
		/// <value>
		/// 	<c>true</c> to allow truncation for the constructor, otherwise <c>false</c>.
		/// </value>
		[Obsolete("Use the extended rule syntax instead of automatic truncation: ~X (don't use X for constructor)", false)]
		public bool AllowTruncationForConstructor {
			get {
				return allowTruncationForConstructor;
			}
			set {
				allowTruncationForConstructor = value;
			}
		}

		/// <summary>
		/// Allows to define an explicit mapping of symbols to constructor parameters. The indices are 0-based.
		/// </summary>
		/// <value>An array with exacltly one integer index for each constructor parameter. The index must point to one of the symbols (0-based).</value>
		/// <example>
		/// Rule: <c>&lt;List&gt; ::= Item ',' &lt;List&gt;</c>
		/// Constructor: <c>MyList(MyList next, Item item)</c>
		/// Mapping: <c>new int[] {2, 0}</c>
		/// </example>
		[Obsolete("To map the constructor arguments, use the extended rule syntax: 0:X (map X to index 0) or ~X (don't use X for constructor)", false)]
		public int[] ConstructorParameterMapping {
			get {
				return constructorParameterMapping;
			}
			set {
				constructorParameterMapping = value;
			}
		}

		/// <summary>
		/// Gets the generic type parameters.
		/// </summary>
		/// <value>The generic type parameters.</value>
		public Type[] GenericTypeParameters {
			get {
				return genericTypeParameters ?? Type.EmptyTypes;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance has a constructor parameter mapping.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has a constructor parameter mapping; otherwise, <c>false</c>.
		/// </value>
		public bool HasConstructorParameterMapping {
			get {
				return constructorParameterMapping != null;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is instantiating a generic type.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is instantiating a generic type; otherwise, <c>false</c>.
		/// </value>
		public bool IsGeneric {
			get {
				return (genericTypeParameters != null) && (genericTypeParameters.Length > 0);
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether constructor parameters will be strictly matched.
		/// </summary>
		/// <value>
		/// If <c>true</c> then every constructor parameter must be matched to a symbol. Otherwise, constructor parameters that don't have a matching symbol will be null.
		/// </value>
		/// <remarks>If not set, the setting passed into the <see cref="SemanticActions{T}.Initialize(bool,bool)"/> method will be used.</remarks>
		public bool StrictlyMatchParameters {
			get {
				return strictlyMatchParameters.GetValueOrDefault();
			}
			set {
				strictlyMatchParameters = value;
			}
		}

		public override bool Equals(object obj) {
			return base.Equals(obj as RuleAttribute);
		}

		public override int GetHashCode() {
			return ParsedRule.ToString().GetHashCode();
		}

		internal bool StrictlyMatchParametersOrDefault(bool strongParameterCheck) {
			return strictlyMatchParameters.GetValueOrDefault(strongParameterCheck);
		}

		public bool Equals(RuleAttribute other) {
			return (other != null) && ParsedRule.ToString().Equals(other.ParsedRule.ToString(), StringComparison.Ordinal);
		}
	}
}
