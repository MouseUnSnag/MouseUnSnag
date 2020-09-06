namespace MouseUnSnag.Win32Interop.Display
{
    /// <summary>
    /// See: https://docs.microsoft.com/en-us/windows/win32/api/shellscalingapi/ne-shellscalingapi-monitor_dpi_type
    /// </summary>
    public enum DpiType
    {
        /// <summary>
        /// The effective DPI. This value should be used when determining the correct scale factor for scaling UI elements. This incorporates the scale factor set by the user for this specific display.
        /// </summary>
        Effective = 0,

        /// <summary>
        /// The angular DPI. This DPI ensures rendering at a compliant angular resolution on the screen. This does not include the scale factor set by the user for this specific display.
        /// </summary>
        Angular = 1,

        /// <summary>
        /// The raw DPI. This value is the linear DPI of the screen as measured on the screen itself. Use this value when you want to read the pixel density and not the recommended scaling setting. This does not include the scale factor set by the user for this specific display and is not guaranteed to be a supported DPI value.
        /// </summary>
        Raw = 2
    }
}