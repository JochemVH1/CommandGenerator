Imports CommandLine

Namespace Options
    <Verb("template", HelpText := "Create a new template file")>
    Public Class TemplateOptions
        <Value(0, Default:=1, Required := False, HelpText:= "Number of custom properties")>
        Public Property NumberOfCustomPropertys As Integer?

        <[Option]("n"c, "name", Required:= True, HelpText:= "template name")>
        Public Property Name As String
    End Class
End NameSpace