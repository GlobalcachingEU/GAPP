using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlobalcachingApplication.Framework.Data;
using GlobalcachingApplication.Framework.Interfaces;
using GlobalcachingApplication.Utils;
using GlobalcachingApplication.Utils.BasePlugin;
using System.Web;
using Gavaghan.Geodesy;

public class TestTemplate
{
    private Plugin _plugin = null;
    private ICore _core = null;

    public static string Run(Plugin plugin, ICore core)
    {
        TestTemplate instance = new TestTemplate();
        return instance.RunInstance(plugin, core);
    }

    public class MyGeocacheFind
    {
        public Geocache gc;
        public Log lg;
        public DateTime logDate;
        public DateTime logDateForSort;
    }
    private List<MyGeocacheFind> _myFinds;

    private void GetMyFinds()
    {
        List<Geocache> gcList = DataAccess.GetFoundGeocaches(_core.Geocaches, _core.Logs, _core.GeocachingComAccount);
        _myFinds = new List<MyGeocacheFind>();
        string tl = _core.GeocachingComAccount.AccountName.ToLower();
        foreach (Geocache gc in gcList)
        {
            MyGeocacheFind mf = new MyGeocacheFind();
            mf.gc = gc;
            mf.lg = (from Log l in _core.Logs where l.GeocacheCode == gc.Code && l.Finder.ToLower() == tl && l.LogType.AsFound orderby l.Date descending select l).FirstOrDefault();
            if (mf.lg != null)
            {
                mf.logDate = mf.lg.Date;
                mf.logDateForSort = mf.lg.Date;
                int logid = 0;
                if (mf.lg.ID.StartsWith("GL"))
                {
                    logid = Conversion.GetCacheIDFromCacheCode(mf.lg.ID);
                }
                else
                {
                    try
                    {
                        logid = int.Parse(mf.lg.ID);
                    }
                    catch
                    {
                    }
                }
                mf.logDateForSort.AddMilliseconds(logid);
            }
            else
            {
                mf.logDate = DateTime.MinValue;
                mf.logDateForSort = DateTime.MinValue;
            }
            _myFinds.Add(mf);
        }
        _myFinds.Sort(delegate(MyGeocacheFind a, MyGeocacheFind b)
        {
            return a.logDateForSort.CompareTo(b.logDateForSort);
        });
    }

    private string ToHtml(string s)
    {
        return HttpUtility.HtmlEncode(s);
    }

    private void RegisterText(string txt)
    {
        RegisterText(new string[] { txt });
    }

    private void RegisterText(string[] txt)
    {
        foreach (string s in txt)
        {
            _core.LanguageItems.Add(new LanguageItem(s));
        }
    }

    private string Translate(string s)
    {
        return Translate(s, true);
    }

    private string Translate(string s, bool toHtml)
    {
        if (toHtml)
        {
            return ToHtml(LanguageSupport.Instance.GetTranslation(s));
        }
        else
        {
            return LanguageSupport.Instance.GetTranslation(s);
        }
    }

    private void InitExtension()
    {
        GetMyFinds();
    }

    //SKIN

    public class Layout
    {
        public class Statistics
        {
            public class Item
            {
                public string Text { get; set; }
                public bool IsMarker { get; set; }
                public bool IsHtml { get; set; }
                public string Height { get; set; }
                public string Width { get; set; }
                public string Align { get; set; }
            }
            public class Row
            {
                public List<Item> Items;

                public Row()
                {
                    Items = new List<Item>();
                }
            }

            public string Title { get; set; }
            public string AxisLabelX { get; set; }
            public string AxisLabelY { get; set; }
            public string Width { get; set; }
            public string Align { get; set; }
            public List<Row> Rows { get; set; }

            public Statistics(string title)
            {
                Title = title;
                Width = "100%";
                Rows = new List<Row>();
            }
            public Statistics(string title, string axislabelX, string axislabelY): this(title)
            {
                AxisLabelX = axislabelX;
                AxisLabelY = axislabelY;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                //this layout
                //table with header (text is Title)
                //within this table, the table with the stats
                //assumption: all rows are os same length and all columns are of same length. At least 1 row and 1 column present 
                if (Rows.Count > 0)
                {
                    int cols = Rows[0].Items.Count;

                    sb.AppendLine("<table cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" border=\"1\" style=\"border-left-style: none; border-right-style: none; border-top: 1.5pt solid black; border-bottom: 1.5pt solid black\">");
                    sb.AppendLine("<tr>");
                    sb.AppendLine(string.Format("<td style=\"background-color: #c00000; border-left-style: none; border-right-style: none; border-top-style: none; border-bottom: .75pt solid black\" ><b><font color=\"#FFFFFF\">{0}</font></b></td>", HttpUtility.HtmlEncode(Title)));
                    sb.AppendLine("</tr>");
                    sb.AppendLine("<tr>");
                    sb.AppendLine("<td>");
                    if (!string.IsNullOrEmpty(Align))
                    {
                        sb.AppendLine(string.Format("<div align=\"{0}\">", Align));
                    }
                    sb.AppendLine(string.Format("<table width=\"{0}\">", Width));
                    if (!string.IsNullOrEmpty(AxisLabelX) && string.IsNullOrEmpty(AxisLabelY))
                    {
                        sb.AppendLine(string.Format("<tr><td align=\"center\" colspan=\"{1}\">{0}</td></tr>", HttpUtility.HtmlEncode(AxisLabelX), cols));
                    }
                    else if (!string.IsNullOrEmpty(AxisLabelX) && !string.IsNullOrEmpty(AxisLabelY))
                    {
                        sb.AppendLine(string.Format("<tr><td valign=\"middle\" rowspan=\"{3}\">{2}</td><td align=\"center\" colspan=\"{1}\">{0}</td></tr>", HttpUtility.HtmlEncode(AxisLabelX), cols + 1, HttpUtility.HtmlEncode(AxisLabelY), Rows.Count + 1));
                    }
                    foreach (Row r in Rows)
                    {
                        sb.AppendLine("<tr>");
                        foreach (Item it in r.Items)
                        {
                            sb.AppendLine(string.Format("<td{0}{1}{2}{3}>",
                                string.IsNullOrEmpty(it.Height) ? "" : string.Format(" height=\"{0}\"", it.Height),
                                string.IsNullOrEmpty(it.Width) ? "" : string.Format(" width=\"{0}\"", it.Width),
                                string.IsNullOrEmpty(it.Align) ? "" : string.Format(" align=\"{0}\"", it.Align),
                                it.IsMarker ? " style=\"border-style: none; border-bottom: .75pt solid silver; background-color: #DDDDDD\"" : " style=\"border-style: none; border-bottom: .75pt solid silver\"")
                                );
                            sb.AppendLine(string.Format("{0}{1}{2}", it.IsMarker ? "<b>" : "", it.IsHtml ? it.Text : HttpUtility.HtmlEncode(it.Text), it.IsMarker ? "</b>" : ""));
                            sb.AppendLine("</td>");
                        }
                        sb.AppendLine("</tr>");
                    }
                    sb.AppendLine("</table>");
                    if (!string.IsNullOrEmpty(Align))
                    {
                        sb.AppendLine("</div>");
                    }

                    sb.AppendLine("</td>");
                    sb.AppendLine("</tr>");
                    sb.AppendLine("</table>");
                }

                return sb.ToString();
            }
        }

        public Statistics[] StatisticsBlocks { get; private set; }

        public Layout(int statisticsCount)
        {
            StatisticsBlocks = new Statistics[statisticsCount];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            //this layout:
            //table for whole stats, column for each statsblock
            sb.AppendLine("<table width=\"100%\">");
            sb.AppendLine("<tr valign=\"top\">");
            foreach (Statistics stat in StatisticsBlocks)
            {
                sb.AppendLine(string.Format("<td width=\"{0:0}%\">", 100.0 / (double)StatisticsBlocks.Length));
                sb.AppendLine(stat.ToString());
                sb.AppendLine("</td>");
            }
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<br />");
            return sb.ToString();
        }
    }

    // GOOGLE CHART API

