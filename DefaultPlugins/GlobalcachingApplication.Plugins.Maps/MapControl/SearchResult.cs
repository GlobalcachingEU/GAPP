using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.Maps.MapControl
{
    /// <summary>Represents data retrieved from nominatim.openstreetmap.org.</summary>
    public class SearchResult
    {
        /// <summary>Initializes a new instance of the SearchResult class.</summary>
        /// <param name="index">The index of the returned search result.</param>
        public SearchResult(int index)
        {
            this.Index = index;
            this.Size = System.Windows.Size.Empty;
        }

        /// <summary>Gets the formatted name of the search result.</summary>
        public string DisplayName { get; internal set; }

        /// <summary>Gets the returned index from the search.</summary>
        public int Index { get; private set; }

        /// <summary>Gets the latitude coordinate of the center of the search result.</summary>
        public double Latitude { get; internal set; }

        /// <summary>Gets the longitude coordinate of the center of the search result.</summary>
        public double Longitude { get; internal set; }

        /// <summary>Gets the size of the search's bounding box.</summary>
        public System.Windows.Size Size { get; internal set; }
    }
}
