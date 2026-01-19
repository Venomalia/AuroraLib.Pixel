# AuroraLib.Pixel

AuroraLib.Pixel is a high-performance C# library designed for direct manipulation of raw pixel data.
It is not intended for image editing, but provides robust tools for efficiently loading, saving, converting, and managing a wide variety of pixel formats.
The library supports an broad range of raw, indexed (palettized), block-compressed, and other specialized pixel formats.

### AuroraLib.Pixel
Core library.
[![NuGet Package](https://img.shields.io/nuget/v/AuroraLib.Pixel.svg?style=flat-square&label=NuGet%20Package)](https://www.nuget.org/packages/AuroraLib.Pixel)

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