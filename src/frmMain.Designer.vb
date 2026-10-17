<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        mainMenu = New MenuStrip()
        mnuFile = New ToolStripMenuItem("&File")
        mnuOpenFolder = New ToolStripMenuItem("Open save &folder...")
        mnuOpenFile = New ToolStripMenuItem("&Open single file...")
        mnuSaveAll = New ToolStripMenuItem("&Save all")
        mnuSaveAllTo = New ToolStripMenuItem("Save all &to folder...")
        mnuExport = New ToolStripMenuItem("&Export PlayerDatabase as .sqlite...")

        tabs = New TabControl()
        tabDb = New TabPage("PlayerDatabase")
        tabProfile = New TabPage("ProfileData")
        tabState = New TabPage("SaveState")
        tabBinary = New TabPage("BinarySaveState")
        tabVersion = New TabPage("VersionFlags")

        split = New SplitContainer()
        lstTables = New ListBox()
        grid = New DataGridView()

        txtProfileFilter = New TextBox()
        gridProfile = New DataGridView()
        txtStateFilter = New TextBox()
        gridState = New DataGridView()

        splitBin = New SplitContainer()
        lstStrings = New ListBox()
        txtHex = New TextBox()

        lblVersion = New Label()
        numVersion = New NumericUpDown()

        status = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()

        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuOpenFolder, mnuOpenFile, New ToolStripSeparator(), mnuSaveAll, mnuSaveAllTo, New ToolStripSeparator(), mnuExport})
        mainMenu.Items.Add(mnuFile)
        mnuSaveAll.Enabled = False
        mnuSaveAllTo.Enabled = False
        mnuExport.Enabled = False

        ' PlayerDatabase tab
        lstTables.Dock = DockStyle.Fill
        ConfigureGrid(grid)
        split.Dock = DockStyle.Fill
        split.SplitterDistance = 220
        split.Panel1.Controls.Add(lstTables)
        split.Panel2.Controls.Add(grid)
        tabDb.Controls.Add(split)

        ' ProfileData tab
        ConfigureGrid(gridProfile)
        txtProfileFilter.Dock = DockStyle.Top
        txtProfileFilter.PlaceholderText = "Filter by name, e.g. Credits"
        tabProfile.Controls.Add(gridProfile)
        tabProfile.Controls.Add(txtProfileFilter)

        ' SaveState tab
        ConfigureGrid(gridState)
        txtStateFilter.Dock = DockStyle.Top
        txtStateFilter.PlaceholderText = "Filter by object, property or value"
        tabState.Controls.Add(gridState)
        tabState.Controls.Add(txtStateFilter)

        ' BinarySaveState tab (read-only viewer)
        lstStrings.Dock = DockStyle.Fill
        lstStrings.Font = New Font("Consolas", 9.0F)
        txtHex.Dock = DockStyle.Fill
        txtHex.Multiline = True
        txtHex.ReadOnly = True
        txtHex.WordWrap = False
        txtHex.ScrollBars = ScrollBars.Both
        txtHex.Font = New Font("Consolas", 9.0F)
        splitBin.Dock = DockStyle.Fill
        splitBin.SplitterDistance = 320
        splitBin.Panel1.Controls.Add(lstStrings)
        splitBin.Panel2.Controls.Add(txtHex)
        tabBinary.Controls.Add(splitBin)

        ' VersionFlags tab
        lblVersion.AutoSize = True
        lblVersion.Location = New Point(16, 20)
        lblVersion.Text = "Version flags (uint32):"
        numVersion.Location = New Point(16, 46)
        numVersion.Width = 160
        numVersion.Minimum = 0
        numVersion.Maximum = UInteger.MaxValue
        tabVersion.Controls.Add(lblVersion)
        tabVersion.Controls.Add(numVersion)

        tabs.Dock = DockStyle.Fill
        tabs.TabPages.AddRange(New TabPage() {tabDb, tabProfile, tabState, tabBinary, tabVersion})

        status.Items.Add(lblStatus)
        lblStatus.Text = "Open a save folder (the one containing PlayerDatabase, ProfileData, ...) from the File menu."

        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1100, 650)
        Text = "Save Editor"
        Controls.Add(tabs)
        Controls.Add(status)
        Controls.Add(mainMenu)
        MainMenuStrip = mainMenu
    End Sub

    Private Shared Sub ConfigureGrid(g As DataGridView)
        g.Dock = DockStyle.Fill
        g.AllowUserToAddRows = False
        g.AllowUserToDeleteRows = False
        g.ShowCellToolTips = False
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells
    End Sub

    Friend WithEvents mainMenu As MenuStrip
    Friend WithEvents mnuFile As ToolStripMenuItem
    Friend WithEvents mnuOpenFolder As ToolStripMenuItem
    Friend WithEvents mnuOpenFile As ToolStripMenuItem
    Friend WithEvents mnuSaveAll As ToolStripMenuItem
    Friend WithEvents mnuSaveAllTo As ToolStripMenuItem
    Friend WithEvents mnuExport As ToolStripMenuItem
    Friend WithEvents tabs As TabControl
    Friend WithEvents tabDb As TabPage
    Friend WithEvents tabProfile As TabPage
    Friend WithEvents tabState As TabPage
    Friend WithEvents tabBinary As TabPage
    Friend WithEvents tabVersion As TabPage
    Friend WithEvents split As SplitContainer
    Friend WithEvents lstTables As ListBox
    Friend WithEvents grid As DataGridView
    Friend WithEvents txtProfileFilter As TextBox
    Friend WithEvents gridProfile As DataGridView
    Friend WithEvents txtStateFilter As TextBox
    Friend WithEvents gridState As DataGridView
    Friend WithEvents splitBin As SplitContainer
    Friend WithEvents lstStrings As ListBox
    Friend WithEvents txtHex As TextBox
    Friend WithEvents lblVersion As Label
    Friend WithEvents numVersion As NumericUpDown
    Friend WithEvents status As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
End Class
