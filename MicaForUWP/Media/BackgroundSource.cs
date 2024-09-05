using System.Runtime.Versioning;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace MicaForUWP.Media
{
    /// <summary>
    /// Defines values that specify whether the brush samples from the app content or from the content behind the app window.
    /// </summary>
    public enum BackgroundSource
    {
        /// <summary>
        /// The brush samples from the content behind the app window.
        /// </summary>
#if NET
        [SupportedOSPlatform("Windows10.0.15063.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 0x40000u)]
        HostBackdrop = 0,

        /// <summary>
        /// The brush samples from the app content.
        /// </summary>
#if NET
        [SupportedOSPlatform("Windows10.0.14393.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 0x30000u)]
        Backdrop = 1,

        /// <summary>
        /// The brush samples from the wallpaper behind the app window.
        /// </summary>
#if NET
        [SupportedOSPlatform("Windows10.0.22000.0")]
#endif
        [ContractVersion(typeof(UniversalApiContract), 0xD0000u)]
        WallpaperBackdrop = 2,
    }
}
