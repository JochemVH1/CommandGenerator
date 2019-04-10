Imports CommandLine

Namespace Options
    <Verb("generate", HelpText := "generate a vb file.")>
    Public Class GenerateOptions
        <Value(0, Required := True, HelpText :="json file from which to convert")>
        Public Property File As String
    End Class
End NameSpace