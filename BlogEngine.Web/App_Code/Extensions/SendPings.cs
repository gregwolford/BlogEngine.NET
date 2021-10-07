#region using

using System;
using System.Web;
using BlogEngine.Core.Web.Controls;
using BlogEngine.Core;
using System.Net.Mail;
using System.Threading;

#endregion

/// <summary>
/// Pings all the ping services specified on the 
/// PingServices admin page and send track- and pingbacks
/// </summary>
[Extension("Pings all the ping services specified on the PingServices admin page and send track- and pingbacks", "1.3", "BlogEngine.NET")]
public class SendPings
{

  /// <summary>
  /// Hooks up an event handler to the Post.Saved event.
  /// </summary>
  public SendPings()
  {
    Post.Saved += new EventHandler<SavedEventArgs>(Post_Saved);
		Page.Saved += new EventHandler<SavedEventArgs>(Post_Saved);
  }

  /// <summary>
  /// Sends the pings in a new thread.
  /// <remarks>
  /// It opens a new thread and executes the pings from there,
  /// because it takes some time to complete.
  /// </remarks>
  /// </summary>
  private void Post_Saved(object sender, SavedEventArgs e)
  {
		if (e.Action == SaveAction.None || e.Action == SaveAction.Delete)
			return;

    IPublishable item = (IPublishable)sender;

		if (!BlogSettings.Instance.EnableTrackBackSend && !BlogSettings.Instance.EnablePingBackSend)
			return;
		
		if ((HttpContext.Current == null || !HttpContext.Current.Request.IsLocal) && item.IsVisible)
    {
			ThreadPool.QueueUserWorkItem(delegate { Ping(item); });
    }
  }

  /// <summary>
  /// Executes the pings from the new thread.
  /// </summary>
  private void Ping(IPublishable item)
  {
		try
		{
			System.Threading.Thread.Sleep(2000);

			// Ping the specified ping services.
			BlogEngine.Core.Ping.PingService.Send();

			// Send trackbacks and pingbacks.
			if (item.Content.ToLowerInvariant().Contains("http"))
				BlogEngine.Core.Ping.Manager.Send(item);
		}
		catch (Exception)
		{
			// We need to catch this exception so the application doesn't get killed.
		}
  }

}
