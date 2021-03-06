//
// Code generator for Mono.CodeDom.Generic types.
//
// This tool automatically generates Mono.CodeDom.Generic types from
// System.CodeDom types in System.dll.
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Mono.CodeDom.Generic
{
	public class Driver
	{
		public static void Main ()
		{
			new Driver ().Run ();
		}

		Assembly ass;
		CodeNamespace cns = new CodeNamespace ("Mono.CodeDom.Generic");
		TextWriter output = new StringWriter ();

		public void Run ()
		{
			ass = typeof (CodeCompileUnit).Assembly;
			GenerateCode ();
		}
		
		void GenerateCode ()
		{
			var types = from tt in ass.GetTypes () where tt.IsPublic && tt.Namespace == "System.CodeDom" select tt;
			foreach (var t in types) {
				if (t == typeof (CodeNamespaceImportCollection))
					continue; // special: this cannot be automatically imported.
				if (t.BaseType == typeof (CollectionBase))
					GenerateCollectionTypeCode (t);
				else if (!t.IsEnum)
					GenerateOrdinalTypeCode (t);
			}

			string header = @"// This file is generated by codedom-generic skeleton generator.
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

{1}

namespace Mono.CodeDom.Generic
{
";
			using (var fs = File.CreateText ("generic-codedom.generated.cs")) {
				fs.WriteLine (header.Replace ("{1}", String.Join ("\n", (from t in types select String.Format ("using Old{0} = System.CodeDom.{0};", t.Name)).ToArray ())));
				fs.WriteLine (output);

				fs.WriteLine ("} // end of namespace");
			}
		}
		
		void GenerateCollectionTypeCode (Type type)
		{
			string template = @"
	public class {0}Collection : CodeDomCollection<{0}, Old{0}, Old{0}Collection>
	{
		internal {0}Collection (Old{0}Collection c)
			: base (c)
		{
		}

		public {0}Collection ()
			: base ()
		{
		}

		public {0}Collection ({0}Collection source)
			: base (source)
		{
		}

		public {0}Collection (params {0} [] value)
			: base (value)
		{
		}

		internal override Old{0} ToOld ({0} item)
		{
			return (Old{0}) item;
		}
	}
";
			output.Write (template.Replace ("{0}", type.Name.Substring (0, type.Name.LastIndexOf ("Collection"))));
		}

		void GenerateOrdinalTypeCode (Type type)
		{
			output.WriteLine ("// generic ordinal type for " + type);
			string template = @"
	public class {0} : {1}
	{{
		Old{0} old;

		// constructors, copied
{2}

		// old-to-new constructor
		internal {0} (Old{0} old) {6}
		{{
			this.old = old;
			Initialize ();
		}}

		// initializer
		void Initialize ()
		{{
			// repeat for all auto (new-CodeDom-type) properties
			{3}
		}}

		// explicit conversion operator
		public static explicit operator Old{0} ({0} source)
		{{
			return source.old;
		}}

		// For a property whose type is new CodeDom type, just make it auto property (use private set for get-only ones)
		{4}

		// Non-CodeDom properties follow.
		{5}
	}}
";
			var domprops = new List<PropertyInfo> ();
			var miscprops = new List<PropertyInfo> ();
			foreach (var p in type.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
				if (p.PropertyType.Namespace == "System.CodeDom" && !p.PropertyType.IsEnum)
					domprops.Add (p);
				else
					miscprops.Add (p);
			}

			string templateCtor = @"
		public {0} ({1})
			: this (new Old{0} ({2}))
		{{
		}}";

			var csw = new StringWriter ();
			foreach (var ctor in type.GetConstructors ()) {
				string parDefs = String.Join (", ", (from p in ctor.GetParameters () select Format (p)).ToArray ());
				string pars = String.Join (", ", (from p in ctor.GetParameters () select GetValueExpression (p)).ToArray ());
				csw.WriteLine (templateCtor, type.Name, parDefs, pars);
			}

			string templateInit = @"
			if (old.{0} != null)
				{0} = new {1}  (old.{0});";
			var isw = new StringWriter ();
			foreach (var p in domprops)
				isw.WriteLine (templateInit, p.Name, p.PropertyType.Name);

			string templateDomProp = @"
		public {3}{0} {1} {{ get; {2}set; }}";
			var dpsw = new StringWriter ();
			foreach (var p in domprops)
				dpsw.WriteLine (templateDomProp, p.PropertyType.Name, p.Name, p.IsSetterPublic () ? null : "private ", GetModifier (p));

			string templateOrdProp = @"
		public {3}{0} {1} {{
			get {{ return old.{1}; }}
			{2}
		}}";
			var nsw = new StringWriter ();
			foreach (var p in miscprops) {
				var setter = String.Format ("set {{ old.{0} = value; }}", p.Name);
				nsw.WriteLine (templateOrdProp, p.PropertyType.Name, p.Name, p.IsSetterPublic () ? setter : null, GetModifier (p));
			}

			// FIXME: write custom attributes
			output.WriteLine (template, type.Name, type.BaseType.Name, csw, isw, dpsw, nsw, type.BaseType.Namespace == "System.CodeDom" ? " : base (old)" : null);
		}

		string GetValueExpression (ParameterInfo p)
		{
			if (p.ParameterType.Namespace == "System.CodeDom" && !p.ParameterType.IsEnum) {
				if (p.ParameterType.IsArray)
					return String.Format ("(from x in {0} select (Old{1}) x).ToArray ()", p.Name, p.ParameterType.Name.Substring (0, p.ParameterType.Name.LastIndexOf ('[')));
				else
					return "(Old" + p.ParameterType.Name + ") " + p.Name;
			}
			else
				return p.Name;
		}

		string GetModifier (PropertyInfo p)
		{
			var m = p.GetGetMethod ();
			if (m.GetBaseDefinition () != m)
				return "override ";
			if (m.IsVirtual)
				return "virtual ";
			return null;
		}

		string Format (ParameterInfo p)
		{
			return String.Format ("{0}{1} {2}", p.GetCustomAttribute<ParamArrayAttribute> () != null ? "params " : null, p.ParameterType.Name, p.Name);
		}
	}

	static class Extensions
	{
		public static T GetCustomAttribute<T> (this ParameterInfo p)
		{
			foreach (var a in p.GetCustomAttributes (true))
				if (a is T)
					return (T) a;
			return default (T);
		}

		public static bool IsSetterPublic (this PropertyInfo p)
		{
			var mi = p.GetSetMethod ();
			return mi != null && mi.IsPublic;
		}
	}
}
