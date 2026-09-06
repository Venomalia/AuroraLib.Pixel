# AuroraLib.Pixel

AuroraLib.Pixel is a high-performance C# library designed for direct manipulation of raw pixel data.
It is not intended for image editing, but provides robust tools for efficiently loading, saving, converting, and managing a wide variety of pixel formats.
The library supports an broad range of raw, indexed (palettized), block-compressed, and other specialized pixel formats.

### AuroraLib.Pixel
Core library.
[![NuGet Package](https://img.shields.io/nuget/v/AuroraLib.Pixel.svg?style=flat-square&label=NuGet%20Package)](https://www.nuget.org/packages/AuroraLib.Pixel)

#### Example
Create an RGBA32 image.
``` csharp
   using var image = new MemoryImage<RGBA<byte>>(512, 256);
```

Create an palette-based image using RGBA32 colors.
``` csharp
   using var image = new PaletteImage<I<byte>, RGBA<byte>>(512, 256);
```

Create an block-compressed image using the BC1 compression format.
``` csharp
   using var image = new BlockImage<RGBA<byte>>(new BC1Block<RGBA<byte>>(), 512, 256);   
```

Most color types are built from generic channel types that determine the precision and numeric representation of each channel.
For example, `RGBA<T>` can be represented using different component types:
```csharp
RGBA<byte> RGBA32U;  // 8-bit unsigned channels.
RGBA<ushort> RGBA64U; // 16-bit unsigned channels.
RGBA<sbyte> RGBA32S;  // 8-bit signed channels.
RGBA<float> RGBA128F;  // 32-bit floating-point channels.
```
   
### AuroraLib.Pixel.BitmapExtension
Provides integration for System.Drawing.Bitmap.
[![NuGet Package BitmapExtension](https://img.shields.io/nuget/v/AuroraLib.Pixel.BitmapExtension.svg?style=flat-square&label=NuGet%20Package)](https://www.nuget.org/packages/AuroraLib.Pixel.BitmapExtension)

#### Example
Converts a Bitmap to an Aurora Image: Directly accesses the memory without copying.
``` csharp
    using Bitmap bitmap = new Bitmap("Test.png");
    using IImage imageAurora = bitmap.AsAuroraImage();
```
Converts an Aurora Image to a Bitmap.
 ``` csharp
    using IImage<RGBA<byte>> imageAurora = new MemoryImage<RGBA<byte>>(10, 10);
    using Bitmap bitmap = imageAurora.CloneAsBitmap(PixelFormat.Format32bppArgb);
```

### AuroraLib.Pixel.ImageSharpExtension
Provides integration for ImageSharp.
[![NuGet Package ImageSharpExtension](https://img.shields.io/nuget/v/AuroraLib.Pixel.ImageSharpExtension.svg?style=flat-square&label=NuGet%20Package)](https://www.nuget.org/packages/AuroraLib.Pixel.ImageSharpExtension)

#### Example
Try to converts a ImageSharp.Image to an Aurora Image: Directly accesses the memory without copying.
``` csharp
    using Image imageSixLabors = Image.Load("Test.png");
    if (imageSixLabors.TryAsAuroraImage(out IImage imageAurora))
    {
                
    }
```
Converts an Aurora Image to a ImageSharp.Image.
 ``` csharp
    using IImage<RGBA<byte>> imageAurora = new MemoryImage<RGBA<byte>>(10, 10);
    using Image<Rgba32> imageSixLabors = imageAurora.CloneAsImageSharp<Rgba32>();
```

###  AuroraLib.Pixel.SkiaSharpExtension 
Provides integration for SkiaSharp.
[![NuGet Package SkiaSharpExtension](https://img.shields.io/nuget/v/AuroraLib.Pixel.SkiaSharpExtension.svg?style=flat-square&label=NuGet%20Package)](https://www.nuget.org/packages/AuroraLib.Pixel.SkiaSharpExtension)

#### Example
Converts a SKBitmap to an Aurora Image: Directly accesses the memory without copying.
``` csharp
    using SKBitmap bitmap = new SKBitmap(10, 10, SKColorType.Argb4444, SKAlphaType.Opaque);
    using IImage imageAurora = bitmap.AsAuroraImage();
```
Converts an Aurora Image to a SKBitmap.
 ``` csharp
    using IImage<RGBA<byte>> imageAurora = new MemoryImage<RGBA<byte>>(10, 10);
    using SKBitmap bitmap = imageAurora.CloneAsSKBitmap(SKColorType.Bgra8888);
```