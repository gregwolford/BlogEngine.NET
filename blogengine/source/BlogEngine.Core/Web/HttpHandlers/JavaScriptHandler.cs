#region Using

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Text;
using System.Web.Caching;

#endregion

namespace BlogEngine.Core.Web.HttpHandlers
{
	/// <summary>
	/// Removes whitespace in all stylesheets added to the 
	/// header of the HTML document in site.master. 
	/// </summary>
	public class JavaScriptHandler : IHttpHandler
	{
		/// <summary>
		/// Enables processing of HTTP Web requests by a custom 
		/// HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
		/// </summary>
		/// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides 
		/// references to the intrinsic server objects 
		/// (for example, Request, Response, Session, and Server) used to service HTTP requests.
		/// </param>
		public void ProcessRequest(HttpContext context)
		{
			string path = context.Request.QueryString["path"];
			string script = null;

			if (!string.IsNullOrEmpty(path))
			{
				if (context.Cache[path] == null)
				{
					if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase))
					{
						script = RetrieveRemoteScript(path);
					}
					else
					{
						script = RetrieveLocalScript(path);
					}
				}
			}

			script = (string)context.Cache[path];
			if (!string.IsNullOrEmpty(script))
			{
				script = StripWhitespace(script);
				context.Response.Write(script);
				SetHeaders(script.GetHashCode(), context);

				if (BlogSettings.Instance.EnableHttpCompression)
					Compress(context);
			}
		}

		/// <summary>
		/// Retrieves the local script from the disk
		/// </summary>
		private static string RetrieveLocalScript(string file)
		{
			if (!file.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
			{
				throw new System.Security.SecurityException("No access");
			}

			string path = HttpContext.Current.Server.MapPath(file);
			string script = null;
			
			using (StreamReader reader = new StreamReader(path))
			{
				script = reader.ReadToEnd();
				HttpContext.Current.Cache.Insert(file, script, new CacheDependency(path));
			}

			return script;
		}

		/// <summary>
		/// Retrieves the specified remote script using a WebClient.
		/// </summary>
		/// <param name="file">The remote URL</param>
		private static string RetrieveRemoteScript(string file)
		{
			string script = null;

			try
			{
				using (WebClient client = new WebClient())
				{
					client.Credentials = CredentialCache.DefaultNetworkCredentials;
					script = client.DownloadString(file);
					HttpContext.Current.Cache.Insert(file, script, null, Cache.NoAbsoluteExpiration, new TimeSpan(3, 0, 0, 0));
				}
			}
			catch (System.Net.Sockets.SocketException)
			{
				// The remote site is currently down. Try again next time.
			}

			return script;
		}

		/// <summary>
		/// Strips the whitespace from any .css file.
		/// </summary>
		private static string StripWhitespace(string body)
		{
			string[] lines = body.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			StringBuilder sb = new StringBuilder();
			foreach (string line in lines)
			{
				string s = line.Trim();
				if (s.Length > 0 && !s.StartsWith("//"))
					sb.AppendLine(s.Trim());
			}

			body = sb.ToString();
			body = Regex.Replace(body, @"^[\s]+|[ \f\r\t\v]+$", String.Empty);
			body = Regex.Replace(body, @"([+-])\n\1", "$1 $1");
			body = Regex.Replace(body, @"([^+-][+-])\n", "$1");
			body = Regex.Replace(body, @"([^+]) ?(\+)", "$1$2");
			body = Regex.Replace(body, @"(\+) ?([^+])", "$1$2");
			body = Regex.Replace(body, @"([^-]) ?(\-)", "$1$2");
			body = Regex.Replace(body, @"(\-) ?([^-])", "$1$2");
			body = Regex.Replace(body, @"\n([{}()[\],<>/*%&|^!~?:=.;+-])", "$1");
			body = Regex.Replace(body, @"(\W(if|while|for)\([^{]*?\))\n", "$1");
			body = Regex.Replace(body, @"(\W(if|while|for)\([^{]*?\))((if|while|for)\([^{]*?\))\n", "$1$3");
			body = Regex.Replace(body, @"([;}]else)\n", "$1 ");
			body = Regex.Replace(body, @"(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,}(?=&nbsp;)|(?<=&ndsp;)\s{2,}(?=[<])", String.Empty);

			return body;
		}

		/// <summary>
		/// This will make the browser and server keep the output
		/// in its cache and thereby improve performance.
		/// </summary>
		private static void SetHeaders(int hash, HttpContext context)
		{
			context.Response.ContentType = "text/javascript";
			context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;

			context.Response.Cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(7));
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetMaxAge(new TimeSpan(7, 0, 0, 0));
			context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			context.Response.Cache.SetETag("\"" + hash.ToString() + "\"");
		}

		#region Compression

		private const string GZIP = "gzip";
		private const string DEFLATE = "deflate";

		private static void Compress(HttpContext context)
		{
			if (context.Request.UserAgent != null && context.Request.UserAgent.Contains("MSIE 6"))
				return;

			if (IsEncodingAccepted(GZIP))
			{
				context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
				SetEncoding(GZIP);
			}
			else if (IsEncodingAccepted(DEFLATE))
			{
				context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
				SetEncoding(DEFLATE);
			}
		}

		/// <summary>
		/// Checks the request headers to see if the specified
		/// encoding is accepted by the client.
		/// </summary>
		private static bool IsEncodingAccepted(string encoding)
		{
			return HttpContext.Current.Request.Headers["Accept-encoding"] != null && HttpContext.Current.Request.Headers["Accept-encoding"].Contains(encoding);
		}

		/// <summary>
		/// Adds the specified encoding to the response headers.
		/// </summary>
		/// <param name="encoding"></param>
		private static void SetEncoding(string encoding)
		{
			HttpContext.Current.Response.AppendHeader("Content-encoding", encoding);
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"></see> instance.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Web.IHttpHandler"></see> instance is reusable; otherwise, false.</returns>
		public bool IsReusable
		{
			get { return false; }
		}

	}
}