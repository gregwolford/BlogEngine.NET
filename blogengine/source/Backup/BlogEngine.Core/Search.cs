#region Using

using System;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using BlogEngine.Core;
using System.Text.RegularExpressions;

#endregion

namespace BlogEngine.Core
{
  /// <summary>
  /// Searches the post collection and returns a result based on a search term.
  /// <remarks>It is used for related posts and the in-site search feature.</remarks>
  /// </summary>
  public static class Search
  {

    static Search()
    {
      BuildCatalog();
      Post.Saved += new EventHandler<SavedEventArgs>(Post_Saved);
      Page.Saved += new EventHandler<SavedEventArgs>(Page_Saved);
      BlogSettings.Changed += delegate { BuildCatalog(); };
      Post.CommentAdded += new EventHandler<EventArgs>(Post_CommentAdded);
      Post.CommentRemoved += delegate { BuildCatalog(); };
      Comment.Approved += new EventHandler<EventArgs>(Post_CommentAdded);
    }
    
    #region Event handlers

    /// <summary>
    /// Adds a post to the catalog when it is added.
    /// </summary>
    private static void Post_Saved(object sender, SavedEventArgs e)
    {
      lock (_SyncRoot)
      {
        if (e.Action == SaveAction.Insert)
        {
          AddItem(sender as Post);
        }
        else
        {
          BuildCatalog();
        }
      }
    }

    private static void Page_Saved(object sender, SavedEventArgs e)
    {
      lock (_SyncRoot)
      {
        if (e.Action == SaveAction.Insert)
        {
          AddItem(sender as Page);
        }
        else
        {
          BuildCatalog();
        }
      }
    }

		static void Post_CommentAdded(object sender, EventArgs e)
		{
			if (BlogSettings.Instance.EnableCommentSearch)
			{
				Comment comment = (Comment)sender;
				if (comment.IsVisible)
					AddItem(comment);
			}
		}

    #endregion

    #region Search

    /// <summary>
    /// Searches all the posts and returns a ranked result set.
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <param name="includeComments">True to include a post's comments and their authors in search</param>
    public static List<IPublishable> Hits(string searchTerm, bool includeComments)
    {
      lock (_SyncRoot)
      {
        List<Result> results = BuildResultSet(searchTerm, includeComments);
        List<IPublishable> items = results.ConvertAll(new Converter<Result, IPublishable>(ResultToPost));
				results.Clear();
        OnSearcing(searchTerm);
        return items;
      }
    }

    /// <summary>
    /// Returns a list of posts that is related to the specified post.
    /// </summary>
    public static List<IPublishable> FindRelatedItems(IPublishable post)
    {
      string term = post.Title;

      foreach (Category cat in post.Categories)
      {
        term += " " + cat.Title;
      }

      term = CleanContent(term, false);
      return Hits(term, false);
    }

    /// <summary>
    /// Builds the results set and ranks it.
    /// </summary>
    private static List<Result> BuildResultSet(string searchTerm, bool includeComments)
    {
      List<Result> results = new List<Result>();
      string term = CleanContent(searchTerm.ToLowerInvariant().Trim(), false);
      string[] terms = term.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      string regex = string.Format(System.Globalization.CultureInfo.InvariantCulture, "({0})", string.Join("|", terms));

      foreach (Entry entry in _Catalog)
      {
        Result result = new Result();
        if (!(entry.Item is Comment))
        {
          int titleMatches = Regex.Matches(entry.Title, regex).Count;
          result.Rank = titleMatches * 20;

          int postMatches = Regex.Matches(entry.Content, regex).Count;
          result.Rank += postMatches;
        }
        else if (includeComments)
        {
          int commentMatches = Regex.Matches(entry.Content + entry.Title, regex).Count;
          result.Rank += commentMatches;
        }

        if (result.Rank > 0)
        {
          result.Item = entry.Item;
          results.Add(result);
        }
      }

      results.Sort();
      return results;
    }

    /// <summary>
    /// A converter delegate used for converting Results to Posts.
    /// </summary>
    private static IPublishable ResultToPost(Result result)
    {
      return result.Item;
    }

    #endregion

    #region Properties and private fields

    private readonly static object _SyncRoot = new object();
    private readonly static Regex _StripHtml = new Regex("<[^>]*>", RegexOptions.Compiled);
    private readonly static StringCollection _StopWords = GetStopWords();
    private static Collection<Entry> _Catalog = new Collection<Entry>();

    #endregion

    #region BuildCatalog

