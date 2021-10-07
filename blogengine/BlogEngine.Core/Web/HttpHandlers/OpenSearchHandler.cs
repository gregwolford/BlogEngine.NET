#region Using

using System;
using System.Xml;
using System.Web;
using BlogEngine.Core;

#endregion

namespace BlogEngine.Core.Web.HttpHandlers
{
  /// <summary>
  /// Displays the open search XML provider as
  /// descriped at http://opensearch.a9.com/
  /// </summary>
  /// <remarks>
  /// The OpenSearch document needs to be linked to from the 
  /// HTML head tag. This link will be added automatically.
  /// </remarks>
  public class OpenSearchHandler : IHttpHandler
  {

    /// <summary>
    /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"></see> interface.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext"></see> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    public void ProcessRequest(HttpContext context)
    {
      using (XmlWriter writer = XmlWriter.Create(context.Response.OutputStream))
      {
        writer.WriteStartElement("OpenSearchDescription", "http://a9.com/-/spec/opensearch/1.1/");

        writer.WriteElementString("ShortName", BlogSettings.Instance.Name);
        writer.WriteElementString("Description", BlogSettings.Instance.Description);

        writer.WriteRaw("<Image height=\"16\" width=\"16\" type=\"image/vnd.microsoft.icon\">" + Utils.AbsoluteWebRoot.ToString() + "pics/favicon.ico</Image>");

        writer.WriteStartElement("Url");
        writer.WriteAttributeString("type", "text/html");
        writer.WriteAttributeString("template", Utils.AbsoluteWebRoot.ToString() + "search.aspx?q={searchTerms}");

        writer.WriteStartElement("Url");
        writer.WriteAttributeString("type", "application/rss+xml");
        writer.WriteAttributeString("template", Utils.AbsoluteWebRoot.ToString() + "syndication.axd?q={searchTerms}");

        writer.WriteEndElement();
      }

      context.Response.ContentType = "text/xml";
      context.Response.Cache.SetValidUntilExpires(true);
      context.Response.Cache.SetExpires(DateTime.Now.AddDays(30));
      context.Response.Cache.SetCacheability(HttpCacheability.Public);
      context.Response.Cache.SetLastModified(DateTime.Now);
      context.Response.Cache.SetETag(Guid.NewGuid().ToString());

      BlogSettings.Changed += delegate { context.Response.Cache.SetExpires(DateTime.Now.AddDays(30)); };
    }

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