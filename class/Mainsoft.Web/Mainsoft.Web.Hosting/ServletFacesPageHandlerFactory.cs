using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using javax.faces.component;
using javax.servlet;
using javax.faces.context;
using javax.faces.lifecycle;
using javax.servlet.http;
using System;
using javax.faces.webapp;
using javax.faces;

namespace Mainsoft.Web.Hosting
{
	class ServletFacesPageHandlerFactory : IHttpHandlerFactory
	{
		readonly ServletConfig _servletConfig;
		readonly FacesContextFactory _facesContextFactory;
		readonly Lifecycle _lifecycle;

		string getLifecycleId () {
			String lifecycleId = _servletConfig.getServletContext ().getInitParameter (FacesServlet.LIFECYCLE_ID_ATTR);
			return lifecycleId != null ? lifecycleId : LifecycleFactory.DEFAULT_LIFECYCLE;
		}

		public ServletFacesPageHandlerFactory () {

			HttpWorkerRequest wr = (HttpWorkerRequest) ((IServiceProvider) HttpContext.Current).GetService (typeof (HttpWorkerRequest));
			HttpServlet servlet = (HttpServlet) ((IServiceProvider) wr).GetService (typeof (HttpServlet));

			_servletConfig = servlet.getServletConfig ();
			_facesContextFactory = (FacesContextFactory) FactoryFinder.getFactory (FactoryFinder.FACES_CONTEXT_FACTORY);
			//TODO: null-check for Weblogic, that tries to initialize Servlet before ContextListener

			//Javadoc says: Lifecycle instance is shared across multiple simultaneous requests, it must be implemented in a thread-safe manner.
			//So we can acquire it here once:
			LifecycleFactory lifecycleFactory = (LifecycleFactory) FactoryFinder.getFactory (FactoryFinder.LIFECYCLE_FACTORY);
			_lifecycle = lifecycleFactory.getLifecycle (getLifecycleId ());

		}

		public virtual IHttpHandler GetHandler (HttpContext context, string requestType, string url, string path) {
			IHttpHandler handler = PageParser.GetCompiledPageInstance (url, path, context);
			if (!(handler is Page))
				return handler;

			HttpWorkerRequest wr = (HttpWorkerRequest) ((IServiceProvider) context).GetService (typeof (HttpWorkerRequest));
			HttpServletRequest request = (HttpServletRequest) ((IServiceProvider) wr).GetService (typeof (HttpServletRequest));
			HttpServletResponse response = (HttpServletResponse) ((IServiceProvider) wr).GetService (typeof (HttpServletResponse));

			FacesContext facesContext
					= _facesContextFactory.getFacesContext (_servletConfig.getServletContext (),
														   request,
														   response,
														   _lifecycle);

			return new ServletFacesPageHandler (AspNetFacesContext.WrapFacesContext (facesContext, context, handler), _lifecycle);
		}

		public virtual void ReleaseHandler (IHttpHandler handler) {
		}
	}
}

