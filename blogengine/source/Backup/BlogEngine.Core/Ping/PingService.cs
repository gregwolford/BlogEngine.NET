#region Using

using System;
using System.Net;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using BlogEngine.Core;
using BlogEngine.Core.Providers;

#endregion

namespace BlogEngine.Core.Ping
{
  /// <summary>
  /// Pings various blog ping services.
  /// <remarks>
  /// Whenever a post is created or updated, it is important
  /// to notify the ping services so that they have the latest
  /// updated posts from the blog.
  /// </remarks>
  /// </summary>
  public static class PingService
  {
    /// <summary>
    /// Sends a ping to various ping services asynchronously.
    /// </summary>
    public static void Send()
    {
      StringCollection services = BlogService.LoadPingServices();
      foreach (string service in services)
      {
        Execute(service);
      }
    }

    /// <summary>
    /// Creates a web request and invokes the response asynchronous.
    /// </summary>
    private static void Execute(string url)
    {
      try
      {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "text/xml";
        request.Timeout = 3000;
        request.Credentials = CredentialCache.DefaultNetworkCredentials;

        AddXmlToRequest(request);
        request.GetResponse();
      }
      catch (Exception)
      {
        // Log the error.
      }
    }

    /// <summary>
    /// Adds the XML to web request. The XML is the standard
    /// XML used by RPC-XML requests.
    /// </summary>
    private static void AddXmlToRequest(HttpWebRequest request)
    {
      Stream stream = (Stream)request.GetRequestStream();
      using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.ASCII))
      {
        writer.WriteStartDocument();
        writer.WriteStartElement("methodCall");
        writer.WriteElementString("methodName", "weblogUpdates.ping");
        writer.WriteStartElement("params");
        writer.WriteStartElement("param");
        writer.WriteElementString("value", BlogSettings.Instance.Name);
        writer.WriteEndElement();
        writer.WriteStartElement("param");
        writer.WriteElementString("value", Utils.AbsoluteWebRoot.ToString());
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
      }
    }
  }
}