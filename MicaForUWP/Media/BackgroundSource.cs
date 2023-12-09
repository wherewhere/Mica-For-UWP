using Windows.Foundation.Metadata;
using Windows.Foundation;

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
        [ContractVersion(typeof(UniversalApiContract), 262144u)]
        HostBackdrop = 0,

        /// <summary>
        /// The brush samples from the app content.
        /// </summary>
        [ContractVersion(typeof(UniversalApiContract), 196608u)]
        Backdrop = 1,

        /// <summary>
        /// The brush samples from the wallpaper behind the app window.
        /// </summary>
        [ContractVersion(typeof(UniversalApiContract), 851968u)]
        WallpaperBackdrop = 2,
    }
}
