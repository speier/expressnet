using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Routing;
using System.Web.Script.Serialization;

namespace ExpressNet
{
	public class ExpressNetApp : IRouteHandler, IHttpHandler
	{
		private List<ExpressNetAction> middlewares = new List<ExpressNetAction> ();

		public IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			return this;
		}

		public bool IsReusable {
			get { return false; }
		}

		public void ProcessRequest (HttpContext context)
		{
			var req = new ExpressNetReq (context.Request);
			var res = new ExpressNetRes (context.Response);

			var values = new Dictionary<string, object>(context.Request.RequestContext.RouteData.Values);
			foreach (var key in context.Request.Form.AllKeys) {
				values.Add(key, context.Request.Form[key]);
			}

			req.SetParams(values);

			foreach (var middleware in middlewares) {
				middleware (req, res);
			}

			var callbacks = context.Request.RequestContext.RouteData.Values ["callbacks"] as ExpressNetAction[];
			if (callbacks != null) {
				foreach (var cb in callbacks) {
					cb (req, res);
				}
			}
		}

		public void Use (params ExpressNetAction[] middlewares)
		{
			this.middlewares.AddRange (middlewares);
		}

		public void Get (string route, params ExpressNetAction[] callbacks)
		{
			AddRoute (route, callbacks, "GET");
		}

		private void AddRoute (string path, ExpressNetAction[] callbacks, params string[] allowedMethods)
		{
			var url = path.StartsWith ("/") ? path.Remove (0, 1) : path;

			var routes = RouteTable.Routes;
			var routeHandlerType = this.GetType ();

			using (routes.GetWriteLock()) {

				var route = new Route (url, this)
				{
						Defaults = new RouteValueDictionary(new {
							controller = routeHandlerType.Name,
							action = "ProcessRequest",
							callbacks
						}),
						DataTokens = new RouteValueDictionary(new {
							namespaces = routeHandlerType.Namespace
						}),
						Constraints = new RouteValueDictionary(new {
							httpMethod = new HttpMethodConstraint(allowedMethods)
						})
				};

				foreach (Match m in Regex.Matches(url, "{(.*?)}")) {
					var optionalParam = m.Groups [1].Value;
					route.Defaults.Add (optionalParam, string.Empty);
				}

				routes.Add (route);
			}
		}
	}

	public class ExpressNetReq : Dictionary<string, object>
	{
		private HttpRequest httpRequest;

		public dynamic Params { get; private set; }

		public ExpressNetReq (HttpRequest httpRequest)
		{
			this.httpRequest = httpRequest;
			this.Params = new ExpandoObject ();
		}

		public void SetParams(IDictionary<string, object> values)
		{
			this.Params = new DynamicJsonObject(values);
		}
	}

	public class ExpressNetRes
	{
		private HttpResponse httpResponse;

		public ExpressNetRes (HttpResponse httpResponse)
		{
			this.httpResponse = httpResponse;
		}

		public void Send (object body)
		{
			if (body is int) {
				var status = (int)body;
				Send (status, GetStatusText (status));
			} else {
				httpResponse.ContentType = GetContentType (body);

				if (httpResponse.ContentType == "application/json") {
					var json = new JavaScriptSerializer ().Serialize (body);
					httpResponse.Write (json);
				} else {
					httpResponse.Write (body);
				}
			}
		}

		public void Send (int status, object body)
		{
			httpResponse.StatusCode = status;
			Send (body);
		}

		public void Set (string name, string value)
		{
			httpResponse.AppendHeader (name, value);
		}

		private string GetStatusText (int status)
		{
			switch (status) {
			case 200:
				return "OK";
			case 404:
				return "Not Found";
			default:
				return string.Empty;
			}
		}

		private string GetContentType (object body)
		{
			if (body is string) {
				return "text/html";
			}

			return "application/json";
		}
	}

	public delegate void ExpressNetAction (ExpressNetReq req, ExpressNetRes res);
}