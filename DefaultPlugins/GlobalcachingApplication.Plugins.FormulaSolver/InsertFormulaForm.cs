using System.Windows.Forms;
using GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.FormulaSolver
{
    #region GroupItemData
    struct GroupItemData
    {
        public string GroupName;
        public FunctionDescriptor.FunctionGroup GroupDescriptor;

        public GroupItemData(FunctionDescriptor.FunctionGroup grp, string name)
        {
            GroupDescriptor = grp;
            GroupName = name;
        }

        public override string ToString()
        {
            return GroupName;
        }
    }
    #endregion

    #region FunctionItemData
    struct FunctionItemData
    {
        public string FunctionName;
        public string[] AlternateNames;
        public Functor Funct;
        public string Description;

        public FunctionItemData(string name, string[] alternates, Functor fkt, string descr)
        {
            FunctionName = name;
            AlternateNames = alternates;
            Funct = fkt;
            Description = descr;
        }

        public override string ToString()
        {
            return FunctionName;
        }
    }
    #endregion

    public partial class InsertFormulaForm : Form
    {
        FunctionList _fList = new FunctionList();
        public string SelectedFunction
        {
            get
            {
                if (lbAltNames.SelectedIndex != -1)
                {
                    return lbAltNames.SelectedItem.ToString() + "()";
                }
                if (lbFunction.SelectedIndex != -1)
                {
                    return ((FunctionItemData)lbFunction.SelectedItem).FunctionName + "()";
                }
                return "";
            }
            private set
            {

            }
        }

        public InsertFormulaForm()
        {
            InitializeComponent();

            SetLanguageSpecificControlText();

            lbGroup.Items.Clear();
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.TextGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.NumberGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.TrigonometricGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.CoordinateGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.ContextGroup);

            lbGroup.SelectedIndexChanged += new System.EventHandler(lbGroup_SelectedIndexChanged);
            lbFunction.SelectedIndexChanged += new System.EventHandler(lbFunction_SelectedIndexChanged);

            lbGroup.SelectedIndex = 0;
        }

        private void AddFunctionGroup(ListBox lbGroup, FunctionDescriptor.FunctionGroup functionGroup)
        {
            lbGroup.Items.Add(
                new GroupItemData(
                    functionGroup, _fList.GetFunctionGroupString(functionGroup)
                )
            );
        }

        private void SetLanguageSpecificControlText()
        {
            this.Text = StrRes.GetString(StrRes.STR_INSFORM_TITLE);
            lblGroup.Text = StrRes.GetString(StrRes.STR_INSFORM_GROUP);
            lblFunctions.Text = StrRes.GetString(StrRes.STR_INSFORM_FUNCTIONS);
            lblOtherNames.Text = StrRes.GetString(StrRes.STR_INSFORM_OTHER);
            bnInsert.Text = StrRes.GetString(StrRes.STR_INSFORM_INSERT);
            bnCancel.Text = StrRes.GetString(StrRes.STR_INSFORM_CANCEL);
        }

        void lbFunction_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            lbAltNames.Items.Clear();
            if (((FunctionItemData)lbFunction.SelectedItem).AlternateNames != null)
            {
                foreach (string s in ((FunctionItemData)lbFunction.SelectedItem).AlternateNames)
                {
                    lbAltNames.Items.Add(s);
                }
            }
            tbDescription.Text = ((FunctionItemData)lbFunction.SelectedItem).Description;
        }

        void lbGroup_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var fkt = from f in _fList.GetList()
                    where f.Group == ((GroupItemData)lbGroup.SelectedItem).GroupDescriptor
                    select f;

            lbFunction.Items.Clear();
            foreach (var f in fkt)
            {
                lbFunction.Items.Add(new FunctionItemData(f.Name, f.Alternates, f.Functor, f.Description));
            }

            lbFunction.SelectedIndex = 0;
        }

        private void bnInsert_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void lbFunction_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbFunction.SelectedItem != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