    private static void BuildCatalog()
    {
      OnIndexBuilding();

      lock (_SyncRoot)
      {
        _Catalog.Clear();
        foreach (Post post in Post.Posts)
        {
          if (!post.IsVisible)
            continue;

          AddItem(post);
          if (BlogSettings.Instance.EnableCommentSearch)
          {
            foreach (Comment comment in post.Comments)
            {
              if (comment.IsVisible)
                AddItem(comment);
            }
          }
        }

        foreach (Page page in Page.Pages)
        {
          if (page.IsVisible)
            AddItem(page);
        }
      }

      OnIndexBuild();
    }

    /// <summary>
    /// Adds an IPublishable item to the search catalog. 
    /// That will make it immediately searchable.
    /// </summary>
    public static void AddItem(IPublishable item)
    {
      Entry entry = new Entry();
      entry.Item = item;
      entry.Title = CleanContent(item.Title, false);
      entry.Content = HttpUtility.HtmlDecode(CleanContent(item.Content, true));
      if (item is Comment)
      {
        entry.Content += HttpUtility.HtmlDecode(CleanContent(item.Author, false));
      }
      _Catalog.Add(entry);
    }

    /// <summary>
    /// Removes stop words and HTML from the specified string.
    /// </summary>
    private static string CleanContent(string content, bool removeHtml)
    {
      if (removeHtml)
        content = _StripHtml.Replace(content, string.Empty);

			content = content
											.Replace("\\", string.Empty)
											.Replace("|", string.Empty)
											.Replace("(", string.Empty)
											.Replace(")", string.Empty)
											.Replace("[", string.Empty)
											.Replace("]", string.Empty)
											.Replace("+", string.Empty);

      string[] words = content.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < words.Length; i++)
      {
        string word = words[i].ToLowerInvariant().Trim();
        if (word.Length > 1 && !_StopWords.Contains(word))
          sb.Append(word + " ");
      }

      return sb.ToString();
    }

    /// <summary>
    /// Retrieves the stop words from the stopwords.txt file located in App_Data.
    /// </summary>
    private static StringCollection GetStopWords()
    {
      using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(BlogSettings.Instance.StorageLocation) + "stopwords.txt"))
      {
        string file = reader.ReadToEnd();
        string[] words = file.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        StringCollection col = new StringCollection();
        col.AddRange(words);

        return col;
      }
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a search is performed. (The search term is the sender).
    /// </summary>
    public static event EventHandler<EventArgs> Searching;
    /// <summary>
    /// Raises the event in a safe way
    /// </summary>
    private static void OnSearcing(string searchTerm)
    {
      if (Searching != null)
      {
        Searching(searchTerm, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Occurs just before the search index is being build.
    /// </summary>
    public static event EventHandler<EventArgs> IndexBuilding;
    /// <summary>
    /// Raises the event in a safe way
    /// </summary>
    private static void OnIndexBuilding()
    {
      if (IndexBuilding != null)
      {
        IndexBuilding(null, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Occurs after the index has been build.
    /// </summary>
    public static event EventHandler<EventArgs> IndexBuild;
    /// <summary>
    /// Raises the event in a safe way
    /// </summary>
    private static void OnIndexBuild()
    {
      if (IndexBuild != null)
      {
        IndexBuild(null, EventArgs.Empty);
      }
    }

    #endregion

  }

  #region Entry and Result structs

  /// <summary>
  /// A search optimized post object cleansed from HTML and stop words.
  /// </summary>
  internal struct Entry
  {
    /// <summary>The post object reference</summary>
    internal IPublishable Item;
    /// <summary>The title of the post cleansed for stop words</summary>
    internal string Title;
    /// <summary>The content of the post cleansed for stop words and HTML</summary>
    internal string Content;
  }

  /// <summary>
  /// A result is a search result which contains a post and its ranking.
  /// </summary>
  internal struct Result : IComparable<Result>
  {
    /// <summary>
    /// The rank of the post based on the search term. The higher the rank, the higher the post is in the result set.
    /// </summary>
    internal int Rank;

    /// <summary>
    /// The post of the result.
    /// </summary>
    internal IPublishable Item;

    /// <summary>
    /// Compares the current object with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value 
    /// has the following meanings: Value Meaning Less than zero This object is less than the other parameter.Zero 
    /// This object is equal to other. Greater than zero This object is greater than other.
    /// </returns>
    public int CompareTo(Result other)
    {
      return other.Rank.CompareTo(Rank);
    }
  }

  #endregion

}
