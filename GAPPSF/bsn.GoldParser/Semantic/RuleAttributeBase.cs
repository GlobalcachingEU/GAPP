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

using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;

namespace bsn.GoldParser.Semantic {
	public abstract class RuleAttributeBase: Attribute {
		private readonly Reduction parsedRule;

		// This gets rid of the CLS compliance warning without introducing a security problem by inherited attributes
		protected RuleAttributeBase() {
			throw new NotSupportedException("This class is not intended to be inherited");
		}

		internal RuleAttributeBase(string rule) {
			if (string.IsNullOrEmpty(rule)) {
				throw new ArgumentNullException("rule");
			}
			if (!RuleDeclarationParser.TryParse(rule, out parsedRule)) {
				throw new ArgumentException(string.Format("The rule {0} contains a syntax error", rule), "rule");
			}
		}

		public string Rule {
			get {
				return parsedRule.ToString();
			}
		}

		internal Reduction ParsedRule {
			get {
				return parsedRule;
			}
		}

		public Rule Bind(CompiledGrammar grammar) {
			if (grammar == null) {
				throw new ArgumentNullException("grammar");
			}
			Rule rule;
			RuleDeclarationParser.TryBindGrammar(parsedRule, grammar, out rule);
			return rule;
		}
	}
}