//
// System.Web.UI.Design.TemplatedControlDesigner
//
// Authors:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
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
using System.Collections;
using System.ComponentModel.Design;

namespace System.Web.UI.Design
{
	public abstract class TemplatedControlDesigner : ControlDesigner
	{
		public TemplatedControlDesigner ()
		{
		}

		protected abstract ITemplateEditingFrame CreateTemplateEditingFrame (TemplateEditingVerb verb);
		protected abstract TemplateEditingVerb[] GetCachedTemplateEditingVerbs ();
		public abstract string GetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, out bool allowEditing);
		public abstract void SetTemplateContent (ITemplateEditingFrame editingFrame, string templateName, string templateContent);

		[MonoTODO]
		public void EnterTemplateMode (ITemplateEditingFrame newTemplateEditingFrame)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExitTemplateMode (bool fSwitchingTemplates, bool fNested, bool fSave)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string GetPersistInnerHtml ()
		{
			throw new NotImplementedException ();
		}

		public virtual string GetTemplateContainerDataItemProperty (string templateName)
		{
			return string.Empty;
		}

		public virtual IEnumerable GetTemplateContainerDataSource (string templateName)
		{
			return null;
		}

		[MonoTODO]
		public TemplateEditingVerb[] GetTemplateEditingVerbs ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected ITemplate GetTemplateFromText (string text)
		{
			throw new NotImplementedException ();
		}

		public virtual Type GetTemplatePropertyParentType (string templateName)
		{
			return base.Component.GetType ();
		}

		[MonoTODO]
		protected string GetTextFromTemplate (ITemplate template)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnBehaviorAttached ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnComponentChanged (object sender, ComponentChangedEventArgs ce)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void OnSetParent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnTemplateModeChanged ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void SaveActiveTemplateEditingFrame ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void UpdateDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		public ITemplateEditingFrame ActiveTemplateEditingFrame {
			get {
				return _activeTemplateFrame;
			}
		}

		public bool CanEnterTemplateMode {
			get {
				return _enableTemplateEditing;
			}
		}

		protected virtual bool HidePropertiesInTemplateMode {
			get {
				return true;
			}
		}

		public bool InTemplateMode {
			get {
				return _templateMode;
			}
		}

		internal EventHandler TemplateEditingVerbHandler {
			get {
				return _templateVerbHandler;
			}
		}

		private ITemplateEditingFrame _activeTemplateFrame;
		private bool _enableTemplateEditing = true;
		private bool _templateMode;
		private EventHandler _templateVerbHandler;
	}
}
