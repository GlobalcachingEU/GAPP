using System;
using System.Reflection;
using System.Windows.Media;


namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    public class MapOffset
    {
        private EventHandler _offsetChanged;

        private double _mapSize = 0; // Default to zoom 0
        private double _offset;
        private double _size;
        private int _maximumTile;
        private int _tile;

        // Used for animation
        private bool _animating;
        private double _step;
        private double _target;
        private double _value;

        private MapControlFactory _mapControlFactory = null;

        /// <summary>Initializes a new instance of the MapOffset class.</summary>
        /// <param name="property">The property this MapOffset represents.</param>
        /// <param name="offsetChanged">Called when the Offset changes.</param>
        public MapOffset(MapControlFactory mapControlFactory, PropertyInfo property, EventHandler offsetChanged)
        {
            System.Diagnostics.Debug.Assert(property != null, "property cannot be null");
            System.Diagnostics.Debug.Assert(offsetChanged != null, "offsetChanged cannot be null");

            _mapControlFactory = mapControlFactory;
            _mapSize = mapControlFactory.TileGenerator.TileSize;
            _offsetChanged = offsetChanged;
            this.Property = property;
            this.Frames = 24;
            CompositionTarget.Rendering += this.OnRendering; // Used for manual animation
        }

        /// <summary>Gets or sets the number of steps when animating.</summary>
        public int Frames { get; set; }

        /// <summary>Gets the offset from the tile edge to the screen edge.</summary>
        public double Offset
        {
            get
            {
                return _offset;
            }
            private set
            {
                if (_offset != value)
                {
                    _offset = value;
                    _offsetChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>Gets the location of the starting tile in pixels.</summary>
        public double Pixels
        {
            get { return (this.Tile * _mapControlFactory.TileGenerator.TileSize) - this.Offset; }
        }

        /// <summary>Gets the PropertyInfo associated with this offset.</summary>
        /// <remarks>This is used so a generic handler can be called for the _offsetChanged delegate.</remarks>
        public PropertyInfo Property { get; private set; }

        /// <summary>Gets the starting tile index.</summary>
        public int Tile
        {
            get
            {
                return _tile;
            }
            private set
            {
                if (_tile != value)
                {
                    _tile = value;
                }
            }
        }

        /// <summary>Smoothly translates by the specified amount.</summary>
        /// <param name="value">The total distance to translate.</param>
        public void AnimateTranslate(double value)
        {
            if (value == 0)
            {
                _animating = false;
            }
            else
            {
                _value = 0;
                if (value < 0)
                {
                    _target = -value;
                    _step = Math.Min(value / this.Frames, -1.0);
                }
                else
                {
                    _target = value;
                    _step = Math.Max(value / this.Frames, 1);
                }
                _animating = true;
            }
        }

        /// <summary>Adjusts the offset so the specifed tile is in the center of the control.</summary>
        /// <param name="tile">The tile (allowing fractions of the tile) to be centered.</param>
        public void CenterOn(double tile)
        {
            double pixels = (tile * _mapControlFactory.TileGenerator.TileSize) - (_size / 2.0);
            this.Translate(this.Pixels - pixels);
        }

        /// <summary>Called when the size of the parent control changes.</summary>
        /// <param name="size">The nes size of the parent control.</param>
        public void ChangeSize(double size)
        {
            _size = size;
            _maximumTile = (int)((_mapSize - _size) / _mapControlFactory.TileGenerator.TileSize); // Only interested in the integer part, the rest will be truncated
            this.Translate(0); // Force a refresh
        }

        /// <summary>Updates the starting tile index based on the zoom level.</summary>
        /// <param name="zoom">The zoom level.</param>
        /// <param name="offset">The distance from the edge to keep the same when changing zoom.</param>
        public void ChangeZoom(int zoom, double offset)
        {
            int currentZoom = _mapControlFactory.TileGenerator.GetZoom(_mapSize / _mapControlFactory.TileGenerator.TileSize);
            if (currentZoom != zoom)
            {
                _animating = false;

                double scale = Math.Pow(2, zoom - currentZoom); // 2^delta
                double location = ((this.Pixels + offset) * scale) - offset; // Bias new location on the offset

                _mapSize = _mapControlFactory.TileGenerator.GetSize(zoom);
                _maximumTile = (int)((_mapSize - _size) / _mapControlFactory.TileGenerator.TileSize);

                this.Translate(this.Pixels - location);
            }
        }

        /// <summary>Changes the offset by the specified amount.</summary>
        /// <param name="value">The amount to change the offset by.</param>
        public void Translate(double value)
        {
            if (_size > _mapSize)
            {
                this.Tile = 0;
                this.Offset = (_size - _mapSize) / 2;
            }
            else
            {
                double location = this.Pixels - value;
                if (location < 0)
                {
                    this.Tile = 0;
                    this.Offset = 0;
                }
                else if (location + _size > _mapSize)
                {
                    this.Tile = _maximumTile;
                    this.Offset = _size - (_mapSize - (_maximumTile * _mapControlFactory.TileGenerator.TileSize));
                }
                else
                {
                    this.Tile = (int)(location / _mapControlFactory.TileGenerator.TileSize);
                    this.Offset = (this.Tile * _mapControlFactory.TileGenerator.TileSize) - location;
                }
            }
        }

        // Used for animating the Translate.
        private void OnRendering(object sender, EventArgs e)
        {
            if (_animating)
            {
                this.Translate(_step);
                _value += Math.Abs(_step);
                _animating = _value < _target; // Stop animating once we've reached/exceeded the target
            }
        }
    }
}
