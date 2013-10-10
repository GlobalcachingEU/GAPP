using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.MapProviders.OSMOffline
{
    public class MapControlFactoryOSMOffline : MapControl.MapControlFactory
    {
        private OSMBinMap.MapFilesHandler _mapFilesHandler = null;

        public MapControlFactoryOSMOffline(Framework.Interfaces.ICore core)
            : base(core)
        {
        }

        public override void Init()
        {
            _mapFilesHandler = new OSMBinMap.MapFilesHandler(this);

            _bitmapStore = new MapControl.BitmapStoreOSMOffline(_mapFilesHandler);
            _searchProvider = new MapControl.SearchProviderOSM();
            _tileGenerator = new MapControl.TileGeneratorOSM(this);
            _tilePanel = new MapControl.TilePanel(this);
            base.Init();
        }

        public override void SettingsChanged()
        {
            _mapFilesHandler.reload();
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
            get { return "OSMOfflineCache"; }
        }

    }
}
