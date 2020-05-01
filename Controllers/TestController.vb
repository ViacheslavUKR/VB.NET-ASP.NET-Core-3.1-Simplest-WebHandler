Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Authorization
Imports Newtonsoft.Json

<Route("[controller]")>
<ApiController>
Public Class TestController
    Inherits ControllerBase
    Public Function Index() As ActionResult
        Dim D As New ObjectDumper
        Dim Str1 As String = D.Dump(3, Request)
        Return Ok(Str1)
    End Function
End Class
