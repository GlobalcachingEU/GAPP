using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Framework.Interfaces
{
    public interface IGeometry
    {
        List<Data.AreaInfo> GetAreasOfLocation(Data.Location loc);
        List<Data.AreaInfo> GetAreasOfLocation(Data.Location loc, List<Data.AreaInfo> inAreas);
        List<Data.AreaInfo> GetAreasByName(string name);
        List<Data.AreaInfo> GetAreasByName(string name, Data.AreaType level);
        List<Data.AreaInfo> GetAreasByID(object id);
        List<Data.AreaInfo> GetAreasByParentID(object parentid);
        List<Data.AreaInfo> GetAreasByLevel(Data.AreaType level);

        void GetPolygonOfArea(Data.AreaInfo area);
    }
}
