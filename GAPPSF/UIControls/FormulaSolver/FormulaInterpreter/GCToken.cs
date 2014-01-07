using bsn.GoldParser.Semantic;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter
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
