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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using bsn.GoldParser.Grammar;

namespace bsn.GoldParser.Parser {
	internal class LalrStack<T> where T: class, IToken {
		private class RangePop: IList<T> {
			private readonly int bottomIndex;
			private readonly KeyValuePair<T, LalrState>[] items;
			private readonly int topIndex;

			public RangePop(KeyValuePair<T, LalrState>[] items, int topIndex, int bottomIndex) {
				Debug.Assert(items != null);
				Debug.Assert((topIndex < items.Length) && (topIndex >= bottomIndex) && (bottomIndex >= 0));
				this.items = items;
				this.topIndex = topIndex;
				this.bottomIndex = bottomIndex;
			}

			public IEnumerator<T> GetEnumerator() {
				for (int i = bottomIndex+1; i <= topIndex; i++) {
					yield return items[i].Key;
				}
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return GetEnumerator();
			}

			public void Add(T item) {
				throw new NotSupportedException();
			}

			public void Clear() {
				throw new NotSupportedException();
			}

			public bool Contains(T item) {
				return IndexOf(item) >= 0;
			}

			public void CopyTo(T[] array, int arrayIndex) {
				for (int i = bottomIndex+1; i <= topIndex; i++) {
					array[arrayIndex++] = items[i].Key;
				}
			}

			public bool Remove(T item) {
				throw new NotSupportedException();
			}

			public int Count {
				get {
					return topIndex-bottomIndex;
				}
			}

			public bool IsReadOnly {
				get {
					return true;
				}
			}

			public int IndexOf(T item) {
				if (item != null) {
					for (int i = bottomIndex+1; i <= topIndex; i++) {
						if (item == items[i].Key) {
							return i-(bottomIndex+1);
						}
					}
				}
				return -1;
			}

			public void Insert(int index, T item) {
				throw new NotSupportedException();
			}

			public void RemoveAt(int index) {
				throw new NotSupportedException();
			}

			public T this[int index] {
				get {
					if ((index < 0) || (index >= Count)) {
						throw new ArgumentOutOfRangeException("index");
					}
					return items[index+bottomIndex+1].Key;
				}
				set {
					throw new NotSupportedException();
				}
			}
		}

		private KeyValuePair<T, LalrState>[] items = new KeyValuePair<T, LalrState>[128];
		private int topIndex;

		public LalrStack(LalrState initialState) {
			if (initialState == null) {
				throw new ArgumentNullException("initialState");
			}
			items[0] = new KeyValuePair<T, LalrState>(default(T), initialState);
		}

		public LalrState GetTopState() {
			return items[topIndex].Value;
		}

		public T Peek() {
			return items[topIndex].Key;
		}

		public T Pop() {
			Debug.Assert(topIndex >= 0);
			return items[topIndex--].Key;
		}

		public IList<T> PopRange(int count) {
			Debug.Assert(count >= 0);
			int oldTopIndex = topIndex;
			topIndex -= count;
			return new RangePop(items, oldTopIndex, topIndex);
		}

		public void Push(T token, LalrState state) {
			topIndex++;
			if ((topIndex+1) == items.Length) {
				Array.Resize(ref items, items.Length*2);
			}
			items[topIndex] = new KeyValuePair<T, LalrState>(token, state);
		}
	}
}