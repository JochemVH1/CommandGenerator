
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Newtonsoft.Json

Module Program

    Sub Main(args As String())
        If CheckArguments(args)
            Dim commandDefinition = CreateCommandDefinition(args(0))
            Dim unitOfCompilation = commandDefinition.BuildCompileUnit()
            GenerateVBCode(unitOfCompilation, commandDefinition.ClassName + ".vb")
        End If
        Thread.Sleep(2000)
        Exit Sub
    End Sub


    Private Function CreateCommandDefinition(filename As String) As CommandDefinition
        Return JsonConvert.DeserializeObject(Of CommandDefinition)(GetJson(filename))
    End Function

    Private Function GetJson(filename As String) As String
        Dim json = ""
        Using stream  = File.Open(Environment.CurrentDirectory + "/" + filename, FileMode.Open)
            Using streamReader = NEw StreamReader(stream, Encoding.UTF8)
                While Not streamReader.EndOfStream
                    json += streamReader.ReadLine()
                End While
            End Using
        End Using
        Return json
    End Function

    Function CheckArguments(args() As String) As Boolean
        If args.Length > 0
            Const pattern = "([a-zA-Z0-9]*\.json){1}"
            Dim filename = args(0)
            Dim matchResult = Regex.Match(filename,pattern)
            If matchResult.Success
                If File.Exists(Environment.CurrentDirectory + "/" + filename)
                    Console.WriteLine("Succesfully found json file")
                    Return True
                Else 
                    Console.Write("Json file does not exist in current directory. ")
                    Return False
                End If
            Else
                Console.Write("Invalid file extension, please add a json file.")
                Return False
            End If          
        Else 
            Console.Write("Please add a json file as argument.")
            Return False
        End If  
    End Function

    Private Sub GenerateVBCode(compileUnit As CodeCompileUnit, fileName As String)
        Dim provider As CodeDomProvider
        provider = CodeDomProvider.CreateProvider("VisualBasic")
        Dim options As New CodeGeneratorOptions()
        Dim sourceWriter As New StreamWriter(fileName)
        Try
            provider.GenerateCodeFromCompileUnit( _
                compileUnit, sourceWriter, options)
        Finally
            sourceWriter.Dispose()
        End Try

    End Sub
End Module
