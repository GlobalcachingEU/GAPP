using System;
using System.IO;
using System.Text;
using bsn.GoldParser.Grammar;
using bsn.GoldParser.Parser;
using bsn.GoldParser.Semantic;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter;
using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Exception;

[assembly: RuleTrim("<Value> ::= '(' <Expression> ')'", "<Expression>", SemanticTokenType = typeof(GCToken))]

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
{
    class FormulaInterpreter
    {
        private CompiledGrammar grammar;
        private SemanticTypeActions<GCToken> actions;

        public FormulaInterpreter(System.IO.BinaryReader reader)
        {
            grammar = CompiledGrammar.Load(reader);
            actions = new SemanticTypeActions<GCToken>(grammar);
            try
            {
                actions.Initialize(true);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.Write(ex.Message);
            }
        }

        public object Exec(string input, ExecutionContext ctx)
        {
            var processor = new SemanticProcessor<GCToken>(new StringReader(input), actions);
            ParseMessage parseMessage = processor.ParseAll();

            if (parseMessage == ParseMessage.Accept)
            {
                var statement = processor.CurrentToken as Expression;
                if (statement != null)
                {
                    try
                    {
                        return statement.GetValue(ctx);
                    }
                    catch (FormulaSolverException ex)
                    {
                        return "??" + ex.Message;
                    }
                }
            }
            else
            {
                IToken token = processor.CurrentToken;
                return String.Format("??Syntax error at: {0} [{1}]", token.Position.Index, parseMessage);
            }

            return null;
        }

        private void InsertMissingVariableLines(string input, ExecutionContext ctx)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string name in ctx.GetMissingVariableNames())
            {
                sb.AppendLine(name + "=");
            }
            sb.Append(input);
            input = sb.ToString();
        }
    }
}
