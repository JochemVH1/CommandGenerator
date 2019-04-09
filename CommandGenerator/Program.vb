
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Newtonsoft.Json

Module Program

    Private _programState As ProgramState

    Sub Main(args As String())
        If CheckArgs(args)
            SetProgramState(args)
            Select _programState
                Case ProgramState.GenerateTemplate
                    Console.WriteLine("Creating template file...")
                    Console.WriteLine(args(1))
                    Dim classname = "<classname>"
                    Dim filename = "command"
                    Dim numberOfCommandFields = 1
                    If args.Length = 2 Or args.Length = 3
                        Dim result As Integer
                        If Integer.TryParse(args(1), result)
                            numberOfCommandFields = result
                        End If
                    End If
                    If args.Length = 3
                        classname = args(2)
                        filename = args(2)
                    End If
                    Dim commandDefintion As New CommandDefinition With {
                        .ClassName = classname + "Command",
                        .NameSpaceName = "<namespacename>",
                        .AggregateType = "<aggregatetype>",
                        .CommandFields = New List(Of CommandField)
                    }
                    For i = 0 To numberOfCommandFields - 1
                        commandDefintion.CommandFields.Add(New CommandField() With {
                                .DataType = "<datatype>",
                                .Name = "<name>"
                            }
                        )
                    Next
                    Dim jsonString = JsonConvert.SerializeObject(commandDefintion, Formatting.Indented)
                    Dim buffer() = Encoding.ASCII.GetBytes(jsonString)
                    Using inputStream = File.Create(Environment.CurrentDirectory + "/"+filename+"Command.json")  
                        inputStream.Write(buffer, 0, buffer.Length)
                    End Using
                    Console.Write("Created" + filename + "Command.json in current directory.")
                Case ProgramState.GenerateSourceFile
                    If CheckForValidJsonFile(args(0))
                        Dim commandDefinition = CreateCommandDefinition(args(0))
                        GenerateVBCode(New CommandGenerator(commandDefinition).BuildCompileUnit(), commandDefinition.ClassName + ".vb")
                    End If
            End Select
        End If
    End Sub

    Private Function CheckForValidJsonFile(filename As String) As Boolean
        Const pattern = "([a-zA-Z0-9]*\.json){1}"
        Dim matchResult = Regex.Match(filename, pattern)
        If matchResult.Success
            If File.Exists(Environment.CurrentDirectory + "/" + filename)
                Console.WriteLine("Succesfully found json file")
                Return True
            Else 
                Console.Write("Json file does not exist in current directory. ")
            End If
        Else
            Console.Write("Invalid file extension, please add a json file. or use -t to generate template file")           
        End If 
        Return False
    End Function

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

    SUb SetProgramState(args() As String)
        Select args(0)
            Case "-t"
                _programState = ProgramState.GenerateTemplate
            Case Else
                _programState = ProgramState.GenerateSourceFile  
        End Select     
    End SUb

    Private Function CheckArgs(args() As String) As Boolean
        If args.Length > 0
            Return True
        Else 
            Console.WriteLine("")
            Console.WriteLine("To use the commandgenerator please execute following commands:")
            Console.WriteLine("")
            Console.WriteLine("commandgenerator -t")
            Console.WriteLine("")
            Console.WriteLine("For a new template command file.")
            Console.WriteLine("")
            Console.WriteLine("commandgenereator command.json")
            Console.WriteLine("")
            Console.WriteLine("To generate command.vb")
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
