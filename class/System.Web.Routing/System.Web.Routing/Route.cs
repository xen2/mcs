//
// Route.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Web;

namespace System.Web.Routing
{
#if NET_4_0
	[TypeForwardedFrom ("System.Web.Routing, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class Route : RouteBase
	{
		PatternParser url;

		public RouteValueDictionary Constraints { get; set; }

		public RouteValueDictionary DataTokens { get; set; }

		public RouteValueDictionary Defaults { get; set; }

		public IRouteHandler RouteHandler { get; set; }

		public string Url {
			get { return url != null ? url.Url : String.Empty; }
			set { url = value != null ? new PatternParser (value) : new PatternParser (String.Empty); }
		}

		public Route (string url, IRouteHandler routeHandler)
			: this (url, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
			: this (url, defaults, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
			: this (url, defaults, constraints, null, routeHandler)
		{
		}

		public Route (string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
		{
			Url = url;
			Defaults = defaults;
			Constraints = constraints;
			DataTokens = dataTokens;
			RouteHandler = routeHandler;
		}

		public override RouteData GetRouteData (HttpContextBase httpContext)
		{
			var path = httpContext.Request.AppRelativeCurrentExecutionFilePath;
			var pathInfo = httpContext.Request.PathInfo;

			if (pathInfo != String.Empty)
				throw new NotImplementedException ();

			// probably code like this causes ArgumentOutOfRangeException under .NET.
			// It somehow allows such path that is completely equivalent to the Url. Dunno why.
			if (Url != path && path.Substring (0, 2) != "~/")
				return null;
			path = path.Substring (2);

			var values = url.Match (path, Defaults);
			if (values == null)
				return null;

			if (Constraints != null)
				foreach (var p in Constraints)
					if (!ProcessConstraint (httpContext, p.Value, p.Key, values, RouteDirection.IncomingRequest))
						return null;

			var rd = new RouteData (this, RouteHandler);
			RouteValueDictionary rdValues = rd.Values;
			
			foreach (var p in values)
				rdValues.Add (p.Key, p.Value);

			RouteValueDictionary dataTokens = DataTokens;
			if (dataTokens != null) {
				RouteValueDictionary rdDataTokens = rd.DataTokens;
				foreach (var token in dataTokens)
					rdDataTokens.Add (token.Key, token.Value);
			}
			
			return rd;
		}

		public override VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("requestContext");
			if (url == null)
				return new VirtualPathData (this, String.Empty);

			// null values is allowed.
			// if (values == null)
			// 	values = requestContext.RouteData.Values;

			string s;
			if (!url.BuildUrl (this, requestContext, values, out s))
				return null;

			return new VirtualPathData (this, s);
		}

		internal static bool ProcessConstraintInternal (HttpContextBase httpContext, Route route, object constraint, string parameterName,
								RouteValueDictionary values, RouteDirection routeDirection, out bool invalidConstraint)
		{
			invalidConstraint = false;
			IRouteConstraint irc = constraint as IRouteConstraint;
			if (irc != null)
				return irc.Match (httpContext, route, parameterName, values, routeDirection);

			string s = constraint as string;
			if (s != null) {
				string v = values [parameterName] as string;
				if (!String.IsNullOrEmpty (v))
					return Regex.Match (v, s).Success;
				return false;
			}

			invalidConstraint = true;
			return false;
		}
		
		protected virtual bool ProcessConstraint (HttpContextBase httpContext, object constraint, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
		{
			if (parameterName == null)
				throw new ArgumentNullException ("parameterName");

			// .NET "compatibility"
			if (values == null)
				throw new NullReferenceException ();
			
			bool invalidConstraint;
			bool ret = ProcessConstraintInternal (httpContext, this, constraint, parameterName, values, routeDirection, out invalidConstraint);
			
			if (invalidConstraint)
				throw new InvalidOperationException (String.Format ("Constraint parameter '{0}' must be either a string or an IRouteConstraint instance", parameterName));

			return ret;
		}
	}
}
