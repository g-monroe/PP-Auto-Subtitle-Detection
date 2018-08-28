
Imports System.Text
Imports System.Threading
Imports Auto_Subtitle_Detection.RavSoft.GoogleTranslator

Public Class frmMain
#Region "Functions"
    Dim screenshotList As New List(Of Image)
    Dim totalpixels As Integer = 0
    Dim totalchanged As Integer = 0
    Dim start As Boolean = False
    Dim ender As Boolean = False
    Dim rmin As Integer
    Dim rmax As Integer

    Dim bmin As Integer
    Dim bmax As Integer

    Dim gmin As Integer
    Dim gmax As Integer

    Dim bgr As Integer
    Dim bgg As Integer
    Dim bgb As Integer
    Dim newpic As Image
    Public Function customreplace(bmp As Bitmap) As Bitmap
        totalpixels = 0
        totalchanged = 0
        Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
        Dim bmpData As System.Drawing.Imaging.BitmapData = bmp.LockBits(rect,
        Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
        Dim ptr As IntPtr = bmpData.Scan0
        Dim bytes As Integer = bmpData.Stride * bmp.Height
        Dim rgbValues(bytes - 1) As Byte
        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes)
        Dim secondcounter As Integer = 0

        Dim tempred As Integer
        Dim tempblue As Integer
        Dim tempgreen As Integer
        Dim tempalpha As Integer
        Dim tempx As Integer
        Dim tempy As Integer

        While secondcounter < rgbValues.Length
            tempblue = rgbValues(secondcounter)
            tempgreen = rgbValues(secondcounter + 1)
            tempred = rgbValues(secondcounter + 2)
            tempalpha = rgbValues(secondcounter + 3)
            tempalpha = 255
            tempy = ((secondcounter * 0.25) / bmp.Width)
            tempx = (secondcounter * 0.25) - (tempy * bmp.Width)
            If tempx < 0 Then
                tempx = tempx + 640
            End If

            If Not tempred > 240 Or Not tempblue > 240 Or Not tempgreen > 240 Then
                tempred = 0
                tempgreen = 0
                tempblue = 0
                totalchanged += 1
            End If
            totalpixels += 1
            rgbValues(secondcounter) = tempblue
            rgbValues(secondcounter + 1) = tempgreen
            rgbValues(secondcounter + 2) = tempred
            rgbValues(secondcounter + 3) = tempalpha
            secondcounter = secondcounter + 4
        End While
        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes)
        bmp.UnlockBits(bmpData)
        Return bmp
    End Function

    Public Function ImgToOCR(img As Bitmap)
        Using engine As New Tesseract.TesseractEngine(Application.StartupPath & "\tessdata", "eng", Tesseract.EngineMode.TesseractAndCube)

            Dim enginepage = engine.Process(img)
            Return enginepage.GetText
        End Using
    End Function
    Private Shared Function cropImage(img As Image, cropArea As Rectangle) As Image
        Dim bmpImage As New Bitmap(img)
        Return bmpImage.Clone(cropArea, bmpImage.PixelFormat)
    End Function
    Private Function getBitmap(ByVal pCtrl As Control) As Drawing.Bitmap

        Dim myBmp As New Bitmap(pCtrl.Width, pCtrl.Height)
        Dim g As Graphics = Graphics.FromImage(myBmp)
        Dim pt As Point = pCtrl.Parent.PointToScreen(pCtrl.Location)
        g.CopyFromScreen(pt, Point.Empty, myBmp.Size)
        g.Dispose()
        Return myBmp
    End Function
    Enum SubKind
        Start
        Endd
        Unkown
    End Enum
    Public Function NewSub(total As Integer, num As Integer, lastper As Double, img As Bitmap) As String
        Dim newper As Double = num / total
        If newper <= txtPercentage.Text Then
            If Math.Round(newper, 3) = Math.Round(lastper, 3) Then

                Return "ContSub"
            End If
            If (newper - lastper) <= 0.0012 And (newper - lastper) >= -0.0012 Then

                Return "ContSub"
            End If
            If Math.Round(newper, 3) > Math.Round(lastper, 3) Or Math.Round(newper, 3) < Math.Round(lastper, 3) Then
                Return "NewSub"
            End If
        End If
        Return "Nothing"
    End Function

    Public Function SubtitleTime(time As Double) As String
        Dim times As String = time.ToString("N3")
        Dim ts As TimeSpan = TimeSpan.FromSeconds(times)
        Return ts.ToString("hh\:mm\:ss\,fff")

    End Function
    Public Function translateText(input As String) As String
        ' Initialize the translator
        Dim t As New Translator()
        Dim response As String
        ' Translate the text
        Try
            Me.Cursor = Cursors.WaitCursor
            response = t.Translate(input.Trim(), DirectCast(Me._comboFrom.SelectedItem, String), DirectCast(Me._comboTo.SelectedItem, String))
            response = response.Replace(input, "")
            response = Web.HttpUtility.HtmlDecode(response)
            If t.[Error] Is Nothing Then
                response = Web.HttpUtility.HtmlDecode(response)
                response = response.Replace(input, "")
                Return response
            Else
                Return input
            End If
        Catch ex As Exception
            Return input
        Finally
            'Me._lblStatus.Text = String.Format("Translated in {0} mSec", CInt(t.TranslationTime.TotalMilliseconds))
            Me.Cursor = Cursors.[Default]
        End Try
    End Function
