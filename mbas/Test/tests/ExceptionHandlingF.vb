'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module Test
Public i as integer
    Sub Main()
		Try
		    Exit try
		Finally
		    i = 10
		End Try
		if i<>10 then
			Throw new System.Exception("Finally block not working... Expected 10 but got "&i)
		End if
    End Sub
End Module
