<img alt="Mica For UWP LOGO" src="logo.png" width="200px"/>

# Mica For UWP
一个无需 WinUI 便可实现的 Mica 笔刷

[![LICENSE](https://img.shields.io/github/license/wherewhere/Mica-For-UWP.svg?label=License&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/blob/master/LICENSE "LICENSE")
[![Issues](https://img.shields.io/github/issues/wherewhere/Mica-For-UWP.svg?label=Issues&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/wherewhere/Mica-For-UWP.svg?label=Stars&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/stargazers "Stargazers")

[![Microsoft Store](https://img.shields.io/badge/download-Demo-magenta.svg?label=Microsoft%20Store&logo=Microsoft&style=for-the-badge&color=11a2f8)](https://www.microsoft.com/store/apps/9NK6JSM7MDNX "Demo")
[![NuGet](https://img.shields.io/nuget/dt/MicaForUWP.svg?logo=NuGet&style=for-the-badge)](https://www.nuget.org/packages/MicaForUWP "NuGet")

## 目录
- [Mica For UWP](#mica-for-uwp)
  - [目录](#目录)
  - [如何使用](#如何使用)
  - [使用到的模块](#使用到的模块)
  - [参与人员](#参与人员)

## 如何使用
和普通的笔刷一样
```xml
<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:media="using:MicaForUWP.Media">
    <media:BackdropMicaBrush
        x:Key="ApplicationPageBackgroundThemeMicaElementBrush"
        BackgroundSource="Backdrop"
        FallbackColor="{ThemeResource SolidBackgroundFillColorBase}"
        TintColor="{ThemeResource SolidBackgroundFillColorBase}" />
    <media:BackdropMicaBrush
        x:Key="ApplicationPageBackgroundThemeMicaWindowBrush"
        BackgroundSource="HostBackdrop"
        FallbackColor="{ThemeResource SolidBackgroundFillColorBase}"
        TintColor="{ThemeResource SolidBackgroundFillColorBase}" />
    <media:BackdropMicaBrush
        x:Key="ApplicationPageBackgroundThemeMicaWallpaperBrush"
        BackgroundSource="WallpaperBackdrop"
        FallbackColor="{ThemeResource SolidBackgroundFillColorBase}"
        TintColor="{ThemeResource SolidBackgroundFillColorBase}" />
</ResourceDictionary>
```

## 使用到的模块
- [Win2D](https://github.com/Microsoft/Win2D "Win2D")

## 参与人员
[![Contributors](https://contrib.rocks/image?repo=wherewhere/Mica-For-UWP)](https://github.com/wherewhere/Mica-For-UWP/graphs/contributors "Contributors")
