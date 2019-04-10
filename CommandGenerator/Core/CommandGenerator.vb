Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Reflection
Imports CommandGenerator.Infrastructure

Namespace Core

    Public Class CommandGenerator

        Private ReadOnly _commandDefintion As CommandDefinition

        Public Sub New(jsonInputFilename As String)
            _commandDefintion = New JsonFileReader().GetJson(Of CommandDefinition)(jsonInputFilename)
        End Sub

        Private Function BuildCompileUnit() As CodeCompileUnit
            Dim unitOfCompilation As New CodeCompileUnit
            Console.WriteLine("Creating class defintion.")
            AddClassDefinition(unitOfCompilation)
            Console.WriteLine("Creating properties.")
            AddProperties(unitOfCompilation.Namespaces(0).Types.Item(0))
            Console.Write("Adding constructor.")
            AddConstructor(unitOfCompilation.Namespaces(0).Types.Item(0))
            Return unitOfCompilation
        End Function

        Private Sub AddClassDefinition(unitOfCompilation As CodeCompileUnit)
            Dim targetClass as CodeTypeDeclaration
            Dim samples As New CodeNamespace(_commandDefintion.namespaceName)
            samples.Imports.Add(New CodeNamespaceImport("Newtonsoft.Json"))

            targetClass = New CodeTypeDeclaration(_commandDefintion.className) With {
                .IsClass = True,
                .TypeAttributes = TypeAttributes.Public
                }

            targetClass.BaseTypes.Add(New CodeTypeReference("Command"))
            samples.Types.Add(targetClass)
            unitOfCompilation.Namespaces.Add(samples)
        End Sub

        Private Sub AddProperties(targetClass as CodeTypeDeclaration)
            AddStandardFields(targetClass)
            AddCustomerFields(targetClass)
        End Sub

        Private Sub AddCustomerFields(targetClass As CodeTypeDeclaration)
            Dim codeSnippet As CodeSnippetTypeMember      
            For Each commandField In _commandDefintion.CommandFields
                If commandField.IsCustomType
                    codeSnippet = New CodeSnippetTypeMember() With {
                        .Text = "Property " + commandField.Name + " As " + commandField.DataType            
                        }
                    targetClass.Members.Add(codeSnippet)
                Else
                    Dim type = GetSupportedType(commandField.DataType)
                    If Not type Is Nothing
                        codeSnippet = New CodeSnippetTypeMember() With {
                            .Text = "Property " + commandField.Name + " As " + If(type.Name.Equals("Int32"), "Integer", type.Name) 
                            }
                        targetClass.Members.Add(codeSnippet)
                    End If
                End If
            Next
        End Sub

        Private Sub AddStandardFields(targetClass As CodeTypeDeclaration)
            ' Declare the read only Width property.
            Dim codeSnippet As CodeSnippetTypeMember
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property CommandName As String"                
                }
            targetClass.Members.Add(codeSnippet)
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property CommandVersion As Integer"                
                }
            targetClass.Members.Add(codeSnippet)
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property Data As String"                
                }
            targetClass.Members.Add(codeSnippet)
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property AggregateType As String"                
                }
            targetClass.Members.Add(codeSnippet)
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property AggregateGuid As System.Guid"                
                }
            targetClass.Members.Add(codeSnippet)
            codeSnippet = New CodeSnippetTypeMember() With {
                .Text = "Public Overrides Property EmployeeId As Integer"                
                }
            targetClass.Members.Add(codeSnippet)
        End Sub

        Private Sub AddConstructor(targetClass as CodeTypeDeclaration)
            ' Declare the constructor
            Dim constructor As New CodeConstructor With {
                    .Attributes = MemberAttributes.Public
                    }

            ' Add parameters.
            constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetType(Integer), "employeeID"))
            constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetType(Guid), "aggregateGuid"))
            For Each field In _commandDefintion.CommandFields
                If Not field.ExcludeFromConstructor
                    If field.IsCustomType
                        constructor.Parameters.Add(New CodeParameterDeclarationExpression(field.DataType, field.Name.ToLower()))
                        Dim customProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), field.Name)
                        constructor.Statements.Add(New CodeAssignStatement( customProperty, New CodeArgumentReferenceExpression(field.Name.ToLower())))
                    Else
                        constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetSupportedType(field.DataType), field.Name.ToLower()))
                        Dim customProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), field.Name)
                        constructor.Statements.Add(New CodeAssignStatement( customProperty, New CodeArgumentReferenceExpression(field.Name.ToLower())))
                    End If                
                End If
            Next


            ' Add field initialization logic
            Dim employeeProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "EmployeeId")
            constructor.Statements.Add(New CodeAssignStatement( employeeProperty, New CodeArgumentReferenceExpression("employeeID")))

            Dim aggregateGuidProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "AggregateGuid")
            constructor.Statements.Add(New CodeAssignStatement(aggregateGuidProperty, New CodeArgumentReferenceExpression("aggregateGuid")))

            Dim aggregateTypeProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression, "AggregateType")
            constructor.Statements.Add(New CodeAssignStatement(aggregateTypeProperty, New CodePrimitiveExpression(_commandDefintion.AggregateType)))

            Dim commandNameProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression, "CommandName")
            constructor.Statements.Add(New CodeAssignStatement(commandNameProperty,NEw CodeFieldReferenceExpression(New CodeMethodInvokeExpression(New CodeThisReferenceExpression(),"GetType"),"Name")))

            Dim commandVersionProperty As New CodePropertyReferenceExpression(NEw CodeThisReferenceExpression, "CommandVersion")
            constructor.Statements.Add(New CodeAssignStatement(commandVersionProperty, NEw CodePrimitiveExpression(1)))
            Dim dataProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression, "Data")
            constructor.Statements.Add(NEw CodeAssignStateMent(dataProperty, New CodeSnippetExpression("JsonConvert.SerializeObject(New With { aggregateGuid, " + _commandDefintion.CommandFields.Aggregate("", Function(prev, nex) prev + ", " +  nex.Name.ToLower()).Remove(0,2) +"})")))

            targetClass.Members.Add(constructor)
        End Sub

        Private Function GetSupportedType(dataType As String) As Type
            Dim supportedSystemTypes As New List(Of String) From { "int", "integer", "string", "boolean", "guid", "date" }
            If supportedSystemTypes.Contains(dataType)
                Select datatype.ToLower()
                    Case "int", "integer"
                        Return GetType(Integer)
                    Case "string"
                        Return GetType(String)
                    Case "boolean"
                        Return GetType(Boolean)
                    Case "guid"
                        Return GetType(Guid)
                    Case "date"
                        Return GetType(Date)
                End Select   
            End If
            Return Nothing
        End Function

        Public Sub GenerateVBCode(fileName As String)
            Dim provider As CodeDomProvider
            provider = CodeDomProvider.CreateProvider("VisualBasic")
            Dim options As New CodeGeneratorOptions()
            Dim sourceWriter As New StreamWriter(fileName)
            Try
                provider.GenerateCodeFromCompileUnit( _
                    BuildCompileUnit(), sourceWriter, options)
            Finally
                sourceWriter.Dispose()
            End Try

        End Sub
    End Class
End NameSpace