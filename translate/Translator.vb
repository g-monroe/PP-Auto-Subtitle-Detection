
' Copyright (c) 2015 Ravi Bhavnani
' License: Code Project Open License
' http://www.codeproject.com/info/cpol10.aspx

Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Web

Namespace RavSoft.GoogleTranslator
    ''' <summary>
    ''' Translates text using Google's online language tools.
    ''' </summary>
    Public Class Translator
#Region "Properties"

        ''' <summary>
        ''' Gets the supported languages.
        ''' </summary>
        Public Shared ReadOnly Property Languages() As IEnumerable(Of String)
            Get
                Translator.EnsureInitialized()
                Return Translator._languageModeMap.Keys.OrderBy(Function(p) p)
            End Get
        End Property

        ''' <summary>
        ''' Gets the time taken to perform the translation.
        ''' </summary>
        Public Property TranslationTime() As TimeSpan
            Get
                Return m_TranslationTime
            End Get
            Private Set
                m_TranslationTime = Value
            End Set
        End Property
        Private m_TranslationTime As TimeSpan

        ''' <summary>
        ''' Gets the url used to speak the translation.
        ''' </summary>
        ''' <value>The url used to speak the translation.</value>
        Public Property TranslationSpeechUrl() As String
            Get
                Return m_TranslationSpeechUrl
            End Get
            Private Set
                m_TranslationSpeechUrl = Value
            End Set
        End Property
        Private m_TranslationSpeechUrl As String

        ''' <summary>
        ''' Gets the error.
        ''' </summary>
        Public Property [Error]() As Exception
            Get
                Return m_Error
            End Get
            Private Set
                m_Error = Value
            End Set
        End Property
        Private m_Error As Exception

#End Region

