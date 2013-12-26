using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Collections.Generic;

namespace GAPPSF.MapProviders
{
    public class MapCanvas : Canvas
    {
        public event EventHandler<EventArgs> ZoomChanged;

        /// <summary>Identifies the Latitude attached property.</summary>
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.RegisterAttached("Latitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>Identifies the Longitude attached property.</summary>
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.RegisterAttached("Longitude", typeof(double), typeof(MapCanvas), new PropertyMetadata(double.PositiveInfinity, OnLatitudeLongitudePropertyChanged));

        /// <summary>Identifies the Viewport dependency property.</summary>
        public static readonly DependencyProperty ViewportProperty;

        /// <summary>Identifies the Zoom dependency property.</summary>
        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(int), typeof(MapCanvas), new UIPropertyMetadata(0, OnZoomPropertyChanged, OnZoomPropertyCoerceValue));

        private static readonly DependencyPropertyKey ViewportKey =
            DependencyProperty.RegisterReadOnly("Viewport", typeof(Rect), typeof(MapCanvas), new PropertyMetadata());

        private TilePanel _tilePanel = null;
        private Image _cache = new Image();
        private int _updateCount;
        private bool _mouseCaptured;
        private Point _previousMouse;
        private MapOffset _offsetX;
        private MapOffset _offsetY;
        private TranslateTransform _translate = new TranslateTransform();
        private MapControlFactory _mapControlFactory = null;
        private List<UIElement> _massMarkers = new List<UIElement>();

