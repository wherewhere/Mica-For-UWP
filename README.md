# Mica For UWP
一个无需WinUI便可实现的Mica笔刷库

[![LICENSE](https://img.shields.io/github/license/wherewhere/Mica-For-UWP.svg?label=License&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/blob/master/LICENSE "LICENSE")
[![Issues](https://img.shields.io/github/issues/wherewhere/Mica-For-UWP.svg?label=Issues&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/wherewhere/Mica-For-UWP.svg?label=Stars&style=flat-square)](https://github.com/wherewhere/Mica-For-UWP/stargazers "Stargazers")

[![GitHub All Releases](https://img.shields.io/github/downloads/wherewhere/Mica-For-UWP/total.svg?label=DOWNLOAD&logo=github&style=for-the-badge)](https://github.com/wherewhere/Mica-For-UWP/releases/latest "GitHub All Releases")
[![NuGet](https://img.shields.io/nuget/dt/MicaForUWP.svg?logo=NuGet&style=for-the-badge)](https://www.nuget.org/packages/MicaForUWP "NuGet")

## 目录
- [Mica For UWP](#mica-for-uwp)
  - [目录](#目录)
  - [如何使用](#如何使用)

## 如何使用
和普通的笔刷一样
```
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
        BackgroundSource="MicaBackdrop"
        FallbackColor="{ThemeResource SolidBackgroundFillColorBase}"
        TintColor="{ThemeResource SolidBackgroundFillColorBase}" />
</ResourceDictionary>
```
