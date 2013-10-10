using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.QryBuilder
{
    public partial class QueryOperator : ComboBox
    {
        public const string STR_AND = "And";
        public const string STR_OR = "Or";
        public const string STR_END = "End";

        private Operator _previousSelection = Operator.End; 

        public enum Operator: int
        {
            And = 0,
            Or = 1,
            End = 2
        }
        public QueryOperator()
        {
            InitializeComponent();

            DropDownStyle = ComboBoxStyle.DropDownList;

            Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_AND));
            Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_OR));
            Items.Add(Utils.LanguageSupport.Instance.GetTranslation(STR_END));

            SelectedIndex = (int)Operator.End;
        }
        public void LanguageChanged()
        {
            Items[0] = Utils.LanguageSupport.Instance.GetTranslation(STR_AND);
            Items[1] = Utils.LanguageSupport.Instance.GetTranslation(STR_OR);
            Items[2] = Utils.LanguageSupport.Instance.GetTranslation(STR_END);
        }
        public Operator SelectedOperator
        {
            get { return (Operator)SelectedIndex; }
            set { SelectedIndex = (int)value; }
        }
        public Operator PreviousSelectedOperator
        {
            get { return _previousSelection; }
            set { _previousSelection = value; }
        }
    }
}
