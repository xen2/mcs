//
// Mono.Documentation/exceptions.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2008 Novell, Inc.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Mono.Documentation {

	[Flags]
	public enum ExceptionLocations {
		Member              = 0x0,
		Assembly            = 0x1,
		DependentAssemblies = 0x2,
		All = Assembly | DependentAssemblies
	}
	 
	public class ExceptionSources {
		internal ExceptionSources (TypeReference exception)
		{
			Exception = exception;
			SourcesList = new List<IMemberReference> ();
			Sources = new ReadOnlyCollection<IMemberReference> (SourcesList);
		}

		public TypeReference Exception { get; private set; }
		public ReadOnlyCollection<IMemberReference> Sources { get; private set; }
		internal List<IMemberReference> SourcesList;
	}


	public class ExceptionLookup {

		SlashDocMemberFormatter xdoc = new SlashDocMemberFormatter ();

		// xdoc(MemberRef) -> xdoc(TypeRef) -> ExceptionSource
		//   where ExceptionSource.Exception == xdoc(TypeRef)
		Dictionary<string, Dictionary<string, ExceptionSources>> db = new Dictionary<string, Dictionary<string, ExceptionSources>> ();

		ExceptionLocations locations;

		public ExceptionLookup (ExceptionLocations locations)
		{
			this.locations = locations;
		}

		public IEnumerable<ExceptionSources> this [IMemberReference member] {
			get {
				if (member == null)
					throw new ArgumentNullException ("member");
				string memberDecl = xdoc.GetDeclaration (member.GetDefinition ());
				Dictionary<string, ExceptionSources> e;
				if (!db.TryGetValue (memberDecl, out e)) {
					e = new Dictionary<string, ExceptionSources> ();
					var bodies = GetMethodBodies (member);
					foreach (var body in bodies) {
						if (body == null)
							continue;
						FillExceptions (body, e);
					}
					db.Add (memberDecl, e);
				}
				return e.Values;
			}
		}

		MethodBody[] GetMethodBodies (IMemberReference member)
		{
			if (member is MethodReference) {
				return new[]{ ((MethodDefinition) member.GetDefinition ()).Body };
			}
			if (member is PropertyReference) {
				PropertyDefinition prop = (PropertyDefinition) member.GetDefinition ();
				return new[]{
					prop.GetMethod != null ? prop.GetMethod.Body : null,
					prop.SetMethod != null ? prop.SetMethod.Body : null,
				};
			}
			if (member is FieldReference)
				return new MethodBody[]{};
			if (member is EventReference) {
				EventDefinition ev = (EventDefinition) member.GetDefinition ();
				return new[]{
					ev.AddMethod != null ? ev.AddMethod.Body : null,
					ev.InvokeMethod != null ? ev.InvokeMethod.Body : null, 
					ev.RemoveMethod != null ? ev.RemoveMethod.Body : null,
				};
			}
			throw new NotSupportedException ("Unsupported member type: " + member.GetType().FullName);
		}

		void FillExceptions (MethodBody body, Dictionary<string, ExceptionSources> exceptions)
		{
			for (int i = 0; i < body.Instructions.Count; ++i) {
				Instruction instruction = body.Instructions [i];
				switch (instruction.OpCode.Code) {
					case Code.Call:
					case Code.Callvirt: {
						if ((locations & ExceptionLocations.Assembly) == 0 && 
								(locations & ExceptionLocations.DependentAssemblies) == 0)
							break;
						IMemberReference memberRef = ((IMemberReference) instruction.Operand);
						if (((locations & ExceptionLocations.Assembly) != 0 && 
									body.Method.DeclaringType.Scope.Name == memberRef.DeclaringType.Scope.Name) ||
								((locations & ExceptionLocations.DependentAssemblies) != 0 && 
									body.Method.DeclaringType.Scope.Name != memberRef.DeclaringType.Scope.Name)) {
							IEnumerable<ExceptionSources> memberExceptions = this [memberRef];
							AddExceptions (body, instruction, 
									memberExceptions.Select (es => es.Exception),
									memberExceptions.SelectMany (es => es.Sources),
									exceptions);
						}
						break;
					}
					case Code.Newobj: {
						MethodReference ctor = (MethodReference) instruction.Operand;
						if (IsExceptionConstructor (ctor)) {
							AddExceptions (body, instruction,
									new TypeReference[]{ctor.DeclaringType},
									new IMemberReference[]{body.Method},
									exceptions);
						}
						break;
					}
				}
			}
		}

		void AddExceptions (MethodBody body, Instruction instruction, IEnumerable<TypeReference> add, IEnumerable<IMemberReference> sources,
				Dictionary<string, ExceptionSources> exceptions)
		{
			var handlers = body.ExceptionHandlers.Cast<ExceptionHandler> ()
				.Where (eh => instruction.Offset >= eh.TryStart.Offset && 
						instruction.Offset <= eh.TryEnd.Offset);
			foreach (var ex in add) {
				if (!handlers.Any (h => IsExceptionCaught (ex, h.CatchType))) {
					ExceptionSources s;
					string eName = xdoc.GetDeclaration (ex);
					if (!exceptions.TryGetValue (eName, out s)) {
						s = new ExceptionSources (ex);
						exceptions.Add (eName, s);
					}
					s.SourcesList.AddRange (sources);
				}
			}
		}

		static bool IsExceptionConstructor (MethodReference ctor)
		{
			return GetBases (ctor.DeclaringType)
				.Any (t => t.FullName == "System.Exception");
		}

		bool IsExceptionCaught (TypeReference exception, TypeReference catcher)
		{
			return GetBases (exception).Select (e => xdoc.GetDeclaration (e))
				.Union (GetBases (catcher).Select (e => xdoc.GetDeclaration (e)))
				.Any ();
		}

		static IEnumerable<TypeReference> GetBases (TypeReference type)
		{
			yield return type;
			TypeDefinition def = DocUtils.GetTypeDefinition (type);
			while (def != null && def.BaseType != null) {
				yield return def.BaseType;
				def = DocUtils.GetTypeDefinition (def.BaseType);
			}
		}
	}
}
