// Transport Security Layer (TLS)
// Copyright (c) 2003-2004 Carlos Guzman Alvarez

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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	#region Delegates

	public delegate bool CertificateValidationCallback(
		X509Certificate certificate, 
		int[]			certificateErrors);

	public delegate X509Certificate CertificateSelectionCallback(
		X509CertificateCollection	clientCertificates, 
		X509Certificate				serverCertificate, 
		string						targetHost, 
		X509CertificateCollection	serverRequestedCertificates);

	public delegate AsymmetricAlgorithm PrivateKeySelectionCallback(
		X509Certificate	certificate, 
		string			targetHost);

	#endregion

	public class SslClientStream : SslStreamBase
	{
		#region Internal Events
		
		internal event CertificateValidationCallback	ServerCertValidation;
		internal event CertificateSelectionCallback		ClientCertSelection;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Properties

		// required by HttpsClientStream for proxy support
		internal Stream InputBuffer 
		{
			get { return base.inputBuffer; }
		}

		public X509CertificateCollection ClientCertificates
		{
			get { return this.context.ClientSettings.Certificates; }
		}

		public X509Certificate SelectedClientCertificate
		{
			get { return this.context.ClientSettings.ClientCertificate; }
		}

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ServerCertValidationDelegate
		{
			get { return this.ServerCertValidation; }
			set { this.ServerCertValidation = value; }			
		}

		public CertificateSelectionCallback ClientCertSelectionDelegate 
		{
			get { return this.ClientCertSelection; }
			set { this.ClientCertSelection = value; }
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate
		{
			get { return this.PrivateKeySelection; }
			set { this.PrivateKeySelection = value; }
		}
		
		#endregion

		#region Constructors
		
		public SslClientStream(
			Stream	stream, 
			string	targetHost, 
			bool	ownsStream) 
			: this(
				stream, targetHost, ownsStream, 
				SecurityProtocolType.Default, null)
		{
		}
		
		public SslClientStream(
			Stream				stream, 
			string				targetHost, 
			X509Certificate		clientCertificate) 
			: this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				new X509CertificateCollection(new X509Certificate[]{clientCertificate}))
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost, 
			X509CertificateCollection clientCertificates) : 
			this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				clientCertificates)
		{
		}

		public SslClientStream(
			Stream					stream,
			string					targetHost,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType) 
			: this(
				stream, targetHost, ownsStream, securityProtocolType,
				new X509CertificateCollection())
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost,
			bool						ownsStream,
			SecurityProtocolType		securityProtocolType,
			X509CertificateCollection	clientCertificates):
			base(stream, ownsStream)
		{
			if (targetHost == null || targetHost.Length == 0)
			{
				throw new ArgumentNullException("targetHost is null or an empty string.");
			}

			this.context = new ClientContext(
				this,
				securityProtocolType, 
				targetHost, 
				clientCertificates);

			this.protocol = new ClientRecordProtocol(innerStream, (ClientContext)this.context);
		}

		#endregion

		#region Finalizer

		~SslClientStream()
		{
			base.Dispose(false);
		}

		#endregion

		#region IDisposable Methods

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				this.ServerCertValidation = null;
				this.ClientCertSelection = null;
				this.PrivateKeySelection = null;
			}
		}

		#endregion

		#region Handshake Methods

		/*
			Client											Server

			ClientHello                 -------->
															ServerHello
															Certificate*
															ServerKeyExchange*
															CertificateRequest*
										<--------			ServerHelloDone
			Certificate*
			ClientKeyExchange
			CertificateVerify*
			[ChangeCipherSpec]
			Finished                    -------->
															[ChangeCipherSpec]
										<--------           Finished
			Application Data            <------->			Application Data

					Fig. 1 - Message flow for a full handshake		
		*/

		internal override IAsyncResult OnBeginNegotiateHandshake(AsyncCallback callback, object state)
		{
			try
			{
				if (this.context.HandshakeState != HandshakeState.None)
				{
					this.context.Clear();
				}

				// Obtain supported cipher suites
				this.context.SupportedCiphers = CipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);

				// Set handshake state
				this.context.HandshakeState = HandshakeState.Started;

				// Send client hello
				return this.protocol.BeginSendRecord(HandshakeType.ClientHello, callback, state);
			}
			catch (TlsException ex)
			{
				this.protocol.SendAlert(ex.Alert);

				throw new IOException("The authentication or decryption has failed.", ex);
			}
			catch (Exception ex)
			{
				this.protocol.SendAlert(AlertDescription.InternalError);

				throw new IOException("The authentication or decryption has failed.", ex);
			}
		}

		internal override void OnNegotiateHandshakeCallback(IAsyncResult asyncResult)
		{
			this.protocol.EndSendRecord(asyncResult);

			// Read server response
			while (this.context.LastHandshakeMsg != HandshakeType.ServerHelloDone)
			{
				// Read next record
				this.protocol.ReceiveRecord(this.innerStream);
			}

			// Send client certificate if requested
			// even if the server ask for it it _may_ still be optional
			bool clientCertificate = this.context.ServerSettings.CertificateRequest;

			// NOTE: sadly SSL3 and TLS1 differs in how they handle this and
			// the current design doesn't allow a very cute way to handle 
			// SSL3 alert warning for NoCertificate (41).
			if (this.context.SecurityProtocol == SecurityProtocolType.Ssl3)
			{
				clientCertificate = ((this.context.ClientSettings.Certificates != null) &&
					(this.context.ClientSettings.Certificates.Count > 0));
				// this works well with OpenSSL (but only for SSL3)
			}

			if (clientCertificate)
			{
				this.protocol.SendRecord(HandshakeType.Certificate);
			}

			// Send Client Key Exchange
			this.protocol.SendRecord(HandshakeType.ClientKeyExchange);

			// Now initialize session cipher with the generated keys
			this.context.Cipher.InitializeCipher();

			// Send certificate verify if requested (optional)
			if (clientCertificate && (this.context.ClientSettings.ClientCertificate != null))
			{
				this.protocol.SendRecord(HandshakeType.CertificateVerify);
			}

			// Send Cipher Spec protocol
			this.protocol.SendChangeCipherSpec();

			// Read record until server finished is received
			while (this.context.HandshakeState != HandshakeState.Finished)
			{
				// If all goes well this will process messages:
				// 		Change Cipher Spec
				//		Server finished
				this.protocol.ReceiveRecord(this.innerStream);
			}

			// Clear Key Info
			this.context.ClearKeyInfo();

		}

		#endregion

		#region Event Methods

		internal override X509Certificate OnLocalCertificateSelection(X509CertificateCollection clientCertificates, X509Certificate serverCertificate, string targetHost, X509CertificateCollection serverRequestedCertificates)
		{
			if (this.ClientCertSelection != null)
			{
				return this.ClientCertSelection(
					clientCertificates,
					serverCertificate,
					targetHost,
					serverRequestedCertificates);
			}

			return null;
		}
		
		internal override bool OnRemoteCertificateValidation(X509Certificate certificate, int[] errors)
		{
			if (this.ServerCertValidation != null)
			{
				return this.ServerCertValidation(certificate, errors);
			}

			return (errors != null && errors.Length == 0);
		}

		internal virtual bool RaiseServerCertificateValidation(
			X509Certificate certificate, 
			int[]			certificateErrors)
		{
			return base.RaiseRemoteCertificateValidation(certificate, certificateErrors);
		}

		internal X509Certificate RaiseClientCertificateSelection(
			X509CertificateCollection	clientCertificates, 
			X509Certificate				serverCertificate, 
			string						targetHost, 
			X509CertificateCollection	serverRequestedCertificates)
		{
			return base.RaiseLocalCertificateSelection(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
		}

		internal override AsymmetricAlgorithm OnLocalPrivateKeySelection(X509Certificate certificate, string targetHost)
		{
			if (this.PrivateKeySelection != null)
			{
				return this.PrivateKeySelection(certificate, targetHost);
			}

			return null;
		}

		internal AsymmetricAlgorithm RaisePrivateKeySelection(
			X509Certificate certificate,
			string targetHost)
		{
			return base.RaiseLocalPrivateKeySelection(certificate, targetHost);
		}

		#endregion
	}
}
