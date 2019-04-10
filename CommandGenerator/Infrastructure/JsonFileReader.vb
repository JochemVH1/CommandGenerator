Imports System.IO
Imports System.Text
Imports Newtonsoft.Json

Namespace Infrastructure
    Public Class JsonFileReader
        Public Function GetJson(Of T)(filename As String) As T
            Dim json = ""
            Using stream  = File.Open(Environment.CurrentDirectory + "/" + filename, FileMode.Open)
                Using streamReader = NEw StreamReader(stream, Encoding.UTF8)
                    While Not streamReader.EndOfStream
                        json += streamReader.ReadLine()
                    End While
                End Using
            End Using
            Return JsonConvert.DeserializeObject(Of T)(json)
        End Function
    End Class

    Public Class JsonFileWriter
        Public SUb WriteJson(Of T)(obj As T, filename As String)
            Dim jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented)
            Dim buffer() = Encoding.ASCII.GetBytes(jsonString)
            Using inputStream = File.Create(Environment.CurrentDirectory + "/"+filename+"Command.json")  
                inputStream.Write(buffer, 0, buffer.Length)
            End Using
        End SUb
    End Class
End NameSpace