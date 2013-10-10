using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Xml;


namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    /// <summary>Uses nominatim.openstreetmap.org to search for the specified information.</summary>
    public class SearchProvider
    {
        protected List<SearchResult> _results = new List<SearchResult>();

        /// <summary>Occurs when the search has completed.</summary>
        public event EventHandler SearchCompleted;

        /// <summary>Occurs if there were errors during the search.</summary>
        public event EventHandler<SearchErrorEventArgs> SearchError;

        /// <summary>Gets the results returned from the most recent search.</summary>
        public SearchResult[] Results
        {
            get { return _results.ToArray(); }
        }

        /// <summary>Searches for the specified query in the specified area.</summary>
        /// <param name="query">The information to search for.</param>
        /// <param name="area">The area to localize results.</param>
        /// <returns>True if search has started, false otherwise.</returns>
        /// <remarks>
        /// The query is first parsed to see if it is a valid coordinate, if not then then a search
        /// is carried out using nominatim.openstreetmap.org. A return valid of false, therefore,
        /// doesn't indicate the method has failed, just that there was no need to perform an online search.
        /// </remarks>
        public virtual bool Search(string query, Rect area)
        {
            return false;
        }

        protected virtual void OnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
        }

        protected void OnSearchCompleted()
        {
            var callback = this.SearchCompleted;
            if (callback != null)
            {
                callback(this, EventArgs.Empty);
            }
        }

        protected void OnSearchError(string error)
        {
            var callback = this.SearchError;
            if (callback != null)
            {
                callback(this, new SearchErrorEventArgs(error));
            }
        }

        protected bool TryParseLatitudeLongitude(string text)
        {
            string[] tokens = text.Split(new char[] { ',', ' ', '°' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                double latitude, longitude;
                if (double.TryParse(tokens[0], out latitude) && double.TryParse(tokens[1], out longitude))
                {
                    SearchResult result = new SearchResult(1);
                    result.DisplayName = string.Format(CultureInfo.CurrentUICulture, "{0:f4}°, {1:f4}°", latitude, longitude);
                    result.Latitude = latitude;
                    result.Longitude = longitude;
                    _results.Clear();
                    _results.Add(result);
                    this.OnSearchCompleted();
                    return true;
                }
            }
            return false;
        }
    }
}
