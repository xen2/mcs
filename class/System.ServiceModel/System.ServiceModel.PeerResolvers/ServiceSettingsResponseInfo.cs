// 
// ServiceSettingsResponseInfo.cs
// 
// Author: 
//     Marcos Cobena (marcoscobena@gmail.com)
// 
// Copyright 2007 Marcos Cobena (http://www.youcannoteatbits.org/)
// 

using System.Runtime.Serialization;

namespace System.ServiceModel.PeerResolvers
{
	[MessageContract (IsWrapped = false)]
	public class ServiceSettingsResponseInfo
	{
		[MessageBodyMember (Name = "ServiceSettings", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
		ServiceSettingsResponseInfoDC body;
		
		public ServiceSettingsResponseInfo ()
		{
			body = new ServiceSettingsResponseInfoDC ();
		}
		
		public ServiceSettingsResponseInfo (bool control)
			: this ()
		{
			body.ControlMeshShape = control;
		}
		
		public bool ControlMeshShape {
			get { return body.ControlMeshShape; }
			set { body.ControlMeshShape = value; }
		}
		
		public bool HasBody ()
		{
			return true; // FIXME: I have no idea when it returns false
		}
	}
	
	[DataContract (Name = "ServiceSettings", Namespace = "http://schemas.microsoft.com/net/2006/05/peer")]
	internal class ServiceSettingsResponseInfoDC
	{
		bool control_mesh_shape;

		public ServiceSettingsResponseInfoDC ()
		{
		}
		
		[DataMember]
		public bool ControlMeshShape {
			get { return control_mesh_shape; }
			set { control_mesh_shape = value; }
		}
	}
}
