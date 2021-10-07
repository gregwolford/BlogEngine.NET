#region Using

using System;
using System.Web;
using System.Text;
using System.IO;
using System.Web.Security;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Threading;
using BlogEngine.Core;

#endregion

public partial class admin_entry : System.Web.UI.Page, System.Web.UI.ICallbackEventHandler
{
	protected void Page_Load(object sender, EventArgs e)
	{
		this.MaintainScrollPositionOnPostBack = true;
		if (!Page.IsPostBack && !Page.IsCallback)
		{
			BindCategories();
			BindUsers();
			BindDrafts();

			Page.Title = Resources.labels.add_Entry;
			Page.ClientScript.GetCallbackEventReference(this, "title", "ApplyCallback", "slug");

			if (!String.IsNullOrEmpty(Request.QueryString["id"]) && Request.QueryString["id"].Length == 36)
			{
				Guid id = new Guid(Request.QueryString["id"]);
				Page.Title = "Edit post";
				BindPost(id);
			}
			else
			{
				PreSelectAuthor(Page.User.Identity.Name);
				txtDate.Text = DateTime.Now.AddHours(BlogSettings.Instance.Timezone).ToString("yyyy-MM-dd HH:mm");
				cbEnableComments.Checked = BlogSettings.Instance.IsCommentsEnabled;
				if (Session["content"] != null)
				{
					txtContent.Text = Session["content"].ToString();
					txtTitle.Text = Session["title"].ToString();
					txtDescription.Text = Session["description"].ToString();
					txtSlug.Text = Session["slug"].ToString();
					txtTags.Text = Session["tags"].ToString();
				}
				BindBookmarklet();
			}

			if (!Page.User.IsInRole("administrators"))
				ddlAuthor.Enabled = false;

			cbEnableComments.Enabled = BlogSettings.Instance.IsCommentsEnabled;
			if (!Utils.IsMono) Page.Form.DefaultButton = btnSave.UniqueID;
		}

		btnSave.Text = Resources.labels.savePost; // mono does not interpret the inline code correctly
		btnSave.Click += new EventHandler(btnSave_Click);
		btnCategory.Click += new EventHandler(btnCategory_Click);
		btnUploadFile.Click += new EventHandler(btnUploadFile_Click);
		btnUploadImage.Click += new EventHandler(btnUploadImage_Click);
		valExist.ServerValidate += new ServerValidateEventHandler(valExist_ServerValidate);
	}

	private void valExist_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = true;

