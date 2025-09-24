using System.Collections.Generic;
using System.Xml.Linq;

namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Represents image metadata. Allows storing and retrieving arbitrary profile information.
    /// </summary>
    public sealed class ImageMetadata
    {
        /// <summary>
        /// XMP metadata (XML format).
        /// </summary>
        public XDocument? XmpProfile
        {
            get => TryGet<XDocument>(nameof(XmpProfile));
            set => Set(nameof(XmpProfile), value);
        }

        /// <summary>
        /// All metadata profiles entries.
        /// </summary>
        public Dictionary<string, object> Profiles { get; }

        public ImageMetadata()
            => Profiles = new Dictionary<string, object>();

        public ImageMetadata(IDictionary<string, object> pairs)
            => Profiles = new Dictionary<string, object>(pairs);

        /// <summary>
        /// Tries to get a metadata value by key and cast it to the given type.
        /// </summary>
        public T? TryGet<T>(string key) where T : class
            => Profiles.TryGetValue(key, out var value) && value is T t ? t : null;

        /// <summary>
        /// Sets a metadata value by key.
        /// If the value is null, the key is removed.
        /// </summary>
        public void Set(string key, object? value)
        {
            if (value == null)
                Profiles.Remove(key);
            else
                Profiles[key] = value;
        }
    }

}
