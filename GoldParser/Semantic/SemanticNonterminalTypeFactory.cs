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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Semantic {
	public class SemanticNonterminalTypeFactory<TBase, TOutput>: SemanticNonterminalFactory<TBase, TOutput> where TBase: SemanticToken where TOutput: TBase {
		private readonly SemanticNonterminalTypeFactoryHelper<TBase>.Activator<TOutput> activator;
		private readonly ReadOnlyCollection<Type> inputTypes;

		public SemanticNonterminalTypeFactory(MethodBase methodBase, int[] parameterMapping, int handleCount, Type baseTokenType) {
			if (methodBase == null) {
				throw new ArgumentNullException("methodBase");
			}
			if (parameterMapping == null) {
				throw new ArgumentNullException("parameterMapping");
			}
			if (baseTokenType == null) {
				throw new ArgumentNullException("baseTokenType");
			}
			ParameterInfo[] parameters = methodBase.GetParameters();
			if (parameterMapping.Length != parameters.Length) {
				throw new ArgumentException("The parameter mapping must have exactly as many items as the "+methodBase.MemberType+" has parameters", "parameterMapping");
			}
			int requiredHandles = 0;
			foreach (int i in parameterMapping) {
				if (i >= 0) {
					requiredHandles++;
				}
			}
			if (handleCount < requiredHandles) {
				throw new ArgumentOutOfRangeException("handleCount");
			}
			Type[] inputTypeBuilder = new Type[handleCount];
			for (int i = 0; i < handleCount; i++) {
				inputTypeBuilder[i] = baseTokenType;
			}
			foreach (ParameterInfo parameter in parameters) {
				int tokenIndex = parameterMapping[parameter.Position];
				if (tokenIndex != -1) {
					inputTypeBuilder[tokenIndex] = parameter.ParameterType;
				}
			}
			inputTypes = Array.AsReadOnly(inputTypeBuilder);
			activator = SemanticNonterminalTypeFactoryHelper<TBase>.CreateActivator(this, methodBase, parameterMapping);
			Debug.Assert(activator != null);
		}

		public override ReadOnlyCollection<Type> InputTypes {
			get {
				return inputTypes;
			}
		}

		public override TOutput Create(Rule rule, IList<TBase> tokens) {
			Debug.Assert((tokens != null) && (tokens.Count == inputTypes.Count));
			return activator(tokens);
		}
	}
}
