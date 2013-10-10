using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.MapProviders.Google
{
    public class MapControlFactoryGoogle : MapControl.MapControlFactory
    {
        public MapControlFactoryGoogle(Framework.Interfaces.ICore core)
            : base(core)
        {
        }

        public override void Init()
        {
            _bitmapStore = new MapControl.BitmapStoreGoogle();
            _searchProvider = new MapControl.SearchProviderGoogle();
            _tileGenerator = new MapControl.TileGeneratorGoogle(this);
            _tilePanel = new MapControl.TilePanel(this);
            _bitmapStore.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
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
            get { return "GoogleCache"; }
        }

    }
}
