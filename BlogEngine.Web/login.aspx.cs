using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class login : BlogEngine.Core.Web.Controls.BlogBasePage
{
	protected void Page_Load(object sender, EventArgs e)
	{
		if (Page.User.Identity.IsAuthenticated)
		{
			changepassword1.Visible = true;
			changepassword1.ContinueButtonClick += new EventHandler(changepassword1_ContinueButtonClick);
			Login1.Visible = false;
			Page.Title = Resources.labels.changePassword;
		}
		else
		{
			Login1.LoggedIn += new EventHandler(Login1_LoggedIn);
			Login1.FindControl("username").Focus();
		}
	}

	/// <summary>
	/// Handles the ContinueButtonClick event of the changepassword1 control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	void changepassword1_ContinueButtonClick(object sender, EventArgs e)
	{
		Response.Redirect(BlogEngine.Core.Utils.RelativeWebRoot, true);
	}

	/// <summary>
	/// Handles the LoggedIn event of the Login1 control.
	/// </summary>
	/// <param name="sender">The source of the event.</param>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	void Login1_LoggedIn(object sender, EventArgs e)
	{
		if (!Roles.IsUserInRole(Login1.UserName, "Administrators"))
			Response.Redirect(BlogEngine.Core.Utils.RelativeWebRoot, true);
	}
}
