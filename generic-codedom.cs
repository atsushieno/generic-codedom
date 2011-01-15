using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

using OldCodeCompileUnit = System.CodeDom.CodeCompileUnit;
using OldCodeAttributeDeclarationCollection = System.CodeDom.CodeAttributeDeclarationCollection;
using OldCodeAttributeDeclaration = System.CodeDom.CodeAttributeDeclaration;
using OldCodeTypeReference = System.CodeDom.CodeTypeReference;
using OldCodeNamespaceImportCollection = System.CodeDom.CodeNamespaceImportCollection;
using OldCodeNamespaceImport = System.CodeDom.CodeNamespaceImport;

namespace Mono.CodeDom.Generic
{
	public abstract class CodeDomCollection<TNew,TOld,TOldCollection> : Collection<TNew> where TOldCollection : CollectionBase,new()
	{
		TOldCollection old;

		internal CodeDomCollection (TOldCollection c)
		{
			this.old = c;
		}

		public CodeDomCollection ()
		{
			old = new TOldCollection ();
		}

		public CodeDomCollection (CodeDomCollection<TNew,TOld,TOldCollection> source)
		{
			old = new TOldCollection ();
			foreach (var item in source)
				((IList) old).Add (item);
		}

		public CodeDomCollection (params TNew [] value)
		{
			foreach (var v in value)
				Add (v);
		}

		protected override void ClearItems ()
		{
			base.ClearItems ();
			if (old != null)
				old.Clear ();
		}

		protected override void InsertItem (int index, TNew item)
		{
			base.InsertItem (index, item);
			((IList) old).Insert (index, ToOld (item));
		}

		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
			old.RemoveAt (index);
		}

