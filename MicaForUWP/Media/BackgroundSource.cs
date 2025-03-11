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
        HostBackdrop = 0,

        /// <summary>
        /// The brush samples from the app content.
        /// </summary>
        Backdrop = 1,

        /// <summary>
        /// The brush samples from the wallpaper behind the app window.
        /// </summary>
        WallpaperBackdrop = 2
    }
}
