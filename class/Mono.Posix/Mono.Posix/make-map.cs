//
// MakeMap.cs: Builds a C map of constants defined on C# land
//
// Author: Miguel de Icaza (miguel@novell.com)
//
// (C) 2003 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Reflection;
using Mono.Posix;
	
class MakeMap {

	static int Main (string [] args)
	{
		if (args.Length != 2){
			Console.WriteLine ("Usage is: make-map assembly output");
			return 1;
		}
		
		StreamWriter sh = new StreamWriter (File.Create (args [1] + ".h"));
		StreamWriter sc = new StreamWriter (File.Create (args [1] + ".c"));
		sh.WriteLine ("/* This file was automatically generated by make-map from {0} */\n", args [0]);
		sc.WriteLine ("/* This file was automatically generated by make-map from {0} */\n", args [0]);
		
		Assembly a = Assembly.LoadFrom (args [0]);
		object [] x = a.GetCustomAttributes (false);
		Console.WriteLine ("Got: " + x.Length);
		foreach (object aattr in a.GetCustomAttributes (false)){
			Console.WriteLine ("Got: " + aattr.GetType ().Name);
			if (aattr.GetType ().Name == "IncludeAttribute"){
				WriteDefines (sc, aattr);
				WriteIncludes (sc, aattr);
			}
		}
		string f = args [1];
		
		if (f.IndexOf ("/") != -1)
			f = f.Substring (f.IndexOf ("/") + 1);
		sc.WriteLine ("#include \"{0}.h\"", f);
		
		Type [] exported_types = a.GetTypes ();
			
		foreach (Type t in exported_types){
			object [] attributes = t.GetCustomAttributes (false);
			bool do_map = false;
			bool do_bits = false;
			
			foreach (object attr in attributes){
				if (attr.GetType ().Name == "MapAttribute")
					do_map = true;
				if (attr.GetType ().Name == "FlagsAttribute")
					do_bits = true;
				
			}
			if (!do_map)
				continue;
			
			string n = t.FullName.Replace (".", "_");
			object inst = Activator.CreateInstance (t);
			foreach (FieldInfo fi in t.GetFields ()){
				if (!fi.IsLiteral)
					continue;
				sh.WriteLine ("#define {0}_{1} 0x{2:x}", n, fi.Name, fi.GetValue (inst));
			}
			sh.WriteLine ();
			
			sc.WriteLine ("int map_{0} (int x)", n);
			sc.WriteLine ("{");
			if (do_bits)
				sc.WriteLine ("\tint r = 0;");
			foreach (FieldInfo fi in t.GetFields ()){
				if (!fi.IsLiteral)
					continue;
				if (do_bits)
					sc.WriteLine ("\tif ((x & {0}_{1}) != 0)\n\t\tr |= {1};", n, fi.Name);
				else
					sc.WriteLine ("\tif (x == {0}_{1})\n\t\t return {1};", n, fi.Name);
			}
			if (do_bits)
				sc.WriteLine ("\treturn r;");
			else
				sc.WriteLine ("\treturn -1;");
			sc.WriteLine ("}\n");
		}
		sh.Close ();
		sc.Close ();

		return 0;
	}

	static void WriteDefines (TextWriter writer, object o)
	{
		PropertyInfo prop = o.GetType ().GetProperty ("Defines");
		if (prop == null)
			throw new Exception ("Cannot find 'Defines' property");

		MethodInfo method = prop.GetGetMethod ();
		string [] defines = (string []) method.Invoke (o, null);
		foreach (string def in defines)
			writer.WriteLine ("#define {0}", def);
	}

	static void WriteIncludes (TextWriter writer, object o)
	{
		PropertyInfo prop = o.GetType ().GetProperty ("Includes");
		if (prop == null)
			throw new Exception ("Cannot find 'Includes' property");

		MethodInfo method = prop.GetGetMethod ();
		string [] includes = (string []) method.Invoke (o, null);
		foreach (string inc in includes)
			writer.WriteLine ("#include <{0}>", inc);
	}
}
