using System.Text.Json;
using ResolutionSwitcher.Application.Abstractions.Configuration;
using ResolutionSwitcher.Application.Configuration.Validation;
using ResolutionSwitcher.Application.Exceptions;
using ResolutionSwitcher.Domain.Configuration.Models;

namespace ResolutionSwitcher.Infrastructure.Configuration.Persistence;

/// <summary>
///     Provides an implementation of the IAppConfigurationStore interface that loads and saves application configuration
///     data in JSON format.
/// </summary>
/// <remarks>
///     This class manages reading from and writing to a JSON configuration file on disk. It automatically
///     creates a default configuration file if one does not exist. The configuration file path is specified at
///     construction
///     and is used for all subsequent operations. This class is not thread-safe; callers should ensure appropriate
///     synchronization if used concurrently.
/// </remarks>
public sealed class JsonConfigurationStore : IAppConfigurationStore
{
    /// <summary>
    ///     Provides preconfigured options for JSON serialization and deserialization.
    /// </summary>
    /// <remarks>
    ///     The options enable trailing commas, make property name matching case-insensitive, skip
    ///     comments during reading, and format the output with indentation. These settings are intended to facilitate
    ///     flexible and human-readable JSON processing.
    /// </remarks>
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    /// <summary>
    ///     Initializes a new instance of the JsonConfigurationStore class using the specified configuration file path.
    /// </summary>
    /// <remarks>
    ///     The provided path is converted to an absolute path. If the file does not exist at the
    ///     specified location, subsequent operations may fail.
    /// </remarks>
    /// <param name="configPath">
    ///     The path to the JSON configuration file. This can be a relative or absolute path and must not
    ///     be null or empty.
    /// </param>
    public JsonConfigurationStore(string configPath)
    {
        ConfigPath = Path.GetFullPath(configPath);
    }

    /// <summary>
    ///     Gets the file system path to the configuration file used by the application.
    /// </summary>
    public string ConfigPath { get; }

    /// <summary>
    ///     Loads the application configuration from the specified path, or creates and saves a default configuration if
    ///     none exists.
    /// </summary>
    /// <remarks>
    ///     If the configuration file does not exist at the configured path, a default configuration is
    ///     created and saved before being returned. Subsequent calls will load the existing configuration.
    /// </remarks>
    /// <returns>An instance of <see cref="AppConfiguration" /> representing the loaded or newly created configuration.</returns>
    public AppConfiguration LoadOrCreate()
    {
        if (!File.Exists(ConfigPath))
        {
            var defaultConfiguration = AppConfiguration.CreateDefault();
            Save(defaultConfiguration);
            return defaultConfiguration;
        }

        return Load();
    }

    /// <summary>
    ///     Saves the specified application configuration to the configured file path, replacing any existing configuration
    ///     file.
    /// </summary>
    /// <remarks>
    ///     The method writes the configuration atomically by first serializing to a temporary file and
    ///     then replacing the target file. This helps prevent data corruption if the process is interrupted.
    /// </remarks>
    /// <param name="configuration">The application configuration to save. Cannot be null and must pass validation.</param>
    ///     Thrown if the configuration file directory cannot be determined from the
    ///     configured file path.
    /// </exception>
    public void Save(AppConfiguration configuration)
    {
        AppConfigurationValidator.Validate(configuration);

        var directory = Path.GetDirectoryName(ConfigPath)
                     ?? throw new ConfigurationException($"Unable to determine the config directory for '{ConfigPath}'.");

        Directory.CreateDirectory(directory);

        var temporaryPath = Path.Combine(directory, $"{Path.GetFileName(ConfigPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            using (var stream = File.Create(temporaryPath))
            {
                JsonSerializer.Serialize(stream, configuration, JsonOptions);
            }

            File.Move(temporaryPath, ConfigPath, true);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

           throw;
        }
    }

    /// <summary>
    ///     Loads the application configuration from the file specified by the configuration path.
    /// </summary>
    /// <remarks>
    ///     The configuration file must exist at the specified path and contain valid JSON that matches
    ///     the <see cref="AppConfiguration" /> schema. Validation is performed after deserialization to ensure the
    ///     configuration is correct.
    /// </remarks>
    /// <returns>An instance of <see cref="AppConfiguration" /> containing the deserialized configuration data.</returns>
    /// <exception cref="ConfigurationException">
    ///     Thrown if the configuration file does not exist, is empty, contains invalid
    ///     JSON, or fails validation.
    /// </exception>
    public AppConfiguration Load()
    {
        if (!File.Exists(ConfigPath))
        {
            throw new ConfigurationException($"Config file '{ConfigPath}' was not found.");
        }

        using var stream = File.OpenRead(ConfigPath);
        var configuration = JsonSerializer.Deserialize<AppConfiguration>(stream, JsonOptions)
                         ?? throw new ConfigurationException($"Config file '{ConfigPath}' is empty or invalid JSON.");

        AppConfigurationValidator.Validate(configuration);
     
  return configuration;
    }
}
