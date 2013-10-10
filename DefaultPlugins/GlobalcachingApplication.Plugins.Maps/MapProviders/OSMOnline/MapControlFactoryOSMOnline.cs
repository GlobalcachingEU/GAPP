using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.MapProviders.OSMOnline
{
    public class MapControlFactoryOSMOnline : MapControl.MapControlFactory
    {
        public MapControlFactoryOSMOnline(Framework.Interfaces.ICore core)
            : base(core)
        {
        }

        public override void Init()
        {
            _bitmapStore = new MapControl.BitmapStoreOSM();
            _searchProvider = new MapControl.SearchProviderOSM();
            _tileGenerator = new MapControl.TileGeneratorOSM(this);
            _tilePanel = new MapControl.TilePanel(this);
            base.Init();
        }

        public override MapControl.MapOffset GetMapOffset(System.Reflection.PropertyInfo property, EventHandler offsetChanged)
        {
            return new MapControl.MapOffset(this, property, offsetChanged);

        }
        public override MapControl.Tile GetTile(int zoom, int x, int y)
        {
            return new MapControl.Tile(this, zoom, x, y);
        }

        public override string ID
        {
            get { return "OSMOnlineCache"; }
        }

    }
}
