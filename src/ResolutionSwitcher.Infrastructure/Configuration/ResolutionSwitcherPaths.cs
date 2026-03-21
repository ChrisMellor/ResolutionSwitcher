namespace ResolutionSwitcher.Infrastructure.Configuration;

/// <summary>
///     Provides static properties for accessing file system paths used by the ResolutionSwitcher application.
/// </summary>
/// <remarks>
///     This class centralizes the management of key directory and configuration file paths to ensure
///     consistency throughout the application. All paths are based on the user's local application data folder, which is
///     appropriate for storing user-specific settings and data.
/// </remarks>
public static class ResolutionSwitcherPaths
{
    /// <summary>
    ///     Gets the full path to the application's local data directory.
    /// </summary>
    /// <remarks>
    ///     The directory is located within the user's local application data folder and is intended for
    ///     storing application-specific data that does not roam with the user profile.
    /// </remarks>
    public static string DataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ResolutionSwitcher");

    /// <summary>
    ///     Gets the full file system path to the application's configuration file.
    /// </summary>
    public static string ConfigPath => Path.Combine(DataDirectory, "resolution-switcher.json");
}