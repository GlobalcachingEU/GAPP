using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GAPPSF.MapProviders
{
    public class MapControlFactoryOSMOffline : MapControlFactory, IDisposable
    {
        private OSMBinMap.MapFilesHandler _mapFilesHandler = null;

        public MapControlFactoryOSMOffline()
            : base()
        {
            _mapFilesHandler = new OSMBinMap.MapFilesHandler(this);
            _bitmapStore = new BitmapStoreOSMOffline(_mapFilesHandler);
            _searchProvider = new SearchProviderOSM();
            _tileGenerator = new TileGeneratorOSM(this);
            _tilePanel = new TilePanel(this);

            OSMBinFilesVisibility = Visibility.Visible;
        }

        public override void SettingsChanged()
        {
            _mapFilesHandler.reload();
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
            get { return "OSMOfflineCache"; }
        }

        public override string ToString()
        {
            return Localization.TranslationManager.Instance.Translate("OpenStreetMapOffline") as string;
        }


        public override int WindowWidth
        {
            get
            {
                return Core.Settings.Default.MapsOSMOfflineWindowWidth;
            }
            set
            {
                Core.Settings.Default.MapsOSMOfflineWindowWidth = value;
            }
        }

        public override int WindowHeight
        {
            get
            {
                return Core.Settings.Default.MapsOSMOfflineWindowHeight;
            }
            set
            {
                Core.Settings.Default.MapsOSMOfflineWindowHeight = value;
            }
        }

        public override int WindowLeft
        {
            get
            {
                return Core.Settings.Default.MapsOSMOfflineWindowLeft;
            }
            set
            {
                Core.Settings.Default.MapsOSMOfflineWindowLeft = value;
            }
        }

        public override int WindowTop
        {
            get
            {
                return Core.Settings.Default.MapsOSMOfflineWindowTop;
            }
            set
            {
                Core.Settings.Default.MapsOSMOfflineWindowTop = value;
            }
        }


        public void Dispose()
        {
            if (_mapFilesHandler!=null)
            {
                _mapFilesHandler.Dispose();
                _mapFilesHandler = null;
            }
        }
    }
}
