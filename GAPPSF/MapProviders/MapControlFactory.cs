using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GAPPSF.MapProviders
{
    public class MapControlFactory
    {
        protected BitmapStore _bitmapStore = null;
        protected SearchProvider _searchProvider = null;
        protected TileGenerator _tileGenerator = null;
        protected TilePanel _tilePanel = null;

        public volatile int _lastRequestedZoomLevel = 0;

        protected MapControlFactory()
        {
        }

        public virtual void SettingsChanged()
        {
        }

        public int LastRequestedZoomLevel
        {
            get { return _lastRequestedZoomLevel; }
            set { _lastRequestedZoomLevel = value; }
        }

        public BitmapStore BitmapStore
        {
            get { if (_bitmapStore == null) _bitmapStore = new BitmapStore(); return _bitmapStore; }
        }
        public SearchProvider SearchProvider
        {
            get { if (_searchProvider == null) _searchProvider = new SearchProvider(); return _searchProvider; }
        }
        public TileGenerator TileGenerator
        {
            get { if (_tileGenerator == null) _tileGenerator = new TileGenerator(this); return _tileGenerator; }
        }
        public TilePanel TilePanel
        {
            get { if (_tilePanel == null) _tilePanel = new TilePanel(this); return _tilePanel; }
        }
        public virtual Tile GetTile(int zoom, int x, int y)
        {
            return null;
        }
        public virtual MapOffset GetMapOffset(PropertyInfo property, EventHandler offsetChanged)
        {
            return new MapOffset(this, property, offsetChanged);
        }
        public virtual string ID
        {
            get { return null; }
        }

        public virtual int WindowWidth { get; set; }
        public virtual int WindowHeight { get; set; }
        public virtual int WindowLeft { get; set; }
        public virtual int WindowTop { get; set; }

    }
}