		internal abstract TOld ToOld (TNew item);
	}

	// It is special: the corresponding System.CodeDom class does not inherit from CollectionBase, so I have manually implemented it.
	public class CodeNamespaceImportCollection : Collection<CodeNamespaceImport>
	{
		OldCodeNamespaceImportCollection old;

		// constructors, copied

		public CodeNamespaceImportCollection ()
		{
			old = new OldCodeNamespaceImportCollection ();
		}


		// old-to-new constructor
		internal CodeNamespaceImportCollection (OldCodeNamespaceImportCollection old)
		{
			this.old = old;
		}

		// explicit conversion operator
		public static explicit operator OldCodeNamespaceImportCollection (CodeNamespaceImportCollection source)
		{
			return source.old;
		}

		protected override void ClearItems ()
		{
			base.ClearItems ();
			if (old != null)
				old.Clear ();
		}

		protected override void InsertItem (int index, CodeNamespaceImport item)
		{
			base.InsertItem (index, item);
			((IList) old).Insert (index, (OldCodeNamespaceImport) item);
		}

		protected override void RemoveItem (int index)
		{
			base.RemoveItem (index);
			((IList) old).RemoveAt (index);
		}
	}

	/*
	public class CodeCompileUnit
	{
		OldCodeCompileUnit ccu = new OldCodeCompileUnit ();

		public CodeCompileUnit ()
		{
			AssemblyCustomAttributes = new CodeAttributeDeclarationCollection (ccu.AssemblyCustomAttributes);
			EndDirectives = new CodeDirectiveCollection (ccu.EndDirectives);
		}

		public CodeAttributeDeclarationCollection AssemblyCustomAttributes { get; private set; }

		public CodeDirectiveCollection EndDirectives { get; private set; }
	}

	public class CodeAttributeDeclarationCollection : CodeDomCollection<CodeAttributeDeclaration, OldCodeAttributeDeclaration, OldCodeAttributeDeclarationCollection>
	{
		internal CodeAttributeDeclarationCollection (OldCodeAttributeDeclarationCollection c)
			: base (c)
		{
		}

		public CodeAttributeDeclarationCollection ()
			: base ()
		{
		}

		public CodeAttributeDeclarationCollection (CodeAttributeDeclarationCollection source)
			: base (source)
		{
		}

		public CodeAttributeDeclarationCollection (params CodeAttributeDeclaration [] value)
			: base (value)
		{
		}

		internal override OldCodeAttributeDeclaration ToOld (CodeAttributeDeclaration item)
		{
			return (OldCodeAttributeDeclaration) item;
		}
	}

	public class CodeAttributeDeclaration
	{
		OldCodeAttributeDeclaration old;

		public CodeAttributeDeclaration ()
		{
			old = new OldCodeAttributeDeclaration ();
			Initialize ();
		}

		public CodeAttributeDeclaration (CodeTypeReference attributeType)
		{
			old = new OldCodeAttributeDeclaration ((OldCodeTypeReference) attributeType);
			Initialize ();
		}

		public CodeAttributeDeclaration (string name, params CodeAttributeArgument[] arguments)
		{
			old = new OldCodeAttributeDeclaration (name, arguments);
			Initialize ();
		}

		void Initialize ()
		{
			if (old.Arguments != null)
				Arguments = new CodeAttributeArgumentCollection (old.Arguments);
			if (old.AttributeType != null)
				AttributeType = new CodeTypeReference (old.AttributeType);
		}

		public CodeAttributeArgumentCollection Arguments { get; private set; }

		public CodeTypeReference AttributeType { get; private set; }

		public string Name {
			get { return old.Name; }
			set { old.Name = value; }
		}

		public static explicit operator OldCodeAttributeDeclaration (CodeAttributeDeclaration source)
		{
			return source.old;
		}
	}

	[SerializableAttribute]
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	public class CodeTypeReference : CodeObject
	{
		OldCodeTypeReference old;

		public CodeTypeReference (CodeTypeParameter typeParameter)
		{
			old = new OldCodeTypeReference ((OldCodeTypeParameter) typeParameter);
			Initialize ();
		}

		internal CodeTypeReference (OldCodeTypeReference old)
		{
			this.old = old;
			Initialize ();
		}

		void Initialize ()
		{
			if (old.ArrayElementType != null)
				ArrayElementType = new CodeTypeReference (old.ArrayElementType);
			if (old.TypeArguments != null)
				TypeArguments = new CodeTypeReferenceCollection (old.TypeArguments);
		}

		public CodeTypeReference ArrayElementType { get; set; }

		public int ArrayRank {
			get { return old.ArrayRank; }
			set { old.ArrayRank = value; }
		}

		public string BaseType {
			get { return old.BaseType; }
			set { old.BaseType = value; }
		}

		[ComVisibleAttribute (false)]
		public CodeTypeReferenceOptions Options {
			get { return old.Options; }
			set { old.Options = value; }
		}

		[ComVisibleAttribute (false)]
		public CodeTypeReferenceCollection TypeArguments { get; private set; }

		public static explicit operator OldCodeTypeReference (CodeTypeReference source)
		{
			return source.old;
		}
	}
	*/

/*
	// 0: class name
	// 1: attributes
	// 2: base type name
	{1}
	public class {0} : {2}
	{
		Old{0} old;

		// constructors, copied
		public {0} (copied constructor arguments)
		{
			old = new Old{0} ((Old{0}) arguments);
			Initialize ();
		}

		// old-to-new constructor
		internal {0} (Old{0} old)
		{
			this.old = old;
			Initialize ();
		}

		// initializer
		void Initialize ()
		{
			// repeat for all auto (new-CodeDom-type) properties
			if (old.{prop} != null)
				{prop} = new {proptype}  (old.{prop});
		}

		// explicit conversion operator
		public static explicit operator Old{0} ({0} source)
		{
			return source.old;
		}

		// For a property whose type is new CodeDom type, just make it auto property (use private set for get-only ones)
		{prop-atts}
		public {CodeDomPropType} {CodeDomProp} { get; set; }

		public {NonCodeDomPropType} {NonCodeDomProp} {
			get { return old.{NonCodeDomProp}; }
			// define setter if exists.
			set { old.{NonCodeDomProp} = value; }
		}
	}
*/
}
