//
// ConsoleLogger.cs: Outputs to the console
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
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class ConsoleLogger : ILogger {
	
		string		parameters;
		int		indent;
		LoggerVerbosity	verbosity;
		WriteHandler	writeHandler;
		int		errorCount;
		int		warningCount;
		DateTime		buildStart;
		bool		performanceSummary;
		bool		showSummary;
		bool		skipProjectStartedText;
		List<string> errors, warnings;
		bool		projectFailed;
		ConsoleColor errorColor, warningColor, eventColor, messageColor, highMessageColor;
		ColorSetter colorSet;
		ColorResetter colorReset;
		bool no_message_color, use_colors;
		
		public ConsoleLogger ()
			: this (LoggerVerbosity.Normal, null, null, null)
		{
		}

		public ConsoleLogger (LoggerVerbosity verbosity)
			: this (LoggerVerbosity.Normal, null, null, null)
		{
		}
		
		public ConsoleLogger (LoggerVerbosity verbosity,
				      WriteHandler write,
				      ColorSetter colorSet,
				      ColorResetter colorReset)
		{
			this.verbosity = verbosity;
			this.indent = 0;
			this.errorCount = 0;
			this.warningCount = 0;
			if (write == null)
				this.writeHandler += new WriteHandler (WriteHandlerFunction);
			else
				this.writeHandler += write;
			this.performanceSummary = false;
			this.showSummary = true;
			this.skipProjectStartedText = false;
			errors = new List<string> ();
			warnings = new List<string> ();
			this.colorSet = colorSet;
			this.colorReset = colorReset;

			//defaults
			errorColor = ConsoleColor.DarkRed;
			warningColor = ConsoleColor.DarkYellow;
			eventColor = ConsoleColor.DarkCyan;
			messageColor = ConsoleColor.DarkGray;
			highMessageColor = ConsoleColor.White;

			// if message color is not set via the env var,
			// then don't use any color for it.
			no_message_color = true;

			use_colors = false;
			if (colorSet == null || colorReset == null)
				return;

			// color support
			string config = Environment.GetEnvironmentVariable ("XBUILD_COLORS");
			if (config == null) {
				use_colors = true;
				return;
			}

			if (config == "disable")
				return;

			use_colors = true;
			string [] pairs = config.Split (new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
			foreach (string pair in pairs) {
				string [] parts = pair.Split (new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 2)
					continue;

				if (parts [0] == "errors")
					TryParseConsoleColor (parts [1], ref errorColor);
				else if (parts [0] == "warnings")
					TryParseConsoleColor (parts [1], ref warningColor);
				else if (parts [0] == "events")
					TryParseConsoleColor (parts [1], ref eventColor);
				else if (parts [0] == "messages") {
					if (TryParseConsoleColor (parts [1], ref messageColor)) {
						highMessageColor = GetBrightColorFor (messageColor);
						no_message_color = false;
					}
				}
			}
		}

		bool TryParseConsoleColor (string color_str, ref ConsoleColor color)
		{
			switch (color_str.ToLower ()) {
			case "black": color = ConsoleColor.Black; break;

			case "blue": color = ConsoleColor.DarkBlue; break;
			case "green": color = ConsoleColor.DarkGreen; break;
			case "cyan": color = ConsoleColor.DarkCyan; break;
			case "red": color = ConsoleColor.DarkRed; break;
			case "magenta": color = ConsoleColor.DarkMagenta; break;
			case "yellow": color = ConsoleColor.DarkYellow; break;
			case "grey": color = ConsoleColor.DarkGray; break;

			case "brightgrey": color = ConsoleColor.Gray; break;
			case "brightblue": color = ConsoleColor.Blue; break;
			case "brightgreen": color = ConsoleColor.Green; break;
			case "brightcyan": color = ConsoleColor.Cyan; break;
			case "brightred": color = ConsoleColor.Red; break;
			case "brightmagenta": color = ConsoleColor.Magenta; break;
			case "brightyellow": color = ConsoleColor.Yellow; break;

			case "white":
			case "brightwhite": color = ConsoleColor.White; break;
			default: return false;
			}

			return true;
		}

		ConsoleColor GetBrightColorFor (ConsoleColor color)
		{
			switch (color) {
			case ConsoleColor.DarkBlue: return ConsoleColor.Blue;
			case ConsoleColor.DarkGreen: return ConsoleColor.Green;
			case ConsoleColor.DarkCyan: return ConsoleColor.Cyan;
			case ConsoleColor.DarkRed: return ConsoleColor.Red;
			case ConsoleColor.DarkMagenta: return ConsoleColor.Magenta;
			case ConsoleColor.DarkYellow: return ConsoleColor.Yellow;
			case ConsoleColor.DarkGray: return ConsoleColor.Gray;
			case ConsoleColor.Gray: return ConsoleColor.White;

			default: return color;
			}
		}
		
		public void ApplyParameter (string parameterName,
					    string parameterValue)
		{
			// FIXME: what we should do here? in msbuild it isn't
			// changing "parameters" property
		}

		public virtual void Initialize (IEventSource eventSource)
		{
                        eventSource.BuildStarted +=  new BuildStartedEventHandler (BuildStartedHandler);
                        eventSource.BuildFinished += new BuildFinishedEventHandler (BuildFinishedHandler);
                        eventSource.ProjectStarted += new ProjectStartedEventHandler (ProjectStartedHandler);
                        eventSource.ProjectFinished += new ProjectFinishedEventHandler (ProjectFinishedHandler);
                        eventSource.TargetStarted += new TargetStartedEventHandler (TargetStartedHandler);
                        eventSource.TargetFinished += new TargetFinishedEventHandler (TargetFinishedHandler);
                        eventSource.TaskStarted += new TaskStartedEventHandler (TaskStartedHandler);
                        eventSource.TaskFinished += new TaskFinishedEventHandler (TaskFinishedHandler);
                        eventSource.MessageRaised += new BuildMessageEventHandler (MessageHandler);
                        eventSource.WarningRaised += new BuildWarningEventHandler (WarningHandler);
                        eventSource.ErrorRaised += new BuildErrorEventHandler (ErrorHandler);
		}
		
		public void BuildStartedHandler (object sender, BuildStartedEventArgs args)
		{
			WriteLine (String.Empty);
			WriteLine (String.Format ("Build started {0}.", args.Timestamp));
			WriteLine ("__________________________________________________");
			buildStart = args.Timestamp;
		}
		
		public void BuildFinishedHandler (object sender, BuildFinishedEventArgs args)
		{
			if (args.Succeeded == true && !projectFailed) {
				WriteLine ("Build succeeded.");
			} else {
				WriteLine ("Build FAILED.");
			}
			if (performanceSummary == true) {
			}

			if (warnings.Count > 0) {
				WriteLine (Environment.NewLine + "Warnings:");
				SetColor (warningColor);
				foreach (string warning in warnings)
					WriteLine (warning);
				ResetColor ();
				WriteLine ("");
			}

			if (errors.Count > 0) {
				WriteLine ("Errors:");
				SetColor (errorColor);
				foreach (string error in errors)
					WriteLine (error);
				ResetColor ();
			}

			if (showSummary == true){
				TimeSpan timeElapsed = args.Timestamp - buildStart;
				WriteLine (String.Format ("\t {0} Warning(s)", warningCount));
				WriteLine (String.Format ("\t {0} Error(s)", errorCount));
				WriteLine (String.Empty);
				WriteLine (String.Format ("Time Elapsed {0}", timeElapsed));
			} 
		}

		public void ProjectStartedHandler (object sender, ProjectStartedEventArgs args)
		{
			SetColor (eventColor);
			WriteLine (String.Format ("Project \"{0}\" ({1} target(s)):", args.ProjectFile, args.TargetNames));
			ResetColor ();
			WriteLine (String.Empty);
		}
		
		public void ProjectFinishedHandler (object sender, ProjectFinishedEventArgs args)
		{
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
				if (indent == 1)
					indent --;
				SetColor (eventColor);
				WriteLine (String.Format ("Done building project \"{0}\".{1}", args.ProjectFile,
							args.Succeeded ? String.Empty : "-- FAILED"));
				ResetColor ();
				WriteLine (String.Empty);
			}
			if (!projectFailed)
				// no project has failed yet, so update the flag
				projectFailed = !args.Succeeded;
		}
		
		public void TargetStartedHandler (object sender, TargetStartedEventArgs args)
		{
			indent++;
			SetColor (eventColor);
			WriteLine (String.Format ("Target {0}:",args.TargetName));
			ResetColor ();
		}
		
		public void TargetFinishedHandler (object sender, TargetFinishedEventArgs args)
		{
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed) || !args.Succeeded) {
				SetColor (eventColor);
				WriteLine (String.Format ("Done building target \"{0}\" in project \"{1}\".{2}",
					args.TargetName, args.ProjectFile,
					args.Succeeded ? String.Empty : "-- FAILED"));
				ResetColor ();
			}
			indent--;

			WriteLine (String.Empty);
		}
		
		public void TaskStartedHandler (object sender, TaskStartedEventArgs args)
		{
			if (this.verbosity == LoggerVerbosity.Detailed) {
				SetColor (eventColor);
				WriteLine (String.Format ("Task \"{0}\"",args.TaskName));
				ResetColor ();
			}
			indent++;
		}
		
		public void TaskFinishedHandler (object sender, TaskFinishedEventArgs args)
		{
			indent--;
			if (this.verbosity == LoggerVerbosity.Detailed || !args.Succeeded) {
				SetColor (eventColor);
				if (args.Succeeded)
					WriteLine (String.Format ("Done executing task \"{0}\"", args.TaskName));
				else
					WriteLine (String.Format ("Task \"{0}\" execution -- FAILED", args.TaskName));
				ResetColor ();
			}
		}
		
		public void MessageHandler (object sender, BuildMessageEventArgs args)
		{
			if (IsMessageOk (args)) {
				if (no_message_color) {
					WriteLine (args.Message);
				} else {
					SetColor (args.Importance == MessageImportance.High ? highMessageColor : messageColor);
					WriteLine (args.Message);
					ResetColor ();
				}
			}
		}
		
		public void WarningHandler (object sender, BuildWarningEventArgs args)
		{
			string msg = FormatWarningEvent (args);
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
				SetColor (warningColor);
				WriteLineWithoutIndent (msg);
				ResetColor ();
			}
			warnings.Add (msg);
			warningCount++;
		}
		
		public void ErrorHandler (object sender, BuildErrorEventArgs args)
		{
			string msg = FormatErrorEvent (args);
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Minimal)) {
				SetColor (errorColor);
				WriteLineWithoutIndent (msg);
				ResetColor ();
			}
			errors.Add (msg);
			errorCount++;
		}
		
		[MonoTODO]
		public void CustomEventHandler (object sender, CustomBuildEventArgs args)
		{
		}
		
		private void WriteLine (string message)
		{
			if (indent > 0) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < indent; i++)
					sb.Append ('\t');
				sb.Append (message);

				writeHandler (sb.ToString ());
			} else {
				writeHandler (message);
			}
		}
		
		private void WriteLineWithoutIndent (string message)
		{
			writeHandler (message);
		}
		
		private void WriteHandlerFunction (string message)
		{
			Console.WriteLine (message);
		}

		void SetColor (ConsoleColor color)
		{
			if (use_colors)
				colorSet (color);
		}

		void ResetColor ()
		{
			if (use_colors)
				colorReset ();
		}
		
		private void ParseParameters ()
		{
			string[] splittedParameters = parameters.Split (';');
			foreach (string s in splittedParameters ) {
				switch (s) {
				case "PerformanceSummary":
					this.performanceSummary = true;
					break;
				case "NoSummary":
					this.showSummary = false;
					break;
				default:
					throw new ArgumentException ("Invalid parameter.");
				}
			}
		}
		
		public virtual void Shutdown ()
		{
		}

		static bool InEmacs = Environment.GetEnvironmentVariable ("EMACS") == "t";
		
		private string FormatErrorEvent (BuildErrorEventArgs args)
		{
			// For some reason we get an 1-char empty string as Subcategory somtimes.
			string subprefix = args.Subcategory == null || args.Subcategory == "" || args.Subcategory == " " ? "" : " ";
			string subcat = subprefix == "" ? "" : args.Subcategory;
				
			if (args.LineNumber != 0){
				if (args.ColumnNumber != 0 && !InEmacs) 
					return String.Format ("{0}({1},{2}): {3}{4}error {5}: {6}",
							      args.File, args.LineNumber, args.ColumnNumber,
							      subprefix, subcat, args.Code, args.Message);

				return String.Format ("{0}({1}): {2}{3}error {4}: {5}",
						      args.File, args.LineNumber,
						      subprefix, subcat, args.Code, args.Message);
			} else {
				return String.Format ("{0}: {1}{2}error {3}: {4}", args.File, subprefix, subcat, args.Code,
					args.Message);
			}
		}

		private string FormatWarningEvent (BuildWarningEventArgs args)
		{
			// For some reason we get an 1-char empty string as Subcategory somtimes.
			string subprefix = args.Subcategory == null || args.Subcategory == "" || args.Subcategory == " " ? "" : " ";
			string subcat = subprefix == "" ? "" : args.Subcategory;

			// FIXME: show more complicated args
			if (args.LineNumber != 0){

				if (args.ColumnNumber != 0 && !InEmacs) {
					return String.Format ("{0}({1},{2}): {3}{4}warning {5}: {6}",
							      args.File, args.LineNumber, args.ColumnNumber,
							      subprefix, subcat, args.Code, args.Message);
				}
				return String.Format ("{0}({1}): {2}{3}warning {4}: {5}",
						      args.File, args.LineNumber,
						      subprefix, subcat, args.Code, args.Message);
			} else {
				return String.Format ("{0}: {1} warning {2}: {3}", args.File, args.Subcategory, args.Code,
					args.Message);
			}
		}
		
		private bool IsMessageOk (BuildMessageEventArgs bsea)
		{
			if (bsea.Importance == MessageImportance.High && IsVerbosityGreaterOrEqual (LoggerVerbosity.Minimal)) {
				return true;
			} else if (bsea.Importance == MessageImportance.Normal && IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
				return true;
			} else if (bsea.Importance == MessageImportance.Low && IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed)) {
				return true;
			} else
				return false;
		}
		
                private bool IsVerbosityGreaterOrEqual (LoggerVerbosity v)
                {
                		if (v == LoggerVerbosity.Diagnostic) {
                			return LoggerVerbosity.Diagnostic <= verbosity;
                		} else if (v == LoggerVerbosity.Detailed) {
                			return LoggerVerbosity.Detailed <= verbosity;
                		} else if (v == LoggerVerbosity.Normal) {
                			return LoggerVerbosity.Normal <= verbosity;
                		} else if (v == LoggerVerbosity.Minimal) {
                			return LoggerVerbosity.Minimal <= verbosity;
                		} else if (v == LoggerVerbosity.Quiet) {
                			return true;
                		} else
                			return false;
                }

		public string Parameters {
			get {
				return parameters;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ();
				parameters = value;
				if (parameters != String.Empty)
					ParseParameters ();
			}
		}
		
		public bool ShowSummary {
			get { return showSummary; }
			set { showSummary = value; }
		}
		
		public bool SkipProjectStartedText {
			get { return skipProjectStartedText; }
			set { skipProjectStartedText = value; }
		}

		public LoggerVerbosity Verbosity {
			get { return verbosity;	}
			set { verbosity = value; }
		}

		protected WriteHandler WriteHandler {
			get { return writeHandler; }
			set { writeHandler = value; }
		}
	}
}

#endif
