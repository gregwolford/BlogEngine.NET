#region Using

using System;
using System.Collections.Generic;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;

#endregion

public partial class User_controls_PostList : System.Web.UI.UserControl
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (!Page.IsCallback)
		{
			BindPosts();
			InitPaging();
		}
	}

	/// <summary>
	/// Binds the list of posts to individual postview.ascx controls
	/// from the current theme. 
	/// </summary>
	private void BindPosts()
	{
		if (Posts == null || Posts.Count == 0)
		{
			hlPrev.Visible = false;
			return;
		}

		int count = BlogSettings.Instance.PostsPerPage;
		if (Posts.Count < count)
			count = Posts.Count;

		int page = GetPageIndex();
		int index = page * count;
		int stop = count;
		if (index + count > Posts.Count)
			stop = Posts.Count - index;

		if (stop < 0 || stop + index > Posts.Count)
		{
			hlPrev.Visible = false;
			hlNext.Visible = false;
			return;
		}

		string theme = BlogSettings.Instance.Theme;
		if (Request.QueryString["theme"] != null)
			theme = Request.QueryString["theme"];

		string path = Utils.RelativeWebRoot + "themes/" + theme + "/PostView.ascx";
		int counter = 0;

		foreach (Post post in Posts.GetRange(index, stop))
		{
			if (counter == stop)
				break;

			if (post.IsVisible || Page.User.Identity.IsAuthenticated)
			{
				PostViewBase postView = (PostViewBase)LoadControl(path);
				postView.ShowExcerpt = BlogSettings.Instance.ShowDescriptionInPostList;
				postView.Post = post;
				postView.Location = ServingLocation.PostList;
				posts.Controls.Add(postView);
				counter++;
			}
		}

		if (index + stop == Posts.Count)
			hlPrev.Visible = false;
	}

	/// <summary>
	/// Retrieves the current page index based on the QueryString.
	/// </summary>
	private int GetPageIndex()
	{
		int index = 0;
		if (int.TryParse(Request.QueryString["page"], out index))
			index--;

		return index;
	}

	/// <summary>
	/// Initializes the Next and Previous links
	/// </summary>
	private void InitPaging()
	{
		string path = Request.RawUrl.Replace("Default.aspx", string.Empty);

		if (path.Contains("?"))
		{
			if (path.Contains("page="))
			{
				int index = path.IndexOf("page=");
				path = path.Substring(0, index);
			}
			else
			{
				path += "&";
			}
		}
		else
		{
			path += "?";
		}

		int page = GetPageIndex();
		string url = path + "page={0}";

		if (page != 1)
			hlNext.HRef = string.Format(url, page);
		else
			hlNext.HRef = path.Replace("?", string.Empty);

		hlPrev.HRef = string.Format(url, page + 2);

		if (page == 0)
			hlNext.Visible = false;
		else
			(Page as BlogBasePage).AddGenericLink("next", "Next page", hlNext.HRef);

		if (hlPrev.Visible)
			(Page as BlogBasePage).AddGenericLink("prev", "Previous page", string.Format(url, page + 2));
	}

	#region Properties

	/// <summary>
	/// The list of posts to display.
	/// </summary>
	public List<Post> Posts
	{
		get { return (List<Post>)(ViewState["Posts"] ?? default(List<Post>)); }
		set { ViewState["Posts"] = value; }
	}

	#endregion

}
