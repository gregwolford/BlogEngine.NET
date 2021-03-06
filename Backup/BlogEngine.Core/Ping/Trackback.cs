#region Using

using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;

#endregion

namespace BlogEngine.Core.Ping
{
  /// <summary>
  /// 
  /// </summary>
  public static class Trackback
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>    
    public static bool Send(TrackbackMessage message)
    {
			if (!BlogSettings.Instance.EnableTrackBackSend)
				return false;

      if (message == null)
        throw new ArgumentNullException("message");

      OnSending(message.UrlToNotifyTrackback);
      //Warning:next line if for local debugging porpuse please donot remove it until you need to
      //tMessage.PostURL = new Uri("http://www.artinsoft.com/webmaster/trackback.html");
      HttpWebRequest request = (HttpWebRequest)System.Net.HttpWebRequest.Create(message.UrlToNotifyTrackback); //HttpHelper.CreateRequest(trackBackItem);
      request.Credentials = CredentialCache.DefaultNetworkCredentials;
      request.Method = "POST";
      request.ContentLength = message.ToString().Length;
      request.ContentType = "application/x-www-form-urlencoded";
      request.KeepAlive = false;
      request.Timeout = 10000;

      using (StreamWriter myWriter = new StreamWriter(request.GetRequestStream()))
      {
        myWriter.Write(message.ToString());
      }

      bool result = false;
      HttpWebResponse response;
      try
      {
        response = (HttpWebResponse)request.GetResponse();
        OnSent(message.UrlToNotifyTrackback);
        string answer;
        using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream()))
        {
          answer = sr.ReadToEnd();
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
          //todo:This could be a strict XML parsing if necesary/maybe logging activity here too
          if (answer.Contains("<error>0</error>"))
          {
            result = true;
          }
          else
          {
            result = false;
          }
        }
        else
        {
          result = false;
        }
      }
      catch //(WebException wex)
      {
        result = false;
      }
      return result;
    }

    #region

    /// <summary>
    /// Occurs just before a trackback is sent.
    /// </summary>
    public static event EventHandler<EventArgs> Sending;
    private static void OnSending(Uri url)
    {
      if (Sending != null)
      {
        Sending(url, new EventArgs());
      }
    }

    /// <summary>
    /// Occurs when a trackback has been sent
    /// </summary>
    public static event EventHandler<EventArgs> Sent;
    private static void OnSent(Uri url)
    {
      if (Sent != null)
      {
        Sent(url, new EventArgs());
      }
    }

    #endregion

  }

  /// <summary>
  /// 
  /// </summary>
  public class TrackbackMessage
  {
    /// <summary>
    /// 
    /// </summary>
    private string _Title;

    /// <summary>
    /// 
    /// </summary>
    public string Title
    {
      get { return _Title; }
      set { _Title = value; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    private Uri _PostUrl;

    /// <summary>
    /// 
    /// </summary>
    public Uri PostUrl
    {
      get { return _PostUrl; }
      set { _PostUrl = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    private string _Excerpt;

    /// <summary>
    /// 
    /// </summary>
    public string Excerpt
    {
      get { return _Excerpt; }
      set { _Excerpt = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    private string _BlogName;

    /// <summary>
    /// 
    /// </summary>
    public string BlogName
    {
      get { return _BlogName; }
      set { _BlogName = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    private Uri _UrlToNotifyTrackback;

    /// <summary>
    /// 
    /// </summary>
    public Uri UrlToNotifyTrackback
    {
      get { return _UrlToNotifyTrackback; }
      set { _UrlToNotifyTrackback = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="post"></param>
    /// <param name="urlToNotifyTrackback"></param>
    public TrackbackMessage(IPublishable item, Uri urlToNotifyTrackback)
    {
      if (item == null)
        throw new ArgumentNullException("post");

      Title = item.Title;
      PostUrl = item.AbsoluteLink;
      Excerpt = item.Title;
      BlogName = BlogSettings.Instance.Name;
      UrlToNotifyTrackback = urlToNotifyTrackback;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (string.IsNullOrEmpty(UrlToNotifyTrackback.Query))
      {
        return String.Format(CultureInfo.InvariantCulture, "?title={0}&url={1}&excerpt={2}&blog_name={3}", Title, PostUrl, Excerpt, BlogName);
      }
      else
      {
        return String.Format(CultureInfo.InvariantCulture, "&title={0}&url={1}&excerpt={2}&blog_name={3}", Title, PostUrl, Excerpt, BlogName);
      }
    }

  }
}