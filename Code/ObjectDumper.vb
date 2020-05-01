Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.RegularExpressions

Public Class ObjectDumper

    Private Log As TextWriter
    Private Pos As Integer
    Private Level As Integer
    Private Depth As Integer
    Private Methods As ArrayList

    Public Sub New()
        Methods = New ArrayList
    End Sub

    Public Sub Dump(ByVal Element As Object, Optional ByVal Depth As Integer = 0)
        Dump(Element, Depth, Console.Out)
    End Sub

    Public Sub Dump(ByVal Element As Object, ByVal Depth As Integer, ByVal Log1 As TextWriter)
        Me.Depth = Depth
        Methods = New ArrayList
        Log = Log1 'перепрописали консолью или параметром
        Writeobject(Nothing, Element)
    End Sub

    Public Function Dump(ByVal Depth As Integer, ByVal Element As Object) As String
        Me.Depth = Depth
        Methods = New ArrayList
        Dim Log1 = New IO.StreamWriter(New IO.MemoryStream())
        Log = Log1
        Writeobject(Nothing, Element)
        Log1.Flush()
        Log1.BaseStream.Position = 0
        Dim Str1 As String
        Using Sr = New IO.StreamReader(Log1.BaseStream, Encoding.UTF8)
            Str1 = Sr.ReadToEnd()
        End Using
        Return Str1
    End Function

    Private Sub GetOrderedMethod()
        If Methods.Count > 0 Then
            Methods.Sort()
            Writeline()
            WriteStr("  Method:")
            Writeline()
            For i As Integer = 0 To Methods.Count - 1
                WriteStr(Methods(i).ToString & "()")
                Writeline()
            Next
        End If
    End Sub

    Private Sub WriteMethod(ByVal S As String)
        If Not Methods.Contains(S) Then
            Methods.Add(S)
        End If
    End Sub

    Private Sub WriteStr(ByVal S As String)
        If S IsNot Nothing Then
            Log.Write(S)
            Pos += S.Length
        End If
    End Sub

    Private Sub Writeindent()
        For I As Integer = 0 To Level - 1
            Log.Write("  ")
        Next
    End Sub

    Private Sub Writeline()
        Log.WriteLine()
        Pos = 0
    End Sub

    Private Sub Writetab()
        WriteStr("  ")

        While Pos Mod 8 <> 0
            WriteStr(" ")
        End While
    End Sub

    Private Sub WriteValue(ByVal O As Object)
        If O Is Nothing Then
            WriteStr("Null")
        ElseIf TypeOf O Is DateTime Then
            WriteStr((CDate(O)).ToShortDateString())
        ElseIf TypeOf O Is ValueType OrElse TypeOf O Is String Then
            WriteStr(O.ToString())
        ElseIf TypeOf O Is IEnumerable Then
            WriteStr("...")
        Else
            WriteStr("{ }")
        End If
    End Sub

    Private Sub WriteHex(ByVal buffer As Byte())
        Dim Len As Integer = buffer.Length
        If Len > 0 Then
            Dim Str1 As New StringBuilder(Len * 2)
            For i = 0 To Len - 1
                Str1.AppendFormat(String.Format("{0:x2}", buffer(i)))
            Next
            WriteStr(String.Format("({0:D4})", Len) & Str1.ToString)
            WriteStr("   ")
            WriteStr(Regex.Replace(Encoding.ASCII.GetString(buffer, 0, Len), "[^\x20-\x7F]", ".").Replace("?", "."))
        End If
    End Sub

    Private Sub Writeobject(ByVal Prefix As String, ByVal Element As Object)
        If Element Is Nothing OrElse TypeOf Element Is ValueType OrElse TypeOf Element Is String Then
            Writeindent()
            WriteStr(Prefix)
            WriteValue(Element)
            Writeline()
        Else
            Dim Enumerableelement As IEnumerable = TryCast(Element, IEnumerable)

            If Enumerableelement IsNot Nothing Then

                For Each Item As Object In Enumerableelement

                    If TypeOf Item Is IEnumerable AndAlso Not (TypeOf Item Is String) Then
                        Writeindent()
                        WriteStr(Prefix)
                        WriteStr("...")
                        Writeline()

                        If Level < Depth Then
                            Level += 1
                            Writeobject(Prefix, Item)
                            Level -= 1
                        End If
                    Else
                        Writeobject(Prefix, Item)
                    End If
                Next
            Else
                Dim Members As MemberInfo() = Element.[GetType]().GetMembers(BindingFlags.[Public] Or BindingFlags.Instance)
                Writeindent()
                WriteStr(Prefix)
                Dim Propwritten As Boolean = False

                For Each M As MemberInfo In Members
                    Dim F As FieldInfo = TryCast(M, FieldInfo)
                    Dim P As PropertyInfo = TryCast(M, PropertyInfo)

                    If F IsNot Nothing OrElse P IsNot Nothing Then

                        If Propwritten Then
                            Writetab()
                        Else
                            Propwritten = True
                        End If

                        WriteStr(vbCrLf & M.Name)
                        WriteStr("=")
                        Dim T As Type = If(F IsNot Nothing, F.FieldType, P.PropertyType)

                        If T.IsValueType OrElse T = GetType(String) Then
                            If F IsNot Nothing Then
                                Try
                                    WriteValue(F.GetValue(Element))
                                Catch Ex As Exception
                                    WriteValue("{Error:" & Ex.Message & "}")
                                End Try
                            Else
                                Try
                                    WriteValue(P.GetValue(Element, Nothing))
                                Catch Ex As Exception
                                    WriteValue("{Error:" & Ex.Message & "}")
                                End Try

                            End If
                        Else

                            If GetType(IEnumerable).IsAssignableFrom(T) Then
                                WriteStr("...")
                            Else
                                WriteStr("{ }")
                            End If
                        End If
                    End If
                Next

                If Propwritten Then Writeline()

                If Level < Depth Then

                    For Each M As MemberInfo In Members

                        Dim F As FieldInfo = TryCast(M, FieldInfo)
                        Dim P As PropertyInfo = TryCast(M, PropertyInfo)

                        If F IsNot Nothing OrElse P IsNot Nothing Then
                            Dim T As Type = If(F IsNot Nothing, F.FieldType, P.PropertyType)

                            If Not (T.IsValueType OrElse T = GetType(String)) Then

                                Dim Value As Object
                                If F IsNot Nothing Then
                                    Try
                                        Value = F.GetValue(Element)
                                    Catch Ex As Exception
                                        WriteValue("{Error:" & Ex.Message & "}")
                                    End Try

                                Else
                                    Try
                                        Value = P.GetValue(Element, Nothing)
                                    Catch Ex As Exception
                                        WriteValue("{Error:" & Ex.Message & "}")
                                    End Try

                                End If

                                If Value IsNot Nothing Then
                                    Level += 1
                                    Writeobject(M.Name & ": ", Value)
                                    Level -= 1
                                End If
                            End If
                        Else
                            WriteMethod(M.Name)
                        End If
                    Next
                    GetOrderedMethod()
                    Methods.Clear()
                End If
            End If
        End If
    End Sub


End Class
