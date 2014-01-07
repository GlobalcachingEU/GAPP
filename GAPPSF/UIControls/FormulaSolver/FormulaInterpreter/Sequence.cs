using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    public class Sequence<T> : GCToken, IEnumerable<T> where T : GCToken
    {
        private readonly T item;
        private Sequence<T> next;

        public Sequence()
            : this(null, null) { }

        [Rule("<ArgumentList> ::= <Expression>", typeof(Expression))]
        [Rule("<ExpressionList> ::= ~'(' <Expression> ~')'", typeof(Expression))]
        public Sequence(T item) 
            : this(item, null) { }

        [Rule("<ArgumentList> ::= <Expression> ~';' <ArgumentList>", typeof(Expression))]
        [Rule("<ExpressionList> ::= ~'(' <Expression> ~')' <ExpressionList>", typeof(Expression))]
        public Sequence(T item, Sequence<T> next) 
        {
            this.item = item;
            this.next = next;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (Sequence<T> sequence = this; sequence != null; sequence = sequence.next)
            {
                if (sequence.item != null)
                {
                    yield return sequence.item;
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
