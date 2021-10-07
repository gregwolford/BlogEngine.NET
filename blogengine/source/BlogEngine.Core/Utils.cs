#region Using

using System;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Globalization;
using System.Web;
using System.Threading;

#endregion

namespace BlogEngine.Core
{
	/// <summary>
	/// Utilities for the entire solution to use.
	/// </summary>
	public static class Utils
	{

		/// <summary>
		/// Strips all illegal characters from the specified title.
		/// </summary>
		public static string RemoveIllegalCharacters(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			text = text.Replace(":", string.Empty);
			text = text.Replace("/", string.Empty);
			text = text.Replace("?", string.Empty);
			text = text.Replace("#", string.Empty);
			text = text.Replace("[", string.Empty);
			text = text.Replace("]", string.Empty);
			text = text.Replace("@", string.Empty);
			text = text.Replace(".", string.Empty);
			text = text.Replace("\"", string.Empty);
			text = text.Replace("&", string.Empty);
			text = text.Replace("'", string.Empty);
			text = text.Replace(" ", "-");
			text = RemoveDiacritics(text);

			return HttpUtility.UrlEncode(text).Replace("%", string.Empty);
		}

		private static String RemoveDiacritics(string text)
		{
			String normalized = text.Normalize(NormalizationForm.FormD);
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < normalized.Length; i++)
			{
				Char c = normalized[i];
				if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
					sb.Append(c);
			}

			return sb.ToString();
		}

		#region URL handling

		/// <summary>
		/// Gets the relative URL of the blog feed. If a Feedburner username
		/// is entered in the admin settings page, it will return the 
		/// absolute Feedburner URL to the feed.
		/// </summary>
		public static string FeedUrl
		{
			get
			{
				if (!string.IsNullOrEmpty(BlogSettings.Instance.AlternateFeedUrl))
					return BlogSettings.Instance.AlternateFeedUrl;
				else
					return AbsoluteWebRoot + "syndication.axd";
			}
		}

		/// <summary>
		/// Gets the relative root of the website.
		/// </summary>
		/// <value>A string that ends with a '/'.</value>
		public static string RelativeWebRoot
		{
			get { return VirtualPathUtility.ToAbsolute(ConfigurationManager.AppSettings["BlogEngine.VirtualPath"]); }
		}

		private static Uri _AbsoluteWebRoot;

		/// <summary>
		/// Gets the absolute root of the website.
		/// </summary>
		/// <value>A string that ends with a '/'.</value>
		public static Uri AbsoluteWebRoot
		{
			get
			{
				if (_AbsoluteWebRoot == null)
				{
					HttpContext context = HttpContext.Current;
					if (context == null)
						throw new System.Net.WebException("The current HttpContext is null");

					_AbsoluteWebRoot = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority + RelativeWebRoot);
				}
				return _AbsoluteWebRoot;
			}
		}

		/// <summary>
		/// Converts a relative URL to an absolute one.
		/// </summary>
		public static Uri ConvertToAbsolute(Uri relativeUri)
		{
			return ConvertToAbsolute(relativeUri.ToString()); ;
		}

		/// <summary>
		/// Converts a relative URL to an absolute one.
		/// </summary>
		public static Uri ConvertToAbsolute(string relativeUri)
		{
			if (String.IsNullOrEmpty(relativeUri))
				throw new ArgumentNullException("relativeUri");

			string absolute = AbsoluteWebRoot.ToString();
			int index = absolute.LastIndexOf(RelativeWebRoot.ToString());

			return new Uri(absolute.Substring(0, index) + relativeUri);
		}

		#endregion

		#region Is mobile device

		private static readonly Regex MOBILE_REGEX = new Regex(ConfigurationManager.AppSettings.Get("BlogEngine.MobileDevices"), RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Gets a value indicating whether the client is a mobile device.
		/// </summary>
		/// <value><c>true</c> if this instance is mobile; otherwise, <c>false</c>.</value>
		public static bool IsMobile
		{
			get
			{
				HttpContext context = HttpContext.Current;
				if (context != null)
				{
					HttpRequest request = context.Request;
					if (request.Browser.IsMobileDevice)
						return true;

					if (!string.IsNullOrEmpty(request.UserAgent) && MOBILE_REGEX.IsMatch(request.UserAgent))
						return true;
				}

				return false;
			}
		}

		#endregion

		#region Is Mono/Linux

		private static int mono = 0;
		/// <summary>
		/// Gets a value indicating whether we're running under Mono.
		/// </summary>
		/// <value><c>true</c> if Mono; otherwise, <c>false</c>.</value>
		public static bool IsMono
		{
			get
			{
				if (mono == 0)
				{
					if (Type.GetType("Mono.Runtime") != null)
						mono = 1;
					else
						mono = 2;
				}

				return mono == 1;
			}
		}

		/// <summary>
		/// Gets a value indicating whether we're running under Linux or a Unix variant.
		/// </summary>
		/// <value><c>true</c> if Linux/Unix; otherwise, <c>false</c>.</value>
		public static bool IsLinux
		{
			get
			{
				int p = (int)Environment.OSVersion.Platform;
				return ((p == 4) || (p == 128));
			}
		}

		#endregion

		#region Send e-mail

		/// <summary>
		/// Sends a MailMessage object using the SMTP settings.
		/// </summary>
		public static void SendMailMessage(MailMessage message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			try
			{
				message.IsBodyHtml = true;
				message.BodyEncoding = Encoding.UTF8;
				SmtpClient smtp = new SmtpClient(BlogSettings.Instance.SmtpServer);
				smtp.Credentials = new System.Net.NetworkCredential(BlogSettings.Instance.SmtpUserName, BlogSettings.Instance.SmtpPassword);
				smtp.Port = BlogSettings.Instance.SmtpServerPort;
				smtp.EnableSsl = BlogSettings.Instance.EnableSsl;
				smtp.Send(message);
				OnEmailSent(message);
			}
			catch (SmtpException)
			{
				OnEmailFailed(message);
			}
			finally
			{
				// Remove the pointer to the message object so the GC can close the thread.
				message.Dispose();
				message = null;
			}
		}

		/// <summary>
		/// Sends the mail message asynchronously in another thread.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public static void SendMailMessageAsync(MailMessage message)
		{
			//ThreadStart threadStart = delegate { Utils.SendMailMessage(message); };
			//Thread thread = new Thread(threadStart);
			//thread.IsBackground = true;
			//thread.Start();
			ThreadPool.QueueUserWorkItem(delegate { Utils.SendMailMessage(message); });
		}

		/// <summary>
		/// Occurs after an e-mail has been sent. The sender is the MailMessage object.
		/// </summary>
		public static event EventHandler<EventArgs> EmailSent;
		private static void OnEmailSent(MailMessage message)
		{
			if (EmailSent != null)
			{
				EmailSent(message, new EventArgs());
			}
		}

		/// <summary>
		/// Occurs after an e-mail has been sent. The sender is the MailMessage object.
		/// </summary>
		public static event EventHandler<EventArgs> EmailFailed;
		private static void OnEmailFailed(MailMessage message)
		{
			if (EmailFailed != null)
			{
				EmailFailed(message, new EventArgs());
			}
		}

		#endregion

	}
}
