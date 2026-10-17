Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>One &lt;Property&gt; of a &lt;Object&gt; in SaveState.</summary>
Public Class SaveStateEntry
    Public Property ObjectId As String
    Public Property ObjectType As String
    Public Property Name As String
    Public Property Value As String
    Friend Property Start As Integer
    Friend Property Length As Integer
    Friend Property Original As String
    Public ReadOnly Property IsEdited As Boolean
        Get
            Return Value <> Original
        End Get
    End Property
End Class

''' <summary>
''' SaveState: an ASCII XML file. Values are edited in place by position so every other
''' byte (indentation, CRLF, "/>" spacing) is preserved exactly.
''' </summary>
Public Class SaveStateFile
    Private Shared ReadOnly Tokens As New Regex(
        "(?<o><Object\s+Id=""(?<id>[^""]*)""\s+Type=""(?<ty>[^""]*)""\s*>)|(?<p><Property\s+Name=""(?<n>[^""]*)""\s+Value=""(?<v>[^""]*)""\s*/>)",
        RegexOptions.Compiled)

    Private _text As String
    Private _utf8Bom As Boolean

    Public ReadOnly Property SourcePath As String
    Public ReadOnly Property Entries As New List(Of SaveStateEntry)
    ''' <summary>Number of &lt;Property&gt; tags found that the parser could not map to an entry.</summary>
    Public ReadOnly Property UnparsedProperties As Integer

    Public Sub New(path As String)
        SourcePath = path
        Dim bytes = File.ReadAllBytes(path)
        _utf8Bom = bytes.Length >= 3 AndAlso bytes(0) = &HEF AndAlso bytes(1) = &HBB AndAlso bytes(2) = &HBF
        _text = New UTF8Encoding(False).GetString(bytes, If(_utf8Bom, 3, 0), bytes.Length - If(_utf8Bom, 3, 0))

        Dim curId As String = "", curType As String = ""
        For Each m As Match In Tokens.Matches(_text)
            If m.Groups("o").Success Then
                curId = WebUtility.HtmlDecode(m.Groups("id").Value)
                curType = WebUtility.HtmlDecode(m.Groups("ty").Value)
            Else
                Dim g = m.Groups("v")
                Entries.Add(New SaveStateEntry With {
                    .ObjectId = curId, .ObjectType = curType,
                    .Name = WebUtility.HtmlDecode(m.Groups("n").Value),
                    .Value = WebUtility.HtmlDecode(g.Value), .Original = WebUtility.HtmlDecode(g.Value),
                    .Start = g.Index, .Length = g.Length})
            End If
        Next
        _UnparsedProperties = Regex.Matches(_text, "<Property\b").Count - Entries.Count
    End Sub

    Private Shared Function Escape(s As String) As String
        Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
    End Function

    Public Function Serialize() As Byte()
        Dim sb As New StringBuilder()
        Dim pos = 0
        For Each e In Entries ' entries are in ascending position order
            sb.Append(_text, pos, e.Start - pos)
            If e.IsEdited Then
                sb.Append(Escape(e.Value))
            Else
                sb.Append(_text, e.Start, e.Length)
            End If
            pos = e.Start + e.Length
        Next
        sb.Append(_text, pos, _text.Length - pos)

        Dim body = New UTF8Encoding(False).GetBytes(sb.ToString())
        If Not _utf8Bom Then Return body
        Return New Byte() {&HEF, &HBB, &HBF}.Concat(body).ToArray()
    End Function
End Class
