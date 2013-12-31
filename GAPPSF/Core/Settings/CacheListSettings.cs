using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GAPPSF.Core
{
    public partial class Settings
    {
        public string ArchivedRowColor
        {
            get { return GetProperty("Red"); }
            set { SetProperty(value); }
        }

        public string DisabledRowColor
        {
            get { return GetProperty("Gray"); }
            set { SetProperty(value); }
        }

        public string FoundRowColor
        {
            get { return GetProperty("Green"); }
            set { SetProperty(value); }
        }

        public string IsOwnRowColor
        {
            get { return GetProperty("Yellow"); }
            set { SetProperty(value); }
        }

        public int CacheListWindowWidth
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int CacheListWindowHeight
        {
            get { return int.Parse(GetProperty("700")); }
            set { SetProperty(value.ToString()); }
        }
        public int CacheListWindowTop
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }
        public int CacheListWindowLeft
        {
            get { return int.Parse(GetProperty("100")); }
            set { SetProperty(value.ToString()); }
        }

        public int CacheListFrozenColumnCount
        {
            get { return int.Parse(GetProperty("1")); }
            set { SetProperty(value.ToString()); }
        }

        public int CacheListSortDirection
        {
            get { return int.Parse(GetProperty("0")); }
            set { SetProperty(value.ToString()); }
        }

        public int CacheListSortOnColumnIndex
        {
            get { return int.Parse(GetProperty("-1")); }
            set { SetProperty(value.ToString()); }
        }

        public bool CacheListEnableAutomaticSorting
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool CacheListShowSelectedOnly
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public bool CacheListShowFlaggedOnly
        {
            get { return bool.Parse(GetProperty("False")); }
            set { SetProperty(value.ToString()); }
        }

        public string CacheListFilterText
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

        public DataGridLength ColumnNameWidth
        {
            get { return new DataGridLength(double.Parse(GetProperty("200"), CultureInfo.InvariantCulture)); }
            set { SetProperty(value.Value.ToString(CultureInfo.InvariantCulture)); }
        }

        public string CacheListColumnInfo
        {
            get { return GetProperty(null); }
            set { SetProperty(value); }
        }

    }
}
