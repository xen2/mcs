// cs3021.cs: `I' does not need a CLSCompliant attribute because the assembly is not marked as CLS-compliant
// Line: 9
// Compiler options: -warn:2 -warnaserror

using System;

[CLSCompliant (false)]
public interface I
{
}
