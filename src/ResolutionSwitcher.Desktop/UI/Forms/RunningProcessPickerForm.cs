using System.Runtime.Versioning;

namespace ResolutionSwitcher.Desktop.UI.Forms;

/// <summary>
///     Represents a modal dialog form that allows the user to select a running process by name from a provided list.
/// </summary>
/// <remarks>
///     This form is intended for use on Windows platforms and displays a filtered, alphabetically sorted
///     list of process names. Only the process name is stored after selection. The dialog is typically shown as a child of
///     another window and is not resizable.
/// </remarks>
[SupportedOSPlatform("windows")]
internal sealed class RunningProcessPickerForm : Form
{
    private readonly List<string> _allProcessNames;
    private readonly TextBox _filterTextBox = new TextBox();
    private readonly ListBox _processListBox = new ListBox();

    /// <summary>
    ///     Initializes a new instance of the RunningProcessPickerForm class with the specified list of process names.
    /// </summary>
    /// <remarks>
    ///     The form is configured as a fixed dialog and is centered relative to its parent. The provided
    ///     process names are used to populate the selection list when the form is displayed.
    /// </remarks>
    /// <param name="processNames">
    ///     A read-only list of process names to display for selection. The list is sorted alphabetically in a
    ///     case-insensitive manner.
    /// </param>
    public RunningProcessPickerForm(IReadOnlyList<string> processNames)
    {
        _allProcessNames = processNames
                          .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                          .ToList();

        Text = "Select Running Process";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(420, 460);

        InitializeLayout();
        LoadProcessList();
    }

    /// <summary>
    ///     Gets the name of the currently selected process, or null if no process is selected.
    /// </summary>
    public string? SelectedProcessName { get; private set; }

    /// <summary>
    ///     Confirms the user's selection of a running process from the list and updates the selected process name.
    /// </summary>
    /// <remarks>
    ///     If no process is selected, displays an informational message and prevents the dialog from
    ///     closing. This method is typically called in response to a user action, such as clicking an OK or Confirm
    ///     button.
    /// </remarks>
    private void ConfirmSelection()
    {
        if (_processListBox.SelectedItem is not string processName)
        {
            MessageBox.Show(this,
                            "Select a running process first.",
                            "No Process Selected",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            DialogResult = DialogResult.None;
            return;
        }

        SelectedProcessName = processName;
    }

    /// <summary>
    ///     Initializes and arranges the user interface controls for the process selection dialog.
    /// </summary>
    /// <remarks>
    ///     This method sets up the layout, labels, filter textbox, process list, and action buttons. It
    ///     should be called during form initialization to ensure all controls are properly configured and displayed. The
    ///     layout supports filtering and selecting a running process, and provides standard OK and Cancel
    ///     actions.
    /// </remarks>
    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var helpLabel = new Label
        {
            AutoSize = true,
            Text = "Pick a currently running process. Only the process name is stored.",
            Padding = new Padding(0, 0, 0, 8)
        };
        root.Controls.Add(helpLabel, 0, 0);

        _filterTextBox.Dock = DockStyle.Fill;
        _filterTextBox.PlaceholderText = "Filter processes";
        _filterTextBox.TextChanged += (_, _) => LoadProcessList();
        root.Controls.Add(_filterTextBox, 0, 1);

        _processListBox.Dock = DockStyle.Fill;
        _processListBox.IntegralHeight = false;
        _processListBox.DoubleClick += (_, _) => ConfirmSelection();
        root.Controls.Add(_processListBox, 0, 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        var okButton = new Button
        {
            AutoSize = true,
            Text = "Use Process",
            DialogResult = DialogResult.OK
        };
        okButton.Click += (_, _) => ConfirmSelection();

        var cancelButton = new Button
        {
            AutoSize = true,
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };

        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Controls.Add(cancelButton);
        root.Controls.Add(buttonsPanel, 0, 3);

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    /// <summary>
    ///     Refreshes the process list displayed in the list box, applying the current filter and preserving the selected
    ///     item when possible.
    /// </summary>
    /// <remarks>
    ///     If a filter is specified, only process names containing the filter text (case-insensitive)
    ///     are shown. The previously selected process is reselected if it remains in the filtered list; otherwise, the
    ///     first item is selected by default.
    /// </remarks>
    private void LoadProcessList()
    {
        var selectedName = _processListBox.SelectedItem as string;
        var filter = _filterTextBox.Text.Trim();

        var matchingNames = string.IsNullOrWhiteSpace(filter)
            ? _allProcessNames
            : _allProcessNames
             .Where(name => name.Contains(filter, StringComparison.OrdinalIgnoreCase))
             .ToList();

        _processListBox.BeginUpdate();
        try
        {
            _processListBox.Items.Clear();

            foreach (var processName in matchingNames)
            {
                _processListBox.Items.Add(processName);
            }
        }
        finally
        {
            _processListBox.EndUpdate();
        }

        if (!string.IsNullOrWhiteSpace(selectedName))
        {
            var selectedIndex = _processListBox.Items.IndexOf(selectedName);
            if (selectedIndex >= 0)
            {
                _processListBox.SelectedIndex = selectedIndex;
                return;
            }
        }

        if (_processListBox.Items.Count > 0)
        {
            _processListBox.SelectedIndex = 0;
        }
    }
}