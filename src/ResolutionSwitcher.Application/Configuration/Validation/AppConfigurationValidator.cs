using ResolutionSwitcher.Application.Exceptions;
using ResolutionSwitcher.Domain.Configuration.Models;

namespace ResolutionSwitcher.Application.Configuration.Validation;

/// <summary>
///     Provides methods for validating application configuration settings to ensure they meet required constraints before
///     use.
/// </summary>
/// <remarks>
///     This class is intended to be used to verify that an AppConfiguration instance and its associated
///     profiles are correctly specified. Validation failures result in a ConfigurationException being thrown, allowing
///     callers to detect and handle configuration errors early in the application lifecycle.
/// </remarks>
public static class AppConfigurationValidator
{
    /// <summary>
    ///     Validates the specified application configuration and throws an exception if any required settings are invalid.
    /// </summary>
    /// <param name="configuration">
    ///     The application configuration to validate. Must not be null and must contain valid
    ///     settings.
    /// </param>
    /// <exception cref="ConfigurationException">
    ///     Thrown if the configuration is invalid, such as when PollIntervalSeconds is
    ///     less than 1 or Profiles is null.
    /// </exception>
    public static void Validate(AppConfiguration configuration)
    {
        if (configuration.PollIntervalSeconds < 1)
        {
            throw new ConfigurationException("PollIntervalSeconds must be at least 1.");
        }

        if (configuration.Profiles is null)
        {
            throw new ConfigurationException("Profiles cannot be null.");
        }

        for (var index = 0; index < configuration.Profiles.Count; index++)
        {
            ValidateProfile(configuration.Profiles[index], index);
        }
    }

    /// <summary>
    ///     Validates the specified game profile and throws a configuration exception if any required property is missing or
    ///     invalid.
    /// </summary>
    /// <param name="profile">The game profile to validate. Must not be null and must contain valid property values.</param>
    /// <param name="index">The zero-based index of the profile in the collection. Used for error reporting.</param>
    /// <exception cref="ConfigurationException">
    ///     Thrown if the profile is missing required information or contains invalid values, such as a missing process
    ///     name, invalid normalized process name, non-positive width or height, or non-positive refresh rate.
    /// </exception>
    private static void ValidateProfile(GameProfile profile, int index)
    {
        var profileNumber = index + 1;

        if (string.IsNullOrWhiteSpace(profile.ProcessName))
        {
            throw new ConfigurationException($"Profile {profileNumber} is missing ProcessName.");
        }

        if (string.IsNullOrWhiteSpace(profile.NormalizedProcessName))
        {
            throw new ConfigurationException($"Profile {profileNumber} has an invalid ProcessName.");
        }

        if ((profile.Width <= 0) || (profile.Height <= 0))
        {
            throw new ConfigurationException($"Profile {profileNumber} must use positive Width and Height values.");
        }

        if (profile.RefreshRate is <= 0)
        {
            throw new ConfigurationException($"Profile {profileNumber} has an invalid RefreshRate.");
        }
    }
}
