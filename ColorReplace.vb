Public Class ColorReplace
    Public Shared pic1 As Image
    Public Shared pic2 As Image
    Public Shared pic3 As Image
    Public Shared pic4 As Image
    Public Shared pic5 As Image
    Public Shared pic6 As Image
    Public Shared totalpixels As Integer = 0
    Public Shared totalchanged As Integer = 0
    Public Shared Function CropImages(bmp As Image, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer) As Image
        For i As Integer = 0 To 5
            Dim piecewidth As Integer = bmp.Width / 6
            Dim x As Integer = piecewidth * i
            Dim CropRect As New Rectangle(x, 0, piecewidth, bmp.Height)
            Dim CropImg = New Bitmap(CropRect.Width, CropRect.Height)
            Using grp = Graphics.FromImage(CropImg)
                grp.DrawImage(bmp, New Rectangle(0, 0, CropRect.Width, CropRect.Height), CropRect, GraphicsUnit.Pixel)
                If i = 0 Then
                    pic1 = CropImg
                ElseIf i = 1 Then
                    pic2 = CropImg
                ElseIf i = 2 Then
                    pic3 = CropImg
                ElseIf i = 3 Then
                    pic4 = CropImg
                ElseIf i = 4 Then
                    pic5 = CropImg
                ElseIf i = 5 Then
                    pic6 = CropImg
                End If
            End Using
        Next
        totalchanged = 0
        totalpixels = 0
        replacecolors(rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        Return CombineImages(bmp)
    End Function
    Public Shared Sub replacecolors(rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
        Using cr As New customReplace
            Dim msd As InvokeCRT1 = AddressOf cr.Thread
            msd.Invoke(pic1, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
        Using cr2 As New customReplace
            Dim msd2 As InvokeCRT2 = AddressOf cr2.Thread
            msd2.Invoke(pic2, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
        Using cr3 As New customReplace
            Dim msd3 As InvokeCRT3 = AddressOf cr3.Thread
            msd3.Invoke(pic3, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
        Using cr4 As New customReplace
            Dim msd4 As InvokeCRT4 = AddressOf cr4.Thread
            msd4.Invoke(pic4, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
        Using cr5 As New customReplace
            Dim msd5 As InvokeCRT5 = AddressOf cr5.Thread
            msd5.Invoke(pic5, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
        Using cr6 As New customReplace
            Dim msd6 As InvokeCRT6 = AddressOf cr6.Thread
            msd6.Invoke(pic6, rmin, rmax, bmin, bmax, gmin, gmax, bgr, bgg, bgb)
        End Using
    End Sub
    Delegate Sub InvokeCRT1(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Delegate Sub InvokeCRT2(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Delegate Sub InvokeCRT3(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Delegate Sub InvokeCRT4(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Delegate Sub InvokeCRT5(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Delegate Sub InvokeCRT6(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)
    Public Shared Function CombineImages(bmp As Image) As Image
        Dim pic As New Bitmap(bmp.Width, bmp.Height)

        For i As Integer = 0 To 5
            Dim piecewidth As Integer = bmp.Width / 6
            Dim x As Integer = piecewidth * i
            Dim imgh As Integer = bmp.Height
            Using g = Graphics.FromImage(pic)

                If i = 0 Then
                    g.DrawImage(pic1, x, 0)
                ElseIf i = 1 Then
                    g.DrawImage(pic2, x, 0)
                ElseIf i = 2 Then
                    g.DrawImage(pic3, x, 0)
                ElseIf i = 3 Then
                    g.DrawImage(pic4, x, 0)
                ElseIf i = 4 Then
                    g.DrawImage(pic5, x, 0)
                ElseIf i = 5 Then
                    g.DrawImage(pic6, x, 0)
                End If
            End Using

        Next
        Return pic
    End Function
    Class customReplace
        Implements IDisposable

        Sub Thread(ByVal bmp As Bitmap, rmin As Integer, rmax As Integer, bmin As Integer, bmax As Integer, gmin As Integer, gmax As Integer, bgr As Integer, bgg As Integer, bgb As Integer)

            Dim rect As New Rectangle(0, 0, bmp.Width, bmp.Height)
            Dim bmpData As System.Drawing.Imaging.BitmapData = bmp.LockBits(rect, Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat)
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

                If Not (tempred >= rmin And tempred <= rmax) Or Not (tempblue >= bmin And tempblue <= bmax) Or Not (tempgreen >= gmin And tempgreen <= gmax) Then
                    tempred = bgr
                    tempgreen = bgg
                    tempblue = bgb
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

        End Sub

#Region "IDisposable Support"
        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
#End Region
    End Class
End Class
