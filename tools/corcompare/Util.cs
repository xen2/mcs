using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Mono.Util.CorCompare.Cecil {

	static class TypeHelper {

		public static AssemblyResolver Resolver = new AssemblyResolver ();

		internal static bool IsPublic (TypeReference typeref)
		{
			if (typeref == null)
				throw new ArgumentException ("typeref must not be null");

			TypeDefinition td = Resolve (typeref);
			return td.IsPublic;
		}

		internal static bool IsDelegate (TypeReference typeref)
		{
			return IsDerivedFrom (typeref, "System.MulticastDelegate");
		}

		static TypeDefinition Resolve (TypeReference reference)
		{
			return Resolver.Resolve (reference);
		}

		internal static bool IsDerivedFrom (TypeReference type, string derivedFrom)
		{
			foreach (var def in WalkHierarchy (type))
				if (def.FullName == derivedFrom)
					return true;

			return false;
		}

		internal static IEnumerable<TypeDefinition> WalkHierarchy (TypeReference type)
		{
			for (var def = Resolve (type); def != null; def = GetBaseType (def))
				yield return def;
		}

		internal static IEnumerable<TypeReference> GetInterfaces (TypeReference type)
		{
			var ifaces = new Dictionary<string, TypeReference> ();

			foreach (var def in WalkHierarchy (type))
				foreach (TypeReference iface in def.Interfaces)
					ifaces [iface.FullName] = iface;

			return ifaces.Values;
		}

		internal static TypeDefinition GetBaseType (TypeDefinition child)
		{
			if (child.BaseType == null)
				return null;

			return Resolve (child.BaseType);
		}

		internal static bool IsPublic (CustomAttribute att)
		{
			return IsPublic (att.Constructor.DeclaringType);
		}

		internal static string GetFullName (CustomAttribute att)
		{
			return att.Constructor.DeclaringType.FullName;
		}

		internal static TypeDefinition GetTypeDefinition (CustomAttribute att)
		{
			return Resolve (att.Constructor.DeclaringType);
		}
	}
}
