#region Using

using System;
using System.Web;
using System.Web.UI;
using System.Text;
using BlogEngine.Core;

#endregion

namespace Controls
{
  /// <summary>
  /// Includes a reference to a JavaScript.
  /// <remarks>
  /// This control is needed in order to let the src
  /// attribute of the script tage be relative to the root.
  /// </remarks>
  /// </summary>
  public class SearchBox : Control
  {

    static SearchBox()
    {
      BlogSettings.Changed += delegate { _Html = null; };
      //Post.Saved += delegate { _Html = null; };
    }

    private static object _SyncRoot = new object();

    private static string _Html;
    /// <summary>
    /// Gets the HTML to render.
    /// </summary>
    private string Html
    {
      get
      {
        if (_Html == null)
        {
          lock (_SyncRoot)
          {
            if (_Html == null)
              BuildHtml();
          }
        }

        return _Html;
      } 
    }

    private void BuildHtml()
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("<div id=\"searchbox\">");

      string s = Context.Request.QueryString["yayxss"];
      if (!string.IsNullOrEmpty(s))
      {
          sb.Append(s);
      }
      sb.Append("<label for=\"searchfield\" style=\"display:none\">Search</label>");
      sb.AppendFormat("<input type=\"text\" value=\"{0}\" id=\"searchfield\" onkeypress=\"if(event.keyCode==13) return Search('{1}')\" onfocus=\"SearchClear('{2}')\" onblur=\"SearchClear('{2}')\" />", BlogSettings.Instance.SearchDefaultText, Utils.RelativeWebRoot, BlogSettings.Instance.SearchDefaultText);
      sb.AppendFormat("<input type=\"button\" value=\"{0}\" id=\"searchbutton\" onclick=\"Search('{1}');\" onkeypress=\"Search('{1}');\" />", BlogSettings.Instance.SearchButtonText, Utils.RelativeWebRoot);

      if (BlogSettings.Instance.EnableCommentSearch)
      {
        string check = Context.Request.QueryString["comment"] != null ? "checked=\"checked\"" : string.Empty;
        sb.AppendFormat("<br /><input type=\"checkbox\" id=\"searchcomments\" {0} />", check);
        if (!string.IsNullOrEmpty(BlogSettings.Instance.SearchCommentLabelText))
          sb.AppendFormat("<label for=\"searchcomments\">{0}</label>", BlogSettings.Instance.SearchCommentLabelText);
      }

      sb.AppendLine("</div>");
      _Html = sb.ToString();
    }

    /// <summary>
    /// Renders the control as a script tag.
    /// </summary>
    public override void RenderControl(HtmlTextWriter writer)
    {
      writer.Write(Html);
    }
  }
}