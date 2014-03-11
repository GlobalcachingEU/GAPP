using GAPPSF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Xml;

namespace GAPPSF.ActionSequence
{
    public class Manager
    {
        private static Manager _uniqueInstance = null;
        private static object _lockObject = new object();

        public ObservableCollection<Sequence> ActionSequences { get; private set; }

        public Manager()
        {
#if DEBUG
            if (_uniqueInstance != null)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif
            ActionSequences = new ObservableCollection<Sequence>();
            loadSequences();
            foreach (var f in ActionSequences)
            {
                AddSequenceToMenu(f);
            }
        }

        public static Manager Instance
        {
            get
            {
                if (_uniqueInstance==null)
                {
                    lock(_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new Manager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public async Task ExecuteSequence(Sequence seq)
        {
            await RunActionSequence(seq);
        }

        public void AddSequenceToMenu(Sequence seq)
        {
            MenuItem mi = new MenuItem();
            mi.Header = seq.Name;
            mi.Name = seq.ID;
            mi.Command = ExecuteSequenceCommand;
            mi.CommandParameter = seq;
            Core.ApplicationData.Instance.MainWindow.actseqmnu.Items.Add(mi);
        }

        public void RemoveSequenceFromMenu(Sequence seq)
        {
            MenuItem mi;
            for (int i = 0; i < Core.ApplicationData.Instance.MainWindow.actseqmnu.Items.Count; i++)
            {
                mi = Core.ApplicationData.Instance.MainWindow.actseqmnu.Items[i] as MenuItem;
                if (mi != null && mi.Name == seq.ID)
                {
                    Core.ApplicationData.Instance.MainWindow.actseqmnu.Items.RemoveAt(i);
                    break;
                }
            }
        }

        public void RenameSequence(Sequence af, string newName)
        {
            af.Name = newName;
            MenuItem mi;
            for (int i = 0; i < Core.ApplicationData.Instance.MainWindow.actseqmnu.Items.Count; i++)
            {
                mi = Core.ApplicationData.Instance.MainWindow.actseqmnu.Items[i] as MenuItem;
                if (mi != null && mi.Name == af.ID)
                {
                    mi.Header = newName;
                    break;
                }
            }
        }

        private void loadSequences()
        {
            try
            {
                if (!string.IsNullOrEmpty(Core.Settings.Default.ActionSequenceXml))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(Core.Settings.Default.ActionSequenceXml);
                    XmlElement root = doc.DocumentElement;

                    XmlNodeList bmNodes = root.SelectNodes("sequence");
                    if (bmNodes != null)
                    {
                        foreach (XmlNode n in bmNodes)
                        {
                            Sequence bm = new Sequence();
                            bm.Name = n.SelectSingleNode("Name").InnerText;
                            bm.ID = n.SelectSingleNode("ID").InnerText;

                            XmlNodeList anl = n.SelectSingleNode("actions").SelectNodes("action");
                            if (anl != null)
                            {
                                foreach (XmlNode an in anl)
                                {
                                    Tuple<string, string> ai = new Tuple<string, string>(an.SelectSingleNode("Item1").InnerText, an.SelectSingleNode("Item2").InnerText);
                                    bm.Actions.Add(ai);
                                }
                            }
                            ActionSequences.Add(bm);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        public void Save()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("sequences");
                doc.AppendChild(root);
                foreach (Sequence bmi in ActionSequences)
                {
                    XmlElement bm = doc.CreateElement("sequence");
                    root.AppendChild(bm);

                    XmlElement el = doc.CreateElement("Name");
                    XmlText txt = doc.CreateTextNode(bmi.Name);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("ID");
                    txt = doc.CreateTextNode(bmi.ID);
                    el.AppendChild(txt);
                    bm.AppendChild(el);

                    el = doc.CreateElement("actions");
                    bm.AppendChild(el);

                    foreach (Tuple<string, string> ai in bmi.Actions)
                    {
                        XmlElement ael = doc.CreateElement("action");
                        el.AppendChild(ael);

                        XmlElement subel = doc.CreateElement("Item1");
                        txt = doc.CreateTextNode(ai.Item1);
                        subel.AppendChild(txt);
                        ael.AppendChild(subel);

                        subel = doc.CreateElement("Item2");
                        txt = doc.CreateTextNode(ai.Item2);
                        subel.AppendChild(txt);
                        ael.AppendChild(subel);
                    }
                }
                StringBuilder sb = new StringBuilder();
                System.IO.TextWriter tr = new System.IO.StringWriter(sb);
                XmlTextWriter wr = new XmlTextWriter(tr);
                wr.Formatting = Formatting.None;
                doc.Save(wr);
                wr.Close();
                Core.Settings.Default.ActionSequenceXml = sb.ToString();
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
        }

        private AsyncDelegateCommand _executeSequenceCommand;
        public AsyncDelegateCommand ExecuteSequenceCommand
        {
            get
            {
                if (_executeSequenceCommand == null)
                {
                    _executeSequenceCommand = new AsyncDelegateCommand(param => RunActionSequence(param as Sequence),
                        param => Core.ApplicationData.Instance.ActiveDatabase != null);
                }
                return _executeSequenceCommand;
            }
        }

        public async Task RunActionSequence(Sequence af)
        {
            if (Core.ApplicationData.Instance.ActiveDatabase != null)
            {
                foreach(var t in af.Actions)
                {
                    MenuItem mni = Core.ApplicationData.Instance.MainWindow.mainMenu.FindName(t.Item1) as MenuItem;
                    if (mni!=null)
                    {
                        if (mni.Command != null)
                        {
                            if (mni.Command is AsyncDelegateCommand)
                            {
                                await (mni.Command as AsyncDelegateCommand).ExecuteAsync(mni.CommandParameter);
                            }
                            else
                            {
                                mni.Command.Execute(mni.CommandParameter);
                            }
                        }
                        else
                        {
                            //MenuItemAutomationPeer p = new MenuItemAutomationPeer(mni);
                            //IInvokeProvider ip = p.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                            //ip.Invoke();
                            mni.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        }
                    }
                }
            }
        }

    }
}
