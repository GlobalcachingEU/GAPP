using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPPSF.MapProviders
{
    public class MapControlFactoryGoogle : MapControlFactory
    {
        public MapControlFactoryGoogle()
            : base()
        {
            _bitmapStore = new BitmapStoreGoogle();
            _searchProvider = new SearchProviderGoogle();
            _tileGenerator = new TileGeneratorGoogle(this);
            _tilePanel = new TilePanel(this);
            _bitmapStore.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("GoogleRoadMapOnline") as string;
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
            get { return "GoogleCache"; }
        }

        public override int WindowWidth
        {
            get
            {
                return Core.Settings.Default.MapsGoogleWindowWidth;
            }
            set
            {
                Core.Settings.Default.MapsGoogleWindowWidth = value;
            }
        }

        public override int WindowHeight
        {
            get
            {
                return Core.Settings.Default.MapsGoogleWindowHeight;
            }
            set
            {
                Core.Settings.Default.MapsGoogleWindowHeight = value;
            }
        }

        public override int WindowLeft
        {
            get
            {
                return Core.Settings.Default.MapsGoogleWindowLeft;
            }
            set
            {
                Core.Settings.Default.MapsGoogleWindowLeft = value;
            }
        }

        public override int WindowTop
        {
            get
            {
                return Core.Settings.Default.MapsGoogleWindowTop;
            }
            set
            {
                Core.Settings.Default.MapsGoogleWindowTop = value;
            }
        }

    }
}
