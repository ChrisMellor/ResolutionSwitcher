using ResolutionSwitcher.Application.Configuration.Commands;
using ResolutionSwitcher.Application.Configuration.Queries;
using ResolutionSwitcher.Application.Display.Queries;
using ResolutionSwitcher.Application.Exceptions;
using ResolutionSwitcher.Application.Processes.Queries;
using ResolutionSwitcher.Desktop.UI.Models;
using ResolutionSwitcher.Domain.Configuration.Models;
using ResolutionSwitcher.Domain.Display.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop.UI.Forms;

/// <summary>
///     Represents the main window for editing game profiles and managing display resolution switching settings.
/// </summary>
/// <remarks>
///     This form provides a user interface for configuring game profiles, adjusting polling intervals,
///     enabling or disabling the watcher, and saving or reloading configuration files. It displays profiles in a grid,
///     allows users to add, edit, remove, and reorder profiles, and provides access to the configuration folder. The form
///     is intended for use on Windows platforms and is not thread-safe. Closing the window minimizes the application to
///     the
///     system tray, where it continues monitoring if enabled.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class MainForm : Form
{
    private readonly string _configPath;
    private readonly ToolStripStatusLabel _configPathLabel = new ToolStripStatusLabel();
    private readonly Button _editButton = new Button();
    private readonly IGetDisplayModesQuery _getDisplayModesQuery;
    private readonly IGetDisplaysQuery _getDisplaysQuery;
    private readonly IGetRunningProcessesQuery _getRunningProcessesQuery;
    private readonly ILoadConfigurationQuery _loadConfigurationQuery;
    private readonly Button _moveDownButton = new Button();
    private readonly Button _moveUpButton = new Button();
    private readonly NumericUpDown _pollIntervalUpDown = new NumericUpDown();
    private readonly BindingList<GameProfileListItem> _profiles = [];
    private readonly DataGridView _profilesGrid = new DataGridView();
    private readonly Button _removeButton = new Button();
    private readonly ISaveConfigurationCommand _saveConfigurationCommand;
    private readonly ToolStripStatusLabel _statusLabel = new ToolStripStatusLabel();
    private readonly CheckBox _watcherEnabledCheckBox = new CheckBox();

    private bool _isLoading;

    /// <summary>
    ///     Initializes a new instance of the MainForm class with the specified configuration and service dependencies.
    /// </summary>
    /// <remarks>
    ///     This constructor sets up the main form, initializes its layout, and wires up event handlers
    ///     for configuration management. All dependencies must be provided to ensure correct operation.
    /// </remarks>
    /// <param name="loadConfigurationQuery">The query used to load application configuration data.</param>
    /// <param name="saveConfigurationCommand">The command used to save application configuration data.</param>
    /// <param name="getDisplaysQuery">The query used to retrieve information about available display devices.</param>
    /// <param name="getDisplayModesQuery">The query used to retrieve supported display modes for connected displays.</param>
    /// <param name="getRunningProcessesQuery">The query used to obtain information about currently running processes.</param>
    /// <param name="configPath">The file path to the application's configuration file. Cannot be null or empty.</param>
    public MainForm(ILoadConfigurationQuery loadConfigurationQuery,
        ISaveConfigurationCommand saveConfigurationCommand,
        IGetDisplaysQuery getDisplaysQuery,
        IGetDisplayModesQuery getDisplayModesQuery,
        IGetRunningProcessesQuery getRunningProcessesQuery,
        string configPath)
    {
        _loadConfigurationQuery = loadConfigurationQuery;
        _saveConfigurationCommand = saveConfigurationCommand;
        _getDisplaysQuery = getDisplaysQuery;
        _getDisplayModesQuery = getDisplayModesQuery;
        _getRunningProcessesQuery = getRunningProcessesQuery;
        _configPath = configPath;

        Text = "Resolution Switcher";
        MinimumSize = new Size(860, 540);
        StartPosition = FormStartPosition.CenterScreen;

        InitializeLayout();

        Load += (_, _) => LoadConfigurationIntoEditor();
        _profilesGrid.SelectionChanged += (_, _) => UpdateButtonStates();
        _watcherEnabledCheckBox.CheckedChanged += (_, _) => PersistConfigurationFromEditor();
        _pollIntervalUpDown.ValueChanged += (_, _) => PersistConfigurationFromEditor();
    }

    /// <summary>
    ///     Occurs when the configuration has been successfully saved.
    /// </summary>
    /// <remarks>
    ///     Subscribers can use this event to perform actions after configuration changes are persisted.
    ///     The event is raised after the save operation completes.
    /// </remarks>
    public event EventHandler? ConfigurationSaved;

    /// <summary>
    ///     Opens a dialog to create a new profile and adds it to the current collection if the user confirms the operation.
    /// </summary>
    /// <remarks>
    ///     The method displays a profile editor dialog to the user. If the user completes the dialog
    ///     successfully, the new profile is added to the collection and the configuration is updated. No action is taken if
    ///     the dialog is canceled.
    /// </remarks>
    private void AddProfile()
    {
        using var dialog = new ProfileEditorForm(_getDisplaysQuery, _getDisplayModesQuery, _getRunningProcessesQuery);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var profiles = _profiles.Select(item => item.Profile)
                                .Append(dialog.Profile)
                                .ToList();
        ReplaceProfileItems(profiles, _getDisplaysQuery.Execute(), profiles.Count - 1);
        PersistConfigurationFromEditor();
    }

    /// <summary>
    ///     Opens an editor dialog for the currently selected profile and updates the profile list if changes are confirmed.
    /// </summary>
    /// <remarks>
    ///     If no profile is selected, the method performs no action. The profile list is updated only
    ///     when the user confirms changes in the editor dialog. The configuration is persisted after a successful
    ///     edit.
    /// </remarks>
    private void EditSelectedProfile()
    {
        var selectedItem = GetSelectedProfileItem();
        if (selectedItem is null)
        {
            return;
        }

        using var dialog = new ProfileEditorForm(_getDisplaysQuery, _getDisplayModesQuery, _getRunningProcessesQuery, selectedItem.Profile);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var index = _profiles.IndexOf(selectedItem);
        var profiles = _profiles.Select(item => item.Profile)
                                .ToList();
        profiles[index] = dialog.Profile;
        ReplaceProfileItems(profiles, _getDisplaysQuery.Execute(), index);
        PersistConfigurationFromEditor();
    }

    /// <summary>
    ///     Retrieves the currently selected game profile item from the profiles grid.
    /// </summary>
    /// <returns>
    ///     The selected <see cref="GameProfileListItem" /> if a row is selected and bound to a profile item; otherwise,
    ///     <see
    ///         langword="null" />
    ///     .
    /// </returns>
    private GameProfileListItem? GetSelectedProfileItem()
    {
        if (_profilesGrid.CurrentRow?.DataBoundItem is GameProfileListItem item)
        {
            return item;
        }

        return null;
    }

    /// <summary>
    ///     Initializes and arranges the user interface layout for the profile editor form, including controls for profile
    ///     management, watcher settings, and status display.
    /// </summary>
    /// <remarks>
    ///     This method sets up all UI elements and their layout within the form. It should be called
    ///     during form initialization before the form is displayed to ensure all controls are properly configured and event
    ///     handlers are attached. Calling this method multiple times may result in duplicate controls or unexpected
    ///     behavior.
    /// </remarks>
    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var infoLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = "This app edits game profiles and watches in the system tray while it is running. Highest priority is the top row.",
            Padding = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(infoLabel, 0, 0);

        var settingsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 0, 0, 8)
        };

        _watcherEnabledCheckBox.AutoSize = true;
        _watcherEnabledCheckBox.Text = "Watcher enabled";

        var pollLabel = new Label
        {
            AutoSize = true,
            Text = "Poll every",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(16, 6, 0, 0)
        };

        _pollIntervalUpDown.Minimum = 1;
        _pollIntervalUpDown.Maximum = 60;
        _pollIntervalUpDown.Value = 3;
        _pollIntervalUpDown.Width = 60;

        var secondsLabel = new Label
        {
            AutoSize = true,
            Text = "seconds",
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 6, 0, 0)
        };

        var openConfigButton = new Button
        {
            AutoSize = true,
            Text = "Open config folder",
            Margin = new Padding(20, 0, 0, 0)
        };
        openConfigButton.Click += (_, _) => OpenConfigFolder();

        settingsPanel.Controls.Add(_watcherEnabledCheckBox);
        settingsPanel.Controls.Add(pollLabel);
        settingsPanel.Controls.Add(_pollIntervalUpDown);
        settingsPanel.Controls.Add(secondsLabel);
        settingsPanel.Controls.Add(openConfigButton);
        root.Controls.Add(settingsPanel, 0, 1);

        _profilesGrid.Dock = DockStyle.Fill;
        _profilesGrid.AllowUserToAddRows = false;
        _profilesGrid.AllowUserToDeleteRows = false;
        _profilesGrid.AllowUserToResizeRows = false;
        _profilesGrid.AutoGenerateColumns = false;
        _profilesGrid.MultiSelect = false;
        _profilesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _profilesGrid.ReadOnly = true;
        _profilesGrid.RowHeadersVisible = false;
        _profilesGrid.DataSource = _profiles;
        _profilesGrid.DoubleClick += (_, _) => EditSelectedProfile();

        _profilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GameProfileListItem.Priority),
            HeaderText = "Priority",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
        });
        _profilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GameProfileListItem.Name),
            HeaderText = "Profile",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });
        _profilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GameProfileListItem.ProcessName),
            HeaderText = "Process",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });
        _profilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GameProfileListItem.DisplayLabel),
            HeaderText = "Display",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 30
        });
        _profilesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(GameProfileListItem.ResolutionLabel),
            HeaderText = "Resolution",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            FillWeight = 20
        });

        root.Controls.Add(_profilesGrid, 0, 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 8, 0, 0)
        };

        var addButton = new Button
        {
            AutoSize = true,
            Text = "Add profile"
        };
        addButton.Click += (_, _) => AddProfile();

        _editButton.AutoSize = true;
        _editButton.Text = "Edit";
        _editButton.Click += (_, _) => EditSelectedProfile();

        _moveUpButton.AutoSize = true;
        _moveUpButton.Text = "Move up";
        _moveUpButton.Click += (_, _) => MoveSelectedProfile(-1);

        _moveDownButton.AutoSize = true;
        _moveDownButton.Text = "Move down";
        _moveDownButton.Click += (_, _) => MoveSelectedProfile(1);

        _removeButton.AutoSize = true;
        _removeButton.Text = "Remove";
        _removeButton.Click += (_, _) => RemoveSelectedProfile();

        var reloadButton = new Button
        {
            AutoSize = true,
            Text = "Reload from disk"
        };
        reloadButton.Click += (_, _) => LoadConfigurationIntoEditor();

        buttonsPanel.Controls.Add(addButton);
        buttonsPanel.Controls.Add(_editButton);
        buttonsPanel.Controls.Add(_moveUpButton);
        buttonsPanel.Controls.Add(_moveDownButton);
        buttonsPanel.Controls.Add(_removeButton);
        buttonsPanel.Controls.Add(reloadButton);
        root.Controls.Add(buttonsPanel, 0, 3);

        var statusStrip = new StatusStrip();
        _statusLabel.Spring = true;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        _statusLabel.Text = "Profiles save here. Closing the window keeps watching in the tray.";
        _configPathLabel.Text = _configPath;
        statusStrip.Items.Add(_statusLabel);
        statusStrip.Items.Add(_configPathLabel);
        Controls.Add(statusStrip);

        UpdateButtonStates();
    }

    /// <summary>
    ///     Loads the current application configuration and updates the editor UI to reflect its values.
    /// </summary>
    /// <remarks>
    ///     If the configuration cannot be loaded due to a validation error, a blank default
    ///     configuration is loaded instead. The method updates relevant UI controls and status messages to match the loaded
    ///     configuration.
    /// </remarks>
    private void LoadConfigurationIntoEditor()
    {
        AppConfiguration configuration;

        try
        {
            configuration = _loadConfigurationQuery.Execute();
        }
        catch (ConfigurationException exception)
        {
            MessageBox.Show(
                $"{exception.Message}{Environment.NewLine}{Environment.NewLine}A blank configuration will be loaded into the editor. Save your changes to replace the invalid file.",
                "Configuration Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);

            configuration = AppConfiguration.CreateDefault();
            SetStatus("Loaded blank configuration after validation failure.");
        }

        var displays = _getDisplaysQuery.Execute();

        _isLoading = true;
        try
        {
            _watcherEnabledCheckBox.Checked = configuration.WatcherEnabled;
            _pollIntervalUpDown.Value = Math.Clamp(configuration.PollIntervalSeconds, (int)_pollIntervalUpDown.Minimum, (int)_pollIntervalUpDown.Maximum);

            ReplaceProfileItems(configuration.Profiles, displays);
        }
        finally
        {
            _isLoading = false;
        }

        UpdateButtonStates();
        SetStatus($"Loaded {configuration.Profiles.Count} profile(s).");
    }

    /// <summary>
    ///     Moves the currently selected profile in the list by the specified offset.
    /// </summary>
    /// <remarks>
    ///     If no profile is selected or the move would place the profile outside the valid range, the
    ///     method performs no action. The method updates the profile order and persists the configuration after a
    ///     successful move.
    /// </remarks>
    /// <param name="offset">
    ///     The number of positions to move the selected profile. A positive value moves the profile down; a negative value
    ///     moves it up. Must not move the profile outside the bounds of the list.
    /// </param>
    private void MoveSelectedProfile(int offset)
    {
        var selectedItem = GetSelectedProfileItem();
        if (selectedItem is null)
        {
            return;
        }

        var currentIndex = _profiles.IndexOf(selectedItem);
        var targetIndex = currentIndex + offset;
        if ((targetIndex < 0) || (targetIndex >= _profiles.Count))
        {
            return;
        }

        var profiles = _profiles.Select(item => item.Profile)
                                .ToList();
        (profiles[currentIndex], profiles[targetIndex]) = (profiles[targetIndex], profiles[currentIndex]);
        ReplaceProfileItems(profiles, _getDisplaysQuery.Execute(), targetIndex);
        PersistConfigurationFromEditor();
    }

    /// <summary>
    ///     Opens the folder containing the configuration file in Windows Explorer and selects the configuration file.
    /// </summary>
    /// <remarks>
    ///     This method creates the configuration folder if it does not already exist. It is intended for
    ///     use on Windows systems where Explorer is available.
    /// </remarks>
    private void OpenConfigFolder()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{_configPath}\"",
            UseShellExecute = true
        });
    }

    /// <summary>
    ///     Saves the current configuration settings as specified in the editor controls.
    /// </summary>
    /// <remarks>
    ///     If the configuration is invalid or cannot be saved, a message box is displayed to inform the
    ///     user. The method does nothing if a configuration load operation is in progress. Upon successful save, the
    ///     ConfigurationSaved event is raised.
    /// </remarks>
    private void PersistConfigurationFromEditor()
    {
        if (_isLoading)
        {
            return;
        }

        var configuration = new AppConfiguration
        {
            WatcherEnabled = _watcherEnabledCheckBox.Checked,
            PollIntervalSeconds = (int)_pollIntervalUpDown.Value,
            Profiles = _profiles.Select(item => item.Profile)
                                .ToList()
        };

        try
        {
            _saveConfigurationCommand.Execute(configuration);
            SetStatus($"Saved {configuration.Profiles.Count} profile(s).");
            ConfigurationSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (ConfigurationException exception)
        {
            MessageBox.Show(exception.Message,
                            "Validation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message,
                            "Save Failed",
                            MessageBoxButtons.OK,
                             MessageBoxIcon.Error);
        }
    }

    /// <summary>
    ///     Removes the currently selected profile from the profile list after user confirmation.
    /// </summary>
    /// <remarks>
    ///     If no profile is selected, the method performs no action. Prompts the user for confirmation
    ///     before removing the profile. Updates the profile list and persists the configuration after removal.
    /// </remarks>
    private void RemoveSelectedProfile()
    {
        var selectedItem = GetSelectedProfileItem();
        if (selectedItem is null)
        {
            return;
        }

        var confirm = MessageBox.Show($"Remove the profile '{selectedItem.Name}'?",
                                      "Remove Profile",
                                      MessageBoxButtons.YesNo,
                                      MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var selectedIndex = _profiles.IndexOf(selectedItem);
        var profiles = _profiles.Select(item => item.Profile)
                                .ToList();
        profiles.RemoveAt(selectedIndex);
        ReplaceProfileItems(profiles, _getDisplaysQuery.Execute(), Math.Min(selectedIndex, profiles.Count - 1));
        PersistConfigurationFromEditor();
    }

    /// <summary>
    ///     Replaces the current list of profile items with the specified profiles and updates the selection state in the
    ///     grid.
    /// </summary>
    /// <remarks>
    ///     If the specified selected index is invalid, the selection is cleared and no profile is
    ///     selected. The method also updates the enabled state of related UI buttons based on the new selection.
    /// </remarks>
    /// <param name="profiles">The collection of game profiles to display. Cannot be null.</param>
    /// <param name="displays">
    ///     The collection of display device information used to associate with each profile. Cannot be
    ///     null.
    /// </param>
    /// <param name="selectedIndex">
    ///     The zero-based index of the profile to select after replacement. If less than zero or out of range, no selection
    ///     is made.
    /// </param>
    private void ReplaceProfileItems(IReadOnlyList<GameProfile> profiles, IReadOnlyList<DisplayDeviceInfo> displays, int selectedIndex = -1)
    {
        _profiles.Clear();

        for (var index = 0; index < profiles.Count; index++)
        {
            _profiles.Add(GameProfileListItem.FromProfile(profiles[index], displays, index + 1));
        }

        if ((selectedIndex < 0) || (selectedIndex >= _profilesGrid.Rows.Count))
        {
            UpdateButtonStates();
            return;
        }

        _profilesGrid.ClearSelection();
        _profilesGrid.Rows[selectedIndex].Selected = true;
        _profilesGrid.CurrentCell = _profilesGrid.Rows[selectedIndex]
                                                  .Cells[0];
        UpdateButtonStates();
    }

    /// <summary>
    ///     Sets the status message displayed to the user.
    /// </summary>
    /// <param name="message">The message to display in the status label. Can be null or empty to clear the status.</param>
    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }

    /// <summary>
    ///     Updates the enabled state of profile-related action buttons based on the current selection.
    /// </summary>
    /// <remarks>
    ///     This method should be called whenever the selection in the profile list changes to ensure
    ///     that the Edit, Move Up, Move Down, and Remove buttons accurately reflect whether their associated actions are
    ///     available. Buttons are enabled or disabled depending on whether a profile is selected and its position in the
    ///     list.
    /// </remarks>
    private void UpdateButtonStates()
    {
        var selectedItem = GetSelectedProfileItem();
        var hasSelection = selectedItem is not null;
        var selectedIndex = selectedItem is null
            ? -1
            : _profiles.IndexOf(selectedItem);

        _editButton.Enabled = hasSelection;
        _moveUpButton.Enabled = selectedIndex > 0;
        _moveDownButton.Enabled = (selectedIndex >= 0) && (selectedIndex < (_profiles.Count - 1));
        _removeButton.Enabled = hasSelection;
    }
}
