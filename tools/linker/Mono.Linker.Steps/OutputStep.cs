//
// OutputStep.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// (C) 2006 Jb Evain
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

using System.IO;

using Mono.Cecil;

namespace Mono.Linker.Steps {

	public class OutputStep : BaseStep {

		protected override void Process ()
		{
			CheckOutputDirectory ();
		}

		void CheckOutputDirectory ()
		{
			if (Directory.Exists (Context.OutputDirectory))
				return;

			Directory.CreateDirectory (Context.OutputDirectory);
		}

		protected override void ProcessAssembly (AssemblyDefinition assembly)
		{
			OutputAssembly (assembly);
		}

		void OutputAssembly (AssemblyDefinition assembly)
		{
			string directory = Context.OutputDirectory;

			CopyConfigFileIfNeeded (assembly, directory);

			switch (Annotations.GetAction (assembly)) {
			case AssemblyAction.Link:
				AssemblyFactory.SaveAssembly (assembly, GetAssemblyFileName (assembly, directory));
				break;
			case AssemblyAction.Copy:
				CopyAssembly (GetOriginalAssemblyFileInfo (assembly), directory);
				break;
			}
		}

		static void CopyConfigFileIfNeeded (AssemblyDefinition assembly, string directory)
		{
			string config = GetConfigFile (GetOriginalAssemblyFileInfo (assembly).FullName);
			if (!File.Exists (config))
				return;

			File.Copy (config, GetConfigFile (GetAssemblyFileName (assembly, directory)), true);
		}

		static string GetConfigFile (string assembly)
		{
			return assembly + ".config";
		}

		static FileInfo GetOriginalAssemblyFileInfo (AssemblyDefinition assembly)
		{
			return assembly.MainModule.Image.FileInformation;
		}

		static void CopyAssembly (FileInfo fi, string directory)
		{
			File.Copy (fi.FullName, Path.Combine (directory, fi.Name), true);
		}

		static string GetAssemblyFileName (AssemblyDefinition assembly, string directory)
		{
			string file = assembly.Name.Name + (assembly.Kind == AssemblyKind.Dll ? ".dll" : ".exe");
			return Path.Combine (directory, file);
		}
	}
}
