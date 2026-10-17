Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' Each revision folder holds "data" plus a "commit" manifest:
'''   Azimuth complete save revision v2
'''   blobs N
'''   &lt;sha256 hex&gt; &lt;size&gt; &lt;file name&gt;   (one line per file, ordinal order, LF endings)
''' The game rejects a revision whose files don't match the manifest, so it must be regenerated after any edit.
''' </summary>
Public Module CommitManifest
    Private Const DefaultHeader As String = "Azimuth complete save revision v2"

    Public Function CommitPathFor(dataFolder As String) As String
        Dim parent = Directory.GetParent(dataFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        Return If(parent Is Nothing, Nothing, Path.Combine(parent.FullName, "commit"))
    End Function

    ''' <summary>Builds manifest text for the given file names inside dataFolder.</summary>
    Public Function Build(dataFolder As String, names As IEnumerable(Of String), header As String) As String
        Dim sorted = names.OrderBy(Function(n) n, StringComparer.Ordinal).ToList()
        Dim sb As New StringBuilder()
        sb.Append(header).Append(vbLf)
        sb.Append("blobs ").Append(sorted.Count).Append(vbLf)
        For Each n In sorted
            Dim bytes = File.ReadAllBytes(Path.Combine(dataFolder, n))
            sb.Append(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()).Append(" "c).
               Append(bytes.Length).Append(" "c).Append(n).Append(vbLf)
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Reads header and blob names from an existing manifest.</summary>
    Public Sub ReadExisting(commitPath As String, ByRef header As String, ByRef names As List(Of String))
        header = DefaultHeader
        names = New List(Of String)
        If Not File.Exists(commitPath) Then Return
        Dim lines = File.ReadAllText(commitPath, New UTF8Encoding(False)).Split(ControlChars.Lf)
        If lines.Length > 0 AndAlso lines(0).Length > 0 Then header = lines(0).TrimEnd(ControlChars.Cr)
        For i = 2 To lines.Length - 1
            Dim parts = lines(i).TrimEnd(ControlChars.Cr).Split(" "c, 3)
            If parts.Length = 3 Then names.Add(parts(2))
        Next
    End Sub

    ''' <summary>Rewrites the revision's commit file so it matches the files now in dataFolder.</summary>
    Public Sub Update(dataFolder As String, fallbackNames As IEnumerable(Of String))
        Dim commitPath = CommitPathFor(dataFolder)
        Dim header As String = Nothing, names As List(Of String) = Nothing
        ReadExisting(commitPath, header, names)
        If names.Count = 0 Then names = fallbackNames.Where(Function(n) File.Exists(Path.Combine(dataFolder, n))).ToList()
        File.WriteAllBytes(commitPath, New UTF8Encoding(False).GetBytes(Build(dataFolder, names, header)))
    End Sub
End Module
