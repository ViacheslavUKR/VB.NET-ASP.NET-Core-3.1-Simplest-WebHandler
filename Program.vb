Imports System
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Logging

Module Program
    Public Sub Main(args As String())
        CreateHostBuilder(args).Build().Run()
    End Sub

    Public Function CreateHostBuilder(ByVal args As String()) As IHostBuilder
        Return Host.
            CreateDefaultBuilder().
            ConfigureLogging(Sub(hostingContext, logging)
                                 logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"))
                                 logging.AddConsole()
                                 logging.AddDebug()
                             End Sub).
            ConfigureWebHostDefaults(Function(webBuilder)
                                         Return webBuilder.UseStartup(Of Startup)()
                                     End Function)
    End Function

End Module
