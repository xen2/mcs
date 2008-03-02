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

	[Guid ("69E5DF00-7B8B-11d3-AF61-00A024FFC08C")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIWebBrowser {

#region nsIWebBrowser
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int addWebBrowserListener (
				[MarshalAs (UnmanagedType.Interface)]   nsIWeakReference aListener,
				[MarshalAs (UnmanagedType.LPStruct)]   Guid aIID);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeWebBrowserListener (
				[MarshalAs (UnmanagedType.Interface)]   nsIWeakReference aListener,
				[MarshalAs (UnmanagedType.LPStruct)]   Guid aIID);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getContainerWindow ([MarshalAs (UnmanagedType.Interface)]  out nsIWebBrowserChrome ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setContainerWindow ([MarshalAs (UnmanagedType.Interface)]  nsIWebBrowserChrome value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getParentURIContentListener ([MarshalAs (UnmanagedType.Interface)]  out nsIURIContentListener ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setParentURIContentListener ([MarshalAs (UnmanagedType.Interface)]  nsIURIContentListener value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getContentDOMWindow ([MarshalAs (UnmanagedType.Interface)]  out nsIDOMWindow ret);

#endregion
	}


	internal class nsWebBrowser {
		public static nsIWebBrowser GetProxy (Mono.WebBrowser.IWebBrowser control, nsIWebBrowser obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIWebBrowser).GUID, obj);
			return o as nsIWebBrowser;
		}
	}
}
