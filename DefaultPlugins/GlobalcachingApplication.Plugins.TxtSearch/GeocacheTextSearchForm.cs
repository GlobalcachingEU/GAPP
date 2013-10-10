using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.TxtSearch
{
    public partial class GeocacheTextSearchForm : Form
    {
        public const string STR_GEOCACHESEARCH = "Geocache search: Text";
        public const string STR_SELECTION = "Selection";
        public const string STR_FIELD = "Field";
        public const string STR_FIND = "Find";
        public const string STR_NEWSEARCH = "New search";
        public const string STR_SEARCHWITHINSELECTION = "Search within selection";
        public const string STR_ADDTOSELECTION = "Add to current selection";
        public const string STR_NAME = "Name";
        public const string STR_CODE = "Code";
        public const string STR_DESCRIPTION = "Description";
        public const string STR_OWNER = "Owner";
        public const string STR_CASESENSATIVE = "Case sensative";

        public class DialogContent
        {
            public enum SelectionSelect
            {
                NewSearch,
                WithinSelection,
                AddToSelection
            }
            public enum FieldSelect
            {
                Name,
                Code,
                Description,
                Owner
            }

            public SelectionSelect Selection { get; set; }
            public FieldSelect Field { get; set; }
            public bool CaseSensative { get; set; }
            public string Text { get; set; }
        }

        public DialogContent SearchOption { get; private set; }

        public GeocacheTextSearchForm()
        {
            InitializeComponent();

            this.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_GEOCACHESEARCH);
            this.groupBox1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SELECTION);
            this.groupBox2.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIELD);
            this.groupBox3.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIND);
            this.label1.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_FIND);
            this.radioButtonAddToCurrent.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_ADDTOSELECTION);
            this.radioButtonWithinSelection.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_SEARCHWITHINSELECTION);
            this.radioButtonNewSearch.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NEWSEARCH);
            this.radioButtonOwner.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_OWNER);
            this.radioButtonName.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_NAME);
            this.radioButtonCode.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CODE);
            this.radioButtonDescription.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_DESCRIPTION);
            this.checkBoxCaseSensative.Text = Utils.LanguageSupport.Instance.GetTranslation(STR_CASESENSATIVE);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            SearchOption = new DialogContent();
            if (radioButtonNewSearch.Checked) SearchOption.Selection = DialogContent.SelectionSelect.NewSearch;
            else if (radioButtonWithinSelection.Checked) SearchOption.Selection = DialogContent.SelectionSelect.WithinSelection;
            else if (radioButtonAddToCurrent.Checked) SearchOption.Selection = DialogContent.SelectionSelect.AddToSelection;

            if (radioButtonName.Checked) SearchOption.Field = DialogContent.FieldSelect.Name;
            else if (radioButtonCode.Checked) SearchOption.Field = DialogContent.FieldSelect.Code;
            else if (radioButtonDescription.Checked) SearchOption.Field = DialogContent.FieldSelect.Description;
            else if (radioButtonOwner.Checked) SearchOption.Field = DialogContent.FieldSelect.Owner;

            SearchOption.CaseSensative = checkBoxCaseSensative.Checked;
            SearchOption.Text = textBoxFind.Text;
        }

        private void textBoxFind_TextChanged(object sender, EventArgs e)
        {
            buttonOK.Enabled = (textBoxFind.Text.Length > 0);
        }
    }
}
