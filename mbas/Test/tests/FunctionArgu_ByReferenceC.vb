'=============================================================================================
'Name:Manish Kumar Sinha 
'Email Address: manishkumarsinha@sify.com
'Test Case Name: Argument passing by Reference:
'APR-1.1.0: If variable elements is of value type, i.e. it contains only a value then procedure '		can change the variable or any of its members
'=============================================================================================
 
Imports System
Module APR_1_1_0
	Function F(ByRef p As String) as String
		p = "Sinha"
		return p
   	End Function 
   
   	Sub Main()
      	Dim a As String = "Manish"
      	Dim b As String = ""
      	b=F(a)
		if (b<>a)
		Throw New System.Exception("#A1, Unexcepted behaviour of ByRef of String Datatype")
		end if
   	End Sub 
End Module

'=============================================================================================