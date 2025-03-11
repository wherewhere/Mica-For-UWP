using System.Runtime.Versioning;
using Windows.Foundation.Metadata;

namespace MicaForUWP.Helpers
{
    /// <summary>
    /// Gets information about the availability of Windows Runtime APIs.
    /// </summary>
#if NET
#pragma warning disable CA1416
    [SupportedOSPlatform("Windows10.0.10240.0")]
#endif
    public static class ApiInfoHelper
    {
        #region Properties

        /// <summary>
        /// Gets is <see cref="Windows.UI.Core.CoreWindow.ActivationMode"/> supported.
        /// </summary>
#if NET
        [SupportedOSPlatformGuard("Windows10.0.16299.0")]
#endif
        public static bool IsActivationModeSupported { get; } = ApiInformation.IsPropertyPresent("Windows.UI.Core.CoreWindow", "ActivationMode");

        #endregion

        #region Methods

        /// <summary>
        /// Gets is <see cref="Windows.UI.Composition.Compositor.CreateBackdropBrush"/> supported.
        /// </summary>
#if NET
        [SupportedOSPlatformGuard("Windows10.0.14393.0")]
#endif
        public static bool IsCreateBackdropBrushSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateBackdropBrush");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Composition.Compositor.CreateHostBackdropBrush"/> supported.
        /// </summary>
#if NET
        [SupportedOSPlatformGuard("Windows10.0.15063.0")]
#endif
        public static bool IsCreateHostBackdropBrushSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "CreateHostBackdropBrush");

        /// <summary>
        /// Gets is <see cref="Windows.UI.Composition.Compositor.TryCreateBlurredWallpaperBackdropBrush"/> supported.
        /// </summary>
#if NET
        [SupportedOSPlatformGuard("Windows10.0.22000.0")]
#endif
        public static bool IsTryCreateBlurredWallpaperBackdropBrushSupported { get; } = ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush");

        #endregion
    }
}
