using bsn.GoldParser.Semantic;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter
{
    [Terminal("(EOF)")]
    [Terminal("(Whitespace)")]
    [Terminal("(Error)")]
    [Terminal("(")]
    [Terminal(")")]
    [Terminal("=")]
    [Terminal(";")]
    public class GCToken : SemanticToken { }
}
