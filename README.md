# WPF Image Processor

An image processing application built in WPF where all filters are applied directly to raw pixel data. Part of the "Computer Graphics 1" course at Warsaw University of Technology.

## Features

- **Convolution filters** — blur, sharpen, edge detection, emboss with a custom kernel editor
- **Median filter** — noise removal with configurable window size
- **Function filters** — brightness, contrast, gamma correction
- **Curve-based color adjustment** — interactive control points on a transfer function
- **Random dithering** — configurable levels per channel
- **Median Cut quantization** — reduces image to N colors with palette preview
- **Greyscale conversion**

All filters work on raw byte arrays through a shared `IFilter` interface and `PixelData` abstraction.

## Tech

C#, WPF, XAML, .NET 8
