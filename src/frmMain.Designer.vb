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
        mainMenu = New MenuStrip()
        mnuFile = New ToolStripMenuItem()
        mnuOpenFolder = New ToolStripMenuItem()
        mnuOpenFile = New ToolStripMenuItem()
        mnuSaveAll = New ToolStripMenuItem()
        mnuSaveAllTo = New ToolStripMenuItem()
        mnuExport = New ToolStripMenuItem()
        tabs = New TabControl()
        tabDb = New TabPage()
        split = New SplitContainer()
        lstTables = New ListBox()
        grid = New DataGridView()
        tabProfile = New TabPage()
        gridProfile = New DataGridView()
        txtProfileFilter = New TextBox()
        tabState = New TabPage()
        gridState = New DataGridView()
        txtStateFilter = New TextBox()
        tabVersion = New TabPage()
        lblVersion = New Label()
        numVersion = New NumericUpDown()
        status = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()
        mainMenu.SuspendLayout()
        tabs.SuspendLayout()
        tabDb.SuspendLayout()
        CType(split, ComponentModel.ISupportInitialize).BeginInit()
        split.Panel1.SuspendLayout()
        split.Panel2.SuspendLayout()
        split.SuspendLayout()
        CType(grid, ComponentModel.ISupportInitialize).BeginInit()
        tabProfile.SuspendLayout()
        CType(gridProfile, ComponentModel.ISupportInitialize).BeginInit()
        tabState.SuspendLayout()
        CType(gridState, ComponentModel.ISupportInitialize).BeginInit()
        tabVersion.SuspendLayout()
        CType(numVersion, ComponentModel.ISupportInitialize).BeginInit()
        status.SuspendLayout()
        SuspendLayout()
        ' 
        ' mainMenu
        ' 
        mainMenu.Items.AddRange(New ToolStripItem() {mnuFile})
        mainMenu.Location = New Point(0, 0)
        mainMenu.Name = "mainMenu"
        mainMenu.Size = New Size(1100, 24)
        mainMenu.TabIndex = 2
        ' 
        ' mnuFile
        ' 
        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuOpenFolder, mnuOpenFile, mnuSaveAll, mnuSaveAllTo, mnuExport})
        mnuFile.Name = "mnuFile"
        mnuFile.Size = New Size(37, 20)
        mnuFile.Text = "&File"
        ' 
        ' mnuOpenFolder
        ' 
        mnuOpenFolder.Name = "mnuOpenFolder"
        mnuOpenFolder.Size = New Size(247, 22)
        mnuOpenFolder.Text = "Open save &folder..."
        ' 
        ' mnuOpenFile
        ' 
        mnuOpenFile.Name = "mnuOpenFile"
        mnuOpenFile.Size = New Size(247, 22)
        mnuOpenFile.Text = "&Open single file..."
        ' 
        ' mnuSaveAll
        ' 
        mnuSaveAll.Enabled = False
        mnuSaveAll.Name = "mnuSaveAll"
        mnuSaveAll.Size = New Size(247, 22)
        mnuSaveAll.Text = "&Save all"
        ' 
        ' mnuSaveAllTo
        ' 
        mnuSaveAllTo.Enabled = False
        mnuSaveAllTo.Name = "mnuSaveAllTo"
        mnuSaveAllTo.Size = New Size(247, 22)
        mnuSaveAllTo.Text = "Save all &to folder..."
        ' 
        ' mnuExport
        ' 
        mnuExport.Enabled = False
        mnuExport.Name = "mnuExport"
        mnuExport.Size = New Size(247, 22)
        mnuExport.Text = "&Export PlayerDatabase as .sqlite..."
        ' 
        ' tabs
        ' 
        tabs.Controls.Add(tabDb)
        tabs.Controls.Add(tabProfile)
        tabs.Controls.Add(tabState)
        tabs.Controls.Add(tabVersion)
        tabs.Dock = DockStyle.Fill
        tabs.Location = New Point(0, 24)
        tabs.Name = "tabs"
        tabs.SelectedIndex = 0
        tabs.Size = New Size(1100, 604)
        tabs.TabIndex = 0
        ' 
        ' tabDb
        ' 
        tabDb.Controls.Add(split)
        tabDb.Location = New Point(4, 24)
        tabDb.Name = "tabDb"
        tabDb.Size = New Size(1092, 576)
        tabDb.TabIndex = 0
        tabDb.Text = "PlayerDatabase"
        ' 
        ' split
        ' 
        split.Dock = DockStyle.Fill
        split.Location = New Point(0, 0)
        split.Name = "split"
        ' 
        ' split.Panel1
        ' 
        split.Panel1.Controls.Add(lstTables)
        ' 
        ' split.Panel2
        ' 
        split.Panel2.Controls.Add(grid)
        split.Size = New Size(1092, 576)
        split.SplitterDistance = 209
        split.TabIndex = 0
        ' 
        ' lstTables
        ' 
        lstTables.Dock = DockStyle.Fill
        lstTables.Location = New Point(0, 0)
        lstTables.Name = "lstTables"
        lstTables.Size = New Size(209, 576)
        lstTables.TabIndex = 0
        ' 
        ' grid
        ' 
        grid.Dock = DockStyle.Fill
        grid.Location = New Point(0, 0)
        grid.Name = "grid"
        grid.Size = New Size(879, 576)
        grid.TabIndex = 0
        ' 
        ' tabProfile
        ' 
        tabProfile.Controls.Add(gridProfile)
        tabProfile.Controls.Add(txtProfileFilter)
        tabProfile.Location = New Point(4, 24)
        tabProfile.Name = "tabProfile"
        tabProfile.Size = New Size(1092, 576)
        tabProfile.TabIndex = 1
        tabProfile.Text = "ProfileData"
        ' 
        ' gridProfile
        ' 
        gridProfile.Dock = DockStyle.Fill
        gridProfile.Location = New Point(0, 23)
        gridProfile.Name = "gridProfile"
        gridProfile.Size = New Size(1092, 553)
        gridProfile.TabIndex = 0
        ' 
        ' txtProfileFilter
        ' 
        txtProfileFilter.Dock = DockStyle.Top
        txtProfileFilter.Location = New Point(0, 0)
        txtProfileFilter.Name = "txtProfileFilter"
        txtProfileFilter.PlaceholderText = "Filter by name, e.g. Credits"
        txtProfileFilter.Size = New Size(1092, 23)
        txtProfileFilter.TabIndex = 1
        ' 
        ' tabState
        ' 
        tabState.Controls.Add(gridState)
        tabState.Controls.Add(txtStateFilter)
        tabState.Location = New Point(4, 24)
        tabState.Name = "tabState"
        tabState.Size = New Size(1092, 576)
        tabState.TabIndex = 2
        tabState.Text = "SaveState"
        ' 
        ' gridState
        ' 
        gridState.Dock = DockStyle.Fill
        gridState.Location = New Point(0, 23)
        gridState.Name = "gridState"
        gridState.Size = New Size(1092, 553)
        gridState.TabIndex = 0
        ' 
        ' txtStateFilter
        ' 
        txtStateFilter.Dock = DockStyle.Top
        txtStateFilter.Location = New Point(0, 0)
        txtStateFilter.Name = "txtStateFilter"
        txtStateFilter.PlaceholderText = "Filter by object, property or value"
        txtStateFilter.Size = New Size(1092, 23)
        txtStateFilter.TabIndex = 1
        ' 
        ' tabVersion
        ' 
        tabVersion.Controls.Add(lblVersion)
        tabVersion.Controls.Add(numVersion)
        tabVersion.Location = New Point(4, 24)
        tabVersion.Name = "tabVersion"
        tabVersion.Size = New Size(1092, 576)
        tabVersion.TabIndex = 4
        tabVersion.Text = "VersionFlags"
        ' 
        ' lblVersion
        ' 
        lblVersion.AutoSize = True
        lblVersion.Location = New Point(16, 20)
        lblVersion.Name = "lblVersion"
        lblVersion.Size = New Size(120, 15)
        lblVersion.TabIndex = 0
        lblVersion.Text = "Version flags (uint32):"
        ' 
        ' numVersion
        ' 
        numVersion.Location = New Point(16, 46)
        numVersion.Maximum = New Decimal(New Integer() {-1, 0, 0, 0})
        numVersion.Name = "numVersion"
        numVersion.Size = New Size(160, 23)
        numVersion.TabIndex = 1
        ' 
        ' status
        ' 
        status.Items.AddRange(New ToolStripItem() {lblStatus})
        status.Location = New Point(0, 628)
        status.Name = "status"
        status.Size = New Size(1100, 22)
        status.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(485, 17)
        lblStatus.Text = "Open a save folder (the one containing PlayerDatabase, ProfileData, ...) from the File menu."
        ' 
        ' frmMain
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1100, 650)
        Controls.Add(tabs)
        Controls.Add(status)
        Controls.Add(mainMenu)
        MainMenuStrip = mainMenu
        Name = "frmMain"
        Text = "Save Editor"
        mainMenu.ResumeLayout(False)
        mainMenu.PerformLayout()
        tabs.ResumeLayout(False)
        tabDb.ResumeLayout(False)
        split.Panel1.ResumeLayout(False)
        split.Panel2.ResumeLayout(False)
        CType(split, ComponentModel.ISupportInitialize).EndInit()
        split.ResumeLayout(False)
        CType(grid, ComponentModel.ISupportInitialize).EndInit()
        tabProfile.ResumeLayout(False)
        tabProfile.PerformLayout()
        CType(gridProfile, ComponentModel.ISupportInitialize).EndInit()
        tabState.ResumeLayout(False)
        tabState.PerformLayout()
        CType(gridState, ComponentModel.ISupportInitialize).EndInit()
        tabVersion.ResumeLayout(False)
        tabVersion.PerformLayout()
        CType(numVersion, ComponentModel.ISupportInitialize).EndInit()
        status.ResumeLayout(False)
        status.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
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
    Friend WithEvents tabVersion As TabPage
    Friend WithEvents split As SplitContainer
    Friend WithEvents lstTables As ListBox
    Friend WithEvents grid As DataGridView
    Friend WithEvents txtProfileFilter As TextBox
    Friend WithEvents gridProfile As DataGridView
    Friend WithEvents txtStateFilter As TextBox
    Friend WithEvents gridState As DataGridView
    Friend WithEvents lblVersion As Label
    Friend WithEvents numVersion As NumericUpDown
    Friend WithEvents status As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
End Class
