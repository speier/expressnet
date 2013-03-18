using System;
using System.Web;
using ExpressNet;

namespace ExpressNetSample
{
	public class Global : HttpApplication
	{
		protected virtual void Application_Start (Object sender, EventArgs e)
		{
			var app = new Express ();

			app.Use (PoweredByExpressNet ());

			app.Get ("/", (req, res) => {
				res.Send ("hello world");
			});

			app.Get ("/json", (req, res) => {
				res.Send (new { name = "Express.NET", type = "Experiment" });
			});

			app.Get ("/user/{id}", LoadUser (), (req, res) => {
				var user = req["user"];
				res.Send (user);
			});
		}

		private ExpressNetAction PoweredByExpressNet ()
		{
			return (req, res) =>
			{
				res.Set ("X-Powered-By", "Express.NET");
			};
		}

		private ExpressNetAction LoadUser ()
		{
			return (req, res) =>
			{
				req["user"] = new { id = req.Params.id, name = "Kalman", age = 34 };
			};
		}
	}
}