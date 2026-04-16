using System.Collections.Generic;
using System.Xml.Linq;

namespace AuroraLib.Pixel.Metadata
{
    /// <summary>
    /// Represents image metadata. Allows storing and retrieving arbitrary profile information.
    /// </summary>
    public sealed class ImageMetadata
    {
        private const float InchToCm = 2.54f;

        /// <summary>
        /// Pixel density expressed as pixels per inch for X and Y axis, also known as (DPI).
        /// </summary>
        public Vector2 PixelsPerInch
        {
            get => Profiles.TryGetValue(nameof(PixelsPerInch), out var value) && value is Vector2 ppi ? ppi : new Vector2(96);
            set => Set(nameof(PixelsPerInch), value);
        }

        /// <summary>
        /// Pixel density expressed as pixels per centimeter for X and Y axis.
        /// </summary>
        public Vector2 PixelsPerCentimeter
        {
            get => PixelsPerInch / InchToCm;
            set => PixelsPerInch = value * InchToCm;
        }

        /// <summary>
        /// Gamma exponent describing the transfer from linear light to stored pixel values.
        /// </summary>
        public float Gamma
        {
            get => Profiles.TryGetValue(nameof(Gamma), out var value) && value is float g ? g : 1;
            set => Set(nameof(Gamma), value);
        }

        /// <summary>
        /// Stores arbitrary textual metadata associated with the image.
        /// </summary>
        public Dictionary<string, string> Text
        {
            get
            {
                if (Profiles.TryGetValue(nameof(Text), out var value) && value is Dictionary<string, string> t)
                    return t;

                t = new Dictionary<string, string>();
                Profiles.Add(nameof(Text), t);
                return t;
            }
            set => Set(nameof(Text), value);
        }

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
