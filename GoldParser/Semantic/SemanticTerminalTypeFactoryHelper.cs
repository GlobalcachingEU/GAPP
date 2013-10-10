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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;

namespace bsn.GoldParser.Semantic {
	internal static class SemanticTerminalTypeFactoryHelper<TBase> where TBase: SemanticToken {
		public delegate T Activator<T>(string text) where T: TBase;

		private static readonly Dictionary<ConstructorInfo, DynamicMethod> dynamicMethods = new Dictionary<ConstructorInfo, DynamicMethod>();

		private static DynamicMethod GetDynamicMethod(ConstructorInfo constructor) {
			Debug.Assert(constructor != null);
			lock (dynamicMethods) {
				DynamicMethod result;
				if (!dynamicMethods.TryGetValue(constructor, out result)) {
					Type ownerType = typeof(SemanticTerminalFactory<,>).MakeGenericType(typeof(TBase), constructor.DeclaringType);
					result = new DynamicMethod(string.Format("SemanticTerminalTypeFactory<{0}>.Activator", constructor.DeclaringType.FullName), constructor.DeclaringType, new[] {ownerType, typeof(string)}, ownerType, true);
					ILGenerator il = result.GetILGenerator();
					foreach (ParameterInfo parameterInfo in constructor.GetParameters()) {
						if ((parameterInfo.Position > 0) || (parameterInfo.ParameterType != typeof(string))) {
							throw new ArgumentException("The constructor may have at most exactly one string parameter");
						}
						il.Emit(OpCodes.Ldarg_1);
					}
					il.Emit(OpCodes.Newobj, constructor);
					il.Emit(OpCodes.Ret);
					dynamicMethods.Add(constructor, result);
				}
				return result;
			}
		}

		public static Activator<T> CreateActivator<T>(SemanticTerminalTypeFactory<TBase, T> target, ConstructorInfo constructor) where T: TBase {
			if (target == null) {
				throw new ArgumentNullException("target");
			}
			if (constructor == null) {
				throw new ArgumentNullException("constructor");
			}
			return (Activator<T>)GetDynamicMethod(constructor).CreateDelegate(typeof(Activator<T>), target);
		}
	}
}