using System;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GAPPSF.MapProviders
{
    public class TilePanel : Canvas
    {
        private Tile _baseTile;
        private int _columns;
        private int _rows;
        private int _zoom;
        private MapControlFactory _mapControlFactory;

        /// <summary>Initializes a new instance of the TilePanel class.</summary>
        public TilePanel(MapControlFactory mapControlFactory)
        {
            _mapControlFactory = mapControlFactory;
            // This stops the Images from being blurry
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
        }

        /// <summary>Gets or sets the tile index at the left edge of the control.</summary>
        public int LeftTile { get; set; }

        /// <summary>Gets or sets the tile index at the top edge of the control.</summary>
        public int TopTile { get; set; }

        /// <summary>Gets a value indicating whether a call to Update is required.</summary>
        public bool RequiresUpdate
        {
            get
            {
                return _baseTile == null ||
                       (_baseTile.TileX + 1) - this.LeftTile != 0 || // The baseTile is located at -1,-1 but LeftTile/TopTile is 0,0
                       (_baseTile.TileY + 1) - this.TopTile != 0;
            }
        }

        /// <summary>Gets or sets the zoom level for the tiles.</summary>
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (_zoom != value)
                {
                    _zoom = value;
                    _baseTile = null; // Force complete refresh
                }
            }
        }

        /// <summary>Re-arranges the Tiles inside the grid.</summary>
        /// <remarks>The control will only update itself when RequiresUpdate returns true.</remarks>
        public void Update()
        {
            if (_baseTile == null)
            {
                this.RegenerateTiles();
            }
            else
            {
                int changeX = (_baseTile.TileX + 1) - this.LeftTile;
                int changeY = (_baseTile.TileY + 1) - this.TopTile;

                if ((changeX != 0) || (changeY != 0))
                {
                    if (((Math.Abs(changeX) > 1) && (Math.Abs(changeY) > 1)) || (this.Children.Count == 0))
                    { // It's changed too much or we don't have any tiles
                        this.RegenerateTiles();
                    }
                    else // Only changed a little
                    {
                        this.RegenerateTiles();
                        /*
                        if (changeX != 0)
                        {
                            this.ChangeColumns(changeX);
                        }
                        if (changeY != 0)
                        {
                            this.ChangeRows(changeY);
                        }
                         * */
                    }
                }
            }
        }

        /// <summary>Ensures the grid has the correct number of rows and columns.</summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            int oldColumns = _columns;
            int oldRows = _rows;
            _rows = (int)Math.Ceiling(sizeInfo.NewSize.Height / _mapControlFactory.TileGenerator.TileSize) + 1;
            _columns = (int)Math.Ceiling(sizeInfo.NewSize.Width / _mapControlFactory.TileGenerator.TileSize) + 1;

            if (oldColumns < _columns || oldRows < _rows) // First check if we need to add tiles
            {
                this.RegenerateTiles();
            }
            else if (oldColumns > _columns || oldRows > _rows) // Don't need to add so we can just trim the columns/rows
            {
                // Would be easier if we could use the IList<T>.RemoveAll but UIElementCollection doesn't have it
                for (int i = this.Children.Count - 1; i >= 0; --i)
                {
                    Tile tile = (Tile)this.Children[i];
                    if (tile.Column >= _columns || tile.Row >= _rows)
                    {
                        this.Children.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>Moves the Tile's column by the specified amount.</summary>
        /// <param name="amount">The amount to change the column by.</param>
        private void ChangeColumns(int amount)
        {
            this.ChangeTiles((tile) =>
            {
                tile.Column += amount;
                if (tile.Column < -1)
                {
                    tile.Column = _columns - 1;
                    tile.TileX += _columns + 1;
                }
                else if (tile.Column > _columns - 1)
                {
                    tile.Column = -1;
                    tile.TileX -= _columns + 1;
                }
            });
        }

        /// <summary>Moves the Tile's row by the specified amount.</summary>
        /// <param name="amount">The amount to change the row by.</param>
        private void ChangeRows(int amount)
        {
            this.ChangeTiles((tile) =>
            {
                tile.Row += amount;
                if (tile.Row < -1)
                {
                    tile.Row = _rows - 1;
                    tile.TileY += _rows + 1;
                }
                else if (tile.Row > _rows - 1)
                {
                    tile.Row = -1;
                    tile.TileY -= _rows + 1;
                }
            });
        }

        /// <summary>Repositions the Tiles after any changes.</summary>
        /// <param name="changeTile">Called on every Tile to allow changes to be made to its position.</param>
        private void ChangeTiles(Action<Tile> changeTile)
        {
            _baseTile = (Tile)this.Children[0]; // We need something to compare to so set it to the first.
            for (int i = 0; i < this.Children.Count; ++i)
            {
                Tile tile = (Tile)this.Children[i];
                changeTile(tile);
                Canvas.SetLeft(tile, _mapControlFactory.TileGenerator.TileSize * tile.Column);
                Canvas.SetTop(tile, _mapControlFactory.TileGenerator.TileSize * tile.Row);
                if (tile.TileX <= _baseTile.TileX && tile.TileY <= _baseTile.TileY) // Find the upper left tile
                {
                    _baseTile = tile;
                }
            }
        }

        /// <summary>Clears and the reloads all the Tiles contained in the control.</summary>
        private void RegenerateTiles()
        {
            this.Children.Clear();
            for (int x = -1; x < _columns; ++x)
            {
                for (int y = -1; y < _rows; ++y)
                {
                    Tile tile = _mapControlFactory.GetTile(this.Zoom, this.LeftTile + x, this.TopTile + y);
                    tile.Column = x;
                    tile.Row = y;
                    Canvas.SetLeft(tile, _mapControlFactory.TileGenerator.TileSize * x);
                    Canvas.SetTop(tile, _mapControlFactory.TileGenerator.TileSize * y);
                    this.Children.Add(tile);
                }
            }
            _baseTile = (Tile)this.Children[0];
        }
    }
}
