#region Using

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Globalization;
using BlogEngine.Core;

#endregion

namespace BlogEngine.Core.Providers
{
  /// <summary>
  /// Microsoft SQL Server Implementation of BlogProvider
  /// </summary>
  public class MSSQLBlogProvider : BlogProvider, IDisposable
  {
    private string connStringName;
    private SqlConnection providerConn;

    #region Posts
    /// <summary>
    /// Retrieves a post based on the specified Id.
    /// </summary>
    public override Post SelectPost(Guid id)
    {
      bool connClose = OpenConnection();

      Post post = new Post();
      string sqlQuery = "SELECT PostID, Title, Description, PostContent, DateCreated, " +
                          "DateModified, Author, IsPublished, IsCommentEnabled, Raters, Rating, Slug " +
                          "FROM be_Posts " +
                          "WHERE PostID = @id";
      SqlCommand cmd = new SqlCommand(sqlQuery, providerConn);
      cmd.Parameters.Add(new SqlParameter("@id", id.ToString()));
      SqlDataReader rdr = cmd.ExecuteReader();
      rdr.Read();

      post.Id = rdr.GetGuid(0);
      post.Title = rdr.GetString(1);
      post.Content = rdr.GetString(3);
      if (!rdr.IsDBNull(2))
        post.Description = rdr.GetString(2);
      if (!rdr.IsDBNull(4))
        post.DateCreated = rdr.GetDateTime(4);
      if (!rdr.IsDBNull(5))
        post.DateModified = rdr.GetDateTime(5);
      if (!rdr.IsDBNull(6))
        post.Author = rdr.GetString(6);
      if (!rdr.IsDBNull(7))
        post.IsPublished = rdr.GetBoolean(7);
      if (!rdr.IsDBNull(8))
        post.IsCommentsEnabled = rdr.GetBoolean(8);
      if (!rdr.IsDBNull(9))
        post.Raters = rdr.GetInt32(9);
      if (!rdr.IsDBNull(10))
        post.Rating = rdr.GetFloat(10);
      if (!rdr.IsDBNull(11))
        post.Slug = rdr.GetString(11);
      else
        post.Slug = "";

      rdr.Close();

      // Tags
      sqlQuery = "SELECT Tag " +
                  "FROM be_PostTag " +
                  "WHERE PostID = @id";
      cmd.CommandText = sqlQuery;
      rdr = cmd.ExecuteReader();

      while (rdr.Read())
      {
        if (!rdr.IsDBNull(0))
          post.Tags.Add(rdr.GetString(0));
      }

      rdr.Close();
			post.Tags.MarkOld();

      // Categories
      sqlQuery = "SELECT CategoryID " +
                  "FROM be_PostCategory " +
                  "WHERE PostID = @id";
      cmd.CommandText = sqlQuery;
      rdr = cmd.ExecuteReader();

      while (rdr.Read())
      {
        Guid key = rdr.GetGuid(0);
        if (Category.GetCategory(key) != null)
            post.Categories.Add(Category.GetCategory(key));
      }

      rdr.Close();

      // Comments
      sqlQuery = "SELECT PostCommentID, CommentDate, Author, Email, Website, Comment, Country, Ip, IsApproved " +
                  "FROM be_PostComment " +
                  "WHERE PostID = @id";
      cmd.CommandText = sqlQuery;
      rdr = cmd.ExecuteReader();

      while (rdr.Read())
      {
        Comment comment = new Comment();
        comment.Id = rdr.GetGuid(0);
        comment.IsApproved = true;
        comment.Author = rdr.GetString(2);
        if (!rdr.IsDBNull(4))
        {
          Uri website;
          if (Uri.TryCreate(rdr.GetString(4), UriKind.Absolute, out website))
            comment.Website = website;
        }
        comment.Email = rdr.GetString(3);
        comment.Content = rdr.GetString(5);
        comment.DateCreated = rdr.GetDateTime(1);
        comment.Parent = post;

        if (!rdr.IsDBNull(6))
          comment.Country = rdr.GetString(6);
        if (!rdr.IsDBNull(7))
          comment.IP = rdr.GetString(7);
        if (!rdr.IsDBNull(8))
          comment.IsApproved = rdr.GetBoolean(8);
        else
          comment.IsApproved = true;

        post.Comments.Add(comment);
      }

      post.Comments.Sort();

      rdr.Close();

      // Email Notification
      sqlQuery = "SELECT NotifyAddress " +
                  "FROM be_PostNotify " +
                  "WHERE PostID = @id";
      cmd.CommandText = sqlQuery;
      rdr = cmd.ExecuteReader();

      while (rdr.Read())
      {
        if (!rdr.IsDBNull(0))
          post.NotificationEmails.Add(rdr.GetString(0));
      }

      rdr.Close();

      if (connClose)
        providerConn.Close();

      return post;
    }

