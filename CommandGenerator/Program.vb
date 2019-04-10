Imports System.IO
Imports System.Text.RegularExpressions
Imports CommandGenerator.Options
Imports CommandLine

Module Program

    Sub Main(args As String())
        If CheckArgs(args)
            Dim res As Boolean = Parser.Default.ParseArguments(Of GenerateOptions, TemplateOptions)(args) _ 
                .MapResult(Function(opts As GenerateOptions) Runner(opts, AddressOf GenerateVbClass),
                           Function(opts As TemplateOptions) Runner(opts, AddressOf CreateTemplateFile),
                           Function(errs As IEnumerable(Of [Error])) False)
        End If
    End Sub

    Function Runner(Of T, TResult)(options As T,func As Func(Of T, TResult)) As TResult
        Return func(options)
    End Function

    Private Function CreateTemplateFile(templateOptions As TemplateOptions) As Boolean
        Console.ForegroundColor = ConsoleColor.DarkCyan
        Console.WriteLine("Creating template file...")
        Dim generator As New Core.TemplateGenerator(templateOptions.Name, templateOptions.NumberOfCustomPropertys)
        generator.CreateTemplate()
        Console.Write("Created " + templateOptions.Name + "Command.json in current directory.")
        Console.ResetColor()
        Return True
    End Function

    Private Function GenerateVbClass(generateOptions As GenerateOptions) As Boolean
        If CheckForValidJsonFile(generateOptions.File)
            Console.ForegroundColor = ConsoleColor.DarkCyan
            Dim generator As New Core.CommandGenerator(generateOptions.File)
            generator.GenerateVBCode(generateOptions.File.Split("."c)(0) + ".vb")
            Console.ResetColor()
            Return True
        End If
        Return False
    End Function

    Private Function CheckForValidJsonFile(filename As String) As Boolean
        Const pattern = "([a-zA-Z0-9]*\.json){1}"
        Dim matchResult = Regex.Match(filename, pattern)
        If matchResult.Success
            If File.Exists(Environment.CurrentDirectory + "/" + filename)
                Console.ForegroundColor = ConsoleColor.DarkCyan
                Console.WriteLine("Succesfully found json file") 
                Console.ResetColor()
                Return True
            Else 
                Console.ForegroundColor = ConsoleColor.Red
                Console.Write("Json file does not exist in current directory. ")
            End If
        Else
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("Invalid file extension, please add a json file. or use -t to generate template file")           
        End If 
        Console.ResetColor()
        Return False
    End Function

    Private Function CheckArgs(args() As String) As Boolean
        If args.Length > 0
            Return True
        Else 
            Console.ForegroundColor = ConsoleColor.DarkCyan          
            Console.WriteLine("")
            Console.WriteLine("  ********************************************************************")
            Console.WriteLine("  * Welcome to command generator                                     *")
            Console.WriteLine("  ********************************************************************")
            Console.WriteLine("")
            Console.WriteLine("  General help: ")
            Console.WriteLine("")
            Console.ForegroundColor = ConsoleColor.Green   
            Console.WriteLine("  commandgenerator template -n command")
            Console.ForegroundColor = ConsoleColor.DarkCyan  
            Console.WriteLine("")
            Console.WriteLine("  Will create a new template json file, after ")
            Console.WriteLine("  editing the properties run:")
            Console.WriteLine("")
            Console.ForegroundColor = ConsoleColor.Green  
            Console.WriteLine("  commandgenerator generate command.json")
            Console.ForegroundColor = ConsoleColor.DarkCyan  
            Console.WriteLine("")
            Console.WriteLine("  To generate command.vb")
            Console.ResetColor()
            Return False
        End If 
    End Function
End Module
