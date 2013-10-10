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
using System.Collections.ObjectModel;
using System.Diagnostics;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Semantic {
	internal class SymbolTypeMap<T> where T: SemanticToken {
		private readonly Dictionary<Type, ReadOnlyCollection<Type>> baseTypeCache;
		private readonly SymbolTypeMap<T> parent;
		private readonly Dictionary<Symbol, Type> symbolType = new Dictionary<Symbol, Type>();
		private int version;

		public SymbolTypeMap(SymbolTypeMap<T> parent) {
			this.parent = parent;
			if (parent == null) {
				baseTypeCache = new Dictionary<Type, ReadOnlyCollection<Type>>();
			}
		}

		public SymbolTypeMap(): this(null) {}

		public int Version {
			get {
				if (parent != null) {
					return parent.Version+version;
				}
				return version;
			}
		}

		public ReadOnlyCollection<Type> GetBaseTypes(Type type) {
			if (parent != null) {
				return parent.GetBaseTypes(type);
			}
			if (type == null) {
				throw new ArgumentNullException("type");
			}
			if (!typeof(T).IsAssignableFrom(type)) {
				throw new ArgumentException("Not an allowed base type", "type");
			}
			ReadOnlyCollection<Type> result;
			lock (baseTypeCache) {
				if (!baseTypeCache.TryGetValue(type, out result)) {
					List<Type> ancestorOrSelf = new List<Type>();
					ancestorOrSelf.Add(type);
					Type ancestor = type;
					while (ancestor != typeof(T)) {
						ancestor = ancestor.BaseType;
						ancestorOrSelf.Add(ancestor);
					}
					ancestorOrSelf.Reverse();
					result = ancestorOrSelf.AsReadOnly();
					baseTypeCache.Add(type, result);
				}
			}
			return result;
		}

		public Type GetCommonBaseType(Type x, Type y) {
			if (x == null) {
				throw new ArgumentNullException("x");
			}
			if (y == null) {
				throw new ArgumentNullException("y");
			}
			return GetCommonbaseTypeInternal(x, y);
		}

		public Type GetSymbolType(Symbol symbol) {
			return GetSymbolTypeInternal(symbol, typeof(T));
		}

		public bool SetTypeForSymbol(Symbol symbol, Type type) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			if (type == null) {
				throw new ArgumentNullException("type");
			}
			bool result;
			Type currentType;
			if (symbolType.TryGetValue(symbol, out currentType)) {
				Type commonBaseType = GetCommonBaseType(currentType, type);
				symbolType[symbol] = commonBaseType;
				result = commonBaseType != currentType;
			} else {
				symbolType.Add(symbol, type);
				result = true;
			}
			version++;
			return result;
		}

		protected Type GetSymbolTypeInternal(Symbol symbol, Type @default) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			Type result;
			symbolType.TryGetValue(symbol, out result);
			if (parent != null) {
				Type parentResult = parent.GetSymbolTypeInternal(symbol, null);
				if (parentResult != null) {
					if (result != null) {
						return GetCommonBaseType(result, parentResult);
					}
					return parentResult;
				}
			}
			return result ?? @default;
		}

		internal void ApplyCommonBaseType(ref Type x, Type y) {
			Debug.Assert(y != null);
			x = (x == null) ? y : GetCommonbaseTypeInternal(x, y);
		}

		private Type GetCommonbaseTypeInternal(Type x, Type y) {
			if (x == y) {
				return x;
			}
			ReadOnlyCollection<Type> xBase = GetBaseTypes(x);
			ReadOnlyCollection<Type> yBase = GetBaseTypes(y);
			Type result = typeof(T);
			for (int i = 0; (i < xBase.Count) && (i < yBase.Count); i++) {
				if (xBase[i] != yBase[i]) {
					break;
				}
				result = xBase[i];
			}
			return result;
		}
	}
}