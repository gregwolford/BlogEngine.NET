#region using

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;

#endregion

/// <summary>
/// Auto resolves URLs in the comments and turn them into valid hyperlinks.
/// </summary>
[Extension("Auto resolves URLs in the comments and turn them into valid hyperlinks.", "1.3", "BlogEngine.NET")]
public class ResolveLinks
{

	public ResolveLinks()
	{
		Comment.Serving += new EventHandler<ServingEventArgs>(Post_CommentServing);
	}

	/// <summary>
	/// The regular expression used to parse links.
	/// </summary>
	private static readonly Regex regex = new Regex("((http://|https://|www\\.)([A-Z0-9.\\-]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private const string link = "<a href=\"{0}{1}\" rel=\"nofollow\">{2}</a>";
	private const int MAX_LENGTH = 50;

	/// <summary>
	/// The event handler that is triggered every time a comment is served to a client.
	/// </summary>
	private void Post_CommentServing(object sender, ServingEventArgs e)
	{
		if (string.IsNullOrEmpty(e.Body))
			return;

		CultureInfo info = CultureInfo.InvariantCulture;

		foreach (Match match in regex.Matches(e.Body))
		{
			if (!match.Value.Contains("://"))
			{
				e.Body = e.Body.Replace(match.Value, string.Format(info, link, "http://", match.Value, ShortenUrl(match.Value, MAX_LENGTH)));
			}
			else
			{
				e.Body = e.Body.Replace(match.Value, string.Format(info, link, string.Empty, match.Value, ShortenUrl(match.Value, MAX_LENGTH)));
			}
		}
	}

	/// <summary>
	/// Shortens any absolute URL to a specified maximum length
	/// </summary>
	private static string ShortenUrl(string url, int max)
	{
		if (url.Length <= max)
			return url;

		// Remove the protocal
		int startIndex = url.IndexOf("://");
		if (startIndex > -1)
			url = url.Substring(startIndex + 3);

		if (url.Length <= max)
			return url;

		// Compress folder structure
		int firstIndex = url.IndexOf("/") + 1;
		int lastIndex = url.LastIndexOf("/");
		if (firstIndex < lastIndex)
		{
			url = url.Remove(firstIndex, lastIndex - firstIndex);
			url = url.Insert(firstIndex, "...");
		}

		if (url.Length <= max)
			return url;

		// Remove URL parameters
		int queryIndex = url.IndexOf("?");
		if (queryIndex > -1)
			url = url.Substring(0, queryIndex);

		if (url.Length <= max)
			return url;

		// Remove URL fragment
		int fragmentIndex = url.IndexOf("#");
		if (fragmentIndex > -1)
			url = url.Substring(0, fragmentIndex);

		if (url.Length <= max)
			return url;

		// Compress page
		firstIndex = url.LastIndexOf("/") + 1;
		lastIndex = url.LastIndexOf(".");
		if (lastIndex - firstIndex > 10)
		{
			string page = url.Substring(firstIndex, lastIndex - firstIndex);
			int length = url.Length - max + 3;
			url = url.Replace(page, "..." + page.Substring(length));
		}

		return url;
	}

}