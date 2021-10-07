#region Using

using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using BlogEngine.Core.Providers;

#endregion

public partial class admin_Pages_PingServices : System.Web.UI.Page
{
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!Page.IsPostBack)
    {
      BindGrid();
    }

    grid.RowEditing += new GridViewEditEventHandler(grid_RowEditing);
    grid.RowUpdating += new GridViewUpdateEventHandler(grid_RowUpdating);
    grid.RowCancelingEdit += delegate { Response.Redirect(Request.RawUrl); };
    grid.RowDeleting += new GridViewDeleteEventHandler(grid_RowDeleting);
    btnAdd.Click += new EventHandler(btnAdd_Click);
    btnAdd.Text = Resources.labels.add + " ping service";
  }

  void btnAdd_Click(object sender, EventArgs e)
  {
    StringCollection col = BlogService.LoadPingServices();
    string service = txtNewCategory.Text.ToLowerInvariant();
    if (!col.Contains(service))
    {
      col.Add(service);
      BlogService.SavePingServices(col);
    }
    Response.Redirect(Request.RawUrl);
  }

  void grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
  {
    string service = grid.DataKeys[e.RowIndex].Value.ToString();
    StringCollection col = BlogService.LoadPingServices();
    col.Remove(service);    
    BlogService.SavePingServices(col);
    Response.Redirect(Request.RawUrl);
  }

  void grid_RowUpdating(object sender, GridViewUpdateEventArgs e)
  {
    string service = grid.DataKeys[e.RowIndex].Value.ToString();
    TextBox textbox = (TextBox)grid.Rows[e.RowIndex].FindControl("txtName");
    
    StringCollection col = BlogService.LoadPingServices();
    col.Remove(service);
    col.Add(textbox.Text.ToLowerInvariant());
    BlogService.SavePingServices(col);
    
    Response.Redirect(Request.RawUrl);
  }

  void grid_RowEditing(object sender, GridViewEditEventArgs e)
  {
    grid.EditIndex = e.NewEditIndex;
    BindGrid();
  }

  private void BindGrid()
  {
    StringCollection col = BlogService.LoadPingServices();
    StringDictionary dic = new StringDictionary();
    foreach (string services in col)
    {
      dic.Add(services, services);
    }

    grid.DataKeyNames = new string[] { "key" };
    grid.DataSource = dic;
    grid.DataBind();
  }

}
