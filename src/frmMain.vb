Imports System.IO
Imports System.Text

Public Class frmMain
    ' Known files in a save's data folder
    Private Const FileDb As String = "PlayerDatabase"
    Private Const FileProfile As String = "ProfileData"
    Private Const FileState As String = "SaveState"
    Private Const FileBinary As String = "BinarySaveState"
    Private Const FileVersion As String = "VersionFlags"

    Private _db As PlayerDb
    Private _profile As ProfileFile
    Private _state As SaveStateFile
    Private _verPath As String

    Private _profileLeaves As List(Of ProfileNode)
    Private _table As String
    Private _hasRowId As Boolean
    Private _dirty As Boolean
    Private _loading As Boolean
    Private _dataFolder As String

    ' Added in code so it doesn't touch the designer file
    Private WithEvents mnuSaveNewRev As New ToolStripMenuItem("Save as &new revision") With {
        .Enabled = False,
        .ToolTipText = "Writes the edited files as the next revision number and leaves the loaded revision untouched"}

#Region "Opening"

    Private Sub mnuOpenFolder_Click(sender As Object, e As EventArgs) Handles mnuOpenFolder.Click
        Using dlg As New FolderBrowserDialog With {.Description = "Select the save's data folder (contains PlayerDatabase, ProfileData, SaveState, ...)"}
            If dlg.ShowDialog() <> DialogResult.OK Then Return
            OpenFolder(dlg.SelectedPath)
        End Using
    End Sub

    Private Sub OpenFolder(folder As String)
        If Not ConfirmDiscard() Then Return
        CloseAll()
        Dim found = 0
        For Each fileName In {FileDb, FileProfile, FileState, FileVersion}
            Dim p = Path.Combine(folder, fileName)
            If File.Exists(p) AndAlso LoadFile(p) Then found += 1
        Next
        If found = 0 Then MessageBox.Show(Me, "None of the known save files were found in that folder.", "Nothing to open", MessageBoxButtons.OK, MessageBoxIcon.Information)
        AfterLoad(folder)
    End Sub

    ' Optional: pass a save folder on the command line to open it at startup
    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        split.SplitterDistance = 260 ' set after layout so the panels get sensible widths
        mnuFile.DropDownItems.Insert(mnuFile.DropDownItems.IndexOf(mnuSaveAll) + 1, mnuSaveNewRev)
        Dim args = Environment.GetCommandLineArgs()
        If args.Length > 1 AndAlso Directory.Exists(args(1)) Then OpenFolder(args(1))
    End Sub

    Private Sub mnuOpenFile_Click(sender As Object, e As EventArgs) Handles mnuOpenFile.Click
        Using dlg As New OpenFileDialog With {.Title = "Open a save file", .Filter = "Save files|PlayerDatabase;ProfileData;SaveState;VersionFlags|All files|*.*"}
            If dlg.ShowDialog() <> DialogResult.OK Then Return
            If LoadFile(dlg.FileName) Then AfterLoad(Path.GetDirectoryName(dlg.FileName))
        End Using
    End Sub

    ''' <summary>Loads one save file, chosen by its file name. Returns False if it failed or isn't recognised.</summary>
    Private Function LoadFile(path As String) As Boolean
        Dim name = System.IO.Path.GetFileName(path)
        Try
            _loading = True
            Select Case True
                Case name.Equals(FileDb, StringComparison.OrdinalIgnoreCase)
                    Dim db As New PlayerDb(path)
                    _db?.Dispose()
                    _db = db
                    grid.DataSource = Nothing
                    lstTables.DataSource = _db.GetTableNames()
                Case name.Equals(FileProfile, StringComparison.OrdinalIgnoreCase)
                    _profile = New ProfileFile(path)
                    BindProfile()
                Case name.Equals(FileState, StringComparison.OrdinalIgnoreCase)
                    _state = New SaveStateFile(path)
                    BindState()
                Case name.Equals(FileVersion, StringComparison.OrdinalIgnoreCase)
                    Dim b = File.ReadAllBytes(path)
                    If b.Length <> 4 Then Throw New InvalidDataException("VersionFlags should be exactly 4 bytes.")
                    _verPath = path
                    numVersion.Value = BitConverter.ToUInt32(b, 0)
                Case Else
                    MessageBox.Show(Me, $"""{name}"" is not one of the known save files.", "Unknown file", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return False
            End Select
            Return True
        Catch ex As Exception
            MessageBox.Show(Me, $"{name}: {ex.Message}", "Cannot open file", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            _loading = False
        End Try
    End Function

    Private Sub AfterLoad(folder As String)
        _dirty = False
        Dim loaded = New List(Of String)
        If _db IsNot Nothing Then loaded.Add(FileDb)
        If _profile IsNot Nothing Then loaded.Add(FileProfile)
        If _state IsNot Nothing Then loaded.Add(FileState)
        If _verPath IsNot Nothing Then loaded.Add(FileVersion)
        Dim any = loaded.Count > 0
        mnuSaveAll.Enabled = any
        mnuSaveAllTo.Enabled = any
        _dataFolder = folder
        Dim commitPath = CommitManifest.CommitPathFor(folder)
        mnuSaveNewRev.Enabled = any AndAlso commitPath IsNot Nothing AndAlso File.Exists(commitPath)
        mnuExport.Enabled = _db IsNot Nothing
        Text = "Save Editor - " & folder
        lblStatus.Text = "Loaded: " & String.Join(", ", loaded)
        If _state IsNot Nothing AndAlso _state.UnparsedProperties > 0 Then
            lblStatus.Text &= $"  (SaveState: {_state.UnparsedProperties} properties could not be read)"
        End If
    End Sub

    Private Sub CloseAll()
        _db?.Dispose()
        _db = Nothing
        _profile = Nothing
        _state = Nothing
        _verPath = Nothing
        grid.DataSource = Nothing
        lstTables.DataSource = Nothing
        gridProfile.DataSource = Nothing
        gridState.DataSource = Nothing
    End Sub

#End Region

#Region "PlayerDatabase tab"

    Private Sub lstTables_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstTables.SelectedIndexChanged
        If _db Is Nothing OrElse lstTables.SelectedItem Is Nothing Then Return
        _table = CStr(lstTables.SelectedItem)
        Dim dt = _db.LoadTable(_table)
        _hasRowId = dt.Columns.Contains("__rowid")
        grid.DataSource = dt
        If _hasRowId Then
            grid.Columns("__rowid").Visible = False
            grid.ReadOnly = False
        Else
            grid.ReadOnly = True
        End If
        lblStatus.Text = $"{_table}: {dt.Rows.Count} rows" & If(_hasRowId, "", " (read-only)")
    End Sub

    Private Sub grid_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles grid.CellEndEdit
        If Not _hasRowId Then Return
        Dim row = grid.Rows(e.RowIndex)
        Dim colName = grid.Columns(e.ColumnIndex).DataPropertyName
        Dim rowId = Convert.ToInt64(row.Cells("__rowid").Value)
        Try
            _db.UpdateCell(_table, rowId, colName, row.Cells(e.ColumnIndex).Value)
            _dirty = True
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Update failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            lstTables_SelectedIndexChanged(Nothing, EventArgs.Empty) ' reload to discard the bad edit
        End Try
    End Sub

    Private Sub grid_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles grid.DataError
        MessageBox.Show(Me, e.Exception.Message, "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        e.ThrowException = False
    End Sub

#End Region

#Region "ProfileData tab"

    Private Sub BindProfile()
        _profileLeaves = _profile.Leaves()
        Dim dt As New DataTable()
        dt.Columns.Add("Idx", GetType(Integer))
        dt.Columns.Add("Name", GetType(String))
        dt.Columns.Add("Type", GetType(String))
        dt.Columns.Add("Value", GetType(String))
        For i = 0 To _profileLeaves.Count - 1
            Dim n = _profileLeaves(i)
            dt.Rows.Add(i, n.Path, n.TypeName, n.DisplayValue())
        Next
        gridProfile.DataSource = dt
        gridProfile.Columns("Idx").Visible = False
        gridProfile.Columns("Name").ReadOnly = True
        gridProfile.Columns("Type").ReadOnly = True
        gridProfile.Columns("Value").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    End Sub

    Private Sub txtProfileFilter_TextChanged(sender As Object, e As EventArgs) Handles txtProfileFilter.TextChanged
        Dim dt = TryCast(gridProfile.DataSource, DataTable)
        If dt Is Nothing Then Return
        dt.DefaultView.RowFilter = If(txtProfileFilter.Text = "", "", $"Name LIKE '%{EscapeLike(txtProfileFilter.Text)}%'")
    End Sub

    Private Sub gridProfile_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles gridProfile.CellEndEdit
        Dim row = gridProfile.Rows(e.RowIndex)
        Dim node = _profileLeaves(CInt(row.Cells("Idx").Value))
        Dim err = node.TrySetValue(Convert.ToString(row.Cells("Value").Value))
        If err IsNot Nothing Then
            MessageBox.Show(Me, err, node.Path, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Else
            _dirty = True
        End If
        row.Cells("Value").Value = node.DisplayValue() ' show the normalised (or restored) value
    End Sub

#End Region

#Region "SaveState tab"

    Private Sub BindState()
        Dim dt As New DataTable()
        dt.Columns.Add("Idx", GetType(Integer))
        dt.Columns.Add("Object", GetType(String))
        dt.Columns.Add("Type", GetType(String))
        dt.Columns.Add("Property", GetType(String))
        dt.Columns.Add("Value", GetType(String))
        For i = 0 To _state.Entries.Count - 1
            Dim en = _state.Entries(i)
            dt.Rows.Add(i, en.ObjectId, en.ObjectType, en.Name, en.Value)
        Next
        gridState.DataSource = dt
        gridState.Columns("Idx").Visible = False
        gridState.Columns("Object").ReadOnly = True
        gridState.Columns("Type").ReadOnly = True
        gridState.Columns("Property").ReadOnly = True
        gridState.Columns("Value").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
    End Sub

    Private Sub txtStateFilter_TextChanged(sender As Object, e As EventArgs) Handles txtStateFilter.TextChanged
        Dim dt = TryCast(gridState.DataSource, DataTable)
        If dt Is Nothing Then Return
        If txtStateFilter.Text = "" Then
            dt.DefaultView.RowFilter = ""
        Else
            Dim s = EscapeLike(txtStateFilter.Text)
            dt.DefaultView.RowFilter = $"Object LIKE '%{s}%' OR Property LIKE '%{s}%' OR Value LIKE '%{s}%'"
        End If
    End Sub

    Private Sub gridState_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles gridState.CellEndEdit
        Dim row = gridState.Rows(e.RowIndex)
        Dim entry = _state.Entries(CInt(row.Cells("Idx").Value))
        Dim newValue = Convert.ToString(row.Cells("Value").Value)
        If newValue <> entry.Value Then
            entry.Value = newValue
            _dirty = True
        End If
    End Sub

#End Region

#Region "VersionFlags tab"

    Private Sub numVersion_ValueChanged(sender As Object, e As EventArgs) Handles numVersion.ValueChanged
        If Not _loading Then _dirty = True
    End Sub

#End Region

#Region "Saving"

    Private Sub mnuSaveAll_Click(sender As Object, e As EventArgs) Handles mnuSaveAll.Click
        SaveAll(Nothing)
    End Sub

    Private Sub mnuSaveAllTo_Click(sender As Object, e As EventArgs) Handles mnuSaveAllTo.Click
        Using dlg As New FolderBrowserDialog With {.Description = "Folder to write the save files into"}
            If dlg.ShowDialog() = DialogResult.OK Then SaveAll(dlg.SelectedPath)
        End Using
    End Sub

    Private Sub mnuSaveNewRev_Click(sender As Object, e As EventArgs) Handles mnuSaveNewRev.Click
        SaveAll(Nothing, newRevision:=True)
    End Sub

    ''' <summary>
    ''' Writes every loaded file, then regenerates the revision's "commit" manifest (the game verifies it).
    ''' destFolder = Nothing overwrites the loaded revision in place (originals are backed up outside the save folder);
    ''' newRevision = True writes a copy of the loaded revision as the next revision number instead.
    ''' </summary>
    Private Sub SaveAll(destFolder As String, Optional newRevision As Boolean = False)
        Try
            gridProfile.EndEdit()
            gridState.EndEdit()
            grid.EndEdit()
            Dim written As New List(Of String)
            Dim target = If(destFolder, _dataFolder)
            Dim backupTag = Path.GetFileName(Path.GetDirectoryName(_dataFolder))

            If newRevision Then
                target = CreateNextRevision(_dataFolder)
                backupTag = Nothing ' nothing existing gets overwritten
            ElseIf destFolder IsNot Nothing Then
                Directory.CreateDirectory(destFolder)
            End If

            If _db IsNot Nothing Then
                Dim p = Path.Combine(target, FileDb)
                BackupExisting(p, backupTag)
                _db.Save(p)
                written.Add(FileDb)
            End If
            If _profile IsNot Nothing Then
                WriteFile(Path.Combine(target, FileProfile), _profile.Serialize(), backupTag)
                written.Add(FileProfile)
            End If
            If _state IsNot Nothing Then
                WriteFile(Path.Combine(target, FileState), _state.Serialize(), backupTag)
                written.Add(FileState)
            End If
            ' BinarySaveState isn't edited: leave it as-is, or copy it when writing to another folder
            If destFolder IsNot Nothing AndAlso File.Exists(Path.Combine(_dataFolder, FileBinary)) Then
                File.Copy(Path.Combine(_dataFolder, FileBinary), Path.Combine(target, FileBinary), True)
            End If
            If _verPath IsNot Nothing Then
                WriteFile(Path.Combine(target, FileVersion), BitConverter.GetBytes(CUInt(numVersion.Value)), backupTag)
                written.Add(FileVersion)
            End If

            Dim commitNote = ""
            Dim commitPath = CommitManifest.CommitPathFor(target)
            If commitPath IsNot Nothing AndAlso (File.Exists(commitPath) OrElse newRevision) Then
                BackupExisting(commitPath, backupTag)
                CommitManifest.Update(target, {FileBinary, FileDb, FileProfile, FileState, FileVersion})
                commitNote = " + commit manifest"
            Else
                commitNote = " (no 'commit' file next to that folder, so no manifest was written)"
            End If

            _dirty = False
            lblStatus.Text = $"Saved to {target}: {String.Join(", ", written)}{commitNote}"
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ''' <summary>Creates revisions\{next}\data containing a copy of every file in dataFolder, plus its commit file.</summary>
    Private Shared Function CreateNextRevision(dataFolder As String) As String
        Dim revFolder = Path.GetDirectoryName(dataFolder)
        Dim revisionsDir = Path.GetDirectoryName(revFolder)
        Dim highest As Long = 0
        For Each d In Directory.GetDirectories(revisionsDir)
            Dim v As Long
            If Long.TryParse(Path.GetFileName(d), Globalization.NumberStyles.HexNumber, Nothing, v) Then highest = Math.Max(highest, v)
        Next
        Dim newRev = Path.Combine(revisionsDir, (highest + 1).ToString("x16"))
        Dim newData = Path.Combine(newRev, "data")
        Directory.CreateDirectory(newData)

        Dim header As String = Nothing, names As List(Of String) = Nothing
        CommitManifest.ReadExisting(Path.Combine(revFolder, "commit"), header, names)
        For Each n In names ' only the files the manifest lists; never stray files like .bak
            If File.Exists(Path.Combine(dataFolder, n)) Then File.Copy(Path.Combine(dataFolder, n), Path.Combine(newData, n))
        Next
        Return newData
    End Function

    Private Shared _backupRoot As String

    ''' <summary>Copies an existing file to %LocalAppData%\SaveEditor\Backups (never into the save folder). Kept once per run.</summary>
    Private Shared Sub BackupExisting(path As String, tag As String)
        If tag Is Nothing OrElse Not File.Exists(path) Then Return
        If _backupRoot Is Nothing Then
            _backupRoot = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                 "SaveEditor", "Backups", Date.Now.ToString("yyyyMMdd-HHmmss"))
        End If
        Dim dir = System.IO.Path.Combine(_backupRoot, tag)
        Directory.CreateDirectory(dir)
        Dim dest = System.IO.Path.Combine(dir, System.IO.Path.GetFileName(path))
        If Not File.Exists(dest) Then File.Copy(path, dest)
    End Sub

    Private Shared Sub WriteFile(path As String, bytes As Byte(), backupTag As String)
        BackupExisting(path, backupTag)
        File.WriteAllBytes(path, bytes)
    End Sub

    Private Sub mnuExport_Click(sender As Object, e As EventArgs) Handles mnuExport.Click
        Using dlg As New SaveFileDialog With {.Title = "Export SQLite", .Filter = "SQLite database|*.sqlite", .FileName = "PlayerDatabase.sqlite"}
            If dlg.ShowDialog() <> DialogResult.OK Then Return
            Try
                _db.ExportSqlite(dlg.FileName)
                lblStatus.Text = "Exported " & dlg.FileName
            Catch ex As Exception
                MessageBox.Show(Me, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

#End Region

    Private Shared Function EscapeLike(s As String) As String
        Dim sb As New StringBuilder()
        For Each c In s
            Select Case c
                Case "'"c : sb.Append("''")
                Case "["c, "]"c, "%"c, "*"c : sb.Append("[").Append(c).Append("]")
                Case Else : sb.Append(c)
            End Select
        Next
        Return sb.ToString()
    End Function

    Private Function ConfirmDiscard() As Boolean
        If Not _dirty Then Return True
        Return MessageBox.Show(Me, "You have unsaved changes. Discard them?", "Unsaved changes",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes
    End Function

    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Not ConfirmDiscard() Then
            e.Cancel = True
        Else
            _db?.Dispose()
        End If
    End Sub
End Class
