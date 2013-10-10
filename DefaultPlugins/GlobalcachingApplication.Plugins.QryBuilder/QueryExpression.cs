using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace GlobalcachingApplication.Plugins.QryBuilder
{
    public partial class QueryExpression : UserControl
    {
        public enum Operator : int
        {
            Equal = 0,
            NotEqual = 1,
            LessOrEqual = 2,
            Less = 3,
            LargerOrEqual = 4,
            Larger = 5,
        }

        private Operator[] _availableOps = (QueryExpression.Operator[])Enum.GetValues(typeof(QueryExpression.Operator));

        public QueryExpression()
        {
            InitializeComponent();

            comboBox3.Items.Add("=");
            comboBox3.Items.Add("<>");
            comboBox3.Items.Add("<=");
            comboBox3.Items.Add("<");
            comboBox3.Items.Add(">=");
            comboBox3.Items.Add(">");
            comboBox3.SelectedIndex = 0;

            comboBox1.Items.AddRange(QueryExpressionImplementation.QueryExpressionImplementations.ToArray());
            comboBox1.SelectedIndex = 0;
        }

        public QueryExpressionImplementation SelectedQueryExpression
        {
            get { return (comboBox1.SelectedItem as QueryExpressionImplementation); }
        }

        public void LanguageChanged()
        {
            typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, comboBox1, new object[] { });
        }

        private void fillOpsCombo()
        {
            comboBox3.Items.Clear();
            if (_availableOps != null)
            {
                foreach (Operator op in _availableOps)
                {
                    switch (op)
                    {
                        case Operator.Equal:
                            comboBox3.Items.Add("=");
                            break;
                        case Operator.NotEqual:
                            comboBox3.Items.Add("<>");
                            break;
                        case Operator.LessOrEqual:
                            comboBox3.Items.Add("<=");
                            break;
                        case Operator.Less:
                            comboBox3.Items.Add("<");
                            break;
                        case Operator.LargerOrEqual:
                            comboBox3.Items.Add(">=");
                            break;
                        case Operator.Larger:
                            comboBox3.Items.Add(">");
                            break;
                        default:
                            comboBox3.Items.Add("?");
                            break;
                    }
                }
                comboBox3.SelectedIndex = 0;
            }
        }

        public void SetExpressionField(string name)
        {
            int i = comboBox1.SelectedIndex;
            QueryExpressionImplementation qei = (from c in QueryExpressionImplementation.QueryExpressionImplementations where c.Name == name select c).FirstOrDefault();
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(qei);
            if (i == comboBox1.SelectedIndex)
            {
                comboBox1_SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        public string ExpressionField
        {
            get { return comboBox1.Text; }
        }

        public string ExpressionValue
        {
            get { return comboBox2.Text; }
            set { comboBox2.Text = value; }
        }

        public Operator ExpressionOperator
        {
            get 
            {
                if (comboBox3.SelectedIndex == -1) return Operator.Equal;
                else if ((string)comboBox3.SelectedItem == "=") return Operator.Equal;
                else if ((string)comboBox3.SelectedItem == "<>") return Operator.NotEqual;
                else if ((string)comboBox3.SelectedItem == "<=") return Operator.LessOrEqual;
                else if ((string)comboBox3.SelectedItem == "<") return Operator.Less;
                else if ((string)comboBox3.SelectedItem == ">=") return Operator.LargerOrEqual;
                else if ((string)comboBox3.SelectedItem == ">") return Operator.Larger;
                else return Operator.Equal;
            }
        }
        public void SetExpressionOperator(string op)
        {
            if (op == "Equal")
            {
                comboBox3.SelectedItem = "=";
            }
            else if (op == "NotEqual")
            {
                comboBox3.SelectedItem = "<>";
            }
            else if (op == "LessOrEqual")
            {
                comboBox3.SelectedItem = "<=";
            }
            else if (op == "Less")
            {
                comboBox3.SelectedItem = "<";
            }
            else if (op == "LargerOrEqual")
            {
                comboBox3.SelectedItem = ">=";
            }
            else if (op == "Larger")
            {
                comboBox3.SelectedItem = ">";
            }
            else
            {
                comboBox3.SelectedItem = op;
            }
        }

        public bool GetExpressionResult(Framework.Data.Geocache gc)
        {
            QueryExpressionImplementation qei = comboBox1.SelectedItem as QueryExpressionImplementation;
            if (qei != null)
            {
                return qei.ExpressionResult(gc, ExpressionOperator, comboBox2.Text);
            }
            else
            {
                return false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            QueryExpressionImplementation qei = comboBox1.SelectedItem as QueryExpressionImplementation;
            if (qei != null)
            {
                comboBox3.Items.Clear();
                _availableOps = qei.InitCombo(comboBox2);
                fillOpsCombo();
            }
            else
            {
                comboBox2.Items.Clear();
            }
        }
    }
}