    public string googleChartImgUrl(Dictionary<string, string> pars)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("http://chart.apis.google.com/chart?");
        bool first = true;
        foreach (var di in pars)
        {
            if (!first)
            {
                sb.Append("&");
            }
            else
            {
                first = false;
            }
            sb.AppendFormat("{0}={1}", di.Key, di.Value);
        }
        return sb.ToString();
    }

    // /GOOGLE CHART API

    public string RunInstance(Plugin plugin, ICore core)
    {
        _plugin = plugin;
        _core = core;

        InitExtension();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<table width=\"740px\">");

        sb.AppendLine("</table>");
        return sb.ToString();
    }

    public string DTMatrix()
    {
        string strDiffTerrCombi = "Difficulty / Terrain combination";
        string strDifficulty = "Difficulty";
        string strTerrain = "Terrain";

        RegisterText(new string[]{
		strDiffTerrCombi,
		strDifficulty,
		strTerrain
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strDiffTerrCombi, false));
        skin.StatisticsBlocks[0] = stats;
        stats.Width = "60%";
        stats.Align = "center";
        stats.AxisLabelX = Translate(strTerrain, false);
        stats.AxisLabelY = Translate(strDifficulty, false);

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = " ";

        for (double d = 1; d < 5.1; d += 0.5)
        {
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = d.ToString("0.#");
            row.Items[row.Items.Count - 1].Width = "10%";
            row.Items[row.Items.Count - 1].Align = "center";
            row.Items[row.Items.Count - 1].IsMarker = true;
        }
        for (double d = 1; d < 5.1; d += 0.5)
        {
            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            string sd = d.ToString("0.#");

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = d.ToString("0.#");
            row.Items[row.Items.Count - 1].Width = "10%";
            row.Items[row.Items.Count - 1].Align = "center";
            row.Items[row.Items.Count - 1].IsMarker = true;

            for (double t = 1; t < 5.1; t += 0.5)
            {
                string st = t.ToString("0.#");
                int cnt = (from mf in _myFinds where mf.gc.Difficulty.ToString("0.#") == sd && mf.gc.Terrain.ToString("0.#") == st select mf).Count();

                row.Items.Add(new Layout.Statistics.Item());
                row.Items[row.Items.Count - 1].Text = cnt.ToString();
                row.Items[row.Items.Count - 1].Width = "10%";
                row.Items[row.Items.Count - 1].Align = "center";
            }
        }
        return stats.ToString();
    }


    public string CacheSizeAndType()
    {
        string strCacheSize = "Cache size";
        string strSize = "Size";
        string strFound = "Found";
        string strPercentage = "Percentage";
        string strCacheType = "Cache type";
        string strType = "Type";

        RegisterText(new string[]{
		strCacheSize,
		strSize,
		strFound,
		strPercentage,
		strCacheType,
		strType
		});

        Layout skin = new Layout(2);
        Layout.Statistics stats = new Layout.Statistics(Translate(strCacheSize, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strSize, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        var csizeGroups =
            (from mf in _myFinds
             group mf by mf.gc.Container into g
             select new { Container = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count());
        foreach (var g in csizeGroups)
        {
            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = Translate(g.Container.Name, false);
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = g.Founds.Count().ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)g.Founds.Count() / (double)_myFinds.Count);
        }

        stats = new Layout.Statistics(Translate(strCacheType, false));
        skin.StatisticsBlocks[1] = stats;

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strType, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        var csizeTypes =
            (from mf in _myFinds
             group mf by mf.gc.GeocacheType into g
             select new { GeocacheType = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count());
        foreach (var g in csizeTypes)
        {
            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = Translate(g.GeocacheType.Name, false);
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = g.Founds.Count().ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)g.Founds.Count() / (double)_myFinds.Count);
        }
        return skin.ToString();
    }


    private string locationsTable()
    {
        string strLocations = "Locations";
        string strStateProv = "States/Provinces";
        string strCountries = "Countries";

        RegisterText(new string[]{
		strLocations,
		strStateProv,
		strCountries
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strLocations, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        bool first = true;
        var states =
            (from mf in _myFinds
             where !string.IsNullOrEmpty(mf.gc.State)
             group mf by mf.gc.State into g
             select new { State = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count()).OrderBy(x => x.State);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{1} ({0})", states.Count(), Translate(strStateProv, false));
        row.Items[row.Items.Count - 1].IsMarker = true;


        StringBuilder sb = new StringBuilder();
        foreach (var g in states)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            else
            {
                first = false;
            }
            sb.Append(string.Format("{0} ({1})", ToHtml(g.State), g.Founds.Count()));
        }
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = sb.ToString();
        row.Items[row.Items.Count - 1].IsHtml = true;

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        first = true;
        var countries =
            (from mf in _myFinds
             where !string.IsNullOrEmpty(mf.gc.Country)
             group mf by mf.gc.Country into g
             select new { Country = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count()).OrderBy(x => x.Country);
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{1} ({0})", countries.Count(), Translate(strCountries, false));
        row.Items[row.Items.Count - 1].IsMarker = true;

        sb.Length = 0;
        foreach (var g in countries)
        {
            if (!first)
            {
                sb.Append(", ");
            }
            else
            {
                first = false;
            }
            sb.Append(string.Format("{0} ({1})", ToHtml(g.Country), g.Founds.Count()));
        }
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = sb.ToString();
        row.Items[row.Items.Count - 1].IsHtml = true;

        return skin.ToString();
    }

    private string worldMap66()
    {
        StringBuilder sb = new StringBuilder();

        var countries =
            (from mf in _myFinds
             where !string.IsNullOrEmpty(mf.gc.Country)
             select mf.gc.Country).Distinct();
        sb.Append("<p><IMG src=\"http://www.world66.com/myworld66/visitedCountries/worldmap?visited=");
        foreach (var g in countries)
        {
            //EUROPE
            if (g == "Albania") sb.Append("AL");
            else if (g == "Andorra") sb.Append("AD");
            else if (g == "Armenia") sb.Append("AM");
            else if (g == "Austria") sb.Append("AT");
            else if (g == "Azerbaijan") sb.Append("AZ");
            else if (g == "Belarus") sb.Append("BY");
            else if (g == "Belgium") sb.Append("BE");
            else if (g == "Bosnia Herzogovina") sb.Append("BA");
            else if (g == "Bulgaria") sb.Append("BG");
            else if (g == "Channel Islands") sb.Append("QI");
            else if (g == "Croatia") sb.Append("HR");
            else if (g == "Cyprus") sb.Append("CY");
            else if (g == "Czech Republic") sb.Append("CZ");
            else if (g == "Denmark") sb.Append("DK");
            else if (g == "England") sb.Append("EN");
            else if (g == "United Kingdom") sb.Append("UK");
            else if (g == "Estonia") sb.Append("EE");
            else if (g == "Finland") sb.Append("FI");
            else if (g == "Faroe Islands") sb.Append("FO");
            else if (g == "France") sb.Append("FR");
            else if (g == "Georgia") sb.Append("GE");
            else if (g == "Germany") sb.Append("DE");
            else if (g == "Gibraltar") sb.Append("GI");
            else if (g == "Greece") sb.Append("GR");
            else if (g == "Hungary") sb.Append("HU");
            else if (g == "Iceland") sb.Append("IS");
            else if (g == "Ireland") sb.Append("IE");
            else if (g == "Italy") sb.Append("IT");
            else if (g == "Latvia") sb.Append("LV");
            else if (g == "Liechtenstein") sb.Append("LI");
            else if (g == "Lithuania") sb.Append("LT");
            else if (g == "Luxembourg") sb.Append("LU");
            else if (g == "Macedonia") sb.Append("MK");
            else if (g == "Malta") sb.Append("MT");
            else if (g == "Moldova") sb.Append("MD");
            else if (g == "Monaco") sb.Append("MC");
            else if (g == "Netherlands") sb.Append("NL");
            else if (g == "Northern Ireland") sb.Append("NI");
            else if (g == "Norway") sb.Append("NO");
            else if (g == "Poland") sb.Append("PL");
            else if (g == "Portugal") sb.Append("PT");
            else if (g == "Romania") sb.Append("RO");
            else if (g == "Russia") sb.Append("RU");
            else if (g == "Scotland") sb.Append("SC");
            else if (g == "San Marino") sb.Append("SM");
            else if (g == "Serbia and Montenegro") sb.Append("YU");
            else if (g == "Slovenia") sb.Append("SI");
            else if (g == "Slovakia") sb.Append("SK");
            else if (g == "Spain") sb.Append("ES");
            else if (g == "Sweden") sb.Append("SE");
            else if (g == "Switzerland") sb.Append("CH");
            else if (g == "Turkey") sb.Append("TU");
            else if (g == "Ukraine") sb.Append("UA");
            else if (g == "Vatican City") sb.Append("VA");
            else if (g == "Wales") sb.Append("WA");

            //NORTH AMERICA
            else if (g == "Canada") sb.Append("CA");
            else if (g == "Greenland") sb.Append("GL");
            else if (g == "United States") sb.Append("US");
            else if (g == "Mexico") sb.Append("MX");

            //Central America and the Caribbean
            else if (g == "Anguilla") sb.Append("AI");
            else if (g == "Antigua and Barbuda") sb.Append("AG");
            else if (g == "Aruba") sb.Append("AW");
            else if (g == "Barbados") sb.Append("BB");
            else if (g == "Bahamas") sb.Append("BS");
            else if (g == "Belize") sb.Append("BZ");
            else if (g == "Bermuda") sb.Append("UV");
            else if (g == "British Virgin Islands") sb.Append("BM");
            else if (g == "Cayman Islands") sb.Append("CQ");
            else if (g == "Costa Rica") sb.Append("CR");
            else if (g == "Cuba") sb.Append("CU");
            else if (g == "Dominica") sb.Append("DM");
            else if (g == "Dominican Republic") sb.Append("DO");
            else if (g == "El Salvador") sb.Append("SV");
            else if (g == "Guadeloupe") sb.Append("GP");
            else if (g == "Guatemala") sb.Append("GT");
            else if (g == "Grenada") sb.Append("GD");
            else if (g == "Haiti") sb.Append("HT");
            else if (g == "Honduras") sb.Append("HN");
            else if (g == "Jamaica") sb.Append("JM");
            else if (g == "Martinique") sb.Append("MQ");
            else if (g == "Monserrat") sb.Append("MS");
            else if (g == "Netherlands Antilles") sb.Append("AN");
            else if (g == "Nicaragua") sb.Append("NI");
            else if (g == "Panama") sb.Append("PA");
            else if (g == "Puerto Rico") sb.Append("PR");
            else if (g == "Saint Kitts and Nevis") sb.Append("KK");
            else if (g == "Saint Lucia") sb.Append("LC");
            else if (g == "Saint Vincent and the Grenadines") sb.Append("VC");
            else if (g == "Turks and Caicos Islands") sb.Append("TQ");
            else if (g == "Trinidad and Tobago") sb.Append("TT");
            else if (g == "Virgin Islands") sb.Append("VI");

            //South America
            else if (g == "Argentina") sb.Append("AR");
            else if (g == "Bolivia") sb.Append("BO");
            else if (g == "Brazil") sb.Append("BR");
            else if (g == "Chile") sb.Append("CL");
            else if (g == "Colombia") sb.Append("CO");
            else if (g == "Ecuador") sb.Append("EC");
            else if (g == "Falkland Islands") sb.Append("FK");
            else if (g == "French Guiana") sb.Append("GF");
            else if (g == "Guyana") sb.Append("GY");
            else if (g == "Paraguay") sb.Append("PY");
            else if (g == "Peru") sb.Append("PE");
            else if (g == "Suriname") sb.Append("SR");
            else if (g == "Uruguay") sb.Append("UY");
            else if (g == "Venezuela") sb.Append("VE");

            //Africa
            else if (g == "Algeria") sb.Append("DZ");
            else if (g == "Angola") sb.Append("AO");
            else if (g == "Benin") sb.Append("BJ");
            else if (g == "Botswana") sb.Append("BW");
            else if (g == "Burkina Faso") sb.Append("BF");
            else if (g == "Burundi") sb.Append("BI");
            else if (g == "Cameroon") sb.Append("CM");
            else if (g == "Cape Verde") sb.Append("CV");
            else if (g == "Central African Republic") sb.Append("CF");
            else if (g == "Chad") sb.Append("TD");
            else if (g == "Comoros") sb.Append("KM");
            else if (g == "Congo Brazzaville") sb.Append("CG");
            else if (g == "Congo Kinshasa") sb.Append("CD");
            else if (g == "Djibouti") sb.Append("DJ");
            else if (g == "Egypt") sb.Append("EG");
            else if (g == "Equatorial Guinea") sb.Append("GQ");
            else if (g == "Eritrea") sb.Append("ER");
            else if (g == "Ethiopia") sb.Append("ET");
            else if (g == "Gabon") sb.Append("GA");
            else if (g == "Gambia") sb.Append("GM");
            else if (g == "Ghana") sb.Append("GH");
            else if (g == "Guinea-Bissau") sb.Append("GW");
            else if (g == "Guinee Conakry") sb.Append("GN");
            else if (g == "Ivory Coast") sb.Append("CI");
            else if (g == "Kenya") sb.Append("KE");
            else if (g == "Lesotho") sb.Append("LS");
            else if (g == "Liberia") sb.Append("LR");
            else if (g == "Libya") sb.Append("LY");
            else if (g == "Madagascar") sb.Append("MG");
            else if (g == "Malawi") sb.Append("MW");
            else if (g == "Mali") sb.Append("ML");
            else if (g == "Mauritania") sb.Append("MR");
            else if (g == "Mauritius") sb.Append("MU");
            else if (g == "Morocco") sb.Append("MA");
            else if (g == "Mozambique") sb.Append("MZ");
            else if (g == "Namibia") sb.Append("NA");
            else if (g == "Niger") sb.Append("NE");
            else if (g == "Nigeria") sb.Append("NG");
            else if (g == "Reunion") sb.Append("RE");
            else if (g == "Rwanda") sb.Append("RW");
            else if (g == "Sao Tome and Principe") sb.Append("ST");
            else if (g == "Senegal") sb.Append("SN");
            else if (g == "Seychelles") sb.Append("SC");
            else if (g == "Sierra Leone") sb.Append("SL");
            else if (g == "Somalia") sb.Append("SO");
            else if (g == "South Africa") sb.Append("ZA");
            else if (g == "Sudan") sb.Append("SD");
            else if (g == "Swaziland") sb.Append("SZ");
            else if (g == "Tanzania") sb.Append("TZ");
            else if (g == "Togo") sb.Append("TG");
            else if (g == "Tunisia") sb.Append("TN");
            else if (g == "Uganda") sb.Append("UG");
            else if (g == "Western Sahara") sb.Append("EH");
            else if (g == "Zambia") sb.Append("ZM");
            else if (g == "Zimbabwe") sb.Append("ZW");

            //the Middle East
            else if (g == "Bahrain") sb.Append("BH");
            else if (g == "Cyprus") sb.Append("CY");
            else if (g == "Iran") sb.Append("IR");
            else if (g == "Iraq") sb.Append("IQ");
            else if (g == "Israel") sb.Append("IL");
            else if (g == "Jordan") sb.Append("JO");
            else if (g == "Kuwait") sb.Append("KW");
            else if (g == "Lebanon") sb.Append("LB");
            else if (g == "Oman") sb.Append("OM");
            else if (g == "Palestinian Authority") sb.Append("PQ");
            else if (g == "Qatar") sb.Append("QA");
            else if (g == "Saudi Arabia") sb.Append("SA");
            else if (g == "Syria") sb.Append("SY");
            else if (g == "Turkey") sb.Append("TR");
            else if (g == "United Arab Emirates") sb.Append("AE");
            else if (g == "Yemen") sb.Append("YE");

            //Asia
            else if (g == "Afghanistan") sb.Append("AF");
            else if (g == "Bangladesh") sb.Append("BD");
            else if (g == "Bhutan") sb.Append("BT");
            else if (g == "Brunei") sb.Append("BN");
            else if (g == "Cambodia") sb.Append("KH");
            else if (g == "China") sb.Append("CN");
            else if (g == "East Timor") sb.Append("TP");
            else if (g == "India") sb.Append("IN");
            else if (g == "Indonesia") sb.Append("ID");
            else if (g == "Japan") sb.Append("JP");
            else if (g == "Kazakhstan") sb.Append("KZ");
            else if (g == "Kyrgyzstan") sb.Append("KG");
            else if (g == "Laos") sb.Append("LA");
            else if (g == "Malaysia") sb.Append("MY");
            else if (g == "Maldives") sb.Append("MV");
            else if (g == "Mongolia") sb.Append("MN");
            else if (g == "Myanmar") sb.Append("MM");
            else if (g == "Nepal") sb.Append("NP");
            else if (g == "North Korea") sb.Append("KP");
            else if (g == "Pakistan") sb.Append("PK");
            else if (g == "Philippines") sb.Append("PH");
            else if (g == "Singapore") sb.Append("SG");
            else if (g == "Sri Lanka") sb.Append("LK");
            else if (g == "South Korea") sb.Append("KR");
            else if (g == "Taiwan") sb.Append("TW");
            else if (g == "Tajikistan") sb.Append("TJ");
            else if (g == "Thailand") sb.Append("TH");
            else if (g == "Turkmenistan") sb.Append("TM");
            else if (g == "Uzbekistan") sb.Append("UZ");
            else if (g == "Vietnam") sb.Append("VN");

            //Australia and Pacific
            else if (g == "American Samoa") sb.Append("AS");
            else if (g == "Australia") sb.Append("AU");
            else if (g == "Fiji") sb.Append("FJ");
            else if (g == "French Polynesia") sb.Append("PF");
            else if (g == "Guam") sb.Append("GU");
            else if (g == "Kiribati") sb.Append("KI");
            else if (g == "Marshall Islands") sb.Append("MH");
            else if (g == "Micronesia, Federated States of") sb.Append("NR");
            else if (g == "Nauru") sb.Append("FM");
            else if (g == "New Caledonia") sb.Append("NC");
            else if (g == "New Zealand") sb.Append("NZ");
            else if (g == "Niue") sb.Append("NU");
            else if (g == "Norfolk Island") sb.Append("NF");
            else if (g == "Northern Mariana Islands") sb.Append("MP");
            else if (g == "Palau") sb.Append("PW");
            else if (g == "Papua New Guinea") sb.Append("PG");
            else if (g == "Pitcairn Islands") sb.Append("PN");
            else if (g == "Rarotonga &amp; the Cook Islands") sb.Append("CK");
            else if (g == "Solomon Islands") sb.Append("SB");
            else if (g == "Tonga") sb.Append("TO");
            else if (g == "Tuvalu") sb.Append("TV");
            else if (g == "Vanuatu") sb.Append("VU");
            else if (g == "Western Samoa") sb.Append("WS");
        }
        sb.AppendLine("\" /><br><i>(<a href=\"http://www.world66.com/myworld66\">map from world66.com</a>)</i></p>");

        sb.AppendLine("<br />");
        return sb.ToString();
    }

    private string europeMap66()
    {
        StringBuilder sb = new StringBuilder();

        var countries =
            (from mf in _myFinds
             where !string.IsNullOrEmpty(mf.gc.Country)
             select mf.gc.Country).Distinct();
        sb.AppendFormat("<p><IMG src=\"http://www.world66.com/myworld66/visitedEurope/countrymap?visited={0}", sb.ToString());
        foreach (var g in countries)
        {
            if (g == "Albania") sb.Append("AL");
            else if (g == "Andorra") sb.Append("AN");
            else if (g == "Armenia") sb.Append("AR");
            else if (g == "Austria") sb.Append("AU");
            else if (g == "Azerbaijan") sb.Append("AZ");
            else if (g == "Belarus") sb.Append("BL");
            else if (g == "Belgium") sb.Append("BE");
            else if (g == "Bosnia Herzogovina") sb.Append("BH");
            else if (g == "Bulgaria") sb.Append("BU");
            else if (g == "Croatia") sb.Append("CR");
            else if (g == "Cyprus") sb.Append("CY");
            else if (g == "Czech Republic") sb.Append("CZ");
            else if (g == "Denmark") sb.Append("DK");
            else if (g == "England") sb.Append("EN");
            else if (g == "United Kingdom") sb.Append("EN");
            else if (g == "Estonia") sb.Append("ES");
            else if (g == "Finland") sb.Append("FI");
            else if (g == "France") sb.Append("FR");
            else if (g == "Georgia") sb.Append("GG");
            else if (g == "Germany") sb.Append("GE");
            else if (g == "Greece") sb.Append("GR");
            else if (g == "Hungary") sb.Append("HU");
            else if (g == "Iceland") sb.Append("IC");
            else if (g == "Ireland") sb.Append("IE");
            else if (g == "Italy") sb.Append("IT");
            else if (g == "Latvia") sb.Append("LE");
            else if (g == "Liechtenstein") sb.Append("LT");
            else if (g == "Lithuania") sb.Append("LI");
            else if (g == "Luxembourg") sb.Append("LU");
            else if (g == "Macedonia") sb.Append("MA");
            else if (g == "Malta") sb.Append("ML");
            else if (g == "Moldova") sb.Append("MO");
            else if (g == "Monaco") sb.Append("MC");
            else if (g == "Netherlands") sb.Append("NL");
            else if (g == "Northern Ireland") sb.Append("NI");
            else if (g == "Norway") sb.Append("NO");
            else if (g == "Poland") sb.Append("PO");
            else if (g == "Portugal") sb.Append("PT");
            else if (g == "Romania") sb.Append("RO");
            else if (g == "Russia") sb.Append("RU");
            else if (g == "Scotland") sb.Append("SC");
            else if (g == "San Marino") sb.Append("SA");
            else if (g == "Serbia and Montenegro") sb.Append("SM");
            else if (g == "Slovenia") sb.Append("SL");
            else if (g == "Slovakia") sb.Append("SV");
            else if (g == "Spain") sb.Append("SP");
            else if (g == "Sweden") sb.Append("SE");
            else if (g == "Switzerland") sb.Append("SW");
            else if (g == "Turkey") sb.Append("TU");
            else if (g == "Ukraine") sb.Append("UK");
            else if (g == "Vatican City") sb.Append("VC");
            else if (g == "Wales") sb.Append("WA");
        }
        sb.AppendLine("\" /><br><i>(<a href=\"http://www.world66.com/myworld66\">map from world66.com</a>)</i></p>");

        sb.AppendLine("<br />");
        return sb.ToString();
    }


    private string diffTerr()
    {
        string strDifficulty = "Difficulty";
        string strTerrain = "Terrain";
        string strFound = "Found";
        string strPercentage = "Percentage";

        RegisterText(new string[]{
		strDifficulty,
		strFound,
		strPercentage,
		strTerrain
		});

        Layout skin = new Layout(2);
        Layout.Statistics stats = new Layout.Statistics(Translate(strDifficulty, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strDifficulty, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        double tot = (double)_myFinds.Count;
        for (double d = 1.0; d < 5.1; d += 0.5)
        {
            string sd = d.ToString("0.#");
            int cnt = (from mf in _myFinds where mf.gc.Difficulty.ToString("0.#") == sd select mf).Count();

            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = sd;
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = cnt.ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)cnt / tot);
        }

        stats = new Layout.Statistics(Translate(strTerrain, false));
        skin.StatisticsBlocks[1] = stats;

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strTerrain, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        for (double d = 1.0; d < 5.1; d += 0.5)
        {
            string sd = d.ToString("0.#");
            int cnt = (from mf in _myFinds where mf.gc.Terrain.ToString("0.#") == sd select mf).Count();

            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = sd;
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = cnt.ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)cnt / tot);
        }
        return skin.ToString();
    }

    private string mileStones()
    {
        int[] milestones = new int[] { 1, 100, 200, 300, 400, 500, 1000 };
        int every = 500;

        //test
        //int[] milestones = new int[] {1, 10, 20, 30, 40, 50, 100 };
        //int every = 50;

        string strMilestones = "Milestones";
        string strNumber = "Number";
        string strDate = "Date";
        string strCache = "Cache";
        string strDaysInBetween = "Days in between";

        RegisterText(new string[]{
		strMilestones,
		strNumber,
		strDate,
		strCache,
		strDaysInBetween
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strMilestones, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strNumber, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strDate, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strCache, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strDaysInBetween, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        int milestonesIndex = 0;
        int index = milestones[milestonesIndex] - 1;
        MyGeocacheFind prev = null;
        while (index < _myFinds.Count)
        {
            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = (index + 1).ToString();
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = _myFinds[index].logDate.ToString("d");
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = _myFinds[index].gc.Name ?? "";
            row.Items.Add(new Layout.Statistics.Item());
            if (prev == null)
            {
                row.Items[row.Items.Count - 1].Text = " ";
            }
            else
            {
                row.Items[row.Items.Count - 1].Text = (_myFinds[index].logDate - prev.logDate).TotalDays.ToString("0");
            }

            prev = _myFinds[index];
            if (milestonesIndex < milestones.Length - 1)
            {
                milestonesIndex++;
                index = milestones[milestonesIndex] - 1;
            }
            else
            {
                index += every;
            }
        }
        return skin.ToString();
    }

    private string foundCaches()
    {
        string strFoundCaches = "Found caches";
        string strTotalFound = "Total found";
        string strFound = "Found";
        string strAverage = "Average";
        string strPerDay = "per day";
        string strPerWeek = "per week";
        string strPerYear = "per year";
        string strArchived = "Archived";
        string strAvgDiff = "Avg. Diff. / Terr.";
        string strAvgOnOneDay = "Avg. on 1 day";
        string strOldestCache = "Oldest cache";
        string strPublishedOn = "published on";
        string strMostOnOneDay = "Most on 1 day";
        string strMostOnOneMonth = "Most on 1 month";

        RegisterText(new string[]{
		strFoundCaches,
		strTotalFound,
		strFound,
		strAverage,
		strPerDay,
		strPerWeek,
		strPerYear,
		strArchived,
		strAvgDiff,
		strAvgOnOneDay,
		strOldestCache,
		strPublishedOn,
		strMostOnOneDay,
		strMostOnOneMonth
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strFoundCaches, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        DateTime minDate = _myFinds.Min(x => x.logDate);
        DateTime maxDate = _myFinds.Max(x => x.logDate);
        TimeSpan ts = (maxDate - minDate);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strTotalFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items[row.Items.Count - 1].Width = "15%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} # {1}", _myFinds.Count, Translate(strFound, false));
        row.Items[row.Items.Count - 1].Width = "35%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strAverage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items[row.Items.Count - 1].Width = "20%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0:0.00} {3} ({1:0.0} {4}, {2:0.0} {5})", (double)_myFinds.Count / ts.TotalDays, 7.0 * (double)_myFinds.Count / ts.TotalDays, 365.0 * (double)_myFinds.Count / ts.TotalDays, Translate(strPerDay, false), Translate(strPerWeek, false), Translate(strPerYear, false));
        row.Items[row.Items.Count - 1].Width = "30%";

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        int arch = (from mf in _myFinds where mf.gc.Archived select mf).Count();

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strArchived, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} ({1:0.0}%)", arch, 100.0 * (double)arch / (double)_myFinds.Count);
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strAvgDiff, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0:0.00} / {1:0.00}", _myFinds.Average(x => x.gc.Difficulty), _myFinds.Average(x => x.gc.Terrain));

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        int uniqueDays = (from mf in _myFinds select mf.logDate.ToString("d")).Distinct().Count();
        DateTime minPub = _myFinds.Min(x => x.gc.PublishedTime);
        Geocache gc = (from mf in _myFinds where mf.gc.PublishedTime == minPub select mf.gc).FirstOrDefault();

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strAvgOnOneDay, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0:0.00}", (double)_myFinds.Count / (double)uniqueDays);
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strOldestCache, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("<a href=\"{1}\" target=\"_blank\">{0}</a> (<span style=\"font-size:75%\">{2} {4} {3})</span>", ToHtml(gc.Name), gc.Url, ToHtml(gc.Code), ToHtml(gc.PublishedTime.ToString("d")), Translate(strPublishedOn));
        row.Items[row.Items.Count - 1].IsHtml = true;

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);
        
        var cdayGroup =
            (from mf in _myFinds
             group mf by mf.logDate.ToString("d") into g
             select new { Day = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count()).FirstOrDefault();
        var cmonthGroup =
            (from mf in _myFinds
             group mf by mf.logDate.ToString("yyyy-MM") into g
             select new { Month = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count()).FirstOrDefault();

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strMostOnOneDay, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} (<span style=\"font-size:75%\">{1}</span>)", cdayGroup.Founds.Count(), ToHtml(cdayGroup.Day));
        row.Items[row.Items.Count - 1].IsHtml = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strMostOnOneMonth, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} (<span style=\"font-size:75%\">{1}</span>)", cmonthGroup.Founds.Count(), ToHtml(cmonthGroup.Month));
        row.Items[row.Items.Count - 1].IsHtml = true;

        return stats.ToString();
    }


    private string daysCached()
    {
        string strDaysCached = "Days cached";
        string strEvery = "every";
        string strDaysOr = "days or";
        string strMostConsec = "Most consecutive days with founds";
        string strMostOneMonth = "Most in 1 month";
        string strMostConsecW = "Most consecutive days without founds";

        RegisterText(new string[]{
		strDaysCached,
		strEvery,
		strDaysOr,
		strMostConsec,
		strMostOneMonth,
		strMostConsecW
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strDaysCached, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        int uniqueDays = (from mf in _myFinds select mf.logDate.ToString("d")).Distinct().Count();
        DateTime minDate = _myFinds.Min(x => x.logDate);
        DateTime maxDate = _myFinds.Max(x => x.logDate);
        TimeSpan ts = (maxDate - minDate);

        DateTime startInterval = minDate;
        DateTime endInterval = minDate;
        DateTime startIntervalMarker = minDate;
        DateTime endIntervalMarker = minDate;
        for (int i = 1; i < _myFinds.Count; i++)
        {
            string send = endInterval.ToString("d");
            string slg = _myFinds[i].logDate.ToString("d");
            if (slg == send)
            {
                //same day
            }
            else if (endInterval.AddDays(1).ToString("d") == slg)
            {
                //next day
                endInterval = _myFinds[i].logDate;
            }
            else
            {
                //interval determined
                if ((int)(endIntervalMarker - startIntervalMarker).TotalDays < (int)(endInterval - startInterval).TotalDays)
                {
                    //new record
                    startIntervalMarker = startInterval;
                    endIntervalMarker = endInterval;
                }
                startInterval = _myFinds[i].logDate;
                endInterval = _myFinds[i].logDate;
            }
        }

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strDaysCached, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items[row.Items.Count - 1].Width = "15%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} ({3} {1:0.0} {4} {2:0.0}%)", uniqueDays, ts.TotalDays / (double)uniqueDays, 100.0 * (double)uniqueDays / ts.TotalDays, Translate(strEvery, false), Translate(strDaysOr, false));
        row.Items[row.Items.Count - 1].Width = "35%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strMostConsec, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items[row.Items.Count - 1].Width = "20%";
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} (<span style=\"font-size:75%\">{1} - {2}</span>)", (int)(endIntervalMarker - startIntervalMarker).TotalDays + 1, startIntervalMarker.ToString("d"), endIntervalMarker.ToString("d"));
        row.Items[row.Items.Count - 1].IsHtml = true;
        row.Items[row.Items.Count - 1].Width = "30%";

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        startInterval = minDate;
        endInterval = minDate;
        DateTime prevFoundDate = minDate;
        startIntervalMarker = minDate;
        endIntervalMarker = minDate;
        for (int i = 1; i < _myFinds.Count; i++)
        {
            string send = prevFoundDate.ToString("d");
            string slg = _myFinds[i].logDate.ToString("d");
            if (slg == send)
            {
                //same day
            }
            else if (prevFoundDate.AddDays(1).ToString("d") == slg)
            {
                //next day
            }
            else
            {
                //start of new gap
                startInterval = _myFinds[i - 1].logDate.AddDays(1);
                endInterval = _myFinds[i].logDate.AddDays(-1);

                //interval determined
                if ((int)(endIntervalMarker - startIntervalMarker).TotalDays < (int)(endInterval - startInterval).TotalDays)
                {
                    //new record
                    startIntervalMarker = startInterval;
                    endIntervalMarker = endInterval;
                }
                prevFoundDate = _myFinds[i].logDate;
            }
        }

        string[] udays = (from mf in _myFinds select mf.logDate.ToString("yyyy-MM-dd")).Distinct().ToArray();
        var cdayGroup =
            (from mf in udays
             group mf by mf.Substring(0, 7) into g
             select new { Month = g.Key, Days = g }).OrderByDescending(x => x.Days.Count()).FirstOrDefault();

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strMostOneMonth, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} (<span style=\"font-size:75%\">{1}</span>)", cdayGroup.Days.Count(), cdayGroup.Month);
        row.Items[row.Items.Count - 1].IsHtml = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strMostConsecW, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("{0} (<span style=\"font-size:75%\">{1} - {2}</span>)", (int)(endIntervalMarker - startIntervalMarker).TotalDays + 1, startIntervalMarker.ToString("d"), endIntervalMarker.ToString("d"));
        row.Items[row.Items.Count - 1].IsHtml = true;

        return stats.ToString();
    }

    private string history()
    {
        string strHistory = "History";
        string strYear = "Year";
        string strTotalFound = "Total found";
        string strDaysCachedFreq = "Days cached / frequency";
        string strCachesPerDay = "Caches/day";
        string strEvery = "Every";
        string strDays = "days";

        RegisterText(new string[]{
		strHistory,
		strYear,
		strTotalFound,
		strDaysCachedFreq,
		strCachesPerDay,
		strEvery,
		strDays
		});


        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strHistory, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strYear, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strTotalFound, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strCachesPerDay, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strDaysCachedFreq, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        DateTime startAt = (from mf in _myFinds where mf.logDate.Year >= 2000 orderby mf.logDate.Year select mf.logDate).FirstOrDefault();
        int startYear = startAt.Year;
        if (startYear >= 2000)
        {
            for (int y = startYear; y <= DateTime.Now.Year; y++)
            {
                int cnt = (from mf in _myFinds where mf.logDate.Year == y select mf).Count();
                int diny;
                DateTime start = new DateTime(y, 1, 1);
                DateTime end;
                if (y == startYear)
                {
                    start = startAt;
                }
                else
                {
                    start = new DateTime(y, 1, 1);
                }
                if (y == DateTime.Now.Year)
                {
                    end = DateTime.Now;
                }
                else
                {
                    end = new DateTime(y, 12, 31);
                }
                diny = (int)(end - start).TotalDays;
                if (diny == 0) diny++;

                int udays = (from mf in _myFinds where mf.logDate >= start && mf.logDate <= end select mf.logDate.ToString("d")).Distinct().Count();

                row = new Layout.Statistics.Row();
                stats.Rows.Add(row);

                row.Items.Add(new Layout.Statistics.Item());
                row.Items[row.Items.Count - 1].Text = y.ToString();
                row.Items[row.Items.Count - 1].IsMarker = true;
                row.Items.Add(new Layout.Statistics.Item());
                row.Items[row.Items.Count - 1].Text = cnt.ToString();
                row.Items.Add(new Layout.Statistics.Item());
                row.Items[row.Items.Count - 1].Text = ((double)cnt / (double)diny).ToString("0.00");
                row.Items.Add(new Layout.Statistics.Item());
                if (udays == 0)
                {
                    row.Items[row.Items.Count - 1].Text = udays.ToString();
                }
                else
                {
                    row.Items[row.Items.Count - 1].Text = string.Format("{0} / {2} {1:0.0} {3}", udays, (double)diny / (double)udays, Translate(strEvery, false), Translate(strDays, false));
                }
            }
        }

        return stats.ToString();
    }

    private string totalFoundsPerMonthGraph()
    {
        string strTotalPerMonth = "Total founds per month";

        RegisterText(new string[]{
		strTotalPerMonth
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strTotalPerMonth, false));
        skin.StatisticsBlocks[0] = stats;
        stats.Rows.Add(new Layout.Statistics.Row());
        stats.Rows[0].Items.Add(new Layout.Statistics.Item());

        //graph
        //http://chart.apis.google.com/chart?cht=lxy&chs=740x300&chf=bg,s,FFF4F4&chxt=r,x,y&chxr=0,0,2693|2,0,1612&chxs=0,0000FF|2,FF0000&chd=e:__,BIDKE5GPG3KGMPPnS3YKbcfxiijhl8potKxB0V82.-,AADMGZJmMz,B5FRILKcLe,MzP.TMWZZmcyf.jMmZplsyv.zM,AAFZI-OmUCc5iXpmuPv3z66E..,zM2Y5l8y..,AAGbL.aMfb&chxl=1:|2010|||||2011||||||||||||2012|||&chco=0000ff,ff0000,ff0000,ff0000&chm=B,76A4FBB0,0,0,0|v,6060FF,0,4,1.0|v,6060FF,0,16,1.0&chg=0,6.2&chls=2,1,0|2,2,4|2,2,4|2,2,4
        Dictionary<string, string> pars = new Dictionary<string, string>();

        //fixed (data independant)
        pars.Add("cht", "ls");
        pars.Add("chs", "740x300");
        pars.Add("chf", "bg,s,FFF4F4");
        pars.Add("chxt", "r,x,y");
        pars.Add("chxs", "0,0000FF|2,FF0000");
        pars.Add("chco", "0000ff,ff0000,ff0000,ff0000");
        pars.Add("chm", "B,76A4FBB0,0,0,0|v,6060FF,0,4,1.0|v,6060FF,0,16,1.0");
        pars.Add("chg", "0,6.2");
        pars.Add("chls", "2,1,0|2,2,4|2,2,4|2,2,4");
        pars.Add("chds", "a");
        //pars.Add("chdl", "Total found|Yearly found");

        //variable, data dependant
        //pars.Add("chxr", "");
        pars.Add("chd", "");
        pars.Add("chxl", "");

        DateTime startAt = (from mf in _myFinds where mf.logDate.Year >= 2000 orderby mf.logDate.Year select mf.logDate).FirstOrDefault();
        if (startAt.Year >= 2000)
        {
            startAt = new DateTime(startAt.Year, startAt.Month, 1);
            DateTime endAt = startAt.AddMonths(1);
            DateTime startOfInterval = startAt;
            StringBuilder chxl = new StringBuilder();
            StringBuilder serie1 = new StringBuilder();
            StringBuilder serie2 = new StringBuilder();

            while (startAt <= DateTime.Now)
            {
                if (chxl.Length == 0 || startAt.Month == 1)
                {
                    chxl.AppendFormat("|{0}", startAt.Year);
                    startOfInterval = startAt;
                }
                else
                {
                    chxl.Append("|");
                }
                if (serie1.Length == 0)
                {
                    serie1.AppendFormat("{0}", (from mf in _myFinds where mf.logDate < endAt select mf).Count());
                    serie2.AppendFormat("{0}", (from mf in _myFinds where mf.logDate >= startOfInterval && mf.logDate < endAt select mf).Count());
                }
                else
                {
                    serie1.AppendFormat(",{0}", (from mf in _myFinds where mf.logDate < endAt select mf).Count());
                    serie2.AppendFormat(",{0}", (from mf in _myFinds where mf.logDate >= startOfInterval && mf.logDate < endAt select mf).Count());
                }


                startAt = endAt;
                endAt = endAt.AddMonths(1);
            }
            //pars["chd"] = string.Format("t:{0}|{1}", serie1.ToString(), serie2.ToString());
            pars["chd"] = string.Format("t:{0}", serie1.ToString());
            pars["chxl"] = string.Format("1:{0}", chxl.ToString());
            //pars["chxr"] = _myFinds.Count.ToString();
        }

        stats.Rows[0].Items[0].Text = googleChartImgUrl(pars);
        stats.Rows[0].Items[0].IsHtml = true;

        return stats.ToString();
    }

    private string diffTerrPie()
    {
        string strFoundsPerDifficulty = "Founds per difficulty";
        string strFoundsPerTerrain = "Founds per terrain";

        RegisterText(new string[]{
		strFoundsPerDifficulty,
		strFoundsPerTerrain
		});

        Layout skin = new Layout(2);
        Layout.Statistics stats = new Layout.Statistics(Translate(strFoundsPerDifficulty, false));
        skin.StatisticsBlocks[0] = stats;
        stats.Rows.Add(new Layout.Statistics.Row());
        stats.Rows[0].Items.Add(new Layout.Statistics.Item());

        //graph Difficulty
        //http://chart.apis.google.com/chart?cht=p3&amp;chs=370x120&amp;chf=bg,s,FFF4F4&amp;chd=t:18.4,46.9,24.0,4.93,3.60,0.85,0.77,0.25,0.14&amp;chl=1%20(18.4%)|1.5%20(46.9%)|2%20(24.0%)|2.5%20(4.93%)|3%20(3.60%)|3.5%20(0.85%)|4%20(0.77%)|4.5%20(0.25%)|5%20(0.14%)&amp;chco=0000f0,ff0000,8080f0,2020f0,8080f0,2020f0,8080f0,2020f0,8080f0
        Dictionary<string, string> pars = new Dictionary<string, string>();
        string[] chco = new string[] { "8080f0", "2020f0", "8080f0", "2020f0", "8080f0", "2020f0", "8080f0", "2020f0", "8080f0" };

        pars.Add("cht", "p3");
        pars.Add("chs", "370x120");
        pars.Add("chf", "bg,s,FFF4F4");
        //pars.Add("chco", "0000f0,ff0000,8080f0,2020f0,8080f0,2020f0,8080f0,2020f0,8080f0");
        pars.Add("chd", "");
        pars.Add("chl", "");
        pars.Add("chco", "");

        StringBuilder chd = new StringBuilder();
        StringBuilder chl = new StringBuilder();
        double tot = (double)_myFinds.Count;
        int maxCnt = 0;
        int maxCntIndex = 0;
        int index = 0;
        for (double d = 1.0; d < 5.1; d += 0.5)
        {
            string sd = d.ToString("0.#");
            int cnt = (from mf in _myFinds where mf.gc.Difficulty.ToString("0.#") == sd select mf).Count();
            if (index == 0)
            {
                chd.AppendFormat("t:{0}", (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
                chl.AppendFormat("{0} {1}({2}%)", sd, cnt, (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
            }
            else
            {
                chd.AppendFormat(",{0}", (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
                chl.AppendFormat("|{0} {1}({2}%)", sd, cnt, (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
            }
            if (cnt > maxCnt)
            {
                maxCnt = cnt;
                maxCntIndex = index;
            }

            index++;
        }
        chco[maxCntIndex] = "ff0000";
        pars["chd"] = chd.ToString();
        pars["chl"] = chl.ToString();
        for (int i = 0; i < chco.Length; i++)
        {
            pars["chco"] = string.Concat(pars["chco"], i == 0 ? "" : ",", chco[i]);
        }

        stats.Rows[0].Items[0].Text = string.Format("<img src=\"{0}\" />", googleChartImgUrl(pars));
        stats.Rows[0].Items[0].IsHtml = true;

        stats = new Layout.Statistics(Translate(strFoundsPerDifficulty, false));
        skin.StatisticsBlocks[1] = stats;
        stats.Rows.Add(new Layout.Statistics.Row());
        stats.Rows[0].Items.Add(new Layout.Statistics.Item());

        //graph Terrain
        pars["chco"] = "";
        chd.Length = 0;
        chl.Length = 0;
        maxCnt = 0;
        maxCntIndex = 0;
        index = 0;
        for (double d = 1.0; d < 5.1; d += 0.5)
        {
            string sd = d.ToString("0.#");
            int cnt = (from mf in _myFinds where mf.gc.Terrain.ToString("0.#") == sd select mf).Count();
            if (index == 0)
            {
                chd.AppendFormat("t:{0}", (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
                chl.AppendFormat("{0} {1}({2}%)", sd, cnt, (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
            }
            else
            {
                chd.AppendFormat(",{0}", (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
                chl.AppendFormat("|{0} {1}({2}%)", sd, cnt, (100.0 * (double)cnt / tot).ToString("0.#").Replace(',', '.'));
            }
            if (cnt > maxCnt)
            {
                maxCnt = cnt;
                maxCntIndex = index;
            }

            index++;
        }
        chco[maxCntIndex] = "ff0000";
        pars["chd"] = chd.ToString();
        pars["chl"] = chl.ToString();
        for (int i = 0; i < chco.Length; i++)
        {
            pars["chco"] = string.Concat(pars["chco"], i == 0 ? "" : ",", chco[i]);
        }

        stats.Rows[0].Items[0].Text = string.Format("<img src=\"{0}\" />", googleChartImgUrl(pars));
        stats.Rows[0].Items[0].IsHtml = true;

        return skin.ToString();
    }

    private string logLengthTable()
    {
        string strLogLengthChar = "Log length (characters)";
        string strLogLengthWords = "Log length (words)";
        string strInterval = "Between";
        string strCount = "Count";
        string strPercentage = "Percentage";

        RegisterText(new string[]{
		strLogLengthChar,
        strInterval,
        strCount,
        strPercentage,
        strLogLengthWords
		});

        Layout skin = new Layout(2);
        Layout.Statistics stats = new Layout.Statistics(Translate(strLogLengthChar, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strInterval, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strCount, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        int[] lengths = (from mf in _myFinds where mf.lg != null select mf.lg.Text.Replace(" ", "").Replace("\r", "").Replace("\n", "").Length).ToArray();
        int lmax = lengths.Max();
        for (int i = 0; i < 10; i++)
        {
            int minV = (int)((double)lmax * (double)i / 10.0);
            int maxV = (int)((double)lmax * ((double)i + 1.0) / 10.0);
            int cnt = (from l in lengths where l > minV && l <= maxV select l).Count();

            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0} - {1}", minV, maxV);
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = cnt.ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)cnt / (double)_myFinds.Count);
        }

        //words
        stats = new Layout.Statistics(Translate(strLogLengthWords, false));
        skin.StatisticsBlocks[1] = stats;

        row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strInterval, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strCount, false);
        row.Items[row.Items.Count - 1].IsMarker = true;
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = Translate(strPercentage, false);
        row.Items[row.Items.Count - 1].IsMarker = true;

        lengths = (from mf in _myFinds where mf.lg != null select mf.lg.Text.Split(new char[] { ' ', '\t', '\r', '\n', '.' }, StringSplitOptions.RemoveEmptyEntries).Length).ToArray();
        lmax = lengths.Max();
        for (int i = 0; i < 10; i++)
        {
            int minV = (int)((double)lmax * (double)i / 10.0);
            int maxV = (int)((double)lmax * ((double)i + 1.0) / 10.0);
            int cnt = (from l in lengths where l > minV && l <= maxV select l).Count();

            row = new Layout.Statistics.Row();
            stats.Rows.Add(row);

            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0} - {1}", minV, maxV);
            row.Items[row.Items.Count - 1].IsMarker = true;
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = cnt.ToString();
            row.Items.Add(new Layout.Statistics.Item());
            row.Items[row.Items.Count - 1].Text = string.Format("{0:0.0} %", 100.0 * (double)cnt / (double)_myFinds.Count);
        }
        return skin.ToString();
    }

    public string CacheTypeRatioGraph()
    {
        string strTypePerMonth = "Cache type percentage per month and total";
        RegisterText(new string[] {
	        strTypePerMonth
        });

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strTypePerMonth, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        //Interval of months to subsum valid:(1,2,3,4,6,12)
        int interval = 1;

        //graph
        Dictionary<string, string> pars = new Dictionary<string, string>();
        Dictionary<string, string> pars2;

        /*
        ?chxt=y
           &chbh=a,0,0
           &chs=300x225
           &cht=bvs
           &chco=A2C180,3D7930
           &chd=t:10,0,60,80,40,60,30|50,0,100,40,20,40,30
           &chtt=Vertical+bar+chart
        */


        //fixed (data independent)
        pars.Add("chs", "740x180");
        pars.Add("chf", "bg,s,FFF4F4");
        pars.Add("chxt", "r,x,y");

        //label colors
        //pars.Add("chxs", "0,0000FF|2,FF0000");

        //bar chart
        pars.Add("cht", "bvs");
        pars.Add("chbh", "a,0,0");

        //line chart
        //pars.Add("cht", "lc");
        //pars.Add("chm", "B,C5D4B5BB,0,0,0|b,FFF4C2B8,1,0,0|b,76A4FBBE,2,1,0|b,C2BDDDBD,3,2,0");
        //pars.Add("chls", "2|2|2|2");

        pars.Add("chdl", "Traditional|Multi|Mystery|Other");
        pars.Add("chdlp", "t");
        pars.Add("chco", "A2C180,FFCC33,4D86FF,A9A9A9");      //normal
        //pars.Add("chco", "C5D4B5,FFF4C2,76A4FB,C2BDDD");      //light
        //pars.Add("chco", "3D7930,FF9900,0000FF,6B687AC0");  //contrast
        pars.Add("chds", "a");

        //variable, data dependent
        pars.Add("chd", "");
        pars.Add("chxl", "");

        //second graph
        pars2 = new Dictionary<string, string>(pars);
        pars2.Remove("chdl"); //no repeating legend
        pars2["chs"] = "740x160";


        DateTime startAt = (from mf in _myFinds where mf.logDate.Year >= 2000 orderby mf.logDate.Year select mf.logDate).FirstOrDefault();
        if (startAt.Year >= 2000)
        {
            startAt = new DateTime(startAt.Year, startAt.Month, 1);
            //debug:startAt = new DateTime(2012,1, 1);
            //start at interval
            while ((startAt.Month - 1) % interval != 0)
            {
                startAt = startAt.AddMonths(-1);
            }

            DateTime endAt = startAt.AddMonths(interval);
            DateTime startOfInterval = startAt;
            StringBuilder chxl = new StringBuilder();

            StringBuilder sbTrad = new StringBuilder();
            StringBuilder sbMult = new StringBuilder();
            StringBuilder sbMyst = new StringBuilder();
            StringBuilder sbOther = new StringBuilder();

            StringBuilder sbTTrad = new StringBuilder();
            StringBuilder sbTMult = new StringBuilder();
            StringBuilder sbTMyst = new StringBuilder();
            StringBuilder sbTOther = new StringBuilder();

            double sumTrad = 0;
            double sumMult = 0;
            double sumMyst = 0;
            double sumOther = 0;
            double sumAll = 0;


            while (startAt <= DateTime.Now)
            {
                //debug:sb.AppendLine(string.Format("Mo: {0}<br>",startAt.Month));
                if (chxl.Length == 0 || startAt.Month == 1)
                {
                    chxl.AppendFormat("|{0}", startAt.Year);
                    startOfInterval = startAt;
                }
                else
                {
                    chxl.Append("|");
                }

                double qTrad = (from mf in _myFinds where mf.logDate >= startAt && mf.logDate < endAt && mf.gc.GeocacheType.ID == 2 select mf).Count();
                double qMult = (from mf in _myFinds where mf.logDate >= startAt && mf.logDate < endAt && mf.gc.GeocacheType.ID == 3 select mf).Count();
                double qMyst = (from mf in _myFinds where mf.logDate >= startAt && mf.logDate < endAt && mf.gc.GeocacheType.ID == 8 select mf).Count();
                double qOther = (from mf in _myFinds
                                 where mf.logDate >= startAt && mf.logDate < endAt &&
                                     mf.gc.GeocacheType.ID != 2 && mf.gc.GeocacheType.ID != 3 && mf.gc.GeocacheType.ID != 8
                                 select mf).Count();
                double qSum = qTrad + qMult + qMyst + qOther;

                sumTrad += qTrad;
                sumMult += qMult;
                sumMyst += qMyst;
                sumOther += qOther;

                sumAll += qSum;

                if (sbTrad.Length != 0)
                {
                    sbTrad.Append(",");
                    sbMult.Append(",");
                    sbMyst.Append(",");
                    sbOther.Append(",");

                    sbTTrad.Append(",");
                    sbTMult.Append(",");
                    sbTMyst.Append(",");
                    sbTOther.Append(",");
                }
                //data for graph per month
                if (qSum > 0.9) //>0
                {
                    //line chart
                    //			sbTrad.Append(String.Format("{0}", Math.Round((qTrad/qSum)*100,2)).Replace(",","."));
                    //			sbMult.Append(String.Format("{0}", Math.Round(((qTrad+qMult)/qSum)*100,2)).Replace(",","."));
                    //			sbMyst.Append(String.Format("{0}", Math.Round(((qTrad+qMult+qMyst)/qSum)*100,2)).Replace(",","."));
                    //			sbOther.Append("100");
                    //bar chart
                    sbTrad.Append(String.Format("{0}", Math.Round((qTrad / qSum) * 100, 2)).Replace(",", "."));
                    sbMult.Append(String.Format("{0}", Math.Round(((qMult) / qSum) * 100, 2)).Replace(",", "."));
                    sbMyst.Append(String.Format("{0}", Math.Round(((qMyst) / qSum) * 100, 2)).Replace(",", "."));
                    sbOther.Append(String.Format("{0}", Math.Round(((qOther) / qSum) * 100, 2)).Replace(",", "."));
                }
                else
                {
                    sbTrad.Append("0");
                    sbMult.Append("0");
                    sbMyst.Append("0");
                    sbOther.Append("0");
                }
                //data for total graph
                if (sumAll > 0.9) //>0
                {
                    //line chart
                    //			sbTTrad.Append(String.Format("{0}", Math.Round((sumTrad/sumAll)*100,2)).Replace(",","."));
                    //			sbTMult.Append(String.Format("{0}", Math.Round(((sumTrad+sumMult)/sumAll)*100,2)).Replace(",","."));
                    //			sbTMyst.Append(String.Format("{0}", Math.Round(((sumTrad+sumMult+sumMyst)/sumAll)*100,2)).Replace(",","."));
                    //			sbTOther.Append("100");
                    //bar chart
                    sbTTrad.Append(String.Format("{0}", Math.Round((sumTrad / sumAll) * 100, 2)).Replace(",", "."));
                    sbTMult.Append(String.Format("{0}", Math.Round(((sumMult) / sumAll) * 100, 2)).Replace(",", "."));
                    sbTMyst.Append(String.Format("{0}", Math.Round(((sumMyst) / sumAll) * 100, 2)).Replace(",", "."));
                    sbTOther.Append(String.Format("{0}", Math.Round(((sumOther) / sumAll) * 100, 2)).Replace(",", "."));
                }
                else
                {	//probably never happens
                    sbTTrad.Append("0");
                    sbTMult.Append("0");
                    sbTMyst.Append("0");
                    sbTOther.Append("0");
                }

                startAt = endAt;
                endAt = endAt.AddMonths(interval);
            }
            pars["chd"] = string.Format("t:{0}|{1}|{2}|{3}", sbTrad.ToString(), sbMult.ToString(), sbMyst.ToString(), sbOther.ToString());
            pars["chxl"] = string.Format("1:{0}", chxl.ToString());

            pars2["chd"] = string.Format("t:{0}|{1}|{2}|{3}", sbTTrad.ToString(), sbTMult.ToString(), sbTMyst.ToString(), sbTOther.ToString());
            pars2["chxl"] = string.Format("1:{0}", chxl.ToString());

            //sb.AppendLine("<p>Debug:");
            //sb.AppendLine(string.Format("{0}<br>{1}<br>{2}<br>{3}</p>", sbTrad.ToString(), sbMult.ToString(), sbMyst.ToString(), sbOther.ToString()));
        }

        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("<img src=\"{0}\" /><br /><img src=\"{1}\" />", googleChartImgUrl(pars), googleChartImgUrl(pars2));
        return skin.ToString();
    }

    private string CacheTravelDistyanceGraph()
    {
        string strTotalPerMonth = "Cache travel distance (direct line from Cache to Cache)";

        RegisterText(new string[] {
	        strTotalPerMonth
        });

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strTotalPerMonth, false));
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        Dictionary<string, string> pars = new Dictionary<string, string>();

        pars.Add("chxl", "1:||mi|3:||km|4:");      //axis 4 is date range |2011|2012
        pars.Add("chxr", "");                          //axis ranges =0,0,1000|2,0,1600|4,2006,2012
        pars.Add("chxs", "0,676767,11.5,0,lt,676767|1,676767,12.5,0,l,676767|2,676767,12.5,0,lt,676767");
        pars.Add("chxt", "y,y,r,r,x");
        pars.Add("chs", "740x300");
        pars.Add("cht", "lc");
        pars.Add("chco", "3D7930");
        pars.Add("chg", "14.3,-1,1,1");
        pars.Add("chls", "2,4,0");
        pars.Add("chm", "B,C5D4B5BB,0,0,0");
        pars.Add("chds", "a");//automatic scaling

        pars.Add("chd", "");

        //skip finds without lat/lon
        List<MyGeocacheFind> _myFinds2 = new List<MyGeocacheFind>();
        for (int m = 0; m < _myFinds.Count; m++)
        {
            if (_myFinds[m].gc.Lat != 0 && _myFinds[m].gc.Lon != 0)
            {
                _myFinds2.Add(_myFinds[m]);
            }
        }
        //= (from mf in _myFinds where mf.gc.Lat!=0 select mf );

        if (_myFinds2.Count > 1)  //need at least to finds for distance
        {
            double sumDist = 0.0;
            double sumDist2 = 0.0;//"Dumb"-Distance without customLat/Lon

            int cnt = 0;

            StringBuilder chxl = new StringBuilder();
            StringBuilder serie1 = new StringBuilder();

            //sum Interval 1 month
            DateTime startAt = new DateTime(_myFinds2[1].logDate.Year, _myFinds2[1].logDate.Month, 1);
            DateTime endAt = startAt.AddMonths(1);

            //_myFinds2 is sorted by Logdate (hopefully)

            for (int i = 1; i < _myFinds2.Count; i++)
            {
                double lat1 = _myFinds2[i - 1].gc.Lat;
                double lon1 = _myFinds2[i - 1].gc.Lon;
                double lat2 = _myFinds2[i].gc.Lat;
                double lon2 = _myFinds2[i].gc.Lon;

                double dist2 = Calculus.CalculateDistance(lat1, lon1, lat2, lon2).EllipsoidalDistance / 1000; //km

                if (_myFinds2[i - 1].gc.CustomLat != null && _myFinds2[i - 1].gc.CustomLon != null)
                {
                    lat1 = (double)_myFinds2[i - 1].gc.CustomLat;
                    lon1 = (double)_myFinds2[i - 1].gc.CustomLon;
                }

                if (_myFinds2[i].gc.CustomLat != null && _myFinds2[i].gc.CustomLon != null)
                {
                    lat2 = (double)_myFinds2[i].gc.CustomLat;
                    lon2 = (double)_myFinds2[i].gc.CustomLon;
                }

                double dist = Calculus.CalculateDistance(lat1, lon1, lat2, lon2).EllipsoidalDistance / 1000; //km

                sumDist += dist;
                sumDist2 += dist2;

                //debugging:
                //sb.AppendLine(string.Format("{0}: d:{1}/{2}; s:{3}/{4}; d:{5} to: {6} <br />", i-1, Math.Round(dist,2), Math.Round(dist2,2), Math.Round(sumDist,1), Math.Round(sumDist2,1),_myFinds2[i].logDate,_myFinds2[i].gc.Name));

                if (i + 1 == _myFinds2.Count || _myFinds2[i + 1].logDate >= endAt)
                {
                    if (chxl.Length == 0 || startAt.Month == 1)
                    {
                        chxl.AppendFormat("|{0}", startAt.Year);
                    }
                    else
                    {
                        chxl.Append("|");
                    }
                    if (serie1.Length != 0)
                    {
                        serie1.Append(",");
                    }
                    serie1.AppendFormat("{0}", Math.Round(sumDist, 0));
                    cnt++;

                    if (i + 1 < _myFinds2.Count) //not last entry -> set new interval
                    {
                        startAt = new DateTime(_myFinds2[i + 1].logDate.Year, _myFinds2[i + 1].logDate.Month, 1);
                        //check for gap
                        while (endAt < startAt) //fill gap
                        {
                            if (chxl.Length == 0 || endAt.Month == 1)
                            {
                                chxl.AppendFormat("|{0}", endAt.Year);
                            }
                            else
                            {
                                chxl.Append("|");
                            }
                            serie1.AppendFormat(",{0}", Math.Round(sumDist, 0));
                            cnt++;
                            endAt = endAt.AddMonths(1);
                        }
                        endAt = startAt.AddMonths(1);
                    }

                }
            }
            pars["chd"] = string.Format("t:{0}", serie1.ToString());
            pars["chxl"] += string.Format("{0}", chxl.ToString());
            //axis ranges =0,0,1000|2,0,1600|4,2006,2012
            pars["chxr"] = string.Format("0,0,{0}|2,0,{1}", Math.Round(sumDist / 1.609344, 0), Math.Round(sumDist, 0));
            //1 mile = 1.609344 kilometers
            pars["chm"] += string.Format("|A{0}km,224499,0,{1}.0,10", Math.Round(sumDist, 0), cnt - 1);
            pars["chm"] += string.Format("|A{0}mi,224499,0,{1}.0,10", Math.Round(sumDist / 1.609344, 0), cnt - 1);

            //debug:
            //sb.AppendLine(string.Format("{0}<br>{1}<br>{2}<br>{3}</p>", pars["chd"], chxl.ToString(), pars["chxr"], sumDist ));

        }
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = string.Format("<img src=\"{0}\" />", googleChartImgUrl(pars));
        return skin.ToString();
    }

    private string myGeoToolsBadges()
    {
        string strTitle = "myGEOtools Badges";

        /*
        read about the badges here:
            http://www.mygeotools.de/content/tools;statistik;badges

        read about the mdCachingPoints here:
            http://www.macdefender.org/products/GCStatistic/mdCachingPoints.html
        */

        RegisterText(new string[]{
		strTitle
		});

        Layout skin = new Layout(1);
        Layout.Statistics stats = new Layout.Statistics(Translate(strTitle, false));
        stats.Align = "center";
        skin.StatisticsBlocks[0] = stats;

        Layout.Statistics.Row row = new Layout.Statistics.Row();
        stats.Rows.Add(row);

        StringBuilder sb = new StringBuilder();
        var csizeTypes =
            (from mf in _myFinds
             group mf by mf.gc.GeocacheType into g
             select new { GeocacheType = g.Key, Founds = g }).OrderByDescending(x => x.Founds.Count());
        double sumall = 0;
        foreach (var g in csizeTypes)
        {
            double sumdt = 0;
            double mfBy = 1.0;
            string cpar = "";
            //sb.AppendLine(string.Format("<br>Typ:{0}:",Translate(g.GeocacheType.Name)));
            foreach (var mf in g.Founds)
            {
                sumdt += mf.gc.Difficulty * mf.gc.Terrain;
            }
            switch (g.GeocacheType.ID)
            {
                case 2: mfBy = 1; cpar = "tradi"; break;//Tradi
                case 3: mfBy = 2; cpar = "multi"; break;//Multi
                case 4: mfBy = 0.5; cpar = "virtual"; break;//Virtual
                case 5: mfBy = 1.5; cpar = "letter"; break;//Letterbox
                case 6: mfBy = 1.5; cpar = "event"; break;//Event
                case 8: mfBy = 2; cpar = "myst"; break;//Mystery
                case 9: mfBy = 4; break;//Project_APE
                case 11: mfBy = 1; cpar = "webcam"; break;//Webcam
                case 12: mfBy = 0.5; break;//Locationless
                case 13: mfBy = 1.5; cpar = "cito"; break;//CITO
                //		case 27: mfBy=1;break;//Benchmark
                case 137: mfBy = 1.5; cpar = "earth"; break;//Earth
                case 453: mfBy = 1.75; cpar = "mega"; break;//Mega_Event
                //		case 605: mfBy=1;break;//Geocache Course
                case 1304: mfBy = 2; break; //GPS Adventures Exhibit
                case 1858: mfBy = 2; cpar = "wherigo"; break; //Whereigo
                case 3653: mfBy = 1.5; break; //Lost_and_Found_Event (geraten)
                case 3773: mfBy = 1; break; //Groundspeak_HQ (geraten)
                case 3774: mfBy = 1; break; //Groundspeak_Lost_and_Found (geraten)
                case 4738: mfBy = 1; break; //Groundspeak_Block_Party (geraten)
                //10Years ? -> 2
                default: mfBy = 1; break;
            }
            //sb.AppendLine(string.Format("<br>Typ:{0}: {1} pts",Translate(g.GeocacheType.Name),sumdt*mfBy));
            if (cpar != "" && sumdt * mfBy > 0.4)
            {
                sb.AppendLine(string.Format("<img src='http://www.mygeotools.de/badgegen.php?type={0}&points={1}'>",
                    cpar, Math.Round(sumdt * mfBy, 0)));
            }
            sumall += sumdt * mfBy;
        }
        sb.AppendLine(string.Format("<br>mdCachingPoints: <b>{0}</b>", Math.Round(sumall, 0)));
        row.Items.Add(new Layout.Statistics.Item());
        row.Items[row.Items.Count - 1].Text = sb.ToString();
        row.Items[row.Items.Count - 1].IsHtml = true;

        return skin.ToString();
    }
}
