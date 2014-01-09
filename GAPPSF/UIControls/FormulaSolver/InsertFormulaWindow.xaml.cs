using GAPPSF.UIControls.FormulaSolver.FormulaInterpreter.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GAPPSF.UIControls.FormulaSolver
{
    /// <summary>
    /// Interaction logic for InsertFormulaWindow.xaml
    /// </summary>
    public partial class InsertFormulaWindow : Window
    {
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

        public InsertFormulaWindow()
        {
            InitializeComponent();

            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.TextGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.NumberGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.TrigonometricGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.CoordinateGroup);
            AddFunctionGroup(lbGroup, FunctionDescriptor.FunctionGroup.ContextGroup);

            lbGroup.SelectionChanged += lbGroup_SelectionChanged;
            lbFunction.SelectionChanged += lbFunction_SelectionChanged;

            lbGroup.SelectedIndex = 0;

        }

        void lbFunction_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbAltNames.Items.Clear();
            if (lbFunction.SelectedItem != null)
            {
                if (((FunctionItemData)lbFunction.SelectedItem).AlternateNames != null)
                {
                    foreach (string s in ((FunctionItemData)lbFunction.SelectedItem).AlternateNames)
                    {
                        lbAltNames.Items.Add(s);
                    }
                }
                tbDescription.Text = ((FunctionItemData)lbFunction.SelectedItem).Description;
            }
            else
            {
                tbDescription.Text = "";
            }
        }

        void lbGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbGroup.SelectedItem != null)
            {
                var fkt = from f in _fList.GetList()
                          where f.Group == ((GroupItemData)lbGroup.SelectedItem).GroupDescriptor
                          select f;

                lbFunction.Items.Clear();
                foreach (var f in fkt)
                {
                    lbFunction.Items.Add(new FunctionItemData(f.Name, f.Alternates, f.Functor, f.Description));
                }
            }

            lbFunction.SelectedIndex = 0;
        }

        private void AddFunctionGroup(ListBox lbGroup, FunctionDescriptor.FunctionGroup functionGroup)
        {
            lbGroup.Items.Add(
                new GroupItemData(
                    functionGroup, _fList.GetFunctionGroupString(functionGroup)
                )
            );
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (lbFunction.SelectedItem != null)
            {
                DialogResult = true;
            }
            Close();
        }

        private void lbFunction_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbFunction.SelectedItem != null)
            {
                DialogResult = true;
            }
            Close();
        }

    }
}
