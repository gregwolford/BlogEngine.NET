#region Using

using System;
using System.Web;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using BlogEngine.Core;

#endregion

public partial class archive : BlogEngine.Core.Web.Controls.BlogBasePage
{
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack && !IsCallback)
    {
      CreateAdminMenu();
      CreateArchive();
      AddTotals();
    }

    Page.Title = Resources.labels.archive;
    base.AddMetaTag("description", BlogSettings.Instance.Description);
  }

  private void CreateAdminMenu()
  {
    foreach (Category cat in Category.Categories)
    {
      HtmlAnchor a = new HtmlAnchor();
      a.InnerHtml = cat.Title;
      a.HRef = "#" + Utils.RemoveIllegalCharacters(cat.Title);
      a.Attributes.Add("rel", "directory");

      HtmlGenericControl li = new HtmlGenericControl("li");
      li.Controls.Add(a);
      ulMenu.Controls.Add(li);
    }

  }

  private SortedDictionary<string, Guid> SortCategories(Dictionary<Guid, string> categories)
  {
    SortedDictionary<string, Guid> dic = new SortedDictionary<string, Guid>();
    foreach (Category cat in Category.Categories)
    {
      dic.Add(cat.Title, cat.Id);
    }

    return dic;
  }

  private void CreateArchive()
  {
    foreach (Category cat in Category.Categories)
    {
      string name = cat.Title;
      List<Post> list = Post.GetPostsByCategory(cat.Id);

      HtmlAnchor feed = new HtmlAnchor();
      feed.HRef = Utils.RelativeWebRoot + "category/syndication.axd?category=" + cat.Id.ToString();

      HtmlImage img = new HtmlImage();
      img.Src = Utils.RelativeWebRoot + "pics/rssButton.gif";
      img.Alt = "RSS";

      feed.Controls.Add(img);

      HtmlGenericControl h2 = new HtmlGenericControl("h2");
      h2.Attributes["id"] = Utils.RemoveIllegalCharacters(name);
      h2.Controls.Add(feed);

      Control header = new LiteralControl(name + " (" + list.Count + ")");
      h2.Controls.Add(header);

      phArchive.Controls.Add(h2);

      HtmlTable table = CreateTable(name);


      foreach (Post post in list)
      {
				if (!post.IsVisible)
					continue;

        HtmlTableRow row = new HtmlTableRow();

        HtmlTableCell date = new HtmlTableCell();
        date.InnerHtml = post.DateCreated.ToString("yyyy-MM-dd");
        date.Attributes.Add("class", "date");
        row.Cells.Add(date);

        HtmlTableCell title = new HtmlTableCell();
        title.InnerHtml = string.Format("<a href=\"{0}\">{1}</a>", post.RelativeLink, post.Title);
        title.Attributes.Add("class", "title");
        row.Cells.Add(title);

        if (BlogSettings.Instance.IsCommentsEnabled)
        {
          HtmlTableCell comments = new HtmlTableCell();
          comments.InnerHtml = post.Comments.Count.ToString();
          comments.Attributes.Add("class", "comments");
          row.Cells.Add(comments);
        }

        if (BlogSettings.Instance.EnableRating)
        {
          HtmlTableCell rating = new HtmlTableCell();
          rating.InnerHtml = post.Raters == 0 ? "None" : Math.Round(post.Rating, 1).ToString();
          rating.Attributes.Add("class", "rating");
          row.Cells.Add(rating);
        }

        table.Rows.Add(row);
      }

      phArchive.Controls.Add(table);
    }
  }

  private HtmlTable CreateTable(string name)
  {
    HtmlTable table = new HtmlTable();
    table.Attributes.Add("summary", name);

    HtmlTableRow header = new HtmlTableRow();

    HtmlTableCell date = new HtmlTableCell("th");
    date.InnerHtml = base.Translate("date");
    header.Cells.Add(date);

    HtmlTableCell title = new HtmlTableCell("th");
    title.InnerHtml = base.Translate("title");
    header.Cells.Add(title);

    if (BlogSettings.Instance.IsCommentsEnabled)
    {
      HtmlTableCell comments = new HtmlTableCell("th");
      comments.InnerHtml = base.Translate("comments");
      comments.Attributes.Add("class", "comments");
      header.Cells.Add(comments);
    }

    if (BlogSettings.Instance.EnableRating)
    {
      HtmlTableCell rating = new HtmlTableCell("th");
      rating.InnerHtml = base.Translate("rating");
      rating.Attributes.Add("class", "rating");
      header.Cells.Add(rating);
    }

    table.Rows.Add(header);

    return table;
  }

  private void AddTotals()
  {
    int comments = 0;
    int raters = 0;
    foreach (Post post in Post.Posts)
    {
      comments += post.ApprovedComments.Count;
      raters += post.Raters;
    }

    ltPosts.Text = Post.Posts.Count + " " + Resources.labels.posts.ToLowerInvariant();
    if (BlogSettings.Instance.IsCommentsEnabled)
      ltComments.Text = comments + " " + Resources.labels.comments.ToLowerInvariant();

    if (BlogSettings.Instance.EnableRating)
      ltRaters.Text = raters + " " + Resources.labels.raters.ToLowerInvariant();
  }
}
