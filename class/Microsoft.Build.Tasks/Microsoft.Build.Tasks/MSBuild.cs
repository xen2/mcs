//
// MSBuild.cs: Task that can run .*proj files
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.Build.Tasks {
	public class MSBuild : TaskExtension {
	
		ITaskItem[]	projects;
		string[]		properties;
		bool		rebaseOutputs;
		bool		runEachTargetSeparately;
		bool		stopOnFirstFailure;
		ITaskItem[]	targetOutputs;
		string[]	targets;
	
		public MSBuild ()
		{
		}

		public override bool Execute ()
		{
			string filename;
			bool result = true;
			stopOnFirstFailure = false;
		
			foreach (ITaskItem project in projects) {
				filename = project.GetMetadata ("FullPath");
				
				result = BuildEngine.BuildProjectFile (filename, targets, new Hashtable (), new Hashtable ());
				if (result == false) {
					Log.LogError ("Error while building {0}", filename);
					if (stopOnFirstFailure)
						break;
				}
			}
			return result;
		}

		[Required]
		public ITaskItem[] Projects {
			get {
				return projects;
			}
			set {
				projects = value;
			}
		}

		public string[] Properties {
			get {
				return properties;
			}
			set {
				properties = value;
			}
		}

		public bool RebaseOutputs {
			get {
				return rebaseOutputs;
			}
			set {
				rebaseOutputs = value;
			}
		}

		public bool RunEachTargetSeparately {
			get {
				return runEachTargetSeparately;
			}
			set {
				runEachTargetSeparately = value;
			}
		}

		public bool StopOnFirstFailure {
			get {
				return stopOnFirstFailure;
			}
			set {
				stopOnFirstFailure = value;
			}
		}

		[Output]
		public ITaskItem[] TargetOutputs {
			get {
				return targetOutputs;
			}
		}

		public string[] Targets {
			get {
				return targets;
			}
			set {
				targets = value;
			}
		}
	}
}

#endif