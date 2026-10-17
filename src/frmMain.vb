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
    Private _binPath As String
    Private _binData As Byte()
    Private _verPath As String

    Private _profileLeaves As List(Of ProfileNode)
    Private _table As String
    Private _hasRowId As Boolean
    Private _dirty As Boolean
    Private _loading As Boolean

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
        For Each fileName In {FileDb, FileProfile, FileState, FileBinary, FileVersion}
            Dim p = Path.Combine(folder, fileName)
            If File.Exists(p) AndAlso LoadFile(p) Then found += 1
        Next
        If found = 0 Then MessageBox.Show(Me, "None of the known save files were found in that folder.", "Nothing to open", MessageBoxButtons.OK, MessageBoxIcon.Information)
        AfterLoad(folder)
    End Sub

    ' Optional: pass a save folder on the command line to open it at startup
    Private Sub frmMain_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        split.SplitterDistance = 260 ' set after layout so the panels get sensible widths
        splitBin.SplitterDistance = 420
        Dim args = Environment.GetCommandLineArgs()
        If args.Length > 1 AndAlso Directory.Exists(args(1)) Then OpenFolder(args(1))
    End Sub

    Private Sub mnuOpenFile_Click(sender As Object, e As EventArgs) Handles mnuOpenFile.Click
        Using dlg As New OpenFileDialog With {.Title = "Open a save file", .Filter = "Save files|PlayerDatabase;ProfileData;SaveState;BinarySaveState;VersionFlags|All files|*.*"}
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
                Case name.Equals(FileBinary, StringComparison.OrdinalIgnoreCase)
                    _binPath = path
                    _binData = File.ReadAllBytes(path)
                    BindBinary()
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
        If _binData IsNot Nothing Then loaded.Add(FileBinary)
        If _verPath IsNot Nothing Then loaded.Add(FileVersion)
        Dim any = loaded.Count > 0
        mnuSaveAll.Enabled = any
        mnuSaveAllTo.Enabled = any
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
        _binData = Nothing
        _binPath = Nothing
        _verPath = Nothing
        grid.DataSource = Nothing
        lstTables.DataSource = Nothing
        gridProfile.DataSource = Nothing
        gridState.DataSource = Nothing
        lstStrings.Items.Clear()
        txtHex.Clear()
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

#Region "BinarySaveState tab (read-only)"

    Private Sub BindBinary()
        Dim sb As New StringBuilder(_binData.Length * 5)
        For off = 0 To _binData.Length - 1 Step 16
            sb.Append(off.ToString("X8")).Append("  ")
            Dim n = Math.Min(16, _binData.Length - off)
            For i = 0 To 15
                sb.Append(If(i < n, _binData(off + i).ToString("X2") & " ", "   "))
            Next
            sb.Append(" ")
            For i = 0 To n - 1
                Dim b = _binData(off + i)
                sb.Append(If(b >= 32 AndAlso b < 127, ChrW(b), "."c))
            Next
            sb.AppendLine()
        Next
        txtHex.Text = sb.ToString()

        lstStrings.BeginUpdate()
        lstStrings.Items.Clear()
        For Each s In FindStrings(_binData)
            lstStrings.Items.Add(s)
        Next
        lstStrings.EndUpdate()
    End Sub

    ''' <summary>Finds ASCII runs (6+ chars) and UTF-16LE runs (4+ chars), e.g. Drivatar names.</summary>
    Private Shared Function FindStrings(data As Byte()) As List(Of String)
        Dim found As New SortedDictionary(Of Integer, String)
        Dim i = 0
        While i < data.Length
            Dim j = i
            While j < data.Length AndAlso data(j) >= 32 AndAlso data(j) < 127
                j += 1
            End While
            If j - i >= 6 Then found(i) = $"{i:X8}  {Encoding.ASCII.GetString(data, i, j - i)}"
            i = j + 1
        End While
        i = 0
        While i + 1 < data.Length
            Dim j = i
            While j + 1 < data.Length AndAlso data(j) >= 32 AndAlso data(j) < 127 AndAlso data(j + 1) = 0
                j += 2
            End While
            If (j - i) \ 2 >= 4 Then found(i) = $"{i:X8}  {Encoding.Unicode.GetString(data, i, j - i)}  (UTF-16)"
            i = If(j > i, j, i + 1)
        End While
        Return found.Values.Take(5000).ToList()
    End Function

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

    ''' <summary>Writes every loaded file. destFolder = Nothing overwrites the originals (a .bak of each is kept once).</summary>
    Private Sub SaveAll(destFolder As String)
        Try
            gridProfile.EndEdit()
            gridState.EndEdit()
            grid.EndEdit()
            Dim written As New List(Of String)

            If _db IsNot Nothing Then
                Dim p = DestPath(_db.SourcePath, destFolder)
                Backup(p)
                _db.Save(p)
                written.Add(FileDb)
            End If
            If _profile IsNot Nothing Then
                Dim bytes = _profile.Serialize()
                WriteFile(DestPath(_profile.SourcePath, destFolder), bytes)
                written.Add(FileProfile)
            End If
            If _state IsNot Nothing Then
                WriteFile(DestPath(_state.SourcePath, destFolder), _state.Serialize())
                written.Add(FileState)
            End If
            If _binData IsNot Nothing Then
                WriteFile(DestPath(_binPath, destFolder), _binData)
                written.Add(FileBinary)
            End If
            If _verPath IsNot Nothing Then
                WriteFile(DestPath(_verPath, destFolder), BitConverter.GetBytes(CUInt(numVersion.Value)))
                written.Add(FileVersion)
            End If

            _dirty = False
            lblStatus.Text = "Saved: " & String.Join(", ", written)
        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Shared Function DestPath(sourcePath As String, destFolder As String) As String
        Return If(destFolder Is Nothing, sourcePath, Path.Combine(destFolder, Path.GetFileName(sourcePath)))
    End Function

    Private Shared Sub Backup(path As String)
        Dim bak = path & ".bak"
        If File.Exists(path) AndAlso Not File.Exists(bak) Then File.Copy(path, bak) ' keep the very first original
    End Sub

    Private Shared Sub WriteFile(path As String, bytes As Byte())
        Backup(path)
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
