/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Extensions.PartitionEntryCountRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Extensions
{
	
	/// <summary>  Returns a count of the number of entries (objects) in the
	/// specified partition.
	/// 
	/// <p>To obtain the count of entries, you must create an instance of this
	/// class and then call the extendedOperation method with this
	/// object as the required LdapExtendedOperation parameter.</p>
	/// 
	/// <p>The returned LdapExtendedResponse object can then be converted to
	/// a PartitionEntryCountResponse object. This class contains
	/// methods for retrieving the returned count.</p>
	/// 
	/// <p>The PartitionEntryCountRequest extension uses the following
	/// OID:<br>
	/// &nbsp;&nbsp;&nbsp;2.16.840.1.113719.1.27.100.13</p>
	/// 
	/// <p>The requestValue has the following format:<br>
	/// 
	/// requestValue ::=<br><br>
	/// &nbsp;&nbsp;&nbsp;&nbsp;    dn &nbsp;&nbsp;&nbsp;     LdapDN
	/// </summary>
	public class PartitionEntryCountRequest:LdapExtendedOperation
	{
		
		/// <summary>  Constructs an extended operation object for counting entries
		/// in a naming context.
		/// 
		/// </summary>
		/// <param name="dn"> The distinguished name of the partition.
		/// 
		/// </param>
		/// <exception cref=""> LdapException A general exception which includes an
		/// error message and an Ldap error code.
		/// </exception>
		
		public PartitionEntryCountRequest(System.String dn):base(ReplicationConstants.NAMING_CONTEXT_COUNT_REQ, null)
		{
			
			try
			{
				
				if (((System.Object) dn == null))
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
				System.IO.MemoryStream encodedData = new System.IO.MemoryStream();
				LBEREncoder encoder = new LBEREncoder();
				
				Asn1OctetString asn1_dn = new Asn1OctetString(dn);
				
				asn1_dn.encode(encoder, encodedData);
				
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, (System.String) null);
			}
		}
	}
}
