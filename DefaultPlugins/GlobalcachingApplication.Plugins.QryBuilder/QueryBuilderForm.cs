using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;

namespace GlobalcachingApplication.Plugins.QryBuilder
{
    public partial class QueryBuilderForm : Utils.BasePlugin.BaseUIChildWindowForm
    {
        public const string STR_TITLE = "Query builder";
        public const string STR_QUERY = "Query";
        public const string STR_EXPLAIN = "The query will be column wise processed";
        public const string STR_EXECUTE = "Execute";
        public const string STR_DELETE = "Delete";
        public const string STR_NEW = "New";
        public const string STR_RENAME = "Rename";
        public const string STR_SAVE = "Save all";
        public const string STR_ERROR = "Error";
        public const string STR_CIRCULARREFRENCE = "Circular reference of queries";

        /*
         *  country = Netherlands
         *        or                and  geocache type = Traditional
         *  country = Germany
         *        or
         *      .....
         *        
         * design: process per column
         * display/editor
         * 
         *      |      COL 0                   | COL 1 |       COL 2                      | COL 3
         *-----------------------------------------------------------------------------------
         *ROW 0 | [][country = Netherlands]    |[and]  |  [][geocachetype = traditional]  | [end]
         *-----------------------------------------------------------------------------------
         *ROW 1 | [or][country = Germany]      |       |  [end]
         *-----------------------------------------------------------------------------------
         *ROW 2 | [end]
         */

        public class GridItem
        {
            public QueryOperator QOperator { get; set; }
            public QueryExpression QExpression { get; set; }
        }
        public class GridRow
        {
            public List<GridItem> Columns { get; private set; }

            public GridRow()
            {
                Columns = new List<GridItem>();
            }
        }
        public class FreeQuery
        {
            public string Name { get; set; }
            public List<GridRow> Rows { get; private set; }

            public FreeQuery()
            {
                Rows = new List<GridRow>();
                Rows.Add(new GridRow());
                Rows.Add(new GridRow());

                GridItem gi = new GridItem();
                gi.QOperator = null;
                gi.QExpression = new QueryExpression();
                Rows[0].Columns.Add(gi);

                gi = new GridItem();
                gi.QOperator = new QueryOperator();
                gi.QOperator.SelectedOperator = QueryOperator.Operator.End;
                gi.QExpression = null;
                Rows[0].Columns.Add(gi);

                gi = new GridItem();
                gi.QOperator = new QueryOperator();
                gi.QOperator.SelectedOperator = QueryOperator.Operator.End;
                gi.QExpression = null;
                Rows[1].Columns.Add(gi);
            }

            public FreeQuery(QueryBuilderForm frm, XmlNode node)
            {
                Rows = new List<GridRow>();

                Name = node.Attributes["name"].Value;
                XmlNodeList nl = node.SelectNodes("row");
                foreach (XmlNode xmlr in nl)
                {
                    Rows.Add(new GridRow());
                    XmlNodeList cl = xmlr.SelectNodes("col");
                    foreach (XmlNode xmlc in cl)
                    {
                        GridItem gi = new GridItem();
                        gi.QOperator = null;
                        gi.QExpression = null;

                        if (xmlc.Attributes["nextoperator"] != null)
                        {
                            gi.QOperator = new QueryOperator();
                            gi.QOperator.SelectedOperator = (QueryOperator.Operator)Enum.Parse(typeof(QueryOperator.Operator), xmlc.Attributes["nextoperator"].Value);
                            gi.QOperator.PreviousSelectedOperator = gi.QOperator.SelectedOperator;
                            gi.QOperator.SelectedValueChanged +=  new EventHandler(frm.QOperator_SelectedValueChanged);
                        }
                        if (xmlc.Attributes["field"] != null)
                        {
                            gi.QExpression = new QueryExpression();
                            gi.QExpression.SetExpressionField(xmlc.Attributes["field"].Value);
                            gi.QExpression.SetExpressionOperator(xmlc.Attributes["operator"].Value);
                            gi.QExpression.ExpressionValue = xmlc.Attributes["condition"].Value;
                        }

                        Rows[Rows.Count-1].Columns.Add(gi);
                    }
                }
            }

            public override string ToString()
            {
                return Name ?? "";
            }
        }

