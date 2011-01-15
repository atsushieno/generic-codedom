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

		protected override void SetItem (int index, TNew item)
		{
			base.SetItem (index, item);
			((IList) old) [index] = ToOld (item);
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

		protected override void SetItem (int index, CodeNamespaceImport item)
		{
			base.SetItem (index, item);
			((IList) old) [index] = (OldCodeNamespaceImport) item;
		}
	}
}
