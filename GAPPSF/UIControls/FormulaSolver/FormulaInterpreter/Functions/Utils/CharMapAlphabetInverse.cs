using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions
{
    public class CharMapAlphabetInverse: ICharacterMapper
    {
        public int GetCharacterCode(char character) 
        {
            char c = Char.ToUpper(character);
            if ((c >= 'A') && (c <= 'Z'))
            {
                return 'Z' - c + 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