#End Region
#Region "Form Items"
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Using ofd As New OpenFileDialog
            If ofd.ShowDialog() = DialogResult.OK Then
                AxWindowsMediaPlayer1.URL = ofd.FileName
                Timer1.Start()
            End If
        End Using
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        AxWindowsMediaPlayer1.Ctlcontrols.play()
        Timer1.Interval = txtTime.Text
        Timer1.Start()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        AxWindowsMediaPlayer1.Ctlcontrols.pause()
        Timer1.Interval = txtTime.Text
        Timer1.Stop()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        AxWindowsMediaPlayer1.settings.volume = 0
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        AxWindowsMediaPlayer1.settings.volume = 50
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        btnapplybgcolor.PerformClick()
        btnapplysubcolor.PerformClick()
        AxWindowsMediaPlayer1.settings.volume = 0
        AxWindowsMediaPlayer1.uiMode = "none"
        Me._comboFrom.Items.AddRange(Translator.Languages.ToArray())
        Me._comboTo.Items.AddRange(Translator.Languages.ToArray())
        Me._comboFrom.SelectedItem = "Indonesian"
        Me._comboTo.SelectedItem = "English"

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        AxWindowsMediaPlayer1.stretchToFit = True
        AxWindowsMediaPlayer1.uiMode = "none"
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        AxWindowsMediaPlayer1.stretchToFit = False
        AxWindowsMediaPlayer1.uiMode = "full"
    End Sub

    Private Sub txtTime_TextChanged(sender As Object, e As EventArgs) Handles txtTime.TextChanged
        Timer1.Interval = txtTime.Text
    End Sub

