using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.MapProviders
{
    public class MapControlFactoryOSMOnline : MapControlFactory
    {
        public MapControlFactoryOSMOnline()
            : base()
        {
            _bitmapStore = new BitmapStoreOSM();
            _searchProvider = new SearchProviderOSM();
            _tileGenerator = new TileGeneratorOSM(this);
            _tilePanel = new TilePanel(this);
        }

        public override MapOffset GetMapOffset(System.Reflection.PropertyInfo property, EventHandler offsetChanged)
        {
            return new MapOffset(this, property, offsetChanged);

        }
        public override Tile GetTile(int zoom, int x, int y)
        {
            return new Tile(this, zoom, x, y);
        }

        public override string ID
        {
            get { return "OSMOnlineCache"; }
        }


        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("OpenStreetMapOnline") as string;
        }


        public override int WindowWidth
        {
            get
            {
                return Core.Settings.Default.MapsOSMOnlineWindowWidth;
            }
            set
            {
                Core.Settings.Default.MapsOSMOnlineWindowWidth = value;
            }
        }

        public override int WindowHeight
        {
            get
            {
                return Core.Settings.Default.MapsOSMOnlineWindowHeight;
            }
            set
            {
                Core.Settings.Default.MapsOSMOnlineWindowHeight = value;
            }
        }

        public override int WindowLeft
        {
            get
            {
                return Core.Settings.Default.MapsOSMOnlineWindowLeft;
            }
            set
            {
                Core.Settings.Default.MapsOSMOnlineWindowLeft = value;
            }
        }

        public override int WindowTop
        {
            get
            {
                return Core.Settings.Default.MapsOSMOnlineWindowTop;
            }
            set
            {
                Core.Settings.Default.MapsOSMOnlineWindowTop = value;
            }
        }

    }
}
