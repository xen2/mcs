// THIS FILE AUTOMATICALLY GENERATED BY xpidl2cs.pl
// EDITING IS PROBABLY UNWISE
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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mono.Mozilla {

	[Guid ("27386cf1-f27e-4d2d-9bf4-c4621d50d299")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIAccessibilityService : nsIAccessibleRetrieval {
#region nsIAccessibleRetrieval
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAccessibleFor ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttachedAccessibleFor ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getRelevantContentNodeFor ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAccessibleInWindow ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIDOMWindow aDOMWin,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAccessibleInWeakShell ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAccessibleInShell ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				 /*nsIPresShell*/ IntPtr aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getCachedAccessNode ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getCachedAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getStringRole ( uint aRole,
				 /*AString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getStringStates ( uint aStates,
				 uint aExtraStates,
				[MarshalAs (UnmanagedType.Interface) ] out nsIDOMDOMStringList ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getStringEventType ( uint aEventType,
				 /*AString*/ HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getStringRelationType ( uint aRelationType,
				 /*AString*/ HandleRef ret);

#endregion

#region nsIAccessibilityService
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createOuterDocAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createRootAccessible ( /*nsIPresShell*/ IntPtr aShell,
				 /*nsIDocument*/ IntPtr aDocument,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTML4ButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHyperTextAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLBRAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLAccessibleByMarkup ( /*nsIFrame*/ IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aWeakShell,
				[MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aDOMNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLLIAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ]  IntPtr aBulletFrame,
				 /*AString*/ HandleRef aBulletText,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLCheckboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLComboboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLGenericAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLGroupboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLHRAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLImageAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLLabelAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLListboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLObjectFrameAccessible ( /*nsObjectFrame*/ IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLRadioButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLSelectOptionAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIAccessible aAccParent,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLTableAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLTableCellAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLTableHeadAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aDOMNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLTextAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLTextFieldAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createHTMLCaptionAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				 /*nsIPresShell*/ IntPtr aPresShell,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aWeakShell,
				out /*nsIFrame*/ IntPtr frameHint,
				out bool aIsHidden,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int addNativeRootAccessible ( IntPtr aAtkAccessible,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeNativeRootAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIAccessible aRootAccessible);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int invalidateSubtreeFor ( /*nsIPresShell*/ IntPtr aPresShell,
				 /*nsIContent*/ IntPtr aChangedContent,
				 UInt32 aEvent);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int processDocLoadEvent ([MarshalAs (UnmanagedType.Interface) ]  nsITimer aTimer,
				 IntPtr aClosure,
				 UInt32 aEventType);

#endregion
	}


	internal class nsAccessibilityService {
		public static nsIAccessibilityService GetProxy (Mono.WebBrowser.IWebBrowser control, nsIAccessibilityService obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIAccessibilityService).GUID, obj);
			return o as nsIAccessibilityService;
		}
	}
}
#if example

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

	internal class AccessibilityService : nsIAccessibilityService {

#region nsIAccessibilityService
		int nsIAccessibilityService.createOuterDocAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createRootAccessible ( /*nsIPresShell*/ IntPtr aShell,
				 /*nsIDocument*/ IntPtr aDocument,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTML4ButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHyperTextAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLBRAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLAccessibleByMarkup ( /*nsIFrame*/ IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aWeakShell,
				[MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aDOMNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLLIAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ]  IntPtr aBulletFrame,
				 /*AString*/ HandleRef aBulletText,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLCheckboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLComboboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLGenericAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLGroupboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLHRAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLImageAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLLabelAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLListboxAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLObjectFrameAccessible ( /*nsObjectFrame*/ IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLRadioButtonAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLSelectOptionAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				[MarshalAs (UnmanagedType.Interface) ]  nsIAccessible aAccParent,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aPresShell,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLTableAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLTableCellAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLTableHeadAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aDOMNode,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLTextAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLTextFieldAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.createHTMLCaptionAccessible ([MarshalAs (UnmanagedType.Interface) ]  IntPtr aFrame,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.getAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIDOMNode aNode,
				 /*nsIPresShell*/ IntPtr aPresShell,
				[MarshalAs (UnmanagedType.Interface) ]  nsIWeakReference aWeakShell,
				out /*nsIFrame*/ IntPtr frameHint,
				out bool aIsHidden,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.addNativeRootAccessible ( IntPtr aAtkAccessible,
				[MarshalAs (UnmanagedType.Interface) ] out nsIAccessible ret)
		{
			return ;
		}



		int nsIAccessibilityService.removeNativeRootAccessible ([MarshalAs (UnmanagedType.Interface) ]  nsIAccessible aRootAccessible)
		{
			return ;
		}



		int nsIAccessibilityService.invalidateSubtreeFor ( /*nsIPresShell*/ IntPtr aPresShell,
				 /*nsIContent*/ IntPtr aChangedContent,
				 UInt32 aEvent)
		{
			return ;
		}



		int nsIAccessibilityService.processDocLoadEvent ([MarshalAs (UnmanagedType.Interface) ]  nsITimer aTimer,
				 IntPtr aClosure,
				 UInt32 aEventType)
		{
			return ;
		}



#endregion
	}
#endif
