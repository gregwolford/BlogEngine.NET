#region Using

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using BlogEngine.Core;

#endregion

namespace BlogEngine.Core.Web.Controls
{

	/// <summary>
	/// Inherit from this class when you are building the
	/// commentview.ascx user control in your custom theme.
	/// </summary>
	/// <remarks>
	/// The class exposes a lot of functionality to the custom
	/// comment control in the theme folder.
	/// </remarks>
	public class CommentViewBase : UserControl
	{

		#region Properties

		private Post _Post;

		/// <summary>
		/// Gets or sets the Post from which the comment belongs.
		/// </summary>
		/// <value>The Post object.</value>
		public Post Post
		{
			get { return _Post; }
			set { _Post = value; }
		}

		private Comment _Comment;

		/// <summary>
		/// Gets or sets the Comment.
		/// </summary>
		/// <value>The comment.</value>
		public Comment Comment
		{
			get { return _Comment; }
			set { _Comment = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// The regular expression used to parse links.
		/// </summary>
		private static readonly Regex regex = new Regex("((http://|www\\.)([A-Z0-9.-]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private const string link = "<a href=\"{0}{1}\" rel=\"nofollow\">{2}</a>";

		/// <summary>
		/// Examins the comment body for any links and turns them
		/// automatically into one that can be clicked.
		/// </summary>
		[Obsolete("Use the Text property instead. This method will be removed in a future version.")]
		protected string ResolveLinks(string body)
		{
			return Text;
		}

		/// <summary>
		/// Gets the text of the comment.
		/// </summary>
		/// <value>The text.</value>
		public string Text
		{
			get
			{
				ServingEventArgs arg = new ServingEventArgs(Comment.Content, ServingLocation.SinglePost);
				Comment.OnServing(Comment, arg);

				return arg.Body.Replace("\n", "<br />"); ;
			}
		}

		/// <summary>
		/// Displays a delete link to visitors that are authenticated
		/// using the default membership provider.
		/// </summary>
		protected string AdminLinks
		{
			get
			{
				if (Page.User.IsInRole("administrators") || Page.User.Identity.Name.Equals(Post.Author))
				{
					BlogBasePage page = (BlogBasePage)Page;
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					sb.AppendFormat(" | <a href=\"mailto:{0}\">{0}</a>", Comment.Email);
					sb.AppendFormat(" | <a href=\"http://www.domaintools.com/go/?service=whois&amp;q={0}/\">{0}</a>", Comment.IP);
					string confirmDelete = string.Format(CultureInfo.InvariantCulture, page.Translate("areYouSure"), page.Translate("delete").ToLowerInvariant(), page.Translate("theComment"));
					sb.AppendFormat(" | <a href=\"javascript:void(0);\" onclick=\"if (confirm('{1}?')) location.href='?deletecomment={0}'\">{2}</a>", Comment.Id, confirmDelete, page.Translate("delete"));

					if (!Comment.IsApproved)
					{
						sb.AppendFormat(" | <a href=\"?approvecomment={0}\">{1}</a>", Comment.Id, page.Translate("approve"));

					}
					return sb.ToString();
				}
				return string.Empty;
			}
		}

		/// <summary>
		/// Displays the flag of the country from which the comment was written.
		/// <remarks>
		/// If the country hasn't been resolved from the authors IP address or
		/// the flag does not exist for that country, nothing is displayed.
		/// </remarks>
		/// </summary>
		protected string Flag
		{
			get
			{
				if (!string.IsNullOrEmpty(Comment.Country))
				{
					string path = Server.MapPath("~/pics/flags/" + Comment.Country + ".png");
					if (File.Exists(path))
					{
						return "<img src=\"" + Utils.RelativeWebRoot + "pics/flags/" + Comment.Country + ".png\" class=\"flag\" alt=\"" + Comment.Country + "\" />";
					}
				}

				return null;
			}
		}

		/// <summary>
		/// Displays the Gravatar image that matches the specified email.
		/// </summary>
		protected string Gravatar(int size)
		{
			if (BlogSettings.Instance.Avatar == "none")
				return null;

			if (String.IsNullOrEmpty(Comment.Email) || !Comment.Email.Contains("@"))
			{
				if (Comment.Website != null && Comment.Website.ToString().Length > 0 && Comment.Website.ToString().Contains("http://"))
				{
					return string.Format(CultureInfo.InvariantCulture, "<img class=\"thumb\" src=\"http://images.websnapr.com/?url={0}&amp;size=t\" alt=\"{1}\" />", Server.UrlEncode(Comment.Website.ToString()), Comment.Email);
				}

				return "<img src=\"" + Utils.RelativeWebRoot + "themes/" + BlogSettings.Instance.Theme + "/noavatar.jpg\" alt=\"" + Comment.Author + "\" />";
			}

			string img = "<img src=\"{0}\" alt=\"{1}\" />";
			string hash = CreateMd5Hash();
			string noavatar = Utils.AbsoluteWebRoot + "themes/" + BlogSettings.Instance.Theme + "/noavatar.jpg";
			string monster = Utils.AbsoluteWebRoot + "monster.axd?seed=" + hash.Substring(0, 10) + "&size=" + size;
			string gravatar = "http://www.gravatar.com/avatar.php?gravatar_id=" + hash + "&amp;size=" + size + "&amp;default=";

			string link = string.Empty;
			switch (BlogSettings.Instance.Avatar)
			{
				case "monster":
					link = monster.Replace("&", "&amp;");
					break;

				case "gravatar":
					link = gravatar + Server.UrlEncode(noavatar);
					break;

				case "combine":
					link = gravatar + Server.UrlEncode(monster);
					break;
			}

			return string.Format(CultureInfo.InvariantCulture, img, link, Comment.Author);
		}

		private string CreateMd5Hash()
		{
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(Encoding.ASCII.GetBytes(Comment.Email));

			System.Text.StringBuilder hash = new System.Text.StringBuilder();
			for (int i = 0; i < result.Length; i++)
			{
				hash.Append(result[i].ToString("x2", CultureInfo.InvariantCulture));
			}

			return hash.ToString();
		}
		#endregion

	}
}