        private static List<FreeQuery> _freeQueries = new List<FreeQuery>();
        private FreeQuery _activeQuery = null;
        private string _queriesFileName = null;

        //circular referene detection
        private static Framework.Data.Geocache _refgc = null;
        private static List<FreeQuery> _processedfq = new List<FreeQuery>();

        public QueryBuilderForm()
        {
            InitializeComponent();
        }

        public QueryBuilderForm(Framework.Interfaces.IPlugin owner, Framework.Interfaces.ICore core)
            : base(owner, core)
        {
            InitializeComponent();

            _queriesFileName = System.IO.Path.Combine(new string[] { core.PluginDataPath, "FreeQueries.xml" });

            try
            {
                if (System.IO.File.Exists(_queriesFileName))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_queriesFileName);
                    XmlNodeList nl = doc.SelectNodes("/queries/query");
                    foreach (XmlNode n in nl)
                    {
                        FreeQuery fq = new FreeQuery(this, n);

                        _freeQueries.Add(fq);
                        comboBox1.Items.Add(fq);
                    }
                }
            }
            catch
            {
            }

            SelectedLanguageChanged(this, EventArgs.Empty);
        }

        public void ApplicationInitialized()
        {
            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            foreach (FreeQuery fq in _freeQueries)
            {
                main.AddAction(OwnerPlugin, "Query builder", fq.Name);
            }
        }

        public static List<FreeQuery> AvailableQueries
        {
            get { return _freeQueries; }
        }

        protected override void SelectedLanguageChanged(object sender, EventArgs e)
        {
            base.SelectedLanguageChanged(sender, e);
            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_TITLE);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_QUERY);
            this.label3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXPLAIN);
            this.button5.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_EXECUTE);
            this.button1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DELETE);
            this.button2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEW);
            this.button3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_RENAME);
            this.button4.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SAVE);

            foreach (FreeQuery fq in _freeQueries)
            {
                foreach (GridRow gr in fq.Rows)
                {
                    foreach (GridItem gi in gr.Columns)
                    {
                        if (gi.QExpression != null)
                        {
                            gi.QExpression.LanguageChanged();
                        }
                        if (gi.QOperator != null)
                        {
                            gi.QOperator.LanguageChanged();
                        }
                    }
                }
            }
        }

        private void QueryBuilderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FreeQuery fq = new FreeQuery();
            fq.Name = textBox1.Text;
            _freeQueries.Add(fq);

            Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
            main.AddAction(OwnerPlugin, "Query builder", fq.Name);

            comboBox1.Items.Add(fq);
            comboBox1.SelectedItem = fq;
            for (int r = 0; r < fq.Rows.Count; r++)
            {
                for (int c = 0; c < fq.Rows[r].Columns.Count; c++)
                {
                    if (fq.Rows[r].Columns[c].QOperator != null)
                    {
                        fq.Rows[r].Columns[c].QOperator.SelectedValueChanged += new EventHandler(QOperator_SelectedValueChanged);
                    }
                }
            }
        }

        void QOperator_SelectedValueChanged(object sender, EventArgs e)
        {
            QueryOperator qo = sender as QueryOperator;
            if (_activeQuery != null && qo!=null)
            {
                int arow = -1;
                int acol = -1;
                for (int r = 0; r < _activeQuery.Rows.Count && arow < 0; r++)
                {
                    for (int c = 0; c < _activeQuery.Rows[r].Columns.Count && arow<0; c++)
                    {
                        if (_activeQuery.Rows[r].Columns[c].QOperator ==  qo)
                        {
                            arow = r;
                            acol = c;
                        }
                    }
                }
                if (arow >= 0 && acol >= 0)
                {
                    //add column or row?
                    if (arow > 0)
                    {
                        //operator added to column with expressions
                        //add expression or cut off
                        if (qo.SelectedOperator == QueryOperator.Operator.End && qo.PreviousSelectedOperator!= QueryOperator.Operator.End)
                        {
                            //cut off: all after this: null
                            for (int i = arow + 1; i < _activeQuery.Rows.Count; i++)
                            {
                                //if column exists
                                if (acol < _activeQuery.Rows[i].Columns.Count)
                                {
                                    if (_activeQuery.Rows[i].Columns[acol].QExpression != null)
                                    {
                                        panel2.Controls.Remove(_activeQuery.Rows[i].Columns[acol].QExpression);
                                        _activeQuery.Rows[i].Columns[acol].QExpression.Dispose();
                                        _activeQuery.Rows[i].Columns[acol].QExpression = null;
                                    }
                                    if (_activeQuery.Rows[i].Columns[acol].QOperator != null)
                                    {
                                        panel2.Controls.Remove(_activeQuery.Rows[i].Columns[acol].QOperator);
                                        _activeQuery.Rows[i].Columns[acol].QOperator.Dispose();
                                        _activeQuery.Rows[i].Columns[acol].QOperator = null;
                                    }
                                }
                            }
                        }
                        else if (qo.SelectedOperator != QueryOperator.Operator.End && qo.PreviousSelectedOperator == QueryOperator.Operator.End)
                        {
                            //add expression in next row
                            arow++;
                            if (arow <= _activeQuery.Rows.Count)
                            {
                                _activeQuery.Rows.Add(new GridRow());
                            }
                            for (int i = _activeQuery.Rows[arow].Columns.Count; i <= acol; i++)
                            {
                                GridItem gi = new GridItem();
                                gi.QOperator = null;
                                gi.QExpression = null;
                                _activeQuery.Rows[arow].Columns.Add(gi);
                            }
                            _activeQuery.Rows[arow].Columns[acol].QExpression = new QueryExpression();
                            _activeQuery.Rows[arow].Columns[acol].QExpression.Location = new Point(212 + (212 + 80) * acol - _activeQuery.Rows[arow].Columns[acol].QExpression.Width / 2 - panel2.HorizontalScroll.Value, 40 * arow - panel2.VerticalScroll.Value);
                            panel2.Controls.Add(_activeQuery.Rows[arow].Columns[acol].QExpression);

                            //add end operator in next
                            arow++;
                            if (arow <= _activeQuery.Rows.Count)
                            {
                                _activeQuery.Rows.Add(new GridRow());
                            }
                            for (int i = _activeQuery.Rows[arow].Columns.Count; i <= acol; i++)
                            {
                                GridItem gi = new GridItem();
                                gi.QOperator = null;
                                gi.QExpression = null;
                                _activeQuery.Rows[arow].Columns.Add(gi);
                            }
                            _activeQuery.Rows[arow].Columns[acol].QOperator = new QueryOperator();
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedOperator = QueryOperator.Operator.End;
                            _activeQuery.Rows[arow].Columns[acol].QOperator.Location = new Point(212 + (212 + 80) * acol - _activeQuery.Rows[arow].Columns[acol].QOperator.Width / 2 - panel2.HorizontalScroll.Value, 40 * arow - panel2.VerticalScroll.Value);
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedValueChanged += new EventHandler(QOperator_SelectedValueChanged);
                            panel2.Controls.Add(_activeQuery.Rows[arow].Columns[acol].QOperator);
                        }
                    }
                    else
                    {
                        //add column or cutt off
                        //only first row
                        if (qo.SelectedOperator == QueryOperator.Operator.End && qo.PreviousSelectedOperator != QueryOperator.Operator.End)
                        {
                            //cut off
                            for (int r = 0; r < _activeQuery.Rows.Count; r++)
                            {
                                for (int c = acol+1; c < _activeQuery.Rows[r].Columns.Count; c++)
                                {
                                    if (_activeQuery.Rows[r].Columns[c].QExpression != null)
                                    {
                                        panel2.Controls.Remove(_activeQuery.Rows[r].Columns[c].QExpression);
                                        _activeQuery.Rows[r].Columns[c].QExpression.Dispose();
                                        _activeQuery.Rows[r].Columns[c].QExpression = null;
                                    }
                                    if (_activeQuery.Rows[r].Columns[c].QOperator != null)
                                    {
                                        panel2.Controls.Remove(_activeQuery.Rows[r].Columns[c].QOperator);
                                        _activeQuery.Rows[r].Columns[c].QOperator.Dispose();
                                        _activeQuery.Rows[r].Columns[c].QOperator = null;
                                    }
                                }
                            }
                        }
                        else if (qo.SelectedOperator != QueryOperator.Operator.End && qo.PreviousSelectedOperator == QueryOperator.Operator.End)
                        {
                            //add
                            for (int i = _activeQuery.Rows[arow].Columns.Count; i <= acol+2; i++)
                            {
                                GridItem gi = new GridItem();
                                gi.QOperator = null;
                                gi.QExpression = null;
                                _activeQuery.Rows[arow].Columns.Add(gi);
                            }
                            acol++;
                            _activeQuery.Rows[arow].Columns[acol].QExpression = new QueryExpression();
                            _activeQuery.Rows[arow].Columns[acol].QExpression.Location = new Point(212 + (212 + 80) * acol - _activeQuery.Rows[arow].Columns[acol].QExpression.Width / 2 - panel2.HorizontalScroll.Value, 40 * arow - panel2.VerticalScroll.Value);
                            panel2.Controls.Add(_activeQuery.Rows[arow].Columns[acol].QExpression);

                            acol++;
                            _activeQuery.Rows[arow].Columns[acol].QOperator = new QueryOperator();
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedOperator = QueryOperator.Operator.End;
                            _activeQuery.Rows[arow].Columns[acol].QOperator.Location = new Point(212 + (212 + 80) * acol - _activeQuery.Rows[arow].Columns[acol].QOperator.Width / 2 - panel2.HorizontalScroll.Value, 40 * arow - panel2.VerticalScroll.Value);
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedValueChanged += new EventHandler(QOperator_SelectedValueChanged);
                            panel2.Controls.Add(_activeQuery.Rows[arow].Columns[acol].QOperator);

                            arow++;
                            if (arow <= _activeQuery.Rows.Count)
                            {
                                _activeQuery.Rows.Add(new GridRow());
                            }
                            acol--;
                            for (int i = _activeQuery.Rows[arow].Columns.Count; i <= acol; i++)
                            {
                                GridItem gi = new GridItem();
                                gi.QOperator = null;
                                gi.QExpression = null;
                                _activeQuery.Rows[arow].Columns.Add(gi);
                            }
                            _activeQuery.Rows[arow].Columns[acol].QOperator = new QueryOperator();
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedOperator = QueryOperator.Operator.End;
                            _activeQuery.Rows[arow].Columns[acol].QOperator.Location = new Point(212 + (212 + 80) * acol - _activeQuery.Rows[arow].Columns[acol].QOperator.Width / 2 - panel2.HorizontalScroll.Value, 40 * arow - panel2.VerticalScroll.Value);
                            _activeQuery.Rows[arow].Columns[acol].QOperator.SelectedValueChanged += new EventHandler(QOperator_SelectedValueChanged);
                            panel2.Controls.Add(_activeQuery.Rows[arow].Columns[acol].QOperator);

                        }
                    }
                }
            }
            if (qo != null)
            {
                qo.PreviousSelectedOperator = qo.SelectedOperator;
            }
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            _activeQuery = comboBox1.SelectedItem as FreeQuery;
            buildActiveQueryTable();
            checkButtonStates();
        }

        private void buildActiveQueryTable()
        {
            panel2.Controls.Clear();
            if (_activeQuery != null)
            {
                int currentTop = 0;
                for (int r = 0; r < _activeQuery.Rows.Count; r++)
                {
                    int currentMid = 212;
                    for (int c = 0; c < _activeQuery.Rows[r].Columns.Count; c++)
                    {
                        if (_activeQuery.Rows[r].Columns[c].QOperator != null)
                        {
                            _activeQuery.Rows[r].Columns[c].QOperator.Location = new Point(currentMid - _activeQuery.Rows[r].Columns[c].QOperator.Width/2, currentTop); 
                            panel2.Controls.Add(_activeQuery.Rows[r].Columns[c].QOperator);
                        }
                        if (_activeQuery.Rows[r].Columns[c].QExpression != null)
                        {
                            _activeQuery.Rows[r].Columns[c].QExpression.Location = new Point(currentMid - _activeQuery.Rows[r].Columns[c].QExpression.Width/2, currentTop);
                            panel2.Controls.Add(_activeQuery.Rows[r].Columns[c].QExpression);
                        }
                        currentMid += 212 + 80;
                    }
                    if (r % 2 == 0)
                    {
                        currentTop += 40;
                    }
                    else
                    {
                        currentTop += 40;
                    }
                }
            }
        }

        private void checkButtonStates()
        {
            //new / rename
            if (textBox1.Text.Length == 0)
            {
                button2.Enabled = false;
                button3.Enabled = false;
            }
            else
            {
                FreeQuery fq = (from f in _freeQueries where f.Name == textBox1.Text select f).FirstOrDefault();
                button2.Enabled = (fq == null);
                button3.Enabled = (fq == null && _activeQuery!=null);
            }
            button5.Enabled = (_activeQuery != null);
            button1.Enabled = (_activeQuery != null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (_activeQuery != null)
            {
                ExecuteQuery(_activeQuery);
            }
            this.Cursor = Cursors.Default;
        }

        public void ExecuteQuery(string name)
        {
            FreeQuery fq = (from f in _freeQueries where f.Name == name select f).FirstOrDefault();
            if (fq != null)
            {
                ExecuteQuery(fq);
            }
        }

        public void ExecuteQuery(FreeQuery fq)
        {
            if (Core.Geocaches.Count > 0)
            {
                _refgc = Core.Geocaches[0] as Framework.Data.Geocache;
                _processedfq.Clear();

                Core.Geocaches.BeginUpdate();
                try
                {
                    foreach (Framework.Data.Geocache gc in Core.Geocaches)
                    {
                        gc.Selected = GetQueryResult(fq, gc);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
                }
                Core.Geocaches.EndUpdate();
            }
        }

        public static bool GetQueryResult(FreeQuery fq, Framework.Data.Geocache gc)
        {
            //circular check
            if (gc == _refgc)
            {
                if (_processedfq.Contains(fq))
                {
                    throw new Exception(Utils.LanguageSupport.Instance.GetTranslation(STR_CIRCULARREFRENCE));
                }
                else
                {
                    _processedfq.Add(fq);
                }
            }

            //process per column
            bool result = true;
            for (int c = 0; c < fq.Rows[0].Columns.Count; c += 2)
            {
                if (fq.Rows[0].Columns[c].QExpression != null)
                {
                    bool colResult = GetQueryResultForColumn(fq, c, gc);
                    if (c == 0)
                    {
                        result = colResult;
                    }
                    else
                    {
                        switch (fq.Rows[0].Columns[c - 1].QOperator.SelectedOperator)
                        {
                            case QueryOperator.Operator.And:
                                result &= colResult;
                                break;
                            case QueryOperator.Operator.Or:
                                result |= colResult;
                                break;
                            case QueryOperator.Operator.End:
                                c = fq.Rows[0].Columns.Count; //exit
                                break;
                        }
                    }
                }
            }
            return result;
        }
        public static bool GetQueryResultForColumn(FreeQuery fq, int col, Framework.Data.Geocache gc)
        {
            //process per column
            bool result = true;
            for (int r = 0; r < fq.Rows.Count; r += 2)
            {
                if (col < fq.Rows[r].Columns.Count && fq.Rows[r].Columns[col].QExpression!=null)
                {
                    if (r > 0)
                    {
                        switch (fq.Rows[r-1].Columns[col].QOperator.SelectedOperator)
                        {
                            case QueryOperator.Operator.And:
                                result &= fq.Rows[r].Columns[col].QExpression.GetExpressionResult(gc);
                                break;
                            case QueryOperator.Operator.Or:
                                result |= fq.Rows[r].Columns[col].QExpression.GetExpressionResult(gc);
                                break;
                            case QueryOperator.Operator.End:
                                //oeps!
                                result = false;
                                break;
                        }
                    }
                    else
                    {
                        result = fq.Rows[r].Columns[col].QExpression.GetExpressionResult(gc);
                    }
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkButtonStates();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FreeQuery fq = _activeQuery;
            if (fq != null)
            {
                comboBox1.Items.Remove(fq);
                _freeQueries.Remove(fq);

                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                main.RemoveAction(OwnerPlugin, "Query builder", fq.Name);

                for (int r = 0; r < fq.Rows.Count; r++)
                {
                    for (int c = 0; c < fq.Rows[r].Columns.Count; c++)
                    {
                        if (fq.Rows[r].Columns[c].QExpression != null)
                        {
                            panel2.Controls.Remove(fq.Rows[r].Columns[c].QExpression);
                            fq.Rows[r].Columns[c].QExpression.Dispose();
                            fq.Rows[r].Columns[c].QExpression = null;
                        }
                        if (fq.Rows[r].Columns[c].QOperator != null)
                        {
                            panel2.Controls.Remove(fq.Rows[r].Columns[c].QOperator);
                            fq.Rows[r].Columns[c].QOperator.Dispose();
                            fq.Rows[r].Columns[c].QOperator = null;
                        }
                    }
                }

                comboBox1_SelectedValueChanged(this, EventArgs.Empty);
            }
        }

        private void renameQueryReference(string oldName, string newName)
        {
            foreach (FreeQuery fq in _freeQueries)
            {
                for (int r = 0; r < fq.Rows.Count; r++)
                {
                    for (int c = 0; c < fq.Rows[r].Columns.Count; c++)
                    {
                        if (fq.Rows[r].Columns[c].QExpression != null)
                        {
                            if (fq.Rows[r].Columns[c].QExpression.SelectedQueryExpression is QEQuery)
                            {
                                if (fq.Rows[r].Columns[c].QExpression.ExpressionValue == oldName)
                                {
                                    fq.Rows[r].Columns[c].QExpression.ExpressionValue = newName;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FreeQuery fq = _activeQuery;
            if (fq != null)
            {
                Framework.Interfaces.IPluginUIMainWindow main = (from Framework.Interfaces.IPluginUIMainWindow a in Core.GetPlugin(Framework.PluginType.UIMainWindow) select a).FirstOrDefault();
                main.RemoveAction(OwnerPlugin, "Query builder", fq.Name);

                renameQueryReference(fq.Name, textBox1.Text);

                fq.Name = textBox1.Text;
                main.AddAction(OwnerPlugin, "Query builder", fq.Name);

                typeof(ComboBox).InvokeMember("RefreshItems", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, comboBox1, new object[] { });

                comboBox1_SelectedValueChanged(this, EventArgs.Empty);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*
             * <queries>
             *  <query name="">
             *       <row>
             *          <col field="" operator="" condition="">
             *          <col nextoperator="">
             *       </row>
             *       <row>
             *          <col nextoperator="">
             *          <col field="" operator="" condition="">
             *       </row>
             *  </query>
             * </queries>
             */
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("queries");
                doc.AppendChild(root);
                foreach (FreeQuery fq in _freeQueries)
                {
                    XmlElement q = doc.CreateElement("query");
                    XmlAttribute attr = doc.CreateAttribute("name");
                    XmlText txt = doc.CreateTextNode(fq.Name);
                    attr.AppendChild(txt);
                    q.Attributes.Append(attr);
                    root.AppendChild(q);
                    foreach (GridRow gr in fq.Rows)
                    {
                        XmlElement r = doc.CreateElement("row");
                        q.AppendChild(r);
                        foreach (GridItem gi in gr.Columns)
                        {
                            XmlElement c = doc.CreateElement("col");
                            r.AppendChild(c);

                            if (gi.QExpression != null)
                            {
                                attr = doc.CreateAttribute("field");
                                txt = doc.CreateTextNode(gi.QExpression.SelectedQueryExpression != null ? gi.QExpression.SelectedQueryExpression.Name : "");
                                attr.AppendChild(txt);
                                c.Attributes.Append(attr);

                                attr = doc.CreateAttribute("operator");
                                txt = doc.CreateTextNode(gi.QExpression.ExpressionOperator.ToString());
                                attr.AppendChild(txt);
                                c.Attributes.Append(attr);

                                attr = doc.CreateAttribute("condition");
                                txt = doc.CreateTextNode(gi.QExpression.ExpressionValue.ToString());
                                attr.AppendChild(txt);
                                c.Attributes.Append(attr);
                            }
                            if (gi.QOperator != null)
                            {
                                attr = doc.CreateAttribute("nextoperator");
                                txt = doc.CreateTextNode(gi.QOperator.SelectedOperator.ToString());
                                attr.AppendChild(txt);
                                c.Attributes.Append(attr);
                            }
                        }
                    }
                }
                doc.Save(_queriesFileName);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
