'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionStringtoSingleA
	Sub Main()
			Dim a as Single
			Dim b as String= "123.5"
			a = b
			if a <> 123.5
				Throw new System.Exception("Conversion of String to Single not working. Expected 123.5 but got " &a) 
			End if		
	End Sub
End Module
