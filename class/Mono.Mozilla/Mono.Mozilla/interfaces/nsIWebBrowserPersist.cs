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

	[Guid ("dd4e0a6a-210f-419a-ad85-40e8543b9465")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIWebBrowserPersist : nsICancelable {
#region nsICancelable
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int cancel (
				   int aReason);

#endregion

#region nsIWebBrowserPersist
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getPersistFlags ( out uint ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setPersistFlags ( uint value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getCurrentState ( out uint ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getResult ( out uint ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getProgressListener ([MarshalAs (UnmanagedType.Interface)]  out nsIWebProgressListener ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setProgressListener ([MarshalAs (UnmanagedType.Interface)]  nsIWebProgressListener value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int saveURI (
				[MarshalAs (UnmanagedType.Interface)]   nsIURI aURI,
				   IntPtr aCacheKey,
				[MarshalAs (UnmanagedType.Interface)]   nsIURI aReferrer,
				[MarshalAs (UnmanagedType.Interface)]   nsIInputStream aPostData,
				[MarshalAs (UnmanagedType.LPStr)]   string aExtraHeaders,
				   IntPtr aFile);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int saveChannel (
				[MarshalAs (UnmanagedType.Interface)]   nsIChannel aChannel,
				   IntPtr aFile);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int saveDocument (
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMDocument aDocument,
				   IntPtr aFile,
				   IntPtr aDataPath,
				[MarshalAs (UnmanagedType.LPStr)]   string aOutputContentType,
				   uint aEncodingFlags,
				   uint aWrapColumn);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int cancelSave ();

#endregion
	}


	internal class nsWebBrowserPersist {
		public static nsIWebBrowserPersist GetProxy (Mono.WebBrowser.IWebBrowser control, nsIWebBrowserPersist obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIWebBrowserPersist).GUID, obj);
			return o as nsIWebBrowserPersist;
		}
	}
}
