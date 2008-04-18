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

	[Guid ("570F39D1-EFD0-11d3-B093-00A024FFC08C")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIWebProgressListener {

#region nsIWebProgressListener
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int onStateChange (
				[MarshalAs (UnmanagedType.Interface)]   nsIWebProgress aWebProgress,
				[MarshalAs (UnmanagedType.Interface)]   nsIRequest aRequest,
				   uint aStateFlags,
				   int aStatus);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int onProgressChange (
				[MarshalAs (UnmanagedType.Interface)]   nsIWebProgress aWebProgress,
				[MarshalAs (UnmanagedType.Interface)]   nsIRequest aRequest,
				   int aCurSelfProgress,
				   int aMaxSelfProgress,
				   int aCurTotalProgress,
				   int aMaxTotalProgress);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int onLocationChange (
				[MarshalAs (UnmanagedType.Interface)]   nsIWebProgress aWebProgress,
				[MarshalAs (UnmanagedType.Interface)]   nsIRequest aRequest,
				[MarshalAs (UnmanagedType.Interface)]   nsIURI aLocation);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int onStatusChange (
				[MarshalAs (UnmanagedType.Interface)]   nsIWebProgress aWebProgress,
				[MarshalAs (UnmanagedType.Interface)]   nsIRequest aRequest,
				   int aStatus,
				[MarshalAs(UnmanagedType.LPWStr)]   string aMessage);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int onSecurityChange (
				[MarshalAs (UnmanagedType.Interface)]   nsIWebProgress aWebProgress,
				[MarshalAs (UnmanagedType.Interface)]   nsIRequest aRequest,
				   uint aState);

#endregion
	}


	internal class nsWebProgressListener {
		public static nsIWebProgressListener GetProxy (Mono.WebBrowser.IWebBrowser control, nsIWebProgressListener obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIWebProgressListener).GUID, obj);
			return o as nsIWebProgressListener;
		}
	}
}
