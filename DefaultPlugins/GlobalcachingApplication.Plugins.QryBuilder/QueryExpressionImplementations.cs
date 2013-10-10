using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.QryBuilder
{
    public class QueryExpressionImplementation
    {
        public static List<QueryExpressionImplementation> QueryExpressionImplementations = new List<QueryExpressionImplementation>();

        public Framework.Interfaces.ICore Core { get; private set; }
        public string Name { get; private set; }

        public QueryExpressionImplementation(string name,Framework.Interfaces.ICore core)
        {
            Name = name;
            Core = core;
            core.LanguageItems.Add(new Framework.Data.LanguageItem(name));
            QueryExpressionImplementations.Add(this);
        }
        public override string ToString()
        {
            return Utils.LanguageSupport.Instance.GetTranslation(Name);
        }
        public virtual QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            combo.Items.Clear();
            return (QueryExpression.Operator[])Enum.GetValues(typeof(QueryExpression.Operator));
        }
        public virtual bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return false;
        }
        public bool stringExpresionResult(QueryExpression.Operator op, string gcValue, string conditionValue)
        {
            bool result = false;
            switch (op)
            {
                case QueryExpression.Operator.Equal:
                    result = gcValue == conditionValue;
                    break;
                case QueryExpression.Operator.Larger:
                    result = gcValue.CompareTo(conditionValue)>0;
                    break;
                case QueryExpression.Operator.LargerOrEqual:
                    result = gcValue.CompareTo(conditionValue) >= 0;
                    break;
                case QueryExpression.Operator.Less:
                    result = gcValue.CompareTo(conditionValue) < 0;
                    break;
                case QueryExpression.Operator.LessOrEqual:
                    result = gcValue.CompareTo(conditionValue) <= 0;
                    break;
                case QueryExpression.Operator.NotEqual:
                    result = gcValue.CompareTo(conditionValue) != 0;
                    break;
            }
            return result;
        }
        public bool doubleExpresionResult(QueryExpression.Operator op, double gcValue, double conditionValue)
        {
            bool result = false;
            switch (op)
            {
                case QueryExpression.Operator.Equal:
                    result = gcValue == conditionValue;
                    break;
                case QueryExpression.Operator.Larger:
                    result = gcValue.CompareTo(conditionValue) > 0;
                    break;
                case QueryExpression.Operator.LargerOrEqual:
                    result = gcValue.CompareTo(conditionValue) >= 0;
                    break;
                case QueryExpression.Operator.Less:
                    result = gcValue.CompareTo(conditionValue) < 0;
                    break;
                case QueryExpression.Operator.LessOrEqual:
                    result = gcValue.CompareTo(conditionValue) <= 0;
                    break;
                case QueryExpression.Operator.NotEqual:
                    result = gcValue.CompareTo(conditionValue) != 0;
                    break;
            }
            return result;
        }
        public bool boolExpresionResult(QueryExpression.Operator op, bool gcValue, bool conditionValue)
        {
            bool result = false;
            switch (op)
            {
                case QueryExpression.Operator.Equal:
                    result = gcValue == conditionValue;
                    break;
                case QueryExpression.Operator.Larger:
                    result = gcValue.CompareTo(conditionValue) > 0;
                    break;
                case QueryExpression.Operator.LargerOrEqual:
                    result = gcValue.CompareTo(conditionValue) >= 0;
                    break;
                case QueryExpression.Operator.Less:
                    result = gcValue.CompareTo(conditionValue) < 0;
                    break;
                case QueryExpression.Operator.LessOrEqual:
                    result = gcValue.CompareTo(conditionValue) <= 0;
                    break;
                case QueryExpression.Operator.NotEqual:
                    result = gcValue.CompareTo(conditionValue) != 0;
                    break;
            }
            return result;
        }

        public bool intExpresionResult(QueryExpression.Operator op, int gcValue, int conditionValue)
        {
            bool result = false;
            switch (op)
            {
                case QueryExpression.Operator.Equal:
                    result = gcValue == conditionValue;
                    break;
                case QueryExpression.Operator.Larger:
                    result = gcValue.CompareTo(conditionValue) > 0;
                    break;
                case QueryExpression.Operator.LargerOrEqual:
                    result = gcValue.CompareTo(conditionValue) >= 0;
                    break;
                case QueryExpression.Operator.Less:
                    result = gcValue.CompareTo(conditionValue) < 0;
                    break;
                case QueryExpression.Operator.LessOrEqual:
                    result = gcValue.CompareTo(conditionValue) <= 0;
                    break;
                case QueryExpression.Operator.NotEqual:
                    result = gcValue.CompareTo(conditionValue) != 0;
                    break;
            }
            return result;
        }
    }

    public class QECode : QueryExpressionImplementation
    {
        public QECode(Framework.Interfaces.ICore core)
            : base("Code", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.Code ?? "", condition ?? "");
        }
    }

    public class QECountry: QueryExpressionImplementation
    {
        public QECountry(Framework.Interfaces.ICore core)
            : base("Country", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Country) select gc.Country).Distinct().OrderBy(x=>x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.Country ?? "", condition ?? "");
        }
    }
    public class QEState : QueryExpressionImplementation
    {
        public QEState(Framework.Interfaces.ICore core)
            : base("State", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.State) select gc.State).Distinct().OrderBy(x => x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.State ?? "", condition ?? "");
        }
    }

    public class QEMunicipality : QueryExpressionImplementation
    {
        public QEMunicipality(Framework.Interfaces.ICore core)
            : base("Municipality", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Municipality) select gc.Municipality).Distinct().OrderBy(x => x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.Municipality ?? "", condition ?? "");
        }
    }

    public class QECity : QueryExpressionImplementation
    {
        public QECity(Framework.Interfaces.ICore core)
            : base("City", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.City) select gc.City).Distinct().OrderBy(x => x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.City ?? "", condition ?? "");
        }
    }

    public class QEQuery : QueryExpressionImplementation
    {
        public QEQuery(Framework.Interfaces.ICore core)
            : base("Query", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange((from q in QueryBuilderForm.AvailableQueries select q.Name).OrderBy(x => x).ToArray());
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            QueryBuilderForm.FreeQuery fq = (from q in QueryBuilderForm.AvailableQueries where q.Name==condition select q).FirstOrDefault();
            if (fq == null)
            {
                return false;
            }
            else
            {
                return QueryBuilderForm.GetQueryResult(fq, gc);
            }
        }
    }

    public class QEPublished : QueryExpressionImplementation
    {
        public QEPublished(Framework.Interfaces.ICore core)
            : base("Published", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add(string.Format("{0}-{1}-{2}", n.Year, n.Month.ToString("00"), n.Day.ToString("00")));
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, string.Format("{0}-{1}-{2}", gc.PublishedTime.Year, gc.PublishedTime.Month.ToString("00"), gc.PublishedTime.Day.ToString("00")), condition ?? "");
        }
    }

    public class QEDataFromDate : QueryExpressionImplementation
    {
        public QEDataFromDate(Framework.Interfaces.ICore core)
            : base("Date of data", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add(string.Format("{0}-{1}-{2}", n.Year, n.Month.ToString("00"), n.Day.ToString("00")));
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, string.Format("{0}-{1}-{2}", gc.DataFromDate.Year, gc.DataFromDate.Month.ToString("00"), gc.DataFromDate.Day.ToString("00")), condition ?? "");
        }
    }

    public class QELat : QueryExpressionImplementation
    {
        public QELat(Framework.Interfaces.ICore core)
            : base("Latitude", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add(((double)0.0).ToString());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            double con = 0.0;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = Utils.Conversion.StringToDouble(condition);
                }
                catch
                {
                }
            }
            return doubleExpresionResult(op, gc.Lat, con);
        }
    }

    public class QELon : QueryExpressionImplementation
    {
        public QELon(Framework.Interfaces.ICore core)
            : base("Longitude", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add(((double)0.0).ToString());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            double con = 0.0;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = Utils.Conversion.StringToDouble(condition);
                }
                catch
                {
                }
            }
            return doubleExpresionResult(op, gc.Lon, con);
        }
    }

    public class QEDistanceToCenter : QueryExpressionImplementation
    {
        public QEDistanceToCenter(Framework.Interfaces.ICore core)
            : base("Distance to center (km)", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add(((double)0.0).ToString());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            double con = 0.0;
            double dist = 0.0;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = Utils.Conversion.StringToDouble(condition);
                    dist = Utils.Calculus.CalculateDistance(gc, Core.CenterLocation).EllipsoidalDistance / 1000.0;
                }
                catch
                {
                }
            }
            return doubleExpresionResult(op, dist, con);
        }
    }

    public class QEAvailable : QueryExpressionImplementation
    {
        public QEAvailable(Framework.Interfaces.ICore core)
            : base("Available", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[]{true.ToString(), false.ToString()});
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.Available, con);
        }
    }

    public class QEArchived : QueryExpressionImplementation
    {
        public QEArchived(Framework.Interfaces.ICore core)
            : base("Archived", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.Archived, con);
        }
    }

    public class QEGeocacheType : QueryExpressionImplementation
    {
        public QEGeocacheType(Framework.Interfaces.ICore core)
            : base("Geocache type", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange((from g in Core.GeocacheTypes select g.Name).OrderBy(x=>x).ToArray());
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.GeocacheType.Name, condition);
        }
    }

    public class QEPlacedBy : QueryExpressionImplementation
    {
        public QEPlacedBy(Framework.Interfaces.ICore core)
            : base("Placed by", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.PlacedBy) select gc.PlacedBy).Distinct().OrderBy(x => x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.PlacedBy ?? "", condition ?? "");
        }
    }

    public class QEOwner : QueryExpressionImplementation
    {
        public QEOwner(Framework.Interfaces.ICore core)
            : base("Owner", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            combo.Items.AddRange((from Framework.Data.Geocache gc in Core.Geocaches where !string.IsNullOrEmpty(gc.Owner) select gc.Owner).Distinct().OrderBy(x => x).ToArray());
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.Owner ?? "", condition ?? "");
        }
    }

    public class QEContainer : QueryExpressionImplementation
    {
        public QEContainer(Framework.Interfaces.ICore core)
            : base("Container", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange((from g in Core.GeocacheContainers select g.Name).OrderBy(x => x).ToArray());
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            return stringExpresionResult(op, gc.Container.Name, condition);
        }
    }

    public class QETerrain : QueryExpressionImplementation
    {
        public QETerrain(Framework.Interfaces.ICore core)
            : base("Terrain", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.AddRange(new string[]{"1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5"});
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            double con = 0.0;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = Utils.Conversion.StringToDouble(condition);
                }
                catch
                {
                }
            }
            return doubleExpresionResult(op, gc.Terrain, con);
        }
    }

    public class QEDifficulty : QueryExpressionImplementation
    {
        public QEDifficulty(Framework.Interfaces.ICore core)
            : base("Difficulty", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.AddRange(new string[] { "1", "1.5", "2", "2.5", "3", "3.5", "4", "4.5", "5" });
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            double con = 0.0;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = Utils.Conversion.StringToDouble(condition);
                }
                catch
                {
                }
            }
            return doubleExpresionResult(op, gc.Difficulty, con);
        }
    }

    public class QEMemberOnly : QueryExpressionImplementation
    {
        public QEMemberOnly(Framework.Interfaces.ICore core)
            : base("Member only", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.MemberOnly, con);
        }
    }

    public class QECustomCoords : QueryExpressionImplementation
    {
        public QECustomCoords(Framework.Interfaces.ICore core)
            : base("Custom coods", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.CustomCoords, con);
        }
    }
    public class QEFavorites : QueryExpressionImplementation
    {
        public QEFavorites(Framework.Interfaces.ICore core)
            : base("Favorites", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            QueryExpression.Operator[] result = base.InitCombo(combo);
            DateTime n = DateTime.Now;
            combo.Items.Add("1");
            return result;
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            int con = 0;
            if (!string.IsNullOrEmpty(condition))
            {
                int.TryParse(condition, out con);
            }
            return intExpresionResult(op, gc.Favorites, con);
        }
    }

    public class QEFlagged : QueryExpressionImplementation
    {
        public QEFlagged(Framework.Interfaces.ICore core)
            : base("Flagged", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.Flagged, con);
        }
    }

    public class QEFound : QueryExpressionImplementation
    {
        public QEFound(Framework.Interfaces.ICore core)
            : base("Found", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.Found, con);
        }
    }

    public class QELocked : QueryExpressionImplementation
    {
        public QELocked(Framework.Interfaces.ICore core)
            : base("Locked", core)
        {
        }
        public override QueryExpression.Operator[] InitCombo(ComboBox combo)
        {
            base.InitCombo(combo);
            combo.Items.AddRange(new string[] { true.ToString(), false.ToString() });
            return new QueryExpression.Operator[] { QueryExpression.Operator.Equal, QueryExpression.Operator.NotEqual };
        }
        public override bool ExpressionResult(Framework.Data.Geocache gc, QueryExpression.Operator op, string condition)
        {
            bool con = false;
            if (!string.IsNullOrEmpty(condition))
            {
                try
                {
                    con = bool.Parse(condition);
                }
                catch
                {
                }
            }
            return boolExpresionResult(op, gc.Locked, con);
        }
    }

}