#End Region


    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        AxWindowsMediaPlayer1.Ctlcontrols.pause()
        Dim stopw As New Stopwatch()
        stopw.Start()
        Dim tempImg As Bitmap = getBitmap(AxWindowsMediaPlayer1)

        tempImg = cropImage(tempImg, New Rectangle(0, AxWindowsMediaPlayer1.Height - txtPixels.Text, AxWindowsMediaPlayer1.Width, txtPixels.Text))

        Dim temperImg = ColorReplace.CropImages(tempImg, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        totalchanged = ColorReplace.totalchanged
        totalpixels = ColorReplace.totalpixels
        Dim overf As Double = CDbl(txtOverFlow.Text)
        Dim newper As Double = CDbl(totalchanged / totalpixels)
        If overf < newper Then
            tempImg = temperImg
        End If
        'lsbWork.Items.Add(Today.ToString & " :  Finding Subs Bitmap")
        'lsbWork.Refresh()
        PictureBox1.Image = tempImg

        Select Case NewSub(totalpixels, totalchanged, txtCurPercent.Text, tempImg)
            Case "ContSub"
                txtLastPercent.Text = txtCurPercent.Text
                txtCurPercent.Text = totalchanged / totalpixels

            Case "NewSub"
                txtLastPercent.Text = txtCurPercent.Text
                txtCurPercent.Text = totalchanged / totalpixels
                If start = False Then
                    Dim objitem As New ListViewItem(SubtitleTime(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition))
                    Dim text As String = ImgToOCR(tempImg)
                    text = text.Replace(ControlChars.NewLine, "")
                    text = text.Replace(vbNewLine, "")
                    text = text.Replace(ControlChars.CrLf, "")
                    text = text.Replace(vbCrLf, "")
                    Dim response As String = translateText(text)
                    response = Web.HttpUtility.HtmlDecode(response)
                    objitem.SubItems.Add(response)
                    objitem.SubItems.Add(text)
                    screenshotList.Add(tempImg)
                    If Not response = "" Then
                        ListView1.Items.Add(objitem)
                    End If
                End If
                If start = True Then
                    Try
                        ListView1.Items(ListView1.Items.Count - 1).SubItems.Add(SubtitleTime(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition))
                    Catch ex As Exception
                    End Try
                    Dim objitem As New ListViewItem(SubtitleTime(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition))
                    Dim text As String = ImgToOCR(tempImg)
                    text = text.Replace(ControlChars.NewLine, "")
                    text = text.Replace(vbNewLine, "")
                    text = text.Replace(ControlChars.CrLf, "")
                    text = text.Replace(vbCrLf, "")
                    Dim response As String = translateText(text)
                    response = Web.HttpUtility.HtmlDecode(response)
                    objitem.SubItems.Add(response)
                    objitem.SubItems.Add(text)
                    screenshotList.Add(tempImg)
                    If Not response = "" Then
                        ListView1.Items.Add(objitem)
                    End If
                End If
                start = True
            Case "Nothing"
                txtLastPercent.Text = txtCurPercent.Text
                txtCurPercent.Text = totalchanged / totalpixels
                If start = True Then
                    start = False
                    Try
                        ListView1.Items(ListView1.Items.Count - 1).SubItems.Add(SubtitleTime(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition))
                    Catch ex As Exception
                    End Try
                End If
        End Select
        stopw.Stop()
        Dim ts As TimeSpan = stopw.Elapsed
        Dim elapsedTime As String = String.Format("{0:00}.{1:00}", ts.Seconds, ts.Milliseconds / 10)
        lblTime.Text = elapsedTime & "ms (lag)"
        stopw.Reset()
        AxWindowsMediaPlayer1.Ctlcontrols.play()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim arrow As String = " --> "
        Dim output As String = ""
        Dim i As Integer = 0
        For Each listitem As ListViewItem In ListView1.Items
            i += 1
            Dim starttime As String = listitem.Text
            Dim trans As String = listitem.SubItems(1).Text
            Dim endtime As String = "00:00:00,000"
            Try : endtime = listitem.SubItems(3).Text : Catch ex As Exception :: End Try
            output += i & vbNewLine & starttime & arrow & endtime & vbNewLine & trans & vbNewLine
        Next
        Using objwriter As New IO.StreamWriter(Application.StartupPath & "\" & "Subtitle.srt")
            objwriter.Write(output)
        End Using
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        If Not ListView1.SelectedItems.Count < 0 Then

            Try
                ListView1.SelectedItems(0).Remove()
                screenshotList.RemoveAt(ListView1.SelectedItems(0).Index)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        If Not ListView1.SelectedItems.Count < 0 Then
            ListView1.SelectedItems(0).Text = txtstarttime.Text
            ListView1.SelectedItems(0).SubItems(1).Text = rtbSubtitle.Text
            ListView1.SelectedItems(0).SubItems(2).Text = txtendtime.Text
        End If
    End Sub

    Private Sub ListView1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ListView1.SelectedIndexChanged
        If Not ListView1.SelectedItems.Count < 0 Then
            Try
                txtstarttime.Text = ListView1.SelectedItems(0).Text
                rtbSubtitle.Text = ListView1.SelectedItems(0).SubItems(1).Text
                txtendtime.Text = ListView1.SelectedItems(0).SubItems(3).Text
                RichTextBox1.Text = ListView1.SelectedItems(0).SubItems(2).Text
                PictureBox1.Image = screenshotList(ListView1.SelectedItems(0).Index)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        Dim objitem As New ListViewItem(txtstarttime.Text)
        objitem.SubItems.Add(rtbSubtitle.Text)
        objitem.SubItems.Add(txtendtime.Text)
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        AxWindowsMediaPlayer1.Ctlcontrols.pause()
        Dim stopw As New Stopwatch()
        stopw.Start()
        Dim tempImg As Bitmap = getBitmap(AxWindowsMediaPlayer1)

        tempImg = cropImage(tempImg, New Rectangle(0, AxWindowsMediaPlayer1.Height - txtPixels.Text, AxWindowsMediaPlayer1.Width, txtPixels.Text))

        Dim temperImg = ColorReplace.CropImages(tempImg, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        totalchanged = ColorReplace.totalchanged
        totalpixels = ColorReplace.totalpixels
        Dim overf As Double = CDbl(txtOverFlow.Text)
        Dim newper As Double = CDbl(totalchanged / totalpixels)
        If overf < newper Then
            tempImg = temperImg
        End If
        PictureBox1.Image = tempImg
        Dim text As String = ImgToOCR(tempImg)
        RichTextBox1.Text = text
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        RichTextBox1.Text = SubtitleTime(AxWindowsMediaPlayer1.Ctlcontrols.currentPosition)
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        rtbSubtitle.Text = translateText(RichTextBox1.Text)
    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        Dim output As String = ""
        Using ofd As New OpenFileDialog
            Dim result As DialogResult = ofd.ShowDialog()
            If result = DialogResult.OK Then
                Using ojbreader As New IO.StringReader(ofd.FileName)
                    output = ojbreader.ReadToEnd()
                End Using
            End If
        End Using

    End Sub

    Private Sub btnGo1_Click(sender As Object, e As EventArgs) Handles btnGo1.Click
        Dim newtime As String = txtstarttime.Text.Replace(",", ".")
        Dim timesp As TimeSpan = TimeSpan.Parse(newtime)
        AxWindowsMediaPlayer1.Ctlcontrols.currentPosition = timesp.TotalSeconds
        AxWindowsMediaPlayer1.Ctlcontrols.pause()
        Timer1.Interval = txtTime.Text
        Timer1.Stop()
    End Sub

    Private Sub btnGo2_Click(sender As Object, e As EventArgs) Handles btnGo2.Click
        Dim newtime As String = txtendtime.Text.Replace(",", ".")
        Dim timesp As TimeSpan = TimeSpan.Parse(newtime)
        AxWindowsMediaPlayer1.Ctlcontrols.currentPosition = timesp.TotalSeconds
        AxWindowsMediaPlayer1.Ctlcontrols.pause()
        Timer1.Interval = txtTime.Text
        Timer1.Stop()
    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles btnapplysubcolor.Click
        rmin = numRMin.Value
        rmax = numRMax.Value

        bmin = numBMin.Value
        bmax = numBMax.Value

        gmin = numGMin.Value
        gmax = numGMax.Value
    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles btnapplybgcolor.Click
        bgg = numbgg.Value
        bgb = numbgb.Value
        bgr = numbgr.Value
    End Sub
End Class
