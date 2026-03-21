using ResolutionSwitcher.Application.Display.Queries;
using ResolutionSwitcher.Application.Processes.Queries;
using ResolutionSwitcher.Domain.Configuration;
using ResolutionSwitcher.Domain.Display;
using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop.UI;

/// <summary>
///     Represents a dialog form for creating or editing a game profile, allowing users to specify display settings,
///     resolution, refresh rate, and the associated game process.
/// </summary>
/// <remarks>
///     This form is intended for use within a Windows environment and provides UI controls for selecting a
///     display, supported display mode, and entering or browsing for a game executable. The form validates user input
///     before saving the profile. The resulting profile can be accessed via the Profile property after the dialog is
///     closed
///     with a successful result.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class ProfileEditorForm : Form
{
    private readonly List<DisplayChoice> _displayChoices = [];
    private readonly ComboBox _displayComboBox = new ComboBox();
    private readonly IGetDisplayModesQuery _getDisplayModesQuery;
    private readonly IGetDisplaysQuery _getDisplaysQuery;
    private readonly IGetRunningProcessesQuery _getRunningProcessesQuery;
    private readonly NumericUpDown _heightUpDown = new NumericUpDown();
    private readonly ComboBox _modeComboBox = new ComboBox();
    private readonly TextBox _nameTextBox = new TextBox();
    private readonly TextBox _processNameTextBox = new TextBox();
    private readonly NumericUpDown _refreshRateUpDown = new NumericUpDown();
    private readonly bool _suspendDisplayPreview;
    private readonly NumericUpDown _widthUpDown = new NumericUpDown();
    private CancellationTokenSource? _displayPreviewCancellationTokenSource;
    private DisplayHighlightOverlayForm? _displayPreviewOverlay;
    private bool _isLoadingModes;

    /// <summary>
    ///     Initializes a new instance of the ProfileEditorForm class for creating or editing a game profile.
    /// </summary>
    /// <remarks>
    ///     If a profile is provided, the form is initialized for editing that profile; otherwise, it is
    ///     set up for creating a new profile. The form is configured as a fixed dialog and centered on its parent
    ///     window.
    /// </remarks>
    /// <param name="getDisplaysQuery">A query service used to retrieve available display devices for selection.</param>
    /// <param name="getDisplayModesQuery">A query service used to retrieve supported display modes for the selected display.</param>
    /// <param name="getRunningProcessesQuery">
    ///     A query service used to retrieve the list of currently running processes, typically for associating a process
    ///     with the game profile.
    /// </param>
    /// <param name="profile">The game profile to edit, or null to create a new profile.</param>
    public ProfileEditorForm(IGetDisplaysQuery getDisplaysQuery,
        IGetDisplayModesQuery getDisplayModesQuery,
        IGetRunningProcessesQuery getRunningProcessesQuery,
        GameProfile? profile = null)
    {
        _getDisplaysQuery = getDisplaysQuery;
        _getDisplayModesQuery = getDisplayModesQuery;
        _getRunningProcessesQuery = getRunningProcessesQuery;

        Text = profile is null
            ? "Add Game Profile"
            : "Edit Game Profile";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(680, 320);

        _suspendDisplayPreview = true;
        InitializeLayout();
        LoadDisplays();

        if (profile is not null)
        {
            LoadProfile(profile);
        }
        else if (_displayChoices.Count > 0)
        {
            _displayComboBox.SelectedIndex = 0;
        }

        _suspendDisplayPreview = false;
    }

    /// <summary>
    ///     Gets the current game profile associated with this instance.
    /// </summary>
    public GameProfile Profile { get; private set; } = new GameProfile();

    /// <summary>
    ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
    /// </summary>
    /// <remarks>
    ///     This method is called by the public Dispose() method and the finalizer. When disposing is
    ///     true, this method releases all resources held by managed objects. When disposing is false, only unmanaged
    ///     resources are released.
    /// </remarks>
    /// <param name="disposing">
    ///     true to release both managed and unmanaged resources; false to release only unmanaged
    ///     resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeDisplayPreview();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Adds a label and an associated control to a specified row in a TableLayoutPanel.
    /// </summary>
    /// <remarks>
    ///     The label is added to the first column and the control to the second column of the specified
    ///     row. The method automatically configures the row style for auto-sizing and sets appropriate layout properties
    ///     for the label and control.
    /// </remarks>
    /// <param name="table">The TableLayoutPanel to which the label and control will be added. Must not be null.</param>
    /// <param name="rowIndex">
    ///     The zero-based index of the row where the label and control will be placed. Must be within the valid range of
    ///     rows for the table.
    /// </param>
    /// <param name="labelText">The text to display in the label associated with the control.</param>
    /// <param name="control">The control to add next to the label in the specified row. Must not be null.</param>
    private void AddLabeledControl(TableLayoutPanel table, int rowIndex, string labelText, Control control)
    {
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var label = new Label
        {
            AutoSize = true,
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 12, 0)
        };

        control.Dock = DockStyle.Fill;

        table.Controls.Add(label, 0, rowIndex);
        table.Controls.Add(control, 1, rowIndex);
    }

    /// <summary>
    ///     Normalizes and applies the specified process name to the process name text box, and sets the name text box if it
    ///     is empty.
    /// </summary>
    /// <remarks>
    ///     If the provided value is null, empty, or consists only of whitespace, the method does not
    ///     modify any text boxes. If the name text box is already populated, its value is not overwritten.
    /// </remarks>
    /// <param name="value">
    ///     The process name to normalize and apply. Can be null or whitespace, in which case no changes are
    ///     made.
    /// </param>
    private void ApplyProcessName(string value)
    {
        var normalizedProcessName = NormalizeProcessName(value);
        if (string.IsNullOrWhiteSpace(normalizedProcessName))
        {
            return;
        }

        _processNameTextBox.Text = normalizedProcessName;

        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            _nameTextBox.Text = normalizedProcessName;
        }
    }

    /// <summary>
    ///     Applies the currently selected display mode from the mode selection control to the corresponding input fields.
    /// </summary>
    /// <remarks>
    ///     This method updates the width, height, and refresh rate input controls to reflect the values
    ///     of the selected display mode. If no mode is selected or modes are currently loading, the method does
    ///     nothing.
    /// </remarks>
    private void ApplySelectedMode()
    {
        if (_isLoadingModes || _modeComboBox.SelectedItem is not DisplayModeChoice modeChoice)
        {
            return;
        }

        _widthUpDown.Value = ClampToRange(modeChoice.Mode.Width, _widthUpDown);
        _heightUpDown.Value = ClampToRange(modeChoice.Mode.Height, _heightUpDown);
        _refreshRateUpDown.Value = ClampToRange(modeChoice.Mode.RefreshRate ?? 0, _refreshRateUpDown);
    }

    /// <summary>
    ///     Displays a file dialog that allows the user to select an executable file, and applies the selected file's path
    ///     as the process name.
    /// </summary>
    /// <remarks>
    ///     Only files with the .exe extension are shown by default, but the user can choose to display
    ///     all files. If the user cancels the dialog or does not select a file, no action is taken.
    /// </remarks>
    private void BrowseForExecutable()
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            Multiselect = false,
            Title = "Select a game executable"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ApplyProcessName(dialog.FileName);
    }

    /// <summary>
    ///     Clamps the specified integer value to the minimum and maximum range defined by the provided NumericUpDown
    ///     control.
    /// </summary>
    /// <param name="value">The integer value to be clamped within the control's minimum and maximum range.</param>
    /// <param name="control">The NumericUpDown control that defines the minimum and maximum allowable values.</param>
    /// <returns>A decimal representing the clamped value, constrained to the control's minimum and maximum range.</returns>
    private static decimal ClampToRange(int value, NumericUpDown control)
    {
        return Math.Clamp(value, (int)control.Minimum, (int)control.Maximum);
    }

    /// <summary>
    ///     Configures the specified NumericUpDown control with the provided minimum and maximum values.
    /// </summary>
    /// <remarks>
    ///     This method also sets the control's width to 80 pixels. Ensure that the control is properly
    ///     initialized before calling this method.
    /// </remarks>
    /// <param name="control">The NumericUpDown control to configure. Cannot be null.</param>
    /// <param name="minimum">
    ///     The minimum value to set for the control. Must be less than or equal to
    ///     <paramref name="maximum" />.
    /// </param>
    /// <param name="maximum">
    ///     The maximum value to set for the control. Must be greater than or equal to
    ///     <paramref name="minimum" />.
    /// </param>
    private static void ConfigureNumeric(NumericUpDown control, int minimum, int maximum)
    {
        control.Minimum = minimum;
        control.Maximum = maximum;
        control.Width = 80;
    }

    /// <summary>
    ///     Creates and configures a control that allows the user to select a process executable by entering a name,
    ///     browsing for an executable file, or choosing from running processes.
    /// </summary>
    /// <remarks>
    ///     The returned control provides guidance to users by automatically cleaning full paths and file
    ///     extensions from the input, ensuring only the executable name is used. The help label clarifies this
    ///     behavior.
    /// </remarks>
    /// <returns>
    ///     A TableLayoutPanel containing controls for process selection, including a text box, browse and running process
    ///     buttons, and a help label.
    /// </returns>
    private Control CreateProcessSelector()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2,
            Margin = Padding.Empty
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _processNameTextBox.Dock = DockStyle.Fill;

        var browseButton = new Button
        {
            AutoSize = true,
            Text = "Browse EXE...",
            Margin = new Padding(8, 0, 0, 0)
        };
        browseButton.Click += (_, _) => BrowseForExecutable();

        var runningButton = new Button
        {
            AutoSize = true,
            Text = "Use running...",
            Margin = new Padding(8, 0, 0, 0)
        };
        runningButton.Click += (_, _) => SelectRunningProcess();

        var helpLabel = new Label
        {
            AutoSize = true,
            Text = "Only the executable name is used. Full paths and .exe are cleaned automatically.",
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 6, 0, 0)
        };

        panel.Controls.Add(_processNameTextBox, 0, 0);
        panel.Controls.Add(browseButton, 1, 0);
        panel.Controls.Add(runningButton, 2, 0);
        panel.Controls.Add(helpLabel, 0, 1);
        panel.SetColumnSpan(helpLabel, 3);

        return panel;
    }

    /// <summary>
    ///     Releases resources associated with the display preview, including any active overlays and cancellation tokens.
    /// </summary>
    /// <remarks>
    ///     Call this method to clean up display preview resources when they are no longer needed. After
    ///     calling this method, the display preview overlay and related resources are disposed and cannot be
    ///     used.
    /// </remarks>
    private void DisposeDisplayPreview()
    {
        _displayPreviewCancellationTokenSource?.Cancel();
        _displayPreviewCancellationTokenSource?.Dispose();
        _displayPreviewCancellationTokenSource = null;

        if (_displayPreviewOverlay is null)
        {
            return;
        }

        _displayPreviewOverlay.Close();
        _displayPreviewOverlay.Dispose();
        _displayPreviewOverlay = null;
    }

    /// <summary>
    ///     Initializes and arranges the user interface controls for the layout editor dialog.
    /// </summary>
    /// <remarks>
    ///     This method sets up the layout, labels, and configuration for all input fields and action
    ///     buttons. It should be called during form initialization to ensure all controls are properly arranged and
    ///     configured. Calling this method multiple times may result in duplicate controls or unexpected layout
    ///     behavior.
    /// </remarks>
    private void InitializeLayout()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Controls.Add(table);

        AddLabeledControl(table, 0, "Profile name", _nameTextBox);
        _processNameTextBox.PlaceholderText = "eldenring or eldenring.exe";
        AddLabeledControl(table, 1, "Game process", CreateProcessSelector());

        _displayComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _displayComboBox.DisplayMember = nameof(DisplayChoice.Label);
        _displayComboBox.ValueMember = nameof(DisplayChoice.Value);
        _displayComboBox.SelectedIndexChanged += (_, _) => OnSelectedDisplayChanged();
        AddLabeledControl(table, 2, "Display", _displayComboBox);

        _modeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _modeComboBox.DisplayMember = nameof(DisplayModeChoice.Label);
        _modeComboBox.SelectedIndexChanged += (_, _) => ApplySelectedMode();
        AddLabeledControl(table, 3, "Supported mode", _modeComboBox);

        var resolutionPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        ConfigureNumeric(_widthUpDown, 640, 7680);
        ConfigureNumeric(_heightUpDown, 480, 4320);
        ConfigureNumeric(_refreshRateUpDown, 0, 360);

        resolutionPanel.Controls.Add(_widthUpDown);
        resolutionPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "x",
            Padding = new Padding(6, 6, 6, 0)
        });
        resolutionPanel.Controls.Add(_heightUpDown);
        resolutionPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "@",
            Padding = new Padding(10, 6, 6, 0)
        });
        resolutionPanel.Controls.Add(_refreshRateUpDown);
        resolutionPanel.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Hz (0 = auto)",
            Padding = new Padding(6, 6, 0, 0)
        });
        AddLabeledControl(table, 4, "Resolution", resolutionPanel);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 0)
        };

        var okButton = new Button
        {
            AutoSize = true,
            Text = "Save",
            DialogResult = DialogResult.OK
        };
        okButton.Click += (_, _) => ValidateAndSave();

        var cancelButton = new Button
        {
            AutoSize = true,
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };

        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Controls.Add(cancelButton);
        table.Controls.Add(buttonsPanel, 0, 5);
        table.SetColumnSpan(buttonsPanel, 2);

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    /// <summary>
    ///     Refreshes the list of available display choices and updates the display selection UI component.
    /// </summary>
    /// <remarks>
    ///     Call this method to ensure the display selection reflects the current set of connected
    ///     displays. This method clears any existing display choices and repopulates them based on the latest query
    ///     results. The display selection UI is updated to show the refreshed list.
    /// </remarks>
    private void LoadDisplays()
    {
        _displayChoices.Clear();

        foreach (var display in _getDisplaysQuery.Execute())
        {
            _displayChoices.Add(new DisplayChoice(display.IsPrimary
                                                      ? null
                                                      : display.DeviceName,
                                                  display.Label,
                                                  display.Bounds));
        }

        _displayComboBox.DataSource = null;
        _displayComboBox.DataSource = _displayChoices;
    }

    /// <summary>
    ///     Loads and displays the available display modes for the currently selected display in the mode selection combo
    ///     box.
    /// </summary>
    /// <remarks>
    ///     If no display is selected, the method does nothing. The first available mode is automatically
    ///     selected if any modes are found. This method is intended to be called when the selected display changes to
    ///     update the list of available modes accordingly.
    /// </remarks>
    private void LoadModesForSelectedDisplay()
    {
        if (_displayComboBox.SelectedItem is not DisplayChoice displayChoice)
        {
            return;
        }

        _isLoadingModes = true;
        try
        {
            var modes = _getDisplayModesQuery.Execute(displayChoice.Value)
                                             .Select(mode => new DisplayModeChoice(mode))
                                             .ToList();

            _modeComboBox.DataSource = null;
            _modeComboBox.DataSource = modes;

            if (modes.Count > 0)
            {
                _modeComboBox.SelectedIndex = 0;
            }
        }
        finally
        {
            _isLoadingModes = false;
        }
    }

    /// <summary>
    ///     Loads the specified game profile into the user interface controls, updating all relevant fields to reflect the
    ///     profile's settings.
    /// </summary>
    /// <remarks>
    ///     This method updates text boxes, combo boxes, and numeric controls to match the values from
    ///     the provided profile. If a value from the profile is outside the valid range for a control, it is clamped to the
    ///     nearest valid value.
    /// </remarks>
    /// <param name="profile">The game profile whose settings are to be loaded into the interface. Cannot be null.</param>
    private void LoadProfile(GameProfile profile)
    {
        _nameTextBox.Text = profile.Name ?? string.Empty;
        _processNameTextBox.Text = profile.NormalizedProcessName;

        var selectedIndex = _displayChoices.FindIndex(choice => string.Equals(choice.Value, profile.DisplayName, StringComparison.OrdinalIgnoreCase));
        _displayComboBox.SelectedIndex = selectedIndex >= 0
            ? selectedIndex
            : 0;

        _widthUpDown.Value = ClampToRange(profile.Width, _widthUpDown);
        _heightUpDown.Value = ClampToRange(profile.Height, _heightUpDown);
        _refreshRateUpDown.Value = ClampToRange(profile.RefreshRate ?? 0, _refreshRateUpDown);

        SelectMatchingMode(profile.Width, profile.Height, profile.RefreshRate);
    }

    /// <summary>
    ///     Normalizes a process name by trimming whitespace and quotes, and removing any file path and extension
    ///     information.
    /// </summary>
    /// <remarks>
    ///     Use this method to extract the base process name from a user-supplied string that may include
    ///     a file path or extension. This is useful when comparing or displaying process names in a consistent
    ///     format.
    /// </remarks>
    /// <param name="value">
    ///     The process name to normalize. May include leading or trailing whitespace, quotes, a file path, or a file
    ///     extension.
    /// </param>
    /// <returns>
    ///     A normalized process name without path or extension. Returns an empty string if the input is null, empty, or
    ///     consists only of whitespace or quotes.
    /// </returns>
    private static string NormalizeProcessName(string value)
    {
        var trimmedValue = value.Trim()
                                .Trim('"');
        if (string.IsNullOrWhiteSpace(trimmedValue))
        {
            return string.Empty;
        }

        return Path.GetFileNameWithoutExtension(Path.GetFileName(trimmedValue));
    }

    /// <summary>
    ///     Handles changes to the selected display and updates the available display modes and preview accordingly.
    /// </summary>
    /// <remarks>
    ///     This method should be called whenever the selected display changes in the UI. It loads the
    ///     modes for the newly selected display and, if appropriate, initiates an asynchronous preview of the display's
    ///     bounds. If display preview is suspended or the selected item is not a valid display choice, the preview is not
    ///     updated.
    /// </remarks>
    private void OnSelectedDisplayChanged()
    {
        LoadModesForSelectedDisplay();

        if (_suspendDisplayPreview || _displayComboBox.SelectedItem is not DisplayChoice displayChoice)
        {
            return;
        }

        _ = PreviewDisplayAsync(displayChoice.Bounds);
    }

    /// <summary>
    ///     Displays a temporary visual overlay highlighting the specified display bounds for preview purposes.
    /// </summary>
    /// <remarks>
    ///     If the specified bounds have a width or height less than or equal to zero, no overlay is
    ///     shown. Any existing preview overlay is closed before displaying a new one. The overlay is automatically
    ///     dismissed after a short delay or if canceled.
    /// </remarks>
    /// <param name="bounds">The bounds of the display area to highlight. The width and height must be greater than zero.</param>
    /// <returns>A task that represents the asynchronous operation. The task completes when the preview overlay is closed.</returns>
    private async Task PreviewDisplayAsync(DisplayBounds bounds)
    {
        if ((bounds.Width <= 0) || (bounds.Height <= 0))
        {
            return;
        }

        DisposeDisplayPreview();

        var cancellationTokenSource = new CancellationTokenSource();
        _displayPreviewCancellationTokenSource = cancellationTokenSource;

        var overlay = new DisplayHighlightOverlayForm(bounds);
        _displayPreviewOverlay = overlay;
        overlay.Show();

        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1200), cancellationTokenSource.Token);
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (ReferenceEquals(_displayPreviewOverlay, overlay))
            {
                _displayPreviewOverlay = null;
            }

            overlay.Close();
            overlay.Dispose();

            if (ReferenceEquals(_displayPreviewCancellationTokenSource, cancellationTokenSource))
            {
                _displayPreviewCancellationTokenSource = null;
            }

            cancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    ///     Selects the display mode in the combo box that matches the specified width, height, and refresh rate.
    /// </summary>
    /// <remarks>If no matching display mode is found, the current selection remains unchanged.</remarks>
    /// <param name="width">The width, in pixels, of the display mode to select.</param>
    /// <param name="height">The height, in pixels, of the display mode to select.</param>
    /// <param name="refreshRate">
    ///     The refresh rate, in hertz, of the display mode to select. Specify null to match modes with an unspecified
    ///     refresh rate.
    /// </param>
    private void SelectMatchingMode(int width, int height, int? refreshRate)
    {
        for (var index = 0; index < _modeComboBox.Items.Count; index++)
        {
            if (_modeComboBox.Items[index] is not DisplayModeChoice modeChoice)
            {
                continue;
            }

            if ((modeChoice.Mode.Width == width)
             && (modeChoice.Mode.Height == height)
             && (modeChoice.Mode.RefreshRate == refreshRate))
            {
                _modeComboBox.SelectedIndex = index;
                return;
            }
        }
    }

    /// <summary>
    ///     Displays a dialog that allows the user to select a running process and applies the selected process name if
    ///     confirmed.
    /// </summary>
    /// <remarks>
    ///     If the user cancels the dialog or does not select a process, no changes are made. This method
    ///     is typically used to prompt the user for a process selection in interactive scenarios.
    /// </remarks>
    private void SelectRunningProcess()
    {
        using var dialog = new RunningProcessPickerForm(_getRunningProcessesQuery.Execute());
        if ((dialog.ShowDialog(this) != DialogResult.OK) || string.IsNullOrWhiteSpace(dialog.SelectedProcessName))
        {
            return;
        }

        ApplyProcessName(dialog.SelectedProcessName);
    }

    /// <summary>
    ///     Displays a warning message dialog to inform the user of a profile validation issue.
    /// </summary>
    /// <remarks>
    ///     The dialog uses a warning icon and an 'OK' button. The method is intended for notifying users
    ///     of invalid profile data during validation.
    /// </remarks>
    /// <param name="message">The validation message to display in the dialog. Cannot be null.</param>
    private void ShowValidationMessage(string message)
    {
        MessageBox.Show(this,
                        message,
                        "Invalid Profile",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
    }

    /// <summary>
    ///     Validates user input for the game profile and saves the profile if all required fields are valid.
    /// </summary>
    /// <remarks>
    ///     Displays validation messages and prevents saving if any required input is missing or invalid.
    ///     The method updates the Profile property with the entered values when validation succeeds.
    /// </remarks>
    private void ValidateAndSave()
    {
        var processName = NormalizeProcessName(_processNameTextBox.Text);
        if (string.IsNullOrWhiteSpace(processName))
        {
            ShowValidationMessage("Enter the game's process name, or browse to the executable. Only the executable name is stored.");
            DialogResult = DialogResult.None;
            return;
        }

        if ((_widthUpDown.Value <= 0) || (_heightUpDown.Value <= 0))
        {
            ShowValidationMessage("Width and height must both be greater than zero.");
            DialogResult = DialogResult.None;
            return;
        }

        if (_displayComboBox.SelectedItem is not DisplayChoice displayChoice)
        {
            ShowValidationMessage("Select a display.");
            DialogResult = DialogResult.None;
            return;
        }

        Profile = new GameProfile
        {
            Name = string.IsNullOrWhiteSpace(_nameTextBox.Text)
                ? processName
                : _nameTextBox.Text.Trim(),
            ProcessName = processName,
            DisplayName = displayChoice.Value,
            Width = (int)_widthUpDown.Value,
            Height = (int)_heightUpDown.Value,
            RefreshRate = _refreshRateUpDown.Value == 0
                ? null
                : (int)_refreshRateUpDown.Value
        };
    }
}