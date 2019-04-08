Imports System.CodeDom
Imports System.Reflection

Public Class CommandDefinition
    Property ClassName As String
    Property NameSpaceName As String
    Property AggregateType As String
    Property CommandFields As IEnumerable(Of CommandField)

    Public Function BuildCompileUnit() As CodeCompileUnit
        Dim unitOfCompilation As New CodeCompileUnit
        Console.WriteLine("Creating class defintion.")
        AddClassDefinition(unitOfCompilation)
        Console.WriteLine("Creating properties.")
        AddProperties(unitOfCompilation.Namespaces(0).Types.Item(0))
        Console.WriteLine("Adding constructor.")
        AddConstructor(unitOfCompilation.Namespaces(0).Types.Item(0))
        Return unitOfCompilation
    End Function

    Private Sub AddClassDefinition(unitOfCompilation As CodeCompileUnit)
        Dim targetClass as CodeTypeDeclaration
        Dim samples As New CodeNamespace(namespaceName)
        targetClass = New CodeTypeDeclaration(className) With {
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
        
        For Each commandField In CommandFields
            If commandField.IsCustomType
                Dim customerProperty As New CodeMemberProperty With {
                    .Attributes = MemberAttributes.Public,
                    .Name = commandField.Name,
                    .Type = New CodeTypeReference(commandField.DataType)
                }
                targetClass.Members.Add(customerProperty)
            Else 
                Dim codeTypeReference As New CodeTypeReference(GetSupportedType(commandField.DataType))
                If Not codeTypeReference Is Nothing
                    Dim customerProperty As New CodeMemberProperty With {
                            .Attributes = MemberAttributes.Public,
                            .Name = commandField.Name,
                            .Type = codeTypeReference
                            }
                    targetClass.Members.Add(customerProperty)
                End If
            End If
        Next
    End Sub

    Private Sub AddStandardFields(targetClass As CodeTypeDeclaration)
        ' Declare the read only Width property.
        Dim commandName As New CodeMemberProperty With {
            .Attributes = MemberAttributes.Public Or MemberAttributes.Override,
            .Name = "CommandName",
            .Type = New CodeTypeReference(GetType(String))
        }
        targetClass.Members.Add(commandName)
        ' Declare the read only Width property.
        Dim commandVersion As New CodeMemberProperty With {
                .Attributes = MemberAttributes.Public Or MemberAttributes.Override,
                .Name = "CommandVersion",
                .Type = New CodeTypeReference(GetType(Integer))
                }
        targetClass.Members.Add(commandVersion)
        ' Declare the read only Width property.
        Dim data As New CodeMemberProperty With {
                .Attributes =  MemberAttributes.Public Or MemberAttributes.Override,
                .Name = "Data",
                .Type = New CodeTypeReference(GetType(String))
                }
        targetClass.Members.Add(data)
        ' Declare the read only Width property.
        Dim aggregateType As New CodeMemberProperty With {
                .Attributes = _
                MemberAttributes.Public Or MemberAttributes.Override,
                .Name = "AggregateType",
                .Type = New CodeTypeReference(GetType(String))
                }
        targetClass.Members.Add(aggregateType)
        ' Declare the read only Width property.
        Dim aggregateGuid As New CodeMemberProperty With {
                .Attributes = _
                MemberAttributes.Public Or MemberAttributes.Override,
                .Name = "AggregateGuid",
                .Type = New CodeTypeReference(GetType(Guid))
                }
        targetClass.Members.Add(aggregateGuid)
        ' Declare the read only Width property.
        Dim employeeId As New CodeMemberProperty With {
                .Attributes = _
                MemberAttributes.Public Or MemberAttributes.Override,
                .Name = "EmployeeId",
                .Type = New CodeTypeReference(GetType(Integer))
                }
        targetClass.Members.Add(employeeId)
    End Sub

    Private Sub AddConstructor(targetClass as CodeTypeDeclaration)
        ' Declare the constructor
        Dim constructor As New CodeConstructor With {
            .Attributes = MemberAttributes.Public
        }

        ' Add parameters.
        constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetType(Integer), "employeeID"))
        constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetType(Guid), "aggregateGuid"))
        Console.WriteLine("Adding Custom constructor statements.")
        For Each field In CommandFields
            If field.IncludeInConstructor
                If field.IsCustomType
                    constructor.Parameters.Add(New CodeParameterDeclarationExpression(field.DataType, field.Name))
                    Dim customProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), field.Name)
                    constructor.Statements.Add(New CodeAssignStatement( customProperty, New CodeArgumentReferenceExpression(field.Name)))
                Else
                    constructor.Parameters.Add(New CodeParameterDeclarationExpression(GetSupportedType(field.DataType), field.Name))
                    Dim customProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), field.Name)
                    constructor.Statements.Add(New CodeAssignStatement( customProperty, New CodeArgumentReferenceExpression(field.Name)))
                End If                
            End If
        Next


        ' Add field initialization logic
        Dim employeeProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "EmployeeId")
        constructor.Statements.Add(New CodeAssignStatement( employeeProperty, New CodeArgumentReferenceExpression("employeeID")))
        Dim aggregateGuidProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression(), "AggregateGuid")
        constructor.Statements.Add(New CodeAssignStatement(aggregateGuidProperty, New CodeArgumentReferenceExpression("aggregateGuid")))

        Dim aggregateTypeProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression, "aggregateType")
        constructor.Statements.Add(New CodeAssignStatement(aggregateTypeProperty, New CodePrimitiveExpression(AggregateType)))

        Dim commandNameProperty As New CodePropertyReferenceExpression(New CodeThisReferenceExpression, "CommandName")
        constructor.Statements.Add(New CodeAssignStatement(commandNameProperty,NEw CodeFieldReferenceExpression(New CodeMethodInvokeExpression(New CodeThisReferenceExpression(),"GetType"),"Name")))
        Dim commandVersionProperty As New CodePropertyReferenceExpression(NEw CodeThisReferenceExpression, "CommandVersion")
        constructor.Statements.Add(New CodeAssignStatement(commandVersionProperty, NEw CodePrimitiveExpression(1)))


        targetClass.Members.Add(constructor)
    End Sub

    Private Function GetSupportedType(dataType As String) As Type
        Dim supportedSystemTypes As New List(Of String) From { "int", "integer", "string", "boolean", "guid", "date" }
        If supportedSystemTypes.Contains(dataType)
            Select datatype.ToLower()
                Case "int" Or "integer"
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
        Throw New InvalidOperationException("UnsupportedData type. ")
    End Function
End Class