		foreach (Category cat in Category.Categories)
		{
			if (cat.Title.Equals(txtCategory.Text.Trim(), StringComparison.OrdinalIgnoreCase))
				args.IsValid = false;
		}
	}

	private void btnUploadImage_Click(object sender, EventArgs e)
	{
		Upload(BlogSettings.Instance.StorageLocation + "files" + Path.DirectorySeparatorChar, txtUploadImage);
		string path = Utils.AbsoluteWebRoot.ToString();
		string img = string.Format("<img src=\"{0}image.axd?picture={1}\" alt=\"\" />", path, Server.UrlEncode(txtUploadImage.FileName));
		txtContent.Text += string.Format(img, txtUploadImage.FileName);
	}

	private void btnUploadFile_Click(object sender, EventArgs e)
	{
		Upload(BlogSettings.Instance.StorageLocation + "files" + Path.DirectorySeparatorChar, txtUploadFile);

		string a = "<p><a href=\"{0}file.axd?file={1}\" rel=\"enclosure\">{2}</a></p>";
		string text = txtUploadFile.FileName + " (" + SizeFormat(txtUploadFile.FileBytes.Length, "N") + ")";
		txtContent.Text += string.Format(a, Utils.RelativeWebRoot, Server.UrlEncode(txtUploadFile.FileName), text);
	}

	private void Upload(string virtualFolder, FileUpload control)
	{
		string folder = Server.MapPath(virtualFolder);
		control.PostedFile.SaveAs(folder + control.FileName);
	}

	private string SizeFormat(float size, string formatString)
	{
		if (size < 1024)
			return size.ToString(formatString) + " bytes";

		if (size < Math.Pow(1024, 2))
			return (size / 1024).ToString(formatString) + " kb";

		if (size < Math.Pow(1024, 3))
			return (size / Math.Pow(1024, 2)).ToString(formatString) + " mb";

		if (size < Math.Pow(1024, 4))
			return (size / Math.Pow(1024, 3)).ToString(formatString) + " gb";

		return size.ToString(formatString);
	}

	#region Event handlers

	/// <summary>
	/// Creates and saves a new category
	/// </summary>
	private void btnCategory_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			Category cat = new Category(txtCategory.Text, string.Empty);
			cat.Save();
			ListItem item = new ListItem(Server.HtmlEncode(txtCategory.Text), cat.Id.ToString());
			item.Selected = true;
			cblCategories.Items.Add(item);
		}
	}

	/// <summary>
	/// Saves the post
	/// </summary>
	private void btnSave_Click(object sender, EventArgs e)
	{
		if (!Page.IsValid)
			throw new InvalidOperationException("One or more validators are invalid.");

		Post post;
		if (Request.QueryString["id"] != null)
			post = Post.GetPost(new Guid(Request.QueryString["id"]));
		else
			post = new Post();

		if (string.IsNullOrEmpty(txtContent.Text))
			txtContent.Text = "[No text]";

		post.DateCreated = DateTime.ParseExact(txtDate.Text, "yyyy-MM-dd HH:mm", null).AddHours(-BlogSettings.Instance.Timezone);
		post.Author = ddlAuthor.SelectedValue;
		post.Title = txtTitle.Text.Trim();
		post.Content = txtContent.Text;
		post.Description = txtDescription.Text.Trim();
		post.IsPublished = cbPublish.Checked;
		post.IsCommentsEnabled = cbEnableComments.Checked;

		if (!string.IsNullOrEmpty(txtSlug.Text))
			post.Slug = Server.UrlDecode(txtSlug.Text.Trim());

		post.Categories.Clear();

		foreach (ListItem item in cblCategories.Items)
		{
			if (item.Selected)
				post.Categories.Add(Category.GetCategory(new Guid(item.Value)));
		}

		post.Tags.Clear();
		if (txtTags.Text.Trim().Length > 0)
		{
			string[] tags = txtTags.Text.Split(',');
			foreach (string tag in tags)
			{
				post.Tags.Add(tag.Trim().ToLowerInvariant());
			}
		}

		post.Save();

		Session.Remove("content");
		Session.Remove("title");
		Session.Remove("description");
		Session.Remove("slug");
		Session.Remove("tags");
		Response.Redirect(post.RelativeLink.ToString());
	}

	#endregion

	#region Data binding

	private void BindCategories()
	{
		foreach (Category cat in Category.Categories)
		{
			cblCategories.Items.Add(new ListItem(Server.HtmlEncode(cat.Title), cat.Id.ToString()));
		}
	}

	private void BindPost(Guid postId)
	{
		Post post = Post.GetPost(postId);
		txtTitle.Text = post.Title;
		txtContent.Text = post.Content;
		txtDescription.Text = post.Description;
		txtDate.Text = post.DateCreated.ToString("yyyy-MM-dd HH:mm");
		cbEnableComments.Checked = post.IsCommentsEnabled;
		cbPublish.Checked = post.IsPublished;
		txtSlug.Text = Utils.RemoveIllegalCharacters(post.Slug);

		PreSelectAuthor(post.Author);

		foreach (Category cat in post.Categories)
		{
			ListItem item = cblCategories.Items.FindByValue(cat.Id.ToString());
			if (item != null)
				item.Selected = true;
		}

		string[] tags = new string[post.Tags.Count];
		for (int i = 0; i < post.Tags.Count; i++)
		{
			tags[i] = post.Tags[i];
		}
		txtTags.Text = string.Join(",", tags);
	}

	private void PreSelectAuthor(string author)
	{
		ddlAuthor.ClearSelection();
		foreach (ListItem item in ddlAuthor.Items)
		{
			if (item.Text.Equals(author, StringComparison.OrdinalIgnoreCase))
			{
				item.Selected = true;
				break;
			}
		}
	}

	private void BindBookmarklet()
	{
		if (Request.QueryString["title"] != null && Request.QueryString["url"] != null)
		{
			string title = Request.QueryString["title"];
			string url = Request.QueryString["url"];

			txtTitle.Text = title;
			txtContent.Text = string.Format("<p><a href=\"{0}\" title=\"{1}\">{1}</a></p>", url, title);
		}
	}

	private void BindUsers()
	{
		foreach (MembershipUser user in Membership.GetAllUsers())
		{
			ddlAuthor.Items.Add(user.UserName);
		}
	}

	private void BindDrafts()
	{
		Guid id = Guid.Empty;
		if (!String.IsNullOrEmpty(Request.QueryString["id"]) && Request.QueryString["id"].Length == 36)
		{
			id = new Guid(Request.QueryString["id"]);
		}

		int counter = 0;

		foreach (Post post in Post.Posts)
		{
			if (!post.IsPublished && post.Id != id)
			{
				HtmlGenericControl li = new HtmlGenericControl("li");
				HtmlAnchor a = new HtmlAnchor();
				a.HRef = "?id=" + post.Id.ToString();
				a.InnerHtml = post.Title;

				System.Web.UI.LiteralControl text = new System.Web.UI.LiteralControl(" by " + post.Author + " (" + post.DateCreated.ToString("yyyy-dd-MM HH:mm") + ")");

				li.Controls.Add(a);
				li.Controls.Add(text);
				ulDrafts.Controls.Add(li);
				counter++;
			}
		}

		if (counter > 0)
		{
			divDrafts.Visible = true;
			aDrafts.InnerHtml = string.Format(Resources.labels.thereAreXDrafts, counter);
		}
	}

	#endregion


	#region ICallbackEventHandler Members

	private string _Callback;

	public string GetCallbackResult()
	{
		return _Callback;
	}

	public void RaiseCallbackEvent(string eventArgument)
	{
		if (eventArgument.StartsWith("_autosave"))
		{
			string[] fields = eventArgument.Replace("_autosave", string.Empty).Split(new string[] { ";|;" }, StringSplitOptions.None);
			Session["content"] = fields[0];
			Session["title"] = fields[1];
			Session["description"] = fields[2];
			Session["slug"] = fields[3];
			Session["tags"] = fields[4];
		}
		else
		{
			_Callback = Utils.RemoveIllegalCharacters(eventArgument.Trim());
		}
	}

	#endregion
}
