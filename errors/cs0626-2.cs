// cs0626-2.cs: Method, operator, or accessor `ExternClass.ExternMethod.set' is marked external and has no attributes on it. Consider adding a DllImport attribute to specify the external implementation
// Line: 6
// Compiler options: -warnaserror -warn:1

class ExternClass {
        public static extern int ExternMethod { set; }
}
