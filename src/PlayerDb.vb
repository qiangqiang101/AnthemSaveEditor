Imports System.IO
Imports System.Text
Imports Microsoft.Data.Sqlite

''' <summary>
''' Wraps a game PlayerDatabase file: an 8-byte header followed by a plain SQLite 3 database.
''' The database is edited in a temp file; Save writes it back with the original header,
''' padded to the original file size.
''' </summary>
Public Class PlayerDb
    Implements IDisposable

    Private Const HeaderLen As Integer = 8
    Private Shared ReadOnly Magic As Byte() = Encoding.ASCII.GetBytes("SQLite format 3" & vbNullChar)

    Private _header As Byte()
    Private _originalLength As Integer
    Private _tempPath As String
    Private _conn As SqliteConnection

    Public ReadOnly Property SourcePath As String

    Public Sub New(path As String)
        SourcePath = path
        Dim data = File.ReadAllBytes(path)
        If data.Length <= HeaderLen + Magic.Length Then Throw New InvalidDataException("File is too small.")
        For i = 0 To Magic.Length - 1
            If data(HeaderLen + i) <> Magic(i) Then
                Throw New InvalidDataException("No SQLite signature found after the 8-byte header.")
            End If
        Next

        _header = data.Take(HeaderLen).ToArray()
        _originalLength = data.Length
        _tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "pdb_" & Guid.NewGuid().ToString("N") & ".sqlite")
        File.WriteAllBytes(_tempPath, data.Skip(HeaderLen).ToArray())

        _conn = New SqliteConnection($"Data Source={_tempPath};Pooling=False")
        _conn.Open()
        Using cmd = _conn.CreateCommand()
            cmd.CommandText = "PRAGMA journal_mode=DELETE"
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public ReadOnly Property HeaderHex As String
        Get
            Return BitConverter.ToString(_header).Replace("-", "")
        End Get
    End Property

    Public Function GetTableNames() As List(Of String)
        Dim names As New List(Of String)
        Using cmd = _conn.CreateCommand()
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name"
            Using r = cmd.ExecuteReader()
                While r.Read()
                    names.Add(r.GetString(0))
                End While
            End Using
        End Using
        Return names
    End Function

    Private Shared Function Q(identifier As String) As String
        Return """" & identifier.Replace("""", """""") & """"
    End Function

    ''' <summary>Loads a table. Column 0 is the hidden rowid (named __rowid) used for updates.</summary>
    Public Function LoadTable(table As String) As DataTable
        Dim dt As New DataTable()
        Using cmd = _conn.CreateCommand()
            cmd.CommandText = $"SELECT rowid AS __rowid, * FROM {Q(table)}"
            Try
                Using r = cmd.ExecuteReader()
                    dt.Load(r)
                End Using
            Catch ex As SqliteException
                ' WITHOUT ROWID tables: load read-only without the rowid column
                cmd.CommandText = $"SELECT * FROM {Q(table)}"
                dt = New DataTable()
                Using r = cmd.ExecuteReader()
                    dt.Load(r)
                End Using
            End Try
        End Using
        Return dt
    End Function

    Public Sub UpdateCell(table As String, rowId As Long, column As String, value As Object)
        Using cmd = _conn.CreateCommand()
            cmd.CommandText = $"UPDATE {Q(table)} SET {Q(column)} = $v WHERE rowid = $id"
            cmd.Parameters.AddWithValue("$v", If(value Is Nothing OrElse value Is DBNull.Value, DBNull.Value, value))
            cmd.Parameters.AddWithValue("$id", rowId)
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Writes header + database to destPath, padded to the original size.</summary>
    Public Sub Save(destPath As String)
        _conn.Close()
        _conn.Dispose()
        Try
            Dim body = File.ReadAllBytes(_tempPath)
            Dim capacity = _originalLength - HeaderLen
            If body.Length > capacity Then
                Throw New InvalidOperationException($"Database grew to {body.Length} bytes; the original capacity is {capacity}.")
            End If
            Dim result(_originalLength - 1) As Byte
            Array.Copy(_header, 0, result, 0, HeaderLen)
            Array.Copy(body, 0, result, HeaderLen, body.Length)
            File.WriteAllBytes(destPath, result)
        Finally
            _conn = New SqliteConnection($"Data Source={_tempPath};Pooling=False")
            _conn.Open()
        End Try
    End Sub

    ''' <summary>Exports the plain SQLite database (no game header) for use in other tools.</summary>
    Public Sub ExportSqlite(destPath As String)
        _conn.Close()
        _conn.Dispose()
        Try
            File.Copy(_tempPath, destPath, True)
        Finally
            _conn = New SqliteConnection($"Data Source={_tempPath};Pooling=False")
            _conn.Open()
        End Try
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _conn IsNot Nothing Then
            _conn.Dispose()
            _conn = Nothing
        End If
        Try
            If _tempPath IsNot Nothing Then File.Delete(_tempPath)
        Catch
        End Try
    End Sub
End Class