        /// <summary>Initializes a new instance of the MapCanvas class.</summary>
        static MapCanvas()
        {
            ViewportProperty = ViewportKey.DependencyProperty; // Need to set it here after ViewportKey has been initialized.

            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveDown, (sender, e) => Pan(sender, e.Command, 0, -1)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveLeft, (sender, e) => Pan(sender, e.Command, 1, 0)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveRight, (sender, e) => Pan(sender, e.Command, -1, 0)));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(ComponentCommands.MoveUp, (sender, e) => Pan(sender, e.Command, 0, 1)));

            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(NavigationCommands.DecreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom--));
            CommandManager.RegisterClassCommandBinding(
                typeof(MapCanvas), new CommandBinding(NavigationCommands.IncreaseZoom, (sender, e) => ((MapCanvas)sender).Zoom++));
        }
        public MapCanvas()
        {
            _mapControlFactory = UIControls.Maps.Control.MapControlFactoryToUse;
            if (_mapControlFactory != null)
            {
                _offsetX = _mapControlFactory.GetMapOffset(_translate.GetType().GetProperty("X"), this.OnOffsetChanged);
                _offsetY = _mapControlFactory.GetMapOffset(_translate.GetType().GetProperty("Y"), this.OnOffsetChanged);
                _tilePanel = _mapControlFactory.TilePanel;
                _tilePanel.RenderTransform = _translate;
            }
            this.Background = Brushes.Transparent; // Register all mouse clicks
            this.Children.Add(_cache);
            if (_mapControlFactory != null)
            {
                this.Children.Add(_tilePanel);
            }
            this.ClipToBounds = true;
            this.Focusable = true;
            this.FocusVisualStyle = null;
            this.SnapsToDevicePixels = true;
        }

        public MapControlFactory MapControlFactory
        {
            get { return _mapControlFactory; }
        }

        /// <summary>Gets the visible area of the map in latitude/longitude coordinates.</summary>
        public Rect Viewport
        {
            get { return (Rect)this.GetValue(ViewportProperty); }
            private set { this.SetValue(ViewportKey, value); }
        }

        /// <summary>Gets or sets the zoom level of the map.</summary>
        public int Zoom
        {
            get { return (int)this.GetValue(ZoomProperty); }
            set { this.SetValue(ZoomProperty, value); }
        }

        /// <summary>Gets the value of the Latitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Latitude coordinate of the specified element.</returns>
        public static double GetLatitude(DependencyObject obj)
        {
            return (double)obj.GetValue(LatitudeProperty);
        }

        /// <summary>Gets the value of the Longitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element from which the property value is read.</param>
        /// <returns>The Longitude coordinate of the specified element.</returns>
        public static double GetLongitude(DependencyObject obj)
        {
            return (double)obj.GetValue(LongitudeProperty);
        }

        /// <summary>Sets the value of the Latitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Latitude coordinate of the specified element.</param>
        public static void SetLatitude(DependencyObject obj, double value)
        {
            obj.SetValue(LatitudeProperty, value);
        }

        /// <summary>Sets the value of the Longitude attached property for a given depencency object.</summary>
        /// <param name="obj">The element to which the property value is written.</param>
        /// <param name="value">Sets the Longitude coordinate of the specified element.</param>
        public static void SetLongitude(DependencyObject obj, double value)
        {
            obj.SetValue(LongitudeProperty, value);
        }

        /// <summary>Centers the map on the specified coordinates.</summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="zoom">The zoom level for the map.</param>
        public void Center(double latitude, double longitude, int zoom)
        {
            this.BeginUpdate();
            this.Zoom = zoom;
            _offsetX.CenterOn(_mapControlFactory.TileGenerator.GetTileX(longitude, this.Zoom));
            _offsetY.CenterOn(_mapControlFactory.TileGenerator.GetTileY(latitude, this.Zoom));
            this.EndUpdate();
        }

        /// <summary>Centers the map on the specified coordinates, calculating the required zoom level.</summary>
        /// <param name="latitude">The latitude cooridinate.</param>
        /// <param name="longitude">The longitude coordinates.</param>
        /// <param name="size">The minimum size that must be visible, centered on the coordinates.</param>
        public void Center(double latitude, double longitude, Size size)
        {
            double left = _mapControlFactory.TileGenerator.GetTileX(longitude - (size.Width / 2.0), 0);
            double right = _mapControlFactory.TileGenerator.GetTileX(longitude + (size.Width / 2.0), 0);
            double top = _mapControlFactory.TileGenerator.GetTileY(latitude - (size.Height / 2.0), 0);
            double bottom = _mapControlFactory.TileGenerator.GetTileY(latitude + (size.Height / 2.0), 0);

            double height = (top - bottom) * _mapControlFactory.TileGenerator.TileSize;
            double width = (right - left) * _mapControlFactory.TileGenerator.TileSize;
            int zoom = Math.Min(_mapControlFactory.TileGenerator.GetZoom(this.ActualHeight / height), _mapControlFactory.TileGenerator.GetZoom(this.ActualWidth / width));
            this.Center(latitude, longitude, zoom);
        }

        /// <summary>Creates a static image of the current view.</summary>
        /// <returns>An image of the current map.</returns>
        public ImageSource CreateImage()
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(Math.Max(128,(int)this.ActualWidth), Math.Max(128,(int)this.ActualHeight), 96, 96, PixelFormats.Default);
            bitmap.Render(_tilePanel);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>Calculates the coordinates of the specifed point.</summary>
        /// <param name="point">A point, in pixels, relative to the top left corner of the control.</param>
        /// <returns>A Point filled with the Latitude (Y) and Longitude (X) of the specifide point.</returns>
        public Point GetLocation(Point point)
        {
            Point output = new Point();
            output.X = _mapControlFactory.TileGenerator.GetLongitude((_offsetX.Pixels + point.X) / _mapControlFactory.TileGenerator.TileSize, this.Zoom);
            output.Y = _mapControlFactory.TileGenerator.GetLatitude((_offsetY.Pixels + point.Y) / _mapControlFactory.TileGenerator.TileSize, this.Zoom);
            return output;
        }

        /// <summary>Tries to capture the mouse to enable dragging of the map.</summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Focus(); // Make sure we get the keyboard
            if (e.ClickCount == 2)
            {
                if (this.Zoom < MapControlFactory.TileGenerator.MaxZoom)
                {
                    int newZoom = this.Zoom + 1;
                    Point mouse = e.GetPosition(this);

                    this.BeginUpdate();
                    _offsetX.ChangeZoom(newZoom, mouse.X);
                    _offsetY.ChangeZoom(newZoom, mouse.Y);
                    this.Zoom = newZoom; // Set this after we've altered the offsets
                    this.EndUpdate();
                }
            }
            else if (this.CaptureMouse())
            {
                _mouseCaptured = true;
                _previousMouse = e.GetPosition(null);
            }
        }

        /// <summary>Releases the mouse capture and stops dragging of the map.</summary>
        /// <param name="e">The MouseButtonEventArgs that contains the event data.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.ReleaseMouseCapture();
            _mouseCaptured = false;
        }

        /// <summary>Drags the map, if the mouse was succesfully captured.</summary>
        /// <param name="e">The MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_mouseCaptured)
            {
                this.BeginUpdate();
                Point position = e.GetPosition(null);
                _offsetX.Translate(position.X - _previousMouse.X);
                _offsetY.Translate(position.Y - _previousMouse.Y);
                _previousMouse = position;
                this.EndUpdate();
            }
        }

        /// <summary>Alters the zoom of the map, maintaing the same point underneath the mouse at the new zoom level.</summary>
        /// <param name="e">The MouseWheelEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            int newZoom = _mapControlFactory.TileGenerator.GetValidZoom(this.Zoom + (e.Delta / Mouse.MouseWheelDeltaForOneLine));
            Point mouse = e.GetPosition(this);

            this.BeginUpdate();
            _offsetX.ChangeZoom(newZoom, mouse.X);
            _offsetY.ChangeZoom(newZoom, mouse.Y);
            this.Zoom = newZoom; // Set this after we've altered the offsets
            this.EndUpdate();
        }

        /// <summary>Notifies child controls that the size has changed.</summary>
        /// <param name="sizeInfo">
        /// The packaged parameters (SizeChangedInfo), which includes old and new sizes, and which dimension actually changes.
        /// </param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (_mapControlFactory != null)
            {
                this.BeginUpdate();
                _offsetX.ChangeSize(sizeInfo.NewSize.Width);
                _offsetY.ChangeSize(sizeInfo.NewSize.Height);
                _tilePanel.Width = sizeInfo.NewSize.Width;
                _tilePanel.Height = sizeInfo.NewSize.Height;
                this.EndUpdate();
            }
        }

        private bool IsKeyboardCommand(RoutedCommand command)
        {
            foreach (var gesture in command.InputGestures)
            {
                var key = gesture as KeyGesture;
                if (key != null)
                {
                    if (Keyboard.IsKeyDown(key.Key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void OnLatitudeLongitudePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Search for a MapControl parent
            MapCanvas canvas = null;
            FrameworkElement child = d as FrameworkElement;
            while (child != null)
            {
                canvas = child as MapCanvas;
                if (canvas != null)
                {
                    break;
                }
                child = child.Parent as FrameworkElement;
            }
            if (canvas != null)
            {
                canvas.RepositionChildren();
            }
        }

        private static void OnZoomPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MapCanvas)d).OnZoomChanged();
        }

        private static object OnZoomPropertyCoerceValue(DependencyObject d, object baseValue)
        {
            MapCanvas canvas = null;
            FrameworkElement child = d as FrameworkElement;
            while (child != null)
            {
                canvas = child as MapCanvas;
                if (canvas != null)
                {
                    break;
                }
                child = child.Parent as FrameworkElement;
            }
            if (canvas != null)
            {
                return canvas._mapControlFactory.TileGenerator.GetValidZoom((int)baseValue);
            }
            return null;
        }

        private static void Pan(object sender, ICommand command, double x, double y)
        {
            MapCanvas instance = (MapCanvas)sender;
            if (instance != null)
            {
                if (!instance.IsKeyboardCommand((RoutedCommand)command)) // Move a whole square instead of a pixel if it wasn't the keyboard who sent it
                {
                    x *= instance._mapControlFactory.TileGenerator.TileSize;
                    y *= instance._mapControlFactory.TileGenerator.TileSize;
                }
                instance._offsetX.AnimateTranslate(x);
                instance._offsetY.AnimateTranslate(y);
                instance.Focus();
            }
        }

        private void OnOffsetChanged(object sender, EventArgs e)
        {
            this.BeginUpdate();
            MapOffset offset = (MapOffset)sender;
            offset.Property.SetValue(_translate, offset.Offset, null);
            this.EndUpdate();
        }

        private void OnZoomChanged()
        {
            this.BeginUpdate();
            _offsetX.ChangeZoom(this.Zoom, this.ActualWidth / 2.0);
            _offsetY.ChangeZoom(this.Zoom, this.ActualHeight / 2.0);
            _tilePanel.Zoom = this.Zoom;
            if (ZoomChanged != null)
            {
                ZoomChanged(this, EventArgs.Empty);
            }
            this.EndUpdate();
        }

        private void BeginUpdate()
        {
            _updateCount++;
        }

        private void EndUpdate()
        {
            System.Diagnostics.Debug.Assert(_updateCount != 0, "Must call BeginUpdate first");
            if (--_updateCount == 0)
            {
                _tilePanel.LeftTile = _offsetX.Tile;
                _tilePanel.TopTile = _offsetY.Tile;
                CheckMassMarkers();
                if (_tilePanel.RequiresUpdate)
                {
                    _cache.Visibility = Visibility.Visible; // Display a pretty picture while we play with the tiles
                    _tilePanel.Update(); // This will block our thread for a while (UI events will still be processed)
                    this.RepositionChildren();
                    _cache.Visibility = Visibility.Hidden;
                    _cache.Source = this.CreateImage(); // Save our image for later
                }

                // Update viewport
                Point topleft = this.GetLocation(new Point(0, 0));
                Point bottomRight = this.GetLocation(new Point(this.ActualWidth, this.ActualHeight));
                this.Viewport = new Rect(topleft, bottomRight);
            }
        }

        public void SetMassMarkers(List<UIElement> mm)
        {
            foreach (var m in _massMarkers)
            {
                if (m.Visibility == System.Windows.Visibility.Visible)
                {
                    this.Children.Remove(m);
                }
            }
            _massMarkers.Clear();
            _massMarkers.AddRange(mm);
        }

        public void CheckMassMarkers()
        {
            Point topleft = this.GetLocation(new Point(0, 0));
            Point bottomRight = this.GetLocation(new Point(this.ActualWidth, this.ActualHeight));
            foreach (var m in _massMarkers)
            {
                //determine visible or not
                Marker marker = (m as Grid).DataContext as Marker;
                if (marker.Longitude >= topleft.X && marker.Longitude <= bottomRight.X &&
                    marker.Latitude <= topleft.Y && marker.Latitude >= bottomRight.Y)
                {
                    //in view
                    if (m.Visibility == System.Windows.Visibility.Hidden)
                    {
                        m.Visibility = System.Windows.Visibility.Visible;
                        this.Children.Add(m);
                    }
                }
                else
                {
                    if (m.Visibility == System.Windows.Visibility.Visible)
                    {
                        this.Children.Remove(m);
                        m.Visibility = System.Windows.Visibility.Hidden;
                    }
                }
            }
        }

        public void RepositionChildren()
        {
            foreach (UIElement element in this.Children)
            {
                try
                {
                    double latitude = GetLatitude(element);
                    double longitude = GetLongitude(element);
                    if (latitude == double.PositiveInfinity || longitude == double.PositiveInfinity)
                    {
                        if (element is Grid && (element as Grid).DataContext is Marker)
                        {
                            latitude = ((element as Grid).DataContext as Marker).Latitude;
                            longitude = ((element as Grid).DataContext as Marker).Longitude;
                        }
                    }
                    if (latitude != double.PositiveInfinity && longitude != double.PositiveInfinity)
                    {
                        double x = (_mapControlFactory.TileGenerator.GetTileX(longitude, this.Zoom) - _offsetX.Tile) * _mapControlFactory.TileGenerator.TileSize;
                        double y = (_mapControlFactory.TileGenerator.GetTileY(latitude, this.Zoom) - _offsetY.Tile) * _mapControlFactory.TileGenerator.TileSize;
                        Canvas.SetLeft(element, x);
                        Canvas.SetTop(element, y);
                        element.RenderTransform = _translate;
                    }
                }
                catch
                {
                }
            }
        }
    }
}
