using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Windows.Media.Animation;
using System.IO;
using System.Xml;
using System.Windows.Markup;

namespace GlobalcachingApplication.Plugins.Maps
{
    /// <summary>
    /// Interaction logic for MapContainerControl.xaml
    /// </summary>
    public partial class MapContainerControl : UserControl
    {
        private int _maxDownload = -1;
        private MapControl.MapControlFactory _mapControlFactory;
        private List<UIElement> _waypointMarkers = new List<UIElement>();
        private List<UIElement> _geocacheMarkers = new List<UIElement>();

        public event Framework.EventArguments.GeocacheEventHandler GeocacheClick;

        public MapContainerControl()
        {
            _mapControlFactory = MapControl.MapCanvas.MapControlFactoryToUse;

            if (_mapControlFactory != null)
            {
                _mapControlFactory.TileGenerator.DownloadCountChanged += this.OnDownloadCountChanged;
                _mapControlFactory.TileGenerator.DownloadError += this.OnDownloadError;
            }
            this.InitializeComponent();
            CommandManager.AddPreviewExecutedHandler(this, this.PreviewExecuteCommand); // We're going to do some effects when zooming.
            if (_mapControlFactory != null && _mapControlFactory.ID == "GoogleCache")
            {
                creditsinfo.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public MapControl.MapCanvas MapCanvas
        {
            get { return this.tileCanvas; }
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri); // Launch the site in the user's default browser.
        }

        private void OnDownloadCountChanged(object sender, EventArgs e)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.OnDownloadCountChanged(sender, e)), null);
                return;
            }
            if (_mapControlFactory.TileGenerator.DownloadCount == 0)
            {
                this.label.Visibility = Visibility.Hidden;
                this.progress.Visibility = Visibility.Hidden;
                _maxDownload = -1;
            }
            else
            {
                this.errorBar.Visibility = Visibility.Collapsed;

                if (_maxDownload < _mapControlFactory.TileGenerator.DownloadCount)
                {
                    _maxDownload = _mapControlFactory.TileGenerator.DownloadCount;
                }
                this.progress.Value = 100 - (_mapControlFactory.TileGenerator.DownloadCount * 100.0 / _maxDownload);
                this.progress.Visibility = Visibility.Visible;
                this.label.Text = string.Format(
                    CultureInfo.CurrentUICulture,
                    "Downloading {0} item{1}",
                    _mapControlFactory.TileGenerator.DownloadCount,
                    _mapControlFactory.TileGenerator.DownloadCount != 1 ? 's' : ' ');
                this.label.Visibility = Visibility.Visible;
            }
        }

        private void OnDownloadError(object sender, EventArgs e)
        {
            if (this.Dispatcher.Thread != Thread.CurrentThread)
            {
                this.Dispatcher.BeginInvoke(new Action(() => this.OnDownloadError(sender, e)), null);
                return;
            }

            this.errorBar.Text = "Unable to contact the server to download map data.";
            this.errorBar.Visibility = Visibility.Visible;
        }

        public void SetCacheMarker(MapControl.Marker marker)
        {
            if (marker == null)
            {
                this.cacheMarker.Visibility = Visibility.Hidden;
            }
            else
            {
                this.tileCanvas.Center(marker.Latitude, marker.Longitude, this.tileCanvas.Zoom);
                this.cacheMarker.Visibility = Visibility.Visible;
            }
            this.cacheMarker.DataContext = marker;
        }

        public void SetCurposMarker(MapControl.Marker marker)
        {
            if (marker == null)
            {
                this.curposMarker.Visibility = Visibility.Hidden;
                this.gotoCurpos.Visibility = Visibility.Hidden;
            }
            else
            {
                if (this.curposMarker.Visibility != Visibility.Visible)
                {
                    this.curposMarker.Visibility = Visibility.Visible;
                }
                if (this.gotoCurpos.Visibility != Visibility.Visible)
                {
                    this.gotoCurpos.Visibility = Visibility.Visible;
                }
            }
            this.curposMarker.DataContext = marker;
        }


        public void SetGeocacheMarkers(List<MapControl.Marker> markers)
        {
            _geocacheMarkers.Clear();

            if (markers != null && markers.Count > 0)
            {
                System.Windows.Media.RadialGradientBrush rb1 = new RadialGradientBrush(Color.FromArgb(0xFF, 0x0C, 0x5A, 0xA6), Color.FromArgb(0x66, 0x0C, 0x5A, 0xA6));
                System.Windows.Media.RadialGradientBrush rb2 = new RadialGradientBrush(Color.FromArgb(0xFF, 0xFF, 0xAE, 0x00), Color.FromArgb(0x66, 0xFF, 0xAE, 0x00));
                System.Windows.Media.RadialGradientBrush rb3 = new RadialGradientBrush(Color.FromArgb(0xFF, 0xA6, 0x0C, 0x00), Color.FromArgb(0x66, 0xA6, 0x0C, 0x00));

                System.Windows.Media.Effects.DropShadowEffect effect = new System.Windows.Media.Effects.DropShadowEffect();
                effect.Color = Colors.Black;
                effect.BlurRadius = 3;
                effect.ShadowDepth = 2;

                foreach (var m in markers)
                {
                    Grid rootElement = new Grid();
                    rootElement.Height = 34;
                    rootElement.Width = 34;
                    rootElement.Margin = new Thickness(-17, -34, 0, 0);
                    rootElement.Visibility = System.Windows.Visibility.Hidden;

                    if (m.ImagePath.Length > 0)
                    {
                        Image imgElement = new Image();
                        BitmapImage bi3 = new BitmapImage();
                        bi3.BeginInit();
                        bi3.UriSource = new Uri(m.ImagePath);
                        bi3.EndInit();
                        imgElement.Stretch = Stretch.None;
                        imgElement.Source = bi3;
                        rootElement.Children.Add(imgElement);
                    }
                    else
                    {
                        System.Windows.Shapes.Ellipse el = new Ellipse();
                        el.Width = 34;
                        el.Height = 34;
                        int cnt = int.Parse(m.Tag.ToString());
                        if (cnt < 10)
                        {
                            el.Fill = rb1;
                        }
                        else if (cnt < 50)
                        {
                            el.Fill = rb2;
                        }
                        else
                        {
                            el.Fill = rb3;
                        }
                        el.StrokeThickness = 2;

                        TextBlock elT = new TextBlock();
                        elT.Foreground = System.Windows.Media.Brushes.White;
                        elT.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        elT.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                        elT.Text = m.Tag.ToString();
                        elT.FontSize = 18;

                        /*
                         *                           <TextBlock.Effect>
                            <DropShadowEffect BlurRadius="3" Color="#C222" ShadowDepth="2" />
                          </TextBlock.Effect>

                         */
                        elT.Effect = effect;

                        rootElement.Children.Add(el);
                        rootElement.Children.Add(elT);
                    }

                    rootElement.DataContext = m;
                    rootElement.MouseDown += new MouseButtonEventHandler(rootElement_MouseDown);
                    _geocacheMarkers.Add(rootElement);
                }
            }
            tileCanvas.SetMassMarkers(_geocacheMarkers);
        }

        void rootElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MapControl.Marker m = (sender as Grid).DataContext as MapControl.Marker;
            if (m.Tag is Framework.Data.Geocache)
            {
                if (GeocacheClick != null)
                {
                    GeocacheClick(this, new Framework.EventArguments.GeocacheEventArgs(m.Tag as Framework.Data.Geocache));
                }
            }
            else
            {
                if (tileCanvas.Zoom < _mapControlFactory.TileGenerator.MaxZoom)
                {
                    tileCanvas.Center(m.Latitude, m.Longitude, tileCanvas.Zoom+1);
                }
            }
        }

        public void SetWaypointMarkers(List<MapControl.Marker> markers)
        {
            foreach (var m in _waypointMarkers)
            {
                tileCanvas.Children.Remove(m);
            }
            _waypointMarkers.Clear();

            if (markers != null && markers.Count > 0)
            {
                foreach (var m in markers)
                {
                    Grid rootElement = new Grid();
                    rootElement.Height = 34;
                    rootElement.Width = 34;
                    rootElement.Margin = new Thickness(-17, -34, 0, 0);
                    rootElement.Visibility = System.Windows.Visibility.Visible;

                    Image imgElement = new Image();
                    BitmapImage bi3 = new BitmapImage();
                    bi3.BeginInit();
                    bi3.UriSource = new Uri(m.ImagePath);
                    bi3.EndInit();
                    imgElement.Stretch = Stretch.None;
                    imgElement.Source = bi3;

                    rootElement.Children.Add(imgElement);
                    rootElement.DataContext = m;

                    _waypointMarkers.Add(rootElement);
                    this.tileCanvas.Children.Add(rootElement);
                }
            }
        }

        private void OnSearchControlNavigate(object sender, NavigateEventArgs e)
        {
            if (e.Result == null) // The results have been cleared - hide the marker.
            {
                this.searchMarker.Visibility = Visibility.Hidden;
            }
            else
            {
                this.searchMarker.Visibility = Visibility.Visible;

                this.tileCanvas.Focus();
                if (e.Result.Size.IsEmpty)
                {
                    this.tileCanvas.Center(e.Result.Latitude, e.Result.Longitude, this.tileCanvas.Zoom);
                }
                else
                {
                    this.tileCanvas.Center(e.Result.Latitude, e.Result.Longitude, e.Result.Size);
                }
            }
            this.searchMarker.DataContext = e.Result;
        }

        private void OnZoomStoryboardCompleted(object sender, EventArgs e)
        {
            this.zoomGrid.Visibility = Visibility.Hidden;
            this.zoomImage.Source = null;
        }

        private void PreviewExecuteCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == NavigationCommands.DecreaseZoom)
            {
                if (this.tileCanvas.Zoom > 0) // Make sure we can actualy zoom out
                {
                    this.StartZoom("zoomOut", 1);
                }
            }
            else if (e.Command == NavigationCommands.IncreaseZoom)
            {
                if (this.tileCanvas.Zoom < _mapControlFactory.TileGenerator.MaxZoom)
                {
                    this.StartZoom("zoomIn", 0.5);
                }
            }
        }

        private void StartZoom(string name, double scale)
        {
            this.zoomImage.Source = this.tileCanvas.CreateImage();
            this.zoomRectangle.Height = this.tileCanvas.ActualHeight * scale;
            this.zoomRectangle.Width = this.tileCanvas.ActualWidth * scale;

            this.zoomGrid.RenderTransform = new ScaleTransform(); // Clear the old transform
            this.zoomGrid.Visibility = Visibility.Visible;
            ((Storyboard)this.zoomGrid.FindResource(name)).Begin();
        }

        private void gotoCurpos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MapControl.Marker m = curposMarker.DataContext as MapControl.Marker;
            if (m != null)
            {
                this.tileCanvas.Center(m.Latitude, m.Longitude, this.tileCanvas.Zoom);
            }
        }

    }
}
