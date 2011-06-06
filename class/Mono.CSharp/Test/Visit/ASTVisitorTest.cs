﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.CSharp;
using System.IO;

namespace MonoTests.Visit
{
	[TestFixture]
	public class ASTVisitorTest
	{
		class TestVisitor : StructuralVisitor
		{
		}

		[SetUp]
		public void Setup ()
		{
		}

		[Test]
		public void Simple ()
		{
			//string content = @"class A { }";
			string content = @"

class Foo
{
	void Bar ()
	{
completionList.Add (""delegate"" + sb, ""md-keyword"", GettextCatalog.GetString (""Creates anonymous delegate.""), ""delegate"" + sb + "" {"" + Document.Editor.EolMarker + stateTracker.Engine.ThisLineIndent + TextEditorProperties.IndentString + ""|"" + Document.Editor.EolMarker + stateTracker.Engine.ThisLineIndent +""};"");
	}
}"
	;


			var stream = new MemoryStream (Encoding.UTF8.GetBytes (content));

			var ctx = new CompilerContext (new CompilerSettings (), new Report (new AssertReportPrinter ()));

			ModuleContainer module = new ModuleContainer (ctx);
			CSharpParser parser = new CSharpParser (
				new SeekableStreamReader (stream, Encoding.UTF8),
				new CompilationUnit ("name", "path", 0),
				module);

			RootContext.ToplevelTypes = module;
			Location.AddFile (ctx.Report, "asdfas");
			Location.Initialize ();
			parser.LocationsBag = new LocationsBag ();
			parser.parse ();

			var m = module.Types[0].Methods[0] as Method;
			var s = m.Block.FirstStatement;
			var o = s.loc.Column;
			

			module.Accept (new TestVisitor ());
		}
	}
}
