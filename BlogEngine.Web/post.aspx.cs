#region Using

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;

#endregion

public partial class post : BlogEngine.Core.Web.Controls.BlogBasePage
{
	protected void Page_Init(object sender, EventArgs e)
	{
		if (!Page.IsPostBack && !Page.IsCallback)
		{
			if (Request.RawUrl.Contains("?id=") && Request.QueryString["id"].Length == 36)
			{
				Guid id = new Guid(Request.QueryString["id"]);
				Post post = Post.GetPost(id);
				if (post != null)
				{
					Response.Clear();
					Response.StatusCode = 301;
					Response.AppendHeader("location", post.RelativeLink.ToString());
					Response.End();
				}
			}
		}

		if (Request.QueryString["id"] != null && Request.QueryString["id"].Length == 36)
		{
			Guid id = new Guid(Request.QueryString["id"]);
			this.Post = Post.GetPost(id);

			if (Post != null)
			{
				if (!this.Post.IsVisible && !Page.User.Identity.IsAuthenticated)
					Response.Redirect(Utils.RelativeWebRoot + "error404.aspx", true);

				string path = Utils.RelativeWebRoot + "themes/" + BlogSettings.Instance.Theme + "/PostView.ascx";

				PostViewBase postView = (PostViewBase)LoadControl(path);
				postView.Post = Post;
				postView.Location = ServingLocation.SinglePost;

				pwPost.Controls.Add(postView);
				related.Item = this.Post;
				CommentView1.Post = Post;

				Page.Title = Server.HtmlEncode(Post.Title);
				AddMetaKeywords();
				AddMetaDescription();
				AddGenericLink("last", Post.Posts[0].Title, Post.Posts[0].RelativeLink.ToString());
				AddGenericLink("first", Post.Posts[Post.Posts.Count - 1].Title, Post.Posts[Post.Posts.Count - 1].RelativeLink.ToString());

				InitNavigationLinks();

				phRDF.Visible = BlogSettings.Instance.EnableTrackBackReceive;

				if (BlogSettings.Instance.EnablePingBackReceive)
					Response.AppendHeader("x-pingback", "http://" + Request.Url.Authority + Utils.RelativeWebRoot + "pingback.axd");
			}
		}
		else
		{
			Response.Redirect(Utils.RelativeWebRoot + "error404.aspx", true);
		}
	}

	/// <summary>
	/// Gets the next post filtered for invisible posts.
	/// </summary>
	private Post GetNextPost(Post post)
	{
		if (post.Next == null)
			return null;

		if (post.Next.IsVisible || Page.User.IsInRole("administrators") || Page.User.Identity.Name == post.Next.Author)
			return post.Next;

		return GetNextPost(post.Next);
	}

	/// <summary>
	/// Gets the prev post filtered for invisible posts.
	/// </summary>
	private Post GetPrevPost(Post post)
	{
		if (post.Previous == null)
			return null;

		if (post.Previous.IsVisible || Page.User.IsInRole("administrators") || Page.User.Identity.Name == post.Previous.Author)
			return post.Previous;

		return GetPrevPost(post.Previous);
	}

	/// <summary>
	/// Inits the navigation links above the post and in the HTML head section.
	/// </summary>
	private void InitNavigationLinks()
	{
		if (BlogSettings.Instance.ShowPostNavigation)
		{
			Post next = GetNextPost(Post);
			Post prev = GetPrevPost(Post);

			if (next != null)
			{
				hlNext.NavigateUrl = next.RelativeLink;
				hlNext.Text = Server.HtmlEncode(next.Title + " >>");
				hlNext.ToolTip = Resources.labels.nextPost;
				base.AddGenericLink("next", Post.Next.Title, Post.Next.RelativeLink.ToString());
			}

			if (prev != null)
			{
				hlPrev.NavigateUrl = prev.RelativeLink;
				hlPrev.Text = Server.HtmlEncode("<< " + prev.Title);
				hlPrev.ToolTip = Resources.labels.previousPost;
				base.AddGenericLink("prev", Post.Previous.Title, Post.Previous.RelativeLink.ToString());
			}

			phPostNavigation.Visible = true;
		}
	}

	/// <summary>
	/// Adds the post's description as the description metatag.
	/// </summary>
	private void AddMetaDescription()
	{
		if (!string.IsNullOrEmpty(Post.Description))
			base.AddMetaTag("description", Server.HtmlEncode(Post.Description));
		else
			base.AddMetaTag("description", BlogSettings.Instance.Description);
	}

	/// <summary>
	/// Adds the post's tags as meta keywords.
	/// </summary>
	private void AddMetaKeywords()
	{
		if (Post.Tags.Count > 0)
		{
			string[] tags = new string[Post.Tags.Count];
			for (int i = 0; i < Post.Tags.Count; i++)
			{
				tags[i] = Post.Tags[i];
			}
			base.AddMetaTag("keywords", Server.HtmlEncode(string.Join(",", tags)));
		}
	}

	public Post Post;
}
