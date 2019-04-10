Imports CommandGenerator.Infrastructure

Namespace Core
    Public Class TemplateGenerator
        Private ReadOnly _fileWriter As JsonFileWriter
        Private ReadOnly _className As String
        Private ReadOnly _numberOfCustomProperties As Integer?

        Public Sub New(className As String, numberOfCustomProperties As Integer?)
            _fileWriter = New JsonFileWriter
            _className = className
            _numberOfCustomProperties = numberOfCustomProperties
        End Sub

        Public Function CreateTemplate() As Boolean
            Dim classname = _className
            Dim filename = _className
            Dim commandDefintion As New CommandDefinition With {
                    .ClassName = classname + "Command",
                    .NameSpaceName = "<namespacename>",
                    .AggregateType = "<aggregatetype>",
                    .CommandFields = New List(Of CommandField)
                    }
            Dim numberOfCommandFields = If(_numberOfCustomProperties.HasValue, _numberOfCustomProperties, Nothing)
            If Not numberOfCommandFields Is Nothing
                For i = 0 To numberOfCommandFields - 1
                    commandDefintion.CommandFields.Add(New CommandField() With {
                                                          .DataType = "<datatype>",
                                                          .Name = "<name>"
                                                          }
                                                       )
                Next
            End If
            _fileWriter.WriteJson(commandDefintion, filename)
            Return True
        End Function

    End Class
End NameSpace