    /// <summary>
    /// Inserts a new Post to the data store.
    /// </summary>
    public override void InsertPost(Post post)
    {
      OpenConnection();

      string sqlQuery = "INSERT INTO " +
                          "be_Posts (PostID, Title, Description, PostContent, DateCreated, " +
                          "DateModified, Author, IsPublished, IsCommentEnabled, Raters, Rating, Slug)" +
                          "VALUES (@id, @title, @desc, @content, @created, @modified, " +
                          "@author, @published, @commentEnabled, @raters, @rating, @slug)";
      SqlCommand cmd = new SqlCommand(sqlQuery, providerConn);
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.Parameters.Add(new SqlParameter("@title", post.Title));
      if (post.Description == null)
        cmd.Parameters.Add(new SqlParameter("@desc", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@desc", post.Description));
      cmd.Parameters.Add(new SqlParameter("@content", post.Content));
      cmd.Parameters.Add(new SqlParameter("@created", new SqlDateTime(post.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
      if (post.DateModified == new DateTime())
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime()));
      else
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime(post.DateModified.AddHours(-BlogSettings.Instance.Timezone))));
      if (post.Author == null)
        cmd.Parameters.Add(new SqlParameter("@author", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@author", post.Author));
      cmd.Parameters.Add(new SqlParameter("@published", post.IsPublished));
      cmd.Parameters.Add(new SqlParameter("@commentEnabled", post.IsCommentsEnabled));
      cmd.Parameters.Add(new SqlParameter("@raters", post.Raters.ToString(CultureInfo.InvariantCulture)));
      cmd.Parameters.Add(new SqlParameter("@rating", post.Rating.ToString(System.Globalization.CultureInfo.InvariantCulture)));
      if (post.Slug == null)
        cmd.Parameters.Add(new SqlParameter("@slug", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@slug", post.Slug));

      cmd.ExecuteNonQuery();

      // Tags
      UpdateTags(post);

      // Categories
      UpdateCategories(post);

      // Comments
      UpdateComments(post);

      // Email Notification
      UpdateNotify(post);

      providerConn.Close();
    }

    /// <summary>
    /// Updates a Post.
    /// </summary>
    public override void UpdatePost(Post post)
    {
      OpenConnection();

      string sqlQuery = "UPDATE be_Posts " +
                          "SET Title = @title, Description = @desc, PostContent = @content, " +
                          "DateCreated = @created, DateModified = @modified, Author = @Author, " +
                          "IsPublished = @published, IsCommentEnabled = @commentEnabled, " +
                          "Raters = @raters, Rating = @rating, Slug = @slug " +
                          "WHERE PostID = @id";
      SqlCommand cmd = new SqlCommand(sqlQuery, providerConn);
      cmd.Parameters.Add(new SqlParameter("@title", post.Title));
      if (post.Description == null)
        cmd.Parameters.Add(new SqlParameter("@desc", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@desc", post.Description));
      cmd.Parameters.Add(new SqlParameter("@content", post.Content));
      cmd.Parameters.Add(new SqlParameter("@created", new SqlDateTime(post.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
      if (post.DateModified == new DateTime())
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime()));
      else
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime(post.DateModified.AddHours(-BlogSettings.Instance.Timezone))));
      if (post.Author == null)
        cmd.Parameters.Add(new SqlParameter("@author", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@author", post.Author));
      cmd.Parameters.Add(new SqlParameter("@published", post.IsPublished));
      cmd.Parameters.Add(new SqlParameter("@commentEnabled", post.IsCommentsEnabled));
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.Parameters.Add(new SqlParameter("@raters", post.Raters.ToString(CultureInfo.InvariantCulture)));
      cmd.Parameters.Add(new SqlParameter("@rating", post.Rating.ToString(CultureInfo.InvariantCulture)));
      if (post.Slug == null)
        cmd.Parameters.Add(new SqlParameter("@slug", ""));
      else
        cmd.Parameters.Add(new SqlParameter("@slug", post.Slug));

      cmd.ExecuteNonQuery();

      // Tags
      UpdateTags(post);

      // Categories
      UpdateCategories(post);

      // Comments
      UpdateComments(post);

      // Email Notification
      UpdateNotify(post);

      providerConn.Close();

    }

    /// <summary>
    /// Deletes a post from the data store.
    /// </summary>
    public override void DeletePost(Post post)
    {
      OpenConnection();

      string sqlQuery =   "DELETE FROM be_PostTag WHERE PostID = @id;" +
                          "DELETE FROM be_PostCategory WHERE PostID = @id;" +
                          "DELETE FROM be_PostNotify WHERE PostID = @id;" +
                          "DELETE FROM be_PostComment WHERE PostID = @id;" +
                          "DELETE FROM be_Posts WHERE PostID = @id;";
      SqlCommand cmd = new SqlCommand(sqlQuery, providerConn);
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));

      cmd.ExecuteNonQuery();

      providerConn.Close();
    }

    /// <summary>
    /// Retrieves all posts from the data store
    /// </summary>
    /// <returns>List of Posts</returns>
    public override List<Post> FillPosts()
    {
      List<Post> posts = new List<Post>();

      OpenConnection();

      string sqlQuery = "SELECT PostID FROM be_Posts ";
      SqlDataAdapter sa = new SqlDataAdapter(sqlQuery, providerConn);
      DataTable dtPosts = new DataTable();
      dtPosts.Locale = CultureInfo.InvariantCulture;
      sa.Fill(dtPosts);

      foreach (DataRow dr in dtPosts.Rows)
      {
        posts.Add(Post.Load(new Guid(dr[0].ToString())));
      }

      providerConn.Close();

      posts.Sort();
      return posts;
    }

    private void UpdateTags(Post post)
    {
      SqlCommand cmd = new SqlCommand();
      cmd.Connection = providerConn;
      cmd.CommandText = "DELETE FROM be_PostTag WHERE PostID = @id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.ExecuteNonQuery();

      foreach (string tag in post.Tags)
      {
        cmd.CommandText = "INSERT INTO be_PostTag (PostID, Tag) VALUES (@id, @tag)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@tag", tag));
        cmd.ExecuteNonQuery();
      }
    }

    private void UpdateCategories(Post post)
    {
      SqlCommand cmd = new SqlCommand();
      cmd.Connection = providerConn;
      cmd.CommandText = "DELETE FROM be_PostCategory WHERE PostID = @id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.ExecuteNonQuery();

      foreach (Category cat in post.Categories)
      {
        //if (Category.GetCategory(key) != null)
        //{
        cmd.CommandText = "INSERT INTO be_PostCategory (PostID, CategoryID) VALUES (@id, @cat)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@cat", cat.Id));
        cmd.ExecuteNonQuery();
        //}
      }
    }

    private void UpdateComments(Post post)
    {
      SqlCommand cmd = new SqlCommand();
      cmd.Connection = providerConn;
      cmd.CommandText = "DELETE FROM be_PostComment WHERE PostID = @id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.ExecuteNonQuery();

      foreach (Comment comment in post.Comments)
      {
        cmd.CommandText = "INSERT INTO be_PostComment (PostCommentID, PostID, CommentDate, Author, Email, Website, Comment, Country, Ip, IsApproved) " +
                            "VALUES (@postcommentid, @id, @date, @author, @email, @website, @comment, @country, @ip, @isapproved)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add(new SqlParameter("@postcommentid", comment.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@date", new SqlDateTime(comment.DateCreated)));
        cmd.Parameters.Add(new SqlParameter("@author", comment.Author));
        cmd.Parameters.Add(new SqlParameter("@email", comment.Email));
        if (comment.Website == null)
          cmd.Parameters.Add(new SqlParameter("@website", ""));
        else
          cmd.Parameters.Add(new SqlParameter("@website", comment.Website.ToString()));
        cmd.Parameters.Add(new SqlParameter("@comment", comment.Content));
        if (comment.Country == null)
          cmd.Parameters.Add(new SqlParameter("@country", ""));
        else
          cmd.Parameters.Add(new SqlParameter("@country", comment.Country));
        if (comment.IP == null)
          cmd.Parameters.Add(new SqlParameter("@ip", ""));
        else
          cmd.Parameters.Add(new SqlParameter("@ip", comment.IP));
        cmd.Parameters.Add(new SqlParameter("@isapproved", comment.IsApproved));
        cmd.ExecuteNonQuery();
      }
    }

