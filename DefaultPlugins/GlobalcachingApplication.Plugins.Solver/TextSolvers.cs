using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Solver
{
    public class TextSolver
    {
        public virtual string Name { get { return ""; } }
        public virtual string Process(string input)
        {
            return "";
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class TextSolverRot13 : TextSolver
    {
        public const string STR_NAME = "ROT13";
        private const string ROTATIONTABLE = "abcdefghijklmnopqrstuvwxyz";
        private string _lowerCase;
        private string _upperCase;
        public TextSolverRot13()
        {
            _lowerCase = ROTATIONTABLE;
            _upperCase = ROTATIONTABLE.ToUpper();
        }
        public override string Name { get { return Utils.LanguageSupport.Instance.GetTranslation(STR_NAME); } }
        public override string Process(string input)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (_lowerCase.IndexOf(c) >= 0)
                {
                    sb.Append(_lowerCase[(_lowerCase.IndexOf(c) + 13) % 26]);
                }
                else if (_upperCase.IndexOf(c) >= 0)
                {
                    sb.Append(_upperCase[(_upperCase.IndexOf(c) + 13) % 26]);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class TextSolverWordValue : TextSolver
    {
        public const string STR_NAME = "Word value";
        public const string STR_WORDLENGTHSPACE = "Word length with space";
        public const string STR_WORDLENGTHNOSPACE = "Word length without space";
        private const string ROTATIONTABLE = "abcdefghijklmnopqrstuvwxyz";
        public override string Name { get { return Utils.LanguageSupport.Instance.GetTranslation(STR_NAME); } }
        public override string Process(string input)
        {
            int totala1 = 0;
            int totala0 = 0;
            int totala26 = 0;
            int totala25 = 0;
            input = input.ToLower();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                int pos = ROTATIONTABLE.IndexOf(input[i]);
                if (pos >= 0)
                {
                    totala1 += (pos + 1);
                    totala0 += pos;
                    totala26 += 26 - pos;
                    totala25 += 26 - pos - 1;
                }
            }
            sb.AppendLine(string.Format("a=0...z=25: {0}", totala0));
            sb.AppendLine(string.Format("a=1...z=26: {0}", totala1));
            sb.AppendLine(string.Format("a=25...z=0: {0}", totala25));
            sb.AppendLine(string.Format("a=26...z=1: {0}", totala26));
            sb.AppendLine(string.Format("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_WORDLENGTHSPACE), input.Replace("r", "").Replace("\n", "").Length));
            sb.AppendLine(string.Format("{0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_WORDLENGTHNOSPACE), input.Replace(" ", "").Replace("\t", "").Replace("r", "").Replace("\n", "").Length));
            return sb.ToString();
        }
    }

    public class TextSolverASCII : TextSolver
    {
        public const string STR_NAME = "ASCII";
        public const string STR_ASCIIDEC = "ASCII (DEC)";
        public const string STR_ASCIIHEX = "ASCII (HEX)";
        public override string Name { get { return Utils.LanguageSupport.Instance.GetTranslation(STR_NAME); } }
        public override string Process(string input)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("{0}: ",Utils.LanguageSupport.Instance.GetTranslation(STR_ASCIIDEC)));
            for (int i = 0; i < input.Length; i++)
            {
                sb.AppendFormat("{0} ", (int)input[i]);
            }
            sb.AppendLine();
            sb.Append(string.Format("{0}: ", Utils.LanguageSupport.Instance.GetTranslation(STR_ASCIIHEX)));
            for (int i = 0; i < input.Length; i++)
            {
                sb.AppendFormat("{0} ", ((int)input[i]).ToString("X2"));
            }
            return sb.ToString();
        }
    }

    public class TextSolverFrequency : TextSolver
    {
        public const string STR_NAME = "Frequency";
        public override string Name { get { return Utils.LanguageSupport.Instance.GetTranslation(STR_NAME); } }
        public override string Process(string input)
        {
            StringBuilder sb = new StringBuilder();
            var freq = from c in input
                       group c by c into g
                       select new { Character = g.Key, CharCount = g };
            foreach (var g in freq)
            {
                sb.AppendLine(string.Format("{0}: {1}", g.Character, g.CharCount.Count()));
            }
            return sb.ToString();
        }
    }

    public class TextSolverCipher : TextSolver
    {
        public const string STR_NAME = "Cipher";
        public const string STR_SHIFTCOUNT = "shift letters count";
        private const string ROTATIONTABLE = "abcdefghijklmnopqrstuvwxyz";
        public override string Name { get { return Utils.LanguageSupport.Instance.GetTranslation(STR_NAME); } }
        public override string Process(string input)
        {
            input = input.ToLower();
            StringBuilder sb = new StringBuilder();
            for (int shift = 1; shift < 26; shift++)
            {
                sb.AppendLine(string.Format("=== {0}: {1}", Utils.LanguageSupport.Instance.GetTranslation(STR_SHIFTCOUNT), shift));
                for (int i = 0; i < input.Length; i++)
                {
                    char c = input[i];
                    if (ROTATIONTABLE.IndexOf(c) >= 0)
                    {
                        sb.Append(ROTATIONTABLE[(ROTATIONTABLE.IndexOf(c) + shift) % 26]);
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

}
