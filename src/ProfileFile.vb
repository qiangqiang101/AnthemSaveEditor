Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>One node of the ProfileData tree: a group (type 15) or a typed leaf value.</summary>
Public Class ProfileNode
    Public Const TypeGroup As UInteger = 15

    Public Property Name As String
    Public Property TypeCode As UInteger
    Public Property Value As Object
    Public ReadOnly Property Children As New List(Of ProfileNode)
    Public Property Path As String

    Public ReadOnly Property IsGroup As Boolean
        Get
            Return TypeCode = TypeGroup
        End Get
    End Property

    Public ReadOnly Property TypeName As String
        Get
            Select Case TypeCode
                Case 0 : Return "bool/byte"
                Case 1 : Return "byte"
                Case 3 : Return "int32"
                Case 4 : Return "uint64"
                Case 7 : Return "int32 (flag)"
                Case 9 : Return "float"
                Case 12 : Return "string"
                Case Else : Return "type " & TypeCode
            End Select
        End Get
    End Property

    Public Function DisplayValue() As String
        If IsGroup Then Return ""
        Select Case TypeCode
            Case 9 : Return DirectCast(Value, Single).ToString("R", CultureInfo.InvariantCulture)
            Case Else : Return Convert.ToString(Value, CultureInfo.InvariantCulture)
        End Select
    End Function

    ''' <summary>Parses text into this leaf's value. Returns an error message, or Nothing on success.</summary>
    Public Function TrySetValue(text As String) As String
        text = text.Trim()
        Select Case TypeCode
            Case 0, 1
                Dim v As Byte
                If Not Byte.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, v) Then Return "Enter a whole number from 0 to 255."
                Value = v
            Case 3, 7
                Dim v As Integer
                If Not Integer.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, v) Then Return "Enter a whole number from -2147483648 to 2147483647."
                Value = v
            Case 4
                Dim v As ULong
                If Not ULong.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, v) Then Return "Enter a whole number from 0 to 18446744073709551615."
                Value = v
            Case 9
                Dim v As Single
                If Not Single.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, v) OrElse Single.IsNaN(v) OrElse Single.IsInfinity(v) Then Return "Enter a decimal number."
                Value = v
            Case 12
                Value = text
            Case Else
                Return "This value type is read-only."
        End Select
        Return Nothing
    End Function
End Class

''' <summary>
''' ProfileData: "ssmc", uint32 payload length, then a tree of named nodes, then an opaque tail.
''' Node = uint32 nameLen, name, 5 marker bytes (20 00 00 00 00), uint32 type, value.
''' Group (type 15) value = uint32 child count. The tail after the tree is preserved byte-for-byte.
''' </summary>
Public Class ProfileFile
    Private Shared ReadOnly Magic As Byte() = Encoding.ASCII.GetBytes("ssmc")
    Private Shared ReadOnly Marker As Byte() = {&H20, 0, 0, 0, 0}

    Private _originalLength As Integer
    Private _tail As Byte()

    Public ReadOnly Property SourcePath As String
    Public ReadOnly Property Roots As New List(Of ProfileNode)

    Public Sub New(path As String)
        SourcePath = path
        Dim data = File.ReadAllBytes(path)
        _originalLength = data.Length
        If data.Length < 12 OrElse Not data.Take(4).SequenceEqual(Magic) Then Throw New InvalidDataException("ProfileData: missing 'ssmc' signature.")

        Dim payloadLen = BitConverter.ToInt32(data, 4)
        If payloadLen < 4 OrElse 8 + payloadLen > data.Length Then Throw New InvalidDataException("ProfileData: bad payload length.")
        For i = 8 + payloadLen To data.Length - 1
            If data(i) <> 0 Then Throw New InvalidDataException("ProfileData: unexpected data after the declared payload.")
        Next

        Using ms As New MemoryStream(data, 0, 8 + payloadLen)
            ms.Position = 8
            Using br As New BinaryReader(ms, Encoding.ASCII)
                Dim count = br.ReadInt32()
                For i = 1 To count
                    Roots.Add(ReadNode(br, ""))
                Next
                _tail = br.ReadBytes(CInt(ms.Length - ms.Position))
            End Using
        End Using
    End Sub

    Private Shared Function ReadNode(br As BinaryReader, parentPath As String) As ProfileNode
        Dim nameLen = br.ReadInt32()
        If nameLen < 0 OrElse nameLen > 256 Then Throw New InvalidDataException("ProfileData: corrupt node name.")
        Dim node As New ProfileNode With {.Name = Encoding.ASCII.GetString(br.ReadBytes(nameLen))}
        node.Path = If(parentPath = "", node.Name, parentPath & "/" & node.Name)
        If Not br.ReadBytes(5).SequenceEqual(Marker) Then Throw New InvalidDataException($"ProfileData: unexpected marker at {node.Path}.")
        node.TypeCode = br.ReadUInt32()
        Select Case node.TypeCode
            Case ProfileNode.TypeGroup
                Dim n = br.ReadInt32()
                For i = 1 To n
                    node.Children.Add(ReadNode(br, node.Path))
                Next
            Case 0, 1 : node.Value = br.ReadByte()
            Case 3, 7 : node.Value = br.ReadInt32()
            Case 4 : node.Value = br.ReadUInt64()
            Case 9 : node.Value = br.ReadSingle()
            Case 12
                Dim chars = br.ReadInt32()
                If chars < 0 OrElse chars > 65536 Then Throw New InvalidDataException("ProfileData: corrupt string at " & node.Path)
                node.Value = Encoding.Unicode.GetString(br.ReadBytes(chars * 2))
            Case Else
                Throw New InvalidDataException($"ProfileData: unknown value type {node.TypeCode} at {node.Path}.")
        End Select
        Return node
    End Function

    ''' <summary>All non-group nodes in file order.</summary>
    Public Function Leaves() As List(Of ProfileNode)
        Dim list As New List(Of ProfileNode)
        For Each r In Roots
            Collect(r, list)
        Next
        Return list
    End Function

    Private Shared Sub Collect(n As ProfileNode, list As List(Of ProfileNode))
        If n.IsGroup Then
            For Each c In n.Children
                Collect(c, list)
            Next
        Else
            list.Add(n)
        End If
    End Sub

    Public Function Serialize() As Byte()
        Dim body As Byte()
        Using ms As New MemoryStream()
            Using bw As New BinaryWriter(ms, Encoding.ASCII, True)
                bw.Write(Roots.Count)
                For Each r In Roots
                    WriteNode(bw, r)
                Next
                bw.Write(_tail)
            End Using
            body = ms.ToArray()
        End Using

        Dim total = 8 + body.Length
        If total > _originalLength Then Throw New InvalidOperationException($"ProfileData grew to {total} bytes; the original size is {_originalLength}.")
        Dim result(_originalLength - 1) As Byte
        Array.Copy(Magic, 0, result, 0, 4)
        Array.Copy(BitConverter.GetBytes(body.Length), 0, result, 4, 4)
        Array.Copy(body, 0, result, 8, body.Length)
        Return result
    End Function

    Private Shared Sub WriteNode(bw As BinaryWriter, n As ProfileNode)
        Dim nameBytes = Encoding.ASCII.GetBytes(n.Name)
        bw.Write(nameBytes.Length)
        bw.Write(nameBytes)
        bw.Write(Marker)
        bw.Write(n.TypeCode)
        Select Case n.TypeCode
            Case ProfileNode.TypeGroup
                bw.Write(n.Children.Count)
                For Each c In n.Children
                    WriteNode(bw, c)
                Next
            Case 0, 1 : bw.Write(DirectCast(n.Value, Byte))
            Case 3, 7 : bw.Write(DirectCast(n.Value, Integer))
            Case 4 : bw.Write(DirectCast(n.Value, ULong))
            Case 9 : bw.Write(DirectCast(n.Value, Single))
            Case 12
                Dim s = DirectCast(n.Value, String)
                bw.Write(s.Length)
                bw.Write(Encoding.Unicode.GetBytes(s))
        End Select
    End Sub
End Class