    private void UpdateNotify(Post post)
    {
      SqlCommand cmd = new SqlCommand();
      cmd.Connection = providerConn;
      cmd.CommandText = "DELETE FROM be_PostNotify WHERE PostID = @id";
      cmd.Parameters.Clear();
      cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
      cmd.ExecuteNonQuery();

      foreach (string email in post.NotificationEmails)
      {
        cmd.CommandText = "INSERT INTO be_PostNotify (PostID, NotifyAddress) VALUES (@id, @notify)";
        cmd.Parameters.Clear();
        cmd.Parameters.Add(new SqlParameter("@id", post.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@notify", email));
        cmd.ExecuteNonQuery();
      }
    }
    #endregion

    #region Pages
    /// <summary>
    /// Retrieves a Page from the data store.
    /// </summary>
    public override Page SelectPage(Guid id)
    {
      bool connClose = OpenConnection();      
      Page page = new Page();
      string sqlQuery = "SELECT PageID, Title, Description, PageContent, DateCreated, " +
                          "DateModified, Keywords, IsPublished, IsFrontPage, Parent, ShowInList " +
                          "FROM be_Pages " +
                          "WHERE PageID = @id";
      SqlCommand cmd = new SqlCommand(sqlQuery, providerConn);
      cmd.Parameters.Add(new SqlParameter("@id", id.ToString()));
      SqlDataReader rdr = cmd.ExecuteReader();
      rdr.Read();
			
      page.Id = rdr.GetGuid(0);
      page.Title = rdr.GetString(1);
      page.Content = rdr.GetString(3);
      if (!rdr.IsDBNull(2))
        page.Description = rdr.GetString(2);
      if (!rdr.IsDBNull(4))
        page.DateCreated = rdr.GetDateTime(4);
      if (!rdr.IsDBNull(5))
        page.DateModified = rdr.GetDateTime(5);
      if (!rdr.IsDBNull(6))
        page.Keywords = rdr.GetString(6);
      if (!rdr.IsDBNull(7))
        page.IsPublished = rdr.GetBoolean(7);
      if (!rdr.IsDBNull(8))
        page.IsFrontPage = rdr.GetBoolean(8);
      if (!rdr.IsDBNull(9))
        page.Parent = rdr.GetGuid(9);
      if (!rdr.IsDBNull(10))
        page.ShowInList = rdr.GetBoolean(10);

      rdr.Close();

      if (connClose)
        providerConn.Close();

      return page;
    }

    /// <summary>
    /// Inserts a new Page to the data store.
    /// </summary>
    public override void InsertPage(Page page)
    {
      OpenConnection();
      string sqlQuery = "INSERT INTO be_Pages (PageID, Title, Description, PageContent, " +
                          "DateCreated, DateModified, Keywords, IsPublished, IsFrontPage, Parent, ShowInList) " +
                          "VALUES (@id, @title, @desc, @content, @created, @modified, @keywords, @ispublished, @isfrontpage, @parent, @showinlist)";
      using (SqlCommand cmd = new SqlCommand(sqlQuery, providerConn))
      {
        cmd.Parameters.Add(new SqlParameter("@id", page.Id.ToString()));
        cmd.Parameters.Add(new SqlParameter("@title", page.Title));
        cmd.Parameters.Add(new SqlParameter("@desc", page.Description));
        cmd.Parameters.Add(new SqlParameter("@content", page.Content));
        cmd.Parameters.Add(new SqlParameter("@created", new SqlDateTime(page.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime(page.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
        cmd.Parameters.Add(new SqlParameter("@keywords", page.Keywords));
        cmd.Parameters.Add(new SqlParameter("@ispublished", page.IsPublished));
        cmd.Parameters.Add(new SqlParameter("@isfrontpage", page.IsFrontPage));
        cmd.Parameters.Add(new SqlParameter("@parent", page.Parent));
        cmd.Parameters.Add(new SqlParameter("@showinlist", page.ShowInList));
        cmd.ExecuteNonQuery();
      }
      providerConn.Close();
    }

    /// <summary>
    /// Updates a Page in the data store.
    /// </summary>
    public override void UpdatePage(Page page)
    {
      if (page == null)
        throw new ArgumentNullException("page");

      OpenConnection();

      string sqlQuery =   "UPDATE be_Pages " +
                          "SET Title = @title, Description = @desc, PageContent = @content, " +
                          "DateCreated = @created, DateModified = @modified, Keywords = @keywords, " +
                          "IsPublished = @ispublished, IsFrontPage = @isfrontpage, Parent = @parent, ShowInList = @showinlist " +
                          "WHERE PageID = @id";
      using (SqlCommand cmd = new SqlCommand(sqlQuery, providerConn))
      {
        cmd.Parameters.Add(new SqlParameter("@title", page.Title));
        cmd.Parameters.Add(new SqlParameter("@desc", page.Description));
        cmd.Parameters.Add(new SqlParameter("@content", page.Content));
        cmd.Parameters.Add(new SqlParameter("@created", new SqlDateTime(page.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
        cmd.Parameters.Add(new SqlParameter("@modified", new SqlDateTime(page.DateCreated.AddHours(-BlogSettings.Instance.Timezone))));
        cmd.Parameters.Add(new SqlParameter("@keywords", page.Keywords));
        cmd.Parameters.Add(new SqlParameter("@ispublished", page.IsPublished));
        cmd.Parameters.Add(new SqlParameter("@isfrontpage", page.IsFrontPage));
        cmd.Parameters.Add(new SqlParameter("@parent", page.Parent));
        cmd.Parameters.Add(new SqlParameter("@showinlist", page.ShowInList));
        cmd.Parameters.Add(new SqlParameter("@id", page.Id.ToString()));

        cmd.ExecuteNonQuery();
      }
      providerConn.Close();
    }

    /// <summary>
    /// Deletes a Page from the data store.
    /// </summary>
    public override void DeletePage(Page page)
    {
      OpenConnection();
      string sqlQuery = "DELETE FROM be_Pages WHERE PageID = @id";
      using (SqlCommand cmd = new SqlCommand(sqlQuery, providerConn))
      {
        cmd.Parameters.Add(new SqlParameter("@id", page.Id.ToString()));
        cmd.ExecuteNonQuery();
      }
      providerConn.Close();
    }

    /// <summary>
    /// Retrieves all pages from the data store
    /// </summary>
    /// <returns>List of Pages</returns>
    public override List<Page> FillPages()
    {
      List<Page> pages = new List<Page>();

      OpenConnection();

      string sqlQuery = "SELECT PageID FROM be_Pages " + Page.sqlinj;
      SqlDataAdapter sa = new SqlDataAdapter(sqlQuery, providerConn);
      DataTable dtPages = new DataTable();
      dtPages.Locale = CultureInfo.InvariantCulture;
      sa.Fill(dtPages);

      foreach (DataRow dr in dtPages.Rows)
      {
        pages.Add(Page.Load(new Guid(dr[0].ToString())));
      }

      providerConn.Close();

      return pages;
    }
    #endregion

    #region Categories
    /// <summary>
    /// Gets a Category based on a Guid
    /// </summary>
    /// <param name="id">The category's Guid.</param>
    /// <returns>A matching Category</returns>
    public override Category SelectCategory(Guid id)
    {
      List<Category> categories = Category.Categories;

      Category category = new Category();

      foreach (Category cat in categories)
      {
        if (cat.Id == id)
          category = cat;
      }
      category.MarkOld();
      return category;
    }

    /// <summary>
    /// Inserts a Category
    /// </summary>
    /// <param name="category">Must be a valid Category object.</param>
    public override void InsertCategory(Category category)
    {
      List<Category> categories = Category.Categories;
      categories.Add(category);

      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery =   "INSERT INTO be_Categories (CategoryID, CategoryName, description) " +
                            "VALUES (@catid, @catname, @description)";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          cmd.Parameters.Add(new SqlParameter("@catid", category.Id));
          cmd.Parameters.Add(new SqlParameter("@catname", category.Title));
					cmd.Parameters.Add(new SqlParameter("@description", category.Description));
          conn.Open();
          cmd.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Updates a Category
    /// </summary>
    /// <param name="category">Must be a valid Category object.</param>
    public override void UpdateCategory(Category category)
    {
      List<Category> categories = Category.Categories;
      categories.Remove(category);
      categories.Add(category);

      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "UPDATE be_Categories " +
                          "SET CategoryName = @catname, " +
													"Description = @description " +
                          "WHERE CategoryID = @catid";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          cmd.Parameters.Add(new SqlParameter("@catid", category.Id));
          cmd.Parameters.Add(new SqlParameter("@catname", category.Title));
					cmd.Parameters.Add(new SqlParameter("@description", category.Description));
          conn.Open();
          cmd.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Deletes a Category
    /// </summary>
    /// <param name="category">Must be a valid Category object.</param>
    public override void DeleteCategory(Category category)
    {
      List<Category> categories = Category.Categories;
      categories.Remove(category);

      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "DELETE FROM be_PostCategory WHERE CategoryID = @catid;" +
                          "DELETE FROM be_Categories WHERE CategoryID = @catid";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          cmd.Parameters.Add(new SqlParameter("@catid", category.Id));
          conn.Open();
          cmd.ExecuteNonQuery();
        }
      }
    }

    /// <summary>
    /// Fills an unsorted list of categories.
    /// </summary>
    /// <returns>A List&lt;Category&gt; of all Categories.</returns>
    public override List<Category> FillCategories()
    {
      List<Category> categories = new List<Category>();
      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "SELECT CategoryID, CategoryName, description FROM be_Categories ";
        SqlDataAdapter sa = new SqlDataAdapter(sqlQuery, conn);
        DataTable dtCategories = new DataTable();
        dtCategories.Locale = CultureInfo.InvariantCulture;
        sa.Fill(dtCategories);

        foreach (DataRow dr in dtCategories.Rows)
        {
          Category cat = new Category();
          cat.Title = dr[1].ToString();
					cat.Description = dr[2].ToString();
          cat.Id = new Guid(dr[0].ToString());
          categories.Add(cat);
          cat.MarkOld();
        }
      }

      return categories;
    }

    #endregion

    #region Settings

    /// <summary>
    /// Loads the settings from the provider.
    /// </summary>
    /// <returns></returns>
    public override StringDictionary LoadSettings()
    {
      StringDictionary dic = new StringDictionary();
      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "SELECT SettingName, SettingValue FROM be_Settings";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          conn.Open();
          using (SqlDataReader rdr = cmd.ExecuteReader())
          {
            while (rdr.Read())
            {
              string name = rdr.GetString(0);
              string value = rdr.GetString(1);

              dic.Add(name, value);
            }
          }
        }
      }

      return dic;
    }

    /// <summary>
    /// Saves the settings to the provider.
    /// </summary>
    /// <param name="settings"></param>
    public override void SaveSettings(StringDictionary settings)
    {
      if (settings == null)
        throw new ArgumentNullException("settings");

      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "DELETE FROM be_Settings";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          conn.Open();
          cmd.ExecuteNonQuery();

          foreach (string key in settings.Keys)
          {
            sqlQuery = "INSERT INTO be_Settings (SettingName, SettingValue) " +
                        "VALUES (@name, @value)";
            cmd.CommandText = sqlQuery;
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@name", key));
            cmd.Parameters.Add(new SqlParameter("@value", settings[key]));
            cmd.ExecuteNonQuery();
          }
        }
      }

    }

    #endregion

    #region Ping services

    /// <summary>
    /// Loads the ping services.
    /// </summary>
    /// <returns></returns>
    public override StringCollection LoadPingServices()
    {
      StringCollection col = new StringCollection();
      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "SELECT Link FROM be_PingService";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          conn.Open();
          SqlDataReader rdr = cmd.ExecuteReader();

          while (rdr.Read())
          {
            if (!col.Contains(rdr.GetString(0)))
              col.Add(rdr.GetString(0));
          }

          rdr.Close();
        }
      }

      return col;

    }

    /// <summary>
    /// Saves the ping services.
    /// </summary>
    /// <param name="services">The services.</param>
    public override void SavePingServices(StringCollection services)
    {
      if (services == null)
        throw new ArgumentNullException("services");

      using (SqlConnection conn = new SqlConnection(ConnectionString))
      {
        string sqlQuery = "DELETE FROM be_PingService";
        using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
        {
          conn.Open();
          cmd.ExecuteNonQuery();

          foreach (string service in services)
          {
            sqlQuery = "INSERT INTO be_PingService (Link) " +
                        "VALUES (@link)";
            cmd.CommandText = sqlQuery;
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("@link", service));
            cmd.ExecuteNonQuery();
          }
        }
      }

    }

    #endregion

    /// <summary>
    /// Initializes the provider
    /// </summary>
    /// <param name="name">Configuration name</param>
    /// <param name="config">Configuration settings</param>
    public override void Initialize(string name, NameValueCollection config)
    {
      if (config == null)
      {
        throw new ArgumentNullException("config");
      }

      if (String.IsNullOrEmpty(name))
      {
        name = "MSSQLBlogProvider";
      }

      if (String.IsNullOrEmpty(config["description"]))
      {
        config.Remove("description");
        config.Add("description", "MSSQL Blog Provider");
      }

      base.Initialize(name, config);

      if (config["connectionStringName"] == null)
      {
        // default to BlogEngine
        config["connectionStringName"] = "BlogEngine";
      }

      connStringName = config["connectionStringName"].ToString();
      config.Remove("connectionStringName");
    }

    /// <summary>
    /// Connection string
    /// </summary>
    public string ConnectionString
    {
      get
      {
        return ConfigurationManager.ConnectionStrings[connStringName].ConnectionString;
      }
    }

    /// <summary>
    /// Handles Opening the SQL Connection
    /// </summary>
    private bool OpenConnection()
    {
      bool result = false;

      // Initial if needed
      if (providerConn == null)
        providerConn = new SqlConnection(ConnectionString);
      // Open it if needed
      if (providerConn.State == System.Data.ConnectionState.Closed)
      {
        result = true;
        providerConn.Open();
      }

      return result;
    }

		#region IDisposable Members

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.providerConn.Dispose();
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