#Region "Public methods"

        ''' <summary>
        ''' Translates the specified source text.
        ''' </summary>
        ''' <param name="sourceText">The source text.</param>
        ''' <param name="sourceLanguage">The source language.</param>
        ''' <param name="targetLanguage">The target language.</param>
        ''' <returns>The translation.</returns>
        Public Function Translate(sourceText As String, sourceLanguage As String, targetLanguage As String) As String
            ' Initialize
            Me.[Error] = Nothing
            Me.TranslationSpeechUrl = Nothing
            Me.TranslationTime = TimeSpan.Zero
            Dim tmStart As DateTime = DateTime.Now
            Dim translation As String = String.Empty

            Try
                ' Download translation
                Dim url As String = String.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}", Translator.LanguageEnumToIdentifier(sourceLanguage), Translator.LanguageEnumToIdentifier(targetLanguage), HttpUtility.UrlEncode(sourceText))
                Dim outputFile As String = Path.GetTempFileName()
                Using wc As New WebClient()
                    wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36")
                    wc.DownloadFile(url, outputFile)
                End Using

                ' Get translated text
                If File.Exists(outputFile) Then

                    ' Get phrase collection
                    Dim text As String = File.ReadAllText(outputFile)
                    text = text.Replace(Chr(34), "'")
                    text = text.Replace("[", "")
                    Dim lines As String() = text.Split(New String() {"],"}, StringSplitOptions.None)
                    For Each line As String In lines
                        If line.Contains(",") Then
                            line = line.Split(New String() {"','"}, StringSplitOptions.None).First
                            translation += line.Replace("'", "") & " "
                        End If
                    Next
                    translation = translation.Replace(",id]", "")
                    'Dim index As Integer = text.IndexOf(String.Format(",,""{0}""", Translator.LanguageEnumToIdentifier(sourceLanguage)))
                    'If index = -1 Then
                    '    ' Translation of single word
                    '    Dim startQuote As Integer = text.IndexOf(""""c)
                    '    If startQuote <> -1 Then
                    '        Dim endQuote As Integer = text.IndexOf(""""c, startQuote + 1)
                    '        If endQuote <> -1 Then
                    '            translation = text.Substring(startQuote + 1, endQuote - startQuote - 1)
                    '        End If
                    '    End If
                    'Else
                    '    ' Translation of phrase
                    '    text = text.Substring(0, index)
                    '    text = text.Replace("],[", ",")
                    '    text = text.Replace("]", String.Empty)
                    '    text = text.Replace("[", String.Empty)
                    '    text = text.Replace(""",""", """")

                    '    ' Get translated phrases
                    '    Dim phrases As String() = text.Split(New String() {""""c}, StringSplitOptions.RemoveEmptyEntries)
                    '    Dim i As Integer = 0
                    '    While (i < phrases.Count())
                    '        Dim translatedPhrase As String = phrases(i)
                    '        If translatedPhrase.StartsWith(",,") Then
                    '            i -= 1
                    '            Continue While
                    '        End If
                    '        translation += translatedPhrase & Convert.ToString("  ")
                    '        i += 2
                    '    End While
                    'End If

                    ' Fix up translation
                    translation = translation.Trim()
                    translation = translation.Replace(" ?", "?")
                    translation = translation.Replace(" !", "!")
                    translation = translation.Replace(" ,", ",")
                    translation = translation.Replace(" .", ".")
                    translation = translation.Replace(" ;", ";")
                    translation = translation.Replace("\n", "")
                    'Dim lines As String() = translation.Split(vbNewLine)
                    'translation = ""
                    'For Each line As String In lines
                    '    If line.Trim <> "" Then
                    '        translation += line
                    '    End If
                    'Next
                    'If translation.Contains(sourceText) Then
                    '    translation = translation.Replace(sourceText, "")
                    'Else
                    '    If translation.Contains(". ") Then
                    '        translation = translation.Split(New String() {". "}, StringSplitOptions.None).First & "."
                    '    ElseIf translation.Contains("! ") Then
                    '        translation = translation.Split(New String() {"! "}, StringSplitOptions.None).First & "!"
                    '    ElseIf translation.Contains("? ") Then
                    '        translation = translation.Split(New String() {"? "}, StringSplitOptions.None).First & "?"
                    '    ElseIf translation.Contains(", ") Then
                    '        translation = translation.Split(New String() {", "}, StringSplitOptions.None).First & ","
                    '    ElseIf translation.Contains("; ") Then
                    '        translation = translation.Split(New String() {"; "}, StringSplitOptions.None).First & ";"
                    '    End If
                    'End If
                    ' And translation speech URL
                    Me.TranslationSpeechUrl = String.Format("https://translate.googleapis.com/translate_tts?ie=UTF-8&q={0}&tl={1}&total=1&idx=0&textlen={2}&client=gtx", HttpUtility.UrlEncode(translation), Translator.LanguageEnumToIdentifier(targetLanguage), translation.Length)
                End If
            Catch ex As Exception
                Me.[Error] = ex
            End Try

            ' Return result
            Me.TranslationTime = DateTime.Now - tmStart
            Return translation
        End Function

#End Region

#Region "Private methods"

        ''' <summary>
        ''' Converts a language to its identifier.
        ''' </summary>
        ''' <param name="language">The language."</param>
        ''' <returns>The identifier or <see cref="String.Empty"/> if none.</returns>
        Private Shared Function LanguageEnumToIdentifier(language As String) As String
            Dim mode As String = String.Empty
            Translator.EnsureInitialized()
            Translator._languageModeMap.TryGetValue(language, mode)
            Return mode
        End Function

        ''' <summary>
        ''' Ensures the translator has been initialized.
        ''' </summary>
        Private Shared Sub EnsureInitialized()
            If Translator._languageModeMap Is Nothing Then
                Translator._languageModeMap = New Dictionary(Of String, String)()
                Translator._languageModeMap.Add("Afrikaans", "af")
                Translator._languageModeMap.Add("Albanian", "sq")
                Translator._languageModeMap.Add("Arabic", "ar")
                Translator._languageModeMap.Add("Armenian", "hy")
                Translator._languageModeMap.Add("Azerbaijani", "az")
                Translator._languageModeMap.Add("Basque", "eu")
                Translator._languageModeMap.Add("Belarusian", "be")
                Translator._languageModeMap.Add("Bengali", "bn")
                Translator._languageModeMap.Add("Bulgarian", "bg")
                Translator._languageModeMap.Add("Catalan", "ca")
                Translator._languageModeMap.Add("Chinese", "zh-CN")
                Translator._languageModeMap.Add("Croatian", "hr")
                Translator._languageModeMap.Add("Czech", "cs")
                Translator._languageModeMap.Add("Danish", "da")
                Translator._languageModeMap.Add("Dutch", "nl")
                Translator._languageModeMap.Add("English", "en")
                Translator._languageModeMap.Add("Esperanto", "eo")
                Translator._languageModeMap.Add("Estonian", "et")
                Translator._languageModeMap.Add("Filipino", "tl")
                Translator._languageModeMap.Add("Finnish", "fi")
                Translator._languageModeMap.Add("French", "fr")
                Translator._languageModeMap.Add("Galician", "gl")
                Translator._languageModeMap.Add("German", "de")
                Translator._languageModeMap.Add("Georgian", "ka")
                Translator._languageModeMap.Add("Greek", "el")
                Translator._languageModeMap.Add("Haitian Creole", "ht")
                Translator._languageModeMap.Add("Hebrew", "iw")
                Translator._languageModeMap.Add("Hindi", "hi")
                Translator._languageModeMap.Add("Hungarian", "hu")
                Translator._languageModeMap.Add("Icelandic", "is")
                Translator._languageModeMap.Add("Indonesian", "id")
                Translator._languageModeMap.Add("Irish", "ga")
                Translator._languageModeMap.Add("Italian", "it")
                Translator._languageModeMap.Add("Japanese", "ja")
                Translator._languageModeMap.Add("Korean", "ko")
                Translator._languageModeMap.Add("Lao", "lo")
                Translator._languageModeMap.Add("Latin", "la")
                Translator._languageModeMap.Add("Latvian", "lv")
                Translator._languageModeMap.Add("Lithuanian", "lt")
                Translator._languageModeMap.Add("Macedonian", "mk")
                Translator._languageModeMap.Add("Malay", "ms")
                Translator._languageModeMap.Add("Maltese", "mt")
                Translator._languageModeMap.Add("Norwegian", "no")
                Translator._languageModeMap.Add("Persian", "fa")
                Translator._languageModeMap.Add("Polish", "pl")
                Translator._languageModeMap.Add("Portuguese", "pt")
                Translator._languageModeMap.Add("Romanian", "ro")
                Translator._languageModeMap.Add("Russian", "ru")
                Translator._languageModeMap.Add("Serbian", "sr")
                Translator._languageModeMap.Add("Slovak", "sk")
                Translator._languageModeMap.Add("Slovenian", "sl")
                Translator._languageModeMap.Add("Spanish", "es")
                Translator._languageModeMap.Add("Swahili", "sw")
                Translator._languageModeMap.Add("Swedish", "sv")
                Translator._languageModeMap.Add("Tamil", "ta")
                Translator._languageModeMap.Add("Telugu", "te")
                Translator._languageModeMap.Add("Thai", "th")
                Translator._languageModeMap.Add("Turkish", "tr")
                Translator._languageModeMap.Add("Ukrainian", "uk")
                Translator._languageModeMap.Add("Urdu", "ur")
                Translator._languageModeMap.Add("Vietnamese", "vi")
                Translator._languageModeMap.Add("Welsh", "cy")
                Translator._languageModeMap.Add("Yiddish", "yi")
            End If
        End Sub

#End Region

#Region "Fields"

        ''' <summary>
        ''' The language to translation mode map.
        ''' </summary>
        Private Shared _languageModeMap As Dictionary(Of String, String)

#End Region
    End Class
End Namespace

