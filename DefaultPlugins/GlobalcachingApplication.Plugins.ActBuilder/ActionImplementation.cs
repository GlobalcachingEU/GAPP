using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections;

namespace GlobalcachingApplication.Plugins.ActBuilder
{
    public class ActionImplementation
    {
        [Flags]
        public enum Operator : int
        {
            Equal = 1,
            NotEqual = 2,
            LessOrEqual = 4,
            Less = 8,
            LargerOrEqual = 16,
            Larger = 32,
        }

        public class OutputConnectionInfo
        {
            public Operator OutputOperator { get; set; }
            public ActionImplementation ConnectedAction { get; set; }
            public int PassCounter { get; set; }
        }

        public string Name { get; private set; }
        public string ID { get; set; }
        public Point Location { get; set; }
        private ActionControl _assignedActionControl = null;
        public Framework.Interfaces.ICore Core { get; private set; }
        public List<string> Values { get; private set; }

        //connections
        private List<OutputConnectionInfo> _outputConnectionInfo = null;

        //execution
        private Hashtable _geocachesAtInpuntConnector = null;
        public Hashtable GeocachesAtInputConnector { get { return _geocachesAtInpuntConnector; } }

        public ActionImplementation(string name, Framework.Interfaces.ICore core)
        {
            Name = name;
            Core = core;
            _outputConnectionInfo = new List<OutputConnectionInfo>();
            Values = new List<string>();
            _geocachesAtInpuntConnector = new Hashtable();
        }

        public virtual bool PrepareRun()
        {
            _geocachesAtInpuntConnector.Clear();
            foreach (OutputConnectionInfo oci in _outputConnectionInfo)
            {
                oci.PassCounter = 0;
            }
            return true;
        }
        public virtual void FinalizeRun()
        {
        }

        public void Run(Framework.Data.Geocache gc)
        {
            if (_geocachesAtInpuntConnector[gc.Code]==null)
            {
                _geocachesAtInpuntConnector.Add(gc.Code, gc);

                Operator op = Process(gc);
                foreach (Operator e in (Operator[])Enum.GetValues(typeof(Operator)))
                {
                    if ((op & e) != 0)
                    {
                        var cons = from c in _outputConnectionInfo where c.OutputOperator == e select c;
                        foreach (var c in cons)
                        {
                            c.PassCounter++;
                            c.ConnectedAction.Run(gc);
                        }
                    }
                }
            }
        }

        public virtual Operator Process(Framework.Data.Geocache gc)
        {
            return 0;
        }

        public bool ConnectToOutput(ActionImplementation impl, Operator op)
        {
            if ((from c in _outputConnectionInfo where c.ConnectedAction == impl && c.OutputOperator == op select c).FirstOrDefault() == null)
            {
                OutputConnectionInfo oci = new OutputConnectionInfo();
                oci.OutputOperator = op;
                oci.ConnectedAction = impl;
                _outputConnectionInfo.Add(oci);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SelectedLanguageChanged()
        {
            if (_assignedActionControl != null)
            {
                _assignedActionControl.Title.Content = Utils.LanguageSupport.Instance.GetTranslation(Name);
            }
        }

        public List<ActionImplementation> GetOutputConnections(Operator op)
        {
            return ((from c in _outputConnectionInfo where c.OutputOperator == op select c.ConnectedAction).ToList());
        }

        public int GetOutputConnectorPassCounter(Operator op)
        {
            return ((from c in _outputConnectionInfo where c.OutputOperator == op select c.PassCounter).FirstOrDefault());
        }

        public List<OutputConnectionInfo> GetOutputConnections()
        {
            return _outputConnectionInfo;
        }

        public void RemoveOutputConnection(ActionImplementation impl)
        {
            List<OutputConnectionInfo> ocil = (from c in _outputConnectionInfo where c.ConnectedAction == impl select c).ToList();
            foreach (var oci in ocil)
            {
                _outputConnectionInfo.Remove(oci);
            }
        }
        public void RemoveOutputConnection(ActionImplementation impl, Operator op)
        {
            var oci = (from c in _outputConnectionInfo where c.ConnectedAction == impl && c.OutputOperator == op select c).FirstOrDefault();
            if (oci != null)
            {
                _outputConnectionInfo.Remove(oci);
            }
        }

        public ActionControl UIActionControl
        {
            get { return _assignedActionControl; }
            set { _assignedActionControl = value; }
        }

        public virtual UIElement GetUIElement()
        {
            return null;
        }

        public virtual void CommitUIData(UIElement uiElement)
        {
        }

        public virtual bool AllowEntryPoint
        {
            get { return true; }
        }

        public virtual Operator AllowOperators
        {
            get { return Operator.Equal | Operator.Larger | Operator.LargerOrEqual | Operator.Less | Operator.LessOrEqual | Operator.NotEqual; }
        }

        public ComboBox CreateComboBox(string[] items, string value)
        {

            ComboBox cb = new ComboBox();
            cb.Width = 150;
            if (items != null)
            {
                foreach (string s in items)
                {
                    ComboBoxItem cboxitem = new ComboBoxItem();
                    cboxitem.Content = s;
                    cb.Items.Add(cboxitem);
                }
            }
            cb.HorizontalAlignment = HorizontalAlignment.Center;
            cb.IsEditable = true;
            cb.IsSynchronizedWithCurrentItem = false;
            cb.IsEnabled = true;
            if (Values.Count == 0)
            {
                Values.Add("");
            }
            cb.Text = value;
            return cb;
        }

        public static Operator GetOperators(string sGC, string sV)
        {
            return GetOperators(sGC.CompareTo(sV));
        }

        public static Operator GetOperators(int cmp)
        {
            Operator result = 0;
            if (cmp == 0)
            {
                result |= Operator.Equal;
                result |= Operator.LargerOrEqual;
                result |= Operator.LessOrEqual;
            }
            else
            {
                result |= Operator.NotEqual;
                if (cmp < 0)
                {
                    result |= Operator.Less;
                    result |= Operator.LessOrEqual;
                }
                else
                {
                    result |= Operator.Larger;
                    result |= Operator.LargerOrEqual;
                }
            }
            return result;
        }
    }
    public class ActionImplementationCondition : ActionImplementation
    {
        public ActionImplementationCondition(string name, Framework.Interfaces.ICore core)
            : base(name, core)
        {
        }
    }

    public class ActionImplementationAction : ActionImplementation
    {
        public ActionImplementationAction(string name, Framework.Interfaces.ICore core)
            : base(name, core)
        {
        }

        public override Operator AllowOperators
        {
            get { return 0; }
        }

    }

    public class ActionImplementationExecuteOnce : ActionImplementation
    {
        private bool _hasBeenExecuted = false;

        public ActionImplementationExecuteOnce(string name, Framework.Interfaces.ICore core)
            : base(name, core)
        {
        }

        public override bool PrepareRun()
        {
            _hasBeenExecuted = false;
            return base.PrepareRun();
        }

        public override ActionImplementation.Operator Process(Framework.Data.Geocache gc)
        {
            if (!_hasBeenExecuted)
            {
                _hasBeenExecuted = Execute();               
            }
            return Operator.Equal;
        }

        protected virtual bool Execute()
        {
            return true;
        }

        public override Operator AllowOperators
        {
            get { return Operator.Equal; }
        }

    }

}
