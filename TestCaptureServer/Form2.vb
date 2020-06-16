
Imports System.Net
Imports System.Net.Sockets
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Threading

Public Class Form2

    ''So let start with server , you need a socket | tcp listener , like you want to receive your client !
    Public server As TcpListener


    '     GC.Collect()
    '      GC.WaitForPendingFinalizers()
    'the others functions I've not commented and this above are here to avoid "out of memory" by cleaning it when image is received

    Dim ns As NetworkStream
    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        server = New TcpListener(IPAddress.Any, 6000)

        server.Start()

        ''here i launch a task to check if a client get connection
        Task.Run(Sub() GetCLIT())
    End Sub


    Public Sub GetCLIT()

        While True

            Dim op As TcpClient = server.AcceptTcpClient
            ''server  accepts client 
            Task.Run(Sub() RD(op.GetStream()))  '' Read Stream

            ''Here is a task launched to get image and below a task to send left and right click !!

            Task.Run(Sub() SendControl(op.GetStream())) '' Write Stream 

        End While
    End Sub

    Private Declare Function SetThreadPriority Lib "kernel32" _
  (ByVal hThread As Long,
   ByVal nPriority As Long) _
  As Long

    Private Declare Function GetCurrentThread Lib "kernel32" () As Long

    Private Const THREAD_PRIORITY_NORMAL = 0
    Private Const THREAD_PRIORITY_BELOW_NORMAL = -1
    Private Const THREAD_PRIORITY_LOWEST = -2

    Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal process As IntPtr, ByVal minimumWorkingSetSize As Integer, ByVal maximumWorkingSetSize As Integer) As Integer

    Public Function SetPriority(ByVal plngPriority As Long) As Long
        SetPriority = SetThreadPriority(GetCurrentThread(), plngPriority)
    End Function

    Public Sub RD(ByVal lp As NetworkStream)
        SetPriority(THREAD_PRIORITY_LOWEST)
        Dim bf As New BinaryFormatter

        '' you need a binaryformatter in order to make your image in a stream through a connection !
        ''Same in client to send and in server to get with deserialize and serialize !
        While True
            ' ns = mp.GetStream

            PictureBox1.Image = bf.Deserialize(lp)
            ''this will get the image !

            GC.Collect()
            GC.WaitForPendingFinalizers()
            '     SetProcessWorkingSetSize(Process.GetCurrentProcess.Handle, -1, -1)
            ''clear memory 
        End While
    End Sub


    ''the code below provides you a way to send click , it is the same for keyboard and others
    Private CLK As Boolean = False  ''left click

    Private CLK2 As Boolean = False '' right click
    Private Sub PictureBox1_Click(sender As Object, e As MouseEventArgs) Handles PictureBox1.Click

        If e.Button = MouseButtons.Left Then
            If CLK = False Then

                CLK = True

            ElseIf CLK = True Then
                CLK = False
            End If
        End If
        If e.Button = MouseButtons.Right Then
            If CLK2 = False Then

                CLK2 = True

            ElseIf CLK2 = True Then
                CLK2 = False
            End If
        End If
    End Sub


    Public Async Sub SendControl(ByVal l As NetworkStream)

        While True

            If CheckBox1.Checked Then
                If CLK = True Then

                    Dim Data As Byte() = System.Text.Encoding.UTF8.GetBytes("|LEFT|")


                    Await l.WriteAsync(Data, 0, Data.Length)




                End If
            End If

            If CheckBox2.Checked Then
                If CLK2 = True Then

                    Dim Data As Byte() = System.Text.Encoding.UTF8.GetBytes("|RIGHT|")


                    Await l.WriteAsync(Data, 0, Data.Length)


                End If
            End If
        End While

    End Sub

End Class