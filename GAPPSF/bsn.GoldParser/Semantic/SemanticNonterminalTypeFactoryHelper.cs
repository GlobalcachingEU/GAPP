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
	internal static class SemanticNonterminalTypeFactoryHelper<TBase> where TBase: SemanticToken {
		public delegate T Activator<T>(IList<TBase> tokens);

		private static readonly Dictionary<MethodBase, DynamicMethod> dynamicMethods = new Dictionary<MethodBase, DynamicMethod>();
		private static readonly MethodInfo iListGetItem = GetIListGetItemMethod();

		public static Activator<T> CreateActivator<T>(SemanticNonterminalTypeFactory<TBase, T> target, MethodBase methodBase, int[] parameterMapping) where T: TBase {
			if (target == null) {
				throw new ArgumentNullException("target");
			}
			if (methodBase == null) {
				throw new ArgumentNullException("methodBase");
			}
			return (Activator<T>)GetDynamicMethod(methodBase).CreateDelegate(typeof(Activator<T>), parameterMapping);
		}

		private static DynamicMethod GetDynamicMethod(MethodBase methodBase) {
			Debug.Assert(methodBase != null);
			lock (dynamicMethods) {
				DynamicMethod result;
				if (!dynamicMethods.TryGetValue(methodBase, out result)) {
					ConstructorInfo constructor = methodBase as ConstructorInfo;
					MethodInfo method = methodBase as MethodInfo;
					Type returnType;
					if (constructor != null) {
						returnType = constructor.DeclaringType;
						Debug.Assert(returnType != null);
					} else if (method != null) {
						if (!method.IsStatic) {
							throw new InvalidOperationException("Factories can only be created for static methods");
						}
						returnType = method.ReturnType;
						if (!typeof(TBase).IsAssignableFrom(returnType)) {
							throw new InvalidOperationException("The static method doesn't return the required type");
						}
					} else {
						throw new ArgumentException("Expected methodBase to be one of: ConstructorInfo, MethodInfo, instead is: "+methodBase.GetType());
					}
					result = new DynamicMethod(string.Format("SemanticNonterminalTypeFactory<{0}>.Activator", returnType.FullName), returnType, new[] {typeof(int[]), typeof(IList<TBase>)}, true);
					ILGenerator il = result.GetILGenerator();
					Dictionary<int, ParameterInfo> parameters = new Dictionary<int, ParameterInfo>();
					foreach (ParameterInfo parameter in methodBase.GetParameters()) {
						parameters.Add(parameter.Position, parameter);
					}
					for (int i = 0; i < parameters.Count; i++) {
						if (parameters[i].ParameterType.IsValueType) {
							throw new InvalidOperationException(methodBase.MemberType+" arguments cannot be value types");
						}
						Label loadNull = il.DefineLabel();
						Label end = il.DefineLabel();
						il.Emit(OpCodes.Ldarg_1); // load the IList<TBase>
						il.Emit(OpCodes.Ldarg_0); // load the int[]
						il.Emit(OpCodes.Ldc_I4, i); // load the parameter index
						il.Emit(OpCodes.Ldelem_I4); // get the indirection index
						il.Emit(OpCodes.Dup); // copy the indicrection index
						il.Emit(OpCodes.Ldc_I4_0); // and load a 0
						il.Emit(OpCodes.Blt_S, loadNull); // compare the stored indicrection index and the stored 0, if less (e.g. -1) we need to load a null
						il.Emit(OpCodes.Callvirt, iListGetItem); // otherwise get the item
						il.Emit(OpCodes.Castclass, parameters[i].ParameterType); // make the verifier happy by casting the reference
						il.Emit(OpCodes.Br_S, end); // jump to end
						il.MarkLabel(loadNull);
						il.Emit(OpCodes.Pop); // pop the unused indirection index
						il.Emit(OpCodes.Pop); // pop the unused reference to the IList<TBase>
						il.Emit(OpCodes.Ldnull); // load a null reference instead
						il.MarkLabel(end);
					}
					if (constructor != null) {
						il.Emit(OpCodes.Newobj, constructor); // invoke constructor
					} else {
						il.Emit(OpCodes.Call, method); // invoke static method
					}
					il.Emit(OpCodes.Ret);
					dynamicMethods.Add(methodBase, result);
				}
				return result;
			}
		}

		private static MethodInfo GetIListGetItemMethod() {
			MethodInfo result = typeof(IList<TBase>).GetProperty("Item").GetGetMethod();
			Debug.Assert(result != null);
			return result;
		}
	}
}
