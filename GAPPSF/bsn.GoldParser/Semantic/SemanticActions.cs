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
using System.IO;
using System.Text;
using System.Threading;

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	public abstract class SemanticActions<T> where T: SemanticToken {
		private class SemanticTokenizer: Tokenizer<T> {
			private readonly SemanticActions<T> actions;

			public SemanticTokenizer(TextReader textReader, SemanticActions<T> actions): base(textReader, actions.Grammar) {
				this.actions = actions;
			}

			protected override T CreateToken(Symbol tokenSymbol, LineInfo tokenPosition, string text) {
				return actions.CreateTerminalToken(tokenSymbol, tokenPosition, text);
			}
		}

		internal static CompiledGrammar GetGrammar(SemanticActions<T> actions) {
			if (actions == null) {
				throw new ArgumentNullException("actions");
			}
			return actions.Grammar;
		}

		private readonly CompiledGrammar grammar;
		private readonly Dictionary<Rule, SemanticNonterminalFactory<T>> nonterminalFactories = new Dictionary<Rule, SemanticNonterminalFactory<T>>();
		private readonly Dictionary<Symbol, SemanticTerminalFactory<T>> terminalFactories = new Dictionary<Symbol, SemanticTerminalFactory<T>>();
		private readonly Dictionary<Rule, SemanticTrimFactory<T>> trimFactories = new Dictionary<Rule, SemanticTrimFactory<T>>();
		private int initialized; // 0 = not initialized, 1 = initializing, 2 = initialized
		private int initializingThread = -1;

		protected SemanticActions(CompiledGrammar grammar) {
			if (grammar == null) {
				throw new ArgumentNullException("grammar");
			}
			this.grammar = grammar;
			for (int i = 0; i < grammar.RuleCount; i++) {
				Rule rule = grammar.GetRule(i);
				if (rule.SymbolCount == 1) {
					trimFactories.Add(rule, new SemanticTrimFactory<T>(this, rule, 0));
				}
			}
		}

		public IEnumerable<SemanticTokenFactory<T>> AllTokenFactories {
			get {
				foreach (KeyValuePair<Symbol, SemanticTerminalFactory<T>> terminalFactory in terminalFactories) {
					yield return terminalFactory.Value;
				}
				foreach (KeyValuePair<Rule, SemanticNonterminalFactory<T>> nonterminalFactory in nonterminalFactories) {
					yield return nonterminalFactory.Value;
				}
				foreach (KeyValuePair<Rule, SemanticTrimFactory<T>> trimFactory in trimFactories) {
					if (!nonterminalFactories.ContainsKey(trimFactory.Key)) {
						yield return trimFactory.Value;
					}
				}
			}
		}

		public CompiledGrammar Grammar {
			get {
				return grammar;
			}
		}

		protected internal bool Initialized {
			get {
				return Interlocked.CompareExchange(ref initialized, 2, 2) == 2;
			}
		}

		public abstract Type GetSymbolOutputType(Symbol symbol);

		public void Initialize() {
			Initialize(false);
		}

		/// <summary>
		/// Initializes these actions.
		/// </summary>
		/// <param name="trace"></param>
		public void Initialize(bool trace) {
			Initialize(trace, false);
		}

		/// <summary>
		/// Initializes these actions.
		/// </summary>
		/// <param name="trace"></param>
		/// <param name="strongParameterMatching">Defines the default strong parameter matching which is assigned to a rule if it doesn't have an explicit one. <c>true</c> requires all the parameters to have a symbol assigned.</param>
		public void Initialize(bool trace, bool strongParameterMatching) {
			int initializationState = Interlocked.CompareExchange(ref initialized, 1, 0);
			if (initializationState < 2) {
				PerformInitialization(initializationState, trace, strongParameterMatching);
			}
		}

		public bool TryGetNonterminalFactory(Rule rule, out SemanticNonterminalFactory<T> factory) {
			Initialize();
			if (nonterminalFactories.TryGetValue(rule, out factory)) {
				return true;
			}
			SemanticTrimFactory<T> trimFactory;
			if (trimFactories.TryGetValue(rule, out trimFactory)) {
				factory = trimFactory;
				return true;
			}
			return false;
		}

		public bool TryGetTerminalFactory(Symbol symbol, out SemanticTerminalFactory<T> factory) {
			Initialize();
			return terminalFactories.TryGetValue(symbol, out factory);
		}

		protected internal virtual ITokenizer<T> CreateTokenizer(TextReader reader) {
			Initialize();
			return new SemanticTokenizer(reader, this);
		}

		protected void AssertNotInitialized() {
			if (Initialized) {
				throw new InvalidOperationException("The object is already initialized");
			}
		}

		protected virtual void CheckConsistency(ICollection<string> errors, bool trace) {
			SymbolTypeMap<T> symbolTypes = new SymbolTypeMap<T>();
			// step 1: check that all terminals have a factory and register their output type
			for (int i = 0; i < grammar.SymbolCount; i++) {
				Symbol symbol = grammar.GetSymbol(i);
				if (symbol.Kind != SymbolKind.Nonterminal) {
					SemanticTerminalFactory<T> factory;
					if (TryGetTerminalFactory(symbol, out factory)) {
						//						Debug.WriteLine(factory.OutputType.FullName, symbol.ToString());
						if (symbolTypes.SetTypeForSymbol(symbol, factory.OutputType) && trace) {
							Trace.WriteLine(string.Format("Terminal {0} yields type {1}", symbol, symbolTypes.GetSymbolType(symbol)));
						}
					} else {
						errors.Add(String.Format("Semantic token is missing for terminal {0}", symbol));
					}
				}
			}
			// step 2: check that all rules have a factory and register their output type
			for (int i = 0; i < grammar.RuleCount; i++) {
				Rule rule = grammar.GetRule(i);
				SemanticNonterminalFactory<T> factory;
				if (TryGetNonterminalFactory(rule, out factory)) {
					//					Debug.WriteLine(factory.OutputType.FullName, rule.RuleSymbol.ToString());
					if (symbolTypes.SetTypeForSymbol(rule.RuleSymbol, factory.OutputType) && trace) {
						Trace.WriteLine(string.Format("Rule {0} yields type {1} (static: {2})", rule, symbolTypes.GetSymbolType(rule.RuleSymbol), factory.IsStaticOutputType));
					}
				} else {
					errors.Add(String.Format("Semantic token is missing for rule {0}", rule));
				}
			}
			// step 3: check the input types of all rules
			foreach (KeyValuePair<Rule, SemanticNonterminalFactory<T>> pair in nonterminalFactories) {
				ReadOnlyCollection<Type> inputTypes = pair.Value.InputTypes;
				int index = 0;
				foreach (Symbol inputSymbol in pair.Value.GetInputSymbols(pair.Key)) {
					if (index < inputTypes.Count) {
						Type handleType = symbolTypes.GetSymbolType(inputSymbol);
						if (!inputTypes[index].IsAssignableFrom(handleType)) {
							errors.Add(string.Format("The factory for the type {0} used by rule {1} expects a {2} on index {3}, but receives a {4}", pair.Value.OutputType.FullName, pair.Key.Definition, inputTypes[index].FullName, index, handleType.FullName));
						}
					}
					index++;
				}
				if (index != inputTypes.Count) {
					errors.Add(string.Format("The factory for the type {0} used by rule {1} has a mismatch of input symbol count ({2}) and type count ({3})", pair.Value.OutputType.FullName, pair.Key.Definition, index, inputTypes.Count));
				}
			}
		}

		protected IEnumerable<SemanticTokenFactory<T>> GetTokenFactoriesForSymbol(Symbol symbol) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			if (symbol.Kind == SymbolKind.Nonterminal) {
				foreach (Rule rule in grammar.GetRulesForSymbol(symbol)) {
					SemanticNonterminalFactory<T> nonterminalFactory;
					if (TryGetNonterminalFactory(rule, out nonterminalFactory)) {
						yield return nonterminalFactory;
					}
				}
			} else {
				SemanticTerminalFactory<T> terminalFactory;
				if (TryGetTerminalFactory(symbol, out terminalFactory)) {
					yield return terminalFactory;
				}
			}
		}

		protected abstract void InitializeInternal(ICollection<string> errors, bool trace, bool strongParameterCheck);

		protected virtual void RegisterNonterminalFactory(Rule rule, SemanticNonterminalFactory<T> factory) {
			if (rule == null) {
				throw new ArgumentNullException("rule");
			}
			if (rule.Owner != grammar) {
				throw new ArgumentException("The rule was defined on another grammar", "rule");
			}
			if (factory == null) {
				throw new ArgumentNullException("factory");
			}
			AssertNotInitialized();
			nonterminalFactories.Add(rule, factory);
		}

		protected virtual void RegisterTerminalFactory(Symbol symbol, SemanticTerminalFactory<T> factory) {
			if (symbol == null) {
				throw new ArgumentNullException("symbol");
			}
			if (symbol.Owner != grammar) {
				throw new ArgumentException("The symbol was defined on another grammar", "symbol");
			}
			if (symbol.Kind == SymbolKind.Nonterminal) {
				throw new ArgumentException("Terminal symbol factories can only build terminals and special symbols", "symbol");
			}
			if (factory == null) {
				throw new ArgumentNullException("factory");
			}
			AssertNotInitialized();
			terminalFactories.Add(symbol, factory);
		}

		private T CreateTerminalToken(Symbol tokenSymbol, LineInfo tokenPosition, string text) {
			return terminalFactories[tokenSymbol].CreateAndInitialize(tokenSymbol, tokenPosition, text);
		}

		private void PerformInitialization(int initializationState, bool trace, bool strongParameterCheck) {
			switch (initializationState) {
			case 0:
				initializingThread = Thread.CurrentThread.ManagedThreadId;
				List<string> errors = new List<string>();
				try {
					InitializeInternal(errors, trace, strongParameterCheck);
				} finally {
					Interlocked.Increment(ref initialized);
					initializingThread = -1;
				}
				if (errors.Count == 0) {
					CheckConsistency(errors, trace);
				}
				// throw if errors were found
				if (errors.Count > 0) {
					StringBuilder result = new StringBuilder();
					result.AppendLine("The semantic engine found errors:");
					foreach (string error in errors) {
						result.AppendLine(error);
					}
					throw new InvalidOperationException(result.ToString());
				}
				break;
			case 1:
				// Initialization is already ongoing, therefore block the tread until we're done initializing
				if (initializingThread == Thread.CurrentThread.ManagedThreadId) {
					Debug.Fail("Recursive semantic actions initialization call");
					return;
				}
				while (!Initialized) {
					Thread.Sleep(0);
				}
				break;
			}
		}
	}
}
