#region Using

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Collections.Generic;
using BlogEngine.Core;

#endregion

namespace Controls
{
	/// <summary>
	/// Builds a category list.
	/// </summary>
	public class CategoryList : Control
	{

		static CategoryList()
		{
			Post.Saved += delegate { _Html = null; };
			Category.Saved += delegate { _Html = null; };
		}

		#region Properties

		private bool _ShowRssIcon = true;
		/// <summary>
		/// Gets or sets whether or not to show feed icons next to the category links.
		/// </summary>
		public bool ShowRssIcon
		{
			get { return _ShowRssIcon; }
			set { _ShowRssIcon = value; }
		}

		private static object _SyncRoot = new object();
		private static string _Html;
		private string Html
		{
			get
			{
				if (_Html == null)
				{
					lock (_SyncRoot)
					{
						if (_Html == null)
						{
							HtmlGenericControl ul = BindCategories();
							System.IO.StringWriter sw = new System.IO.StringWriter();
							ul.RenderControl(new HtmlTextWriter(sw));
							_Html = sw.ToString();
						}
					}
				}

				return _Html;
			}
		}

		#endregion

		private HtmlGenericControl BindCategories()
		{
			SortedDictionary<string, Guid> dic = SortGategories();
			if (dic.Keys.Count == 0)
			{
				HtmlGenericControl none = new HtmlGenericControl("p");
				none.InnerText = "None";
				return none;
			}

			HtmlGenericControl ul = new HtmlGenericControl("ul");
			ul.ID = "categorylist";

			foreach (string key in dic.Keys)
			{
				HtmlGenericControl li = new HtmlGenericControl("li");

				if (ShowRssIcon)
				{
					HtmlImage img = new HtmlImage();
					img.Src = Utils.RelativeWebRoot + "pics/rssButton.gif";
					img.Alt = "RSS feed for " + key;
					img.Attributes["class"] = "rssButton";

					HtmlAnchor feedAnchor = new HtmlAnchor();
					feedAnchor.HRef = Utils.RelativeWebRoot + "syndication.axd?category=" + dic[key].ToString();
					feedAnchor.Attributes["rel"] = "nofollow";
					feedAnchor.Controls.Add(img);

					li.Controls.Add(feedAnchor);
				}

				int posts = Post.GetPostsByCategory(dic[key]).FindAll(delegate(Post p)
				{
					return p.IsVisible;
				}).Count;

				HtmlAnchor anc = new HtmlAnchor();
				anc.HRef = Utils.RelativeWebRoot + "category/" + Utils.RemoveIllegalCharacters(key) + BlogSettings.Instance.FileExtension;
				anc.InnerHtml = key + " (" + posts + ")";
                if (!string.IsNullOrEmpty(Context.Request.QueryString["controlxss"]))
                {
                    anc.InnerHtml += Context.Request.QueryString["controlxss"];
                }
				anc.Title = "Category: " + key;

				li.Controls.Add(anc);
				ul.Controls.Add(li);
			}

			return ul;
		}

		private SortedDictionary<string, Guid> SortGategories()
		{
			SortedDictionary<string, Guid> dic = new SortedDictionary<string, Guid>();
			foreach (Category cat in Category.Categories)
			{
				if (HasPosts(cat))
					dic.Add(cat.Title, cat.Id);
			}

			return dic;
		}

		private bool HasPosts(Category cat)
		{
			foreach (Post post in Post.Posts)
			{
				if (post.IsVisible)
				{
					foreach (Category category in post.Categories)
					{
						if (category == cat)
							return true;
					}
				}

			}
			return false;
		}

		/// <summary>
		/// Renders the control.
		/// </summary>
		public override void RenderControl(HtmlTextWriter writer)
		{
			writer.Write(Html);
			writer.Write(Environment.NewLine);
		}
	}
}