'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 18
REM ExpectedError: BC30065
REM ErrorMessage: 'Exit Sub' is not valid in a Function or Property.

Module exitstmt
	Public Dim i as integer
	Function fun()
		Exit Function
		i = i + 1
	End function
	Function fun1()
		i = i + 1
		Exit Sub
	End function
	Sub Main()
		fun()	
		fun1()
		if i <> 1 then
			Throw new System.Exception("Exit statement not working properly ")
		End If
 	End Sub
End Module
