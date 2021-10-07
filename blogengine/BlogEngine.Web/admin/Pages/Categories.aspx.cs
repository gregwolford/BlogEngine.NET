#region Using

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using BlogEngine.Core;

#endregion

public partial class admin_Pages_Categories : System.Web.UI.Page
{
	/// <summary>
	/// Handles the Load event of the Page control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
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
		btnAdd.Text = Resources.labels.add + " " + Resources.labels.category.ToLowerInvariant();
		valExist.ServerValidate += new ServerValidateEventHandler(valExist_ServerValidate);
		Page.Title = Resources.labels.categories;
	}

	/// <summary>
	/// Handles the ServerValidate event of the valExist control.
	/// </summary>
	/// <param name="source">The source of the event.</param>
	/// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
	private void valExist_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = true;

		foreach (Category category in Category.Categories)
		{
			if (category.Title.Equals(txtNewCategory.Text.Trim(), StringComparison.OrdinalIgnoreCase))
				args.IsValid = false;
		}
	}

	/// <summary>
	/// Handles the Click event of the btnAdd control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	void btnAdd_Click(object sender, EventArgs e)
	{
		if (Page.IsValid)
		{
			Category cat = new Category(txtNewCategory.Text, txtNewNewDescription.Text);
			cat.Save();
			Response.Redirect(Request.RawUrl, true);
		}
	}

	/// <summary>
	/// Handles the RowDeleting event of the grid control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewDeleteEventArgs"/> instance containing the event data.</param>
	void grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
	{
		Guid id = (Guid)grid.DataKeys[e.RowIndex].Value;
		Category cat = Category.GetCategory(id);
		
		// Removes all references to the category
		foreach (Post post in Post.Posts)
		{
			if (post.Categories.Contains(cat))
			{
				post.Categories.Remove(cat);
			}
		}

		cat.Delete();
		cat.Save();
		Response.Redirect(Request.RawUrl);
	}

	/// <summary>
	/// Handles the RowUpdating event of the grid control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewUpdateEventArgs"/> instance containing the event data.</param>
	void grid_RowUpdating(object sender, GridViewUpdateEventArgs e)
	{
		Guid id = (Guid)grid.DataKeys[e.RowIndex].Value;
		TextBox textboxTitle = (TextBox)grid.Rows[e.RowIndex].FindControl("txtTitle");
		TextBox textboxDescription = (TextBox)grid.Rows[e.RowIndex].FindControl("txtDescription");
		Category cat = Category.GetCategory(id);
		cat.Title = textboxTitle.Text;
		cat.Description = textboxDescription.Text;
		cat.Save();

		Response.Redirect(Request.RawUrl);
	}

	/// <summary>
	/// Handles the RowEditing event of the grid control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewEditEventArgs"/> instance containing the event data.</param>
	void grid_RowEditing(object sender, GridViewEditEventArgs e)
	{
		grid.EditIndex = e.NewEditIndex;
		BindGrid();
	}

	/// <summary>
	/// Binds the grid with all the categories.
	/// </summary>
	private void BindGrid()
	{
		grid.DataKeyNames = new string[] { "Id" };
		grid.DataSource = Category.Categories;
		grid.DataBind();
	}

}
