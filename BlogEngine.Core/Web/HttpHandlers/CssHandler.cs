#region Using

using System;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Web.Caching;

#endregion

namespace BlogEngine.Core.Web.HttpHandlers
{
	/// <summary>
	/// Removes whitespace in all stylesheets added to the 
	/// header of the HTML document in site.master. 
	/// </summary>
	public class CssHandler : IHttpHandler
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
			string file = context.Server.MapPath(Utils.RelativeWebRoot + "themes/" + BlogSettings.Instance.Theme + "/" + context.Request.QueryString["name"]);
			ReduceCss(file, context);
			SetHeaders(file, context);

			if (BlogSettings.Instance.EnableHttpCompression)
				Compress(context);
		}

		/// <summary>
		/// Removes all unwanted text from the CSS file,
		/// including comments and whitespace.
		/// </summary>
		private static void ReduceCss(string file, HttpContext context)
		{
			if (!file.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
			{
				throw new System.Security.SecurityException("No access");
			}

			if (context.Cache[file + "date"] == null)
			{
				using (StreamReader reader = new StreamReader(file))
				{
					string body = StripWhitespace(reader);
					context.Cache.Insert(file, body, new CacheDependency(file));
					context.Cache.Insert(file + "date", File.GetLastWriteTime(file), new CacheDependency(file));
				}
			}

			context.Response.Write((string)context.Cache[file]);
		}

		/// <summary>
		/// Strips the whitespace from any .css file.
		/// </summary>
		private static string StripWhitespace(StreamReader reader)
		{
			string body = reader.ReadToEnd();

			body = body.Replace("  ", String.Empty);
			body = body.Replace(Environment.NewLine, String.Empty);
			body = body.Replace("\t", string.Empty);
			body = body.Replace(" {", "{");
			body = body.Replace(" :", ":");
			body = body.Replace(": ", ":");
			body = body.Replace(", ", ",");
			body = body.Replace("; ", ";");
			body = body.Replace(";}", "}");
			//body = Regex.Replace(body, @"/\*[^\*]*\*+([^/\*]*\*+)*/", "$1");
			body = Regex.Replace(body, @"(?<=[>])\s{2,}(?=[<])|(?<=[>])\s{2,}(?=&nbsp;)|(?<=&ndsp;)\s{2,}(?=[<])", String.Empty);

			return body;
		}

		/// <summary>
		/// This will make the browser and server keep the output
		/// in its cache and thereby improve performance.
		/// </summary>
		private static void SetHeaders(string file, HttpContext context)
		{
			context.Response.ContentType = "text/css";
			context.Response.Cache.VaryByHeaders["Accept-Encoding"] = true;

			DateTime date = DateTime.Now;
			if (context.Cache[file + "date"] != null)
				date = (DateTime)context.Cache[file + "date"];

			string etag = "\"" + date.GetHashCode() + "\"";
			string incomingEtag = context.Request.Headers["If-None-Match"];

			context.Response.Cache.SetExpires(DateTime.Now.ToUniversalTime().AddDays(7));
			context.Response.Cache.SetCacheability(HttpCacheability.Public);
			context.Response.Cache.SetMaxAge(new TimeSpan(7, 0, 0, 0));
			context.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			context.Response.Cache.SetETag(etag);

			if (String.Compare(incomingEtag, etag) == 0)
			{
				context.Response.StatusCode = (int)HttpStatusCode.NotModified;
				context.Response.End();
			}
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