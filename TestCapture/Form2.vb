Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Threading

Public Class Form2
    Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal process As IntPtr, ByVal minimumWorkingSetSize As Integer, ByVal maximumWorkingSetSize As Integer) As Integer

    '     GC.Collect()
    '      GC.WaitForPendingFinalizers()
    'the others functions I've not commented and this above are here to avoid "out of memory" by cleaning it when image is sent

    Public Cli As New TcpClient

    Public ns As NetworkStream

    <DllImport("user32.dll")>
    Private Shared Function GetCursorPos(<Out> ByRef lpPoint As Point) As Boolean

    End Function
    Public Function Desk() As Image
        Dim primaryMonitorSize As Size = SystemInformation.PrimaryMonitorSize
        Dim iamage As New Bitmap(primaryMonitorSize.Width, primaryMonitorSize.Height)

        Dim graphics As Graphics = Graphics.FromImage(iamage)
        Dim upperLeftSource As New Point(0, 0)
        Dim upperLeftDestination As New Point(0, 0)
        graphics.CopyFromScreen(upperLeftSource, upperLeftDestination, primaryMonitorSize)

        ''this code is always the same to get "screenshot" but it doesn't include cursor so I made a way to get it !

        Dim gaz As New Point

        GetCursorPos(gaz)
        Dim gh As New Rectangle(gaz.X, gaz.Y, Cursor.Current.Clip.Width, Cursor.Current.Clip.Height)
        Cursor.Draw(graphics, gh)

        '' this adds the cursor on your screenshot image

        ''Here 
        Return iamage
    End Function

    Public Sub Send()
        Dim bf As New BinaryFormatter

        ns = Cli.GetStream

        bf.Serialize(ns, Desk)


        GC.Collect()
        GC.WaitForPendingFinalizers
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Cli.Connect("127.0.0.1", 6000)
        Catch ex As Exception

        End Try
    End Sub

    Public h As New Thread(AddressOf Envoi)

    Public Sub Envoi()
        While True

            '  Send()

            Dim bf As New BinaryFormatter

            ns = Cli.GetStream

            bf.Serialize(ns, Desk)


            GC.Collect()
            GC.WaitForPendingFinalizers()


            Thread.Sleep(1)


            GC.Collect()
            GC.WaitForPendingFinalizers()

            '    SetProcessWorkingSetSize(Process.GetCurrentProcess.Handle, -1, -1)

        End While
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        h.Start()  '' Thread to send the images 
        Task.Run(Sub() RDV2(Cli.GetStream))  ''task to capture commands sent (left click and right click)
    End Sub

    Public Sub RDV2(ByVal Herlper As NetworkStream)

        While True

            Dim Buffer(4096) As Byte

            Dim lu As Integer = Herlper.Read(Buffer, 0, Buffer.Length)

            Dim s As String = System.Text.Encoding.UTF8.GetString(Buffer, 0, lu)


            If s = "|LEFT|" Then


                KeyBoardAndMouse.SendLeftClick()


            End If

            If s = "|RIGHT|" Then

                KeyBoardAndMouse.SendRightClick()

            End If




        End While
    End Sub

    ''native class I've made to send left and right click with old api (doesn't like the new)

    Public Class KeyBoardAndMouse
        Private Declare Function keybd_event Lib "user32" Alias "keybd_event" _
           (ByVal bVk As Byte, ByVal bScan As Byte, ByVal dwFlags As Integer,
            ByVal dwExtraInfo As Integer) As Integer

        Private Declare Function mouse_event Lib "user32.dll" Alias "mouse_event" (ByVal dwFlags As Int32, ByVal dX As Int32, ByVal dY As Int32, ByVal cButtons As Int32, ByVal dwExtraInfo As Int32) As Boolean

        Private Const KEYEVENTF_KEYDOWN As Integer = &H0 ' Press key        
        Private Const KEYEVENTF_KEYUP As Integer = &H2 ' Release key        


        ''' <summary>
        ''' Left Click Pressed
        ''' </summary>

        Public Const MOUSEEVENTF_LEFTDOWN = &H2

        ''' <summary>
        ''' Left Click Released
        ''' </summary>
        Public Const MOUSEEVENTF_LEFTUP = &H4

        ''' <summary>
        ''' Right Click Pressed
        ''' </summary>
        Public Const MOUSEEVENTF_RIGHTDOWN = &H8

        ''' <summary>
        ''' Right Click Released
        ''' </summary>
        Public Const MOUSEEVENTF_RIGHTUP = &H10

        ''' <summary>
        ''' This function sends a double left click.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function SendDoubleLeftClick()

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)

        End Function

        Public Shared Function SendLeftClick()

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0)
        End Function
        ''' <summary>
        ''' This function depends on the parameter you set. See example for more information.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function SendCustomClick(ByVal click As Int32)
            mouse_event(click, 0, 0, 0, 0)
        End Function
        ''' <summary>
        '''  This function sends a simple right click.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function SendRightClick()
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0)
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0)
        End Function

        ''' <summary>
        ''' This function sends a key (like Keys.A). Try it in a text editor !
        ''' </summary>
        ''' <param name="k"></param>
        ''' <returns></returns>
        Public Shared Function SendAKey(ByVal k As Keys)

            Return keybd_event(CByte(k), 0, KEYEVENTF_KEYDOWN, 0)
        End Function
    End Class
End Class