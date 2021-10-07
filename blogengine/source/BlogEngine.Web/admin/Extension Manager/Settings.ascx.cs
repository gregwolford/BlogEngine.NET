using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Reflection;
using BlogEngine.Core;

public partial class User_controls_xmanager_Parameters : System.Web.UI.UserControl
{
    #region Private members
    static protected string _extensionName = string.Empty;
    static protected ExtensionSettings _settings = null;
    #endregion

    /// <summary>
    /// Dynamically loads form controls or
    /// data grid and binds data to controls
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        _extensionName = Request.QueryString["ext"];
        _settings = ExtensionManager.GetSettings(_extensionName);

        CreateFormFields();

        if (!Page.IsPostBack)
        {
            if (_settings.IsScalar)
            {
                BindScalar();
            }
            else
            {
                CreateTemplatedGridView();
                BindGrid();
            }
        }

        if (_settings.IsScalar)
        {
            btnAdd.Text = Resources.labels.save;
        }
        else
        {
            grid.RowEditing += new GridViewEditEventHandler(grid_RowEditing);
            grid.RowUpdating += new GridViewUpdateEventHandler(grid_RowUpdating);
            grid.RowCancelingEdit += delegate { Response.Redirect(Request.RawUrl); };
            grid.RowDeleting += new GridViewDeleteEventHandler(grid_RowDeleting);
            btnAdd.Text = Resources.labels.add;
        }

        btnAdd.Click += new EventHandler(btnAdd_Click);
    }

    /// <summary>
    /// Handels adding a new value(s)
    /// </summary>
    /// <param name="sender">Button</param>
    /// <param name="e">Arguments</param>
    void btnAdd_Click(object sender, EventArgs e)
    {
        if (IsValidForm())
        {
            foreach (Control ctl in phAddForm.Controls)
            {
                if (ctl.GetType().Name == "TextBox")
                {
                    TextBox txt = (TextBox)ctl;

                    if (_settings.IsScalar)
                        _settings.UpdateScalarValue(txt.ID, txt.Text);
                    else
                        _settings.AddValue(txt.ID, txt.Text);
                }
            }
            ExtensionManager.SaveSettings(_extensionName, _settings);
            if (_settings.IsScalar)
            {
                InfoMsg.InnerHtml = "The values has been saved";
                InfoMsg.Visible = true;
            }
            else
            {
                BindGrid();
            }
        }
    }

    /// <summary>
    /// Deliting row in the data grid
    /// </summary>
    /// <param name="sender">Grid View</param>
    /// <param name="e">Arguments</param>
    void grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        foreach (ExtensionParameter par in _settings.Parameters)
        {
            par.DeleteValue(e.RowIndex);
        }
        ExtensionManager.SaveSettings(_extensionName, _settings);
        Response.Redirect(Request.RawUrl);
    }

    /// <summary>
    /// Updating row in the grid
    /// </summary>
    /// <param name="sender">Grid View</param>
    /// <param name="e">Event args</param>
    void grid_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // extract and store input values in the collection
        StringCollection updateValues = new StringCollection();
        foreach (DataControlFieldCell cel in grid.Rows[e.RowIndex].Controls)
        {
            foreach (Control ctl in cel.Controls)
            {
                if (ctl.GetType().Name == "TextBox")
                {
                    TextBox txt = (TextBox)ctl;
                    updateValues.Add(txt.Text);
                }
            }
        }

        for (int i = 0; i < _settings.Parameters.Count; i++)
        {
            string parName = _settings.Parameters[i].Name;
            if (_settings.IsRequiredParameter(parName) && string.IsNullOrEmpty(updateValues[i]))
            {
                // throw error if required field is empty
                ErrorMsg.InnerHtml = "\"" + _settings.GetLabel(parName) + "\" is a required field";
                ErrorMsg.Visible = true;
                e.Cancel = true;
                return;
            }
            else if (parName == _settings.KeyField && _settings.IsKeyValueExists(updateValues[i]))
            {
                // check if key value was changed; if not, it's ok to update
                if (!_settings.IsOldValue(parName, updateValues[i], e.RowIndex))
                {
                    // trying to update key field with value that already exists
                    ErrorMsg.InnerHtml = "\"" + updateValues[i] + "\" is already exists";
                    ErrorMsg.Visible = true;
                    e.Cancel = true;
                    return;
                }

            }
            else
                _settings.Parameters[i].Values[e.RowIndex] = updateValues[i];
        }

        ExtensionManager.SaveSettings(_extensionName, _settings);
        Response.Redirect(Request.RawUrl);
    }

    /// <summary>
    /// Editing data in the data grid
    /// </summary>
    /// <param name="sender">Grid View</param>
    /// <param name="e">Event args</param>
    void grid_RowEditing(object sender, GridViewEditEventArgs e)
    {
        grid.EditIndex = e.NewEditIndex;
        BindGrid();
    }

    /// <summary>
    /// Binds settings values formatted as
    /// data table to grid view
    /// </summary>
    private void BindGrid()
    {
        grid.DataKeyNames = new string[] { _settings.KeyField };
        grid.DataSource = _settings.GetDataTable();
        grid.DataBind();
    }

    /// <summary>
    /// Binds single value parameters
    /// to text boxes
    /// </summary>
    private void BindScalar()
    {
        foreach (ExtensionParameter par in _settings.Parameters)
        {
            foreach (Control ctl in phAddForm.Controls)
            {
                if (ctl.GetType().Name == "TextBox")
                {
                    TextBox txt = (TextBox)ctl;
                    if (txt.ID.ToLower() == par.Name.ToLower())
                    {
                        if (par.Values != null)
                            txt.Text = par.Values[0];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates template for data grid view
    /// </summary>
    void CreateTemplatedGridView()
    {
        foreach (ExtensionParameter par in _settings.Parameters)
        {
            BoundField col = new BoundField();
            col.DataField = par.Name;
            col.HeaderText = par.Name;
            grid.Columns.Add(col);
        }
    }

    /// <summary>
    /// Dynamically add controls to the form
    /// </summary>
    void CreateFormFields()
    {
        foreach (ExtensionParameter par in _settings.Parameters)
        {
            ErrorMsg.InnerHtml = string.Empty;
            ErrorMsg.Visible = false;
            InfoMsg.InnerHtml = string.Empty;
            InfoMsg.Visible = false;

            // add label
            Label lbl = new Label();
            lbl.Width = new Unit("100");
            lbl.Text = par.Label;
            phAddForm.Controls.Add(lbl);

            Literal br = new Literal();
            br.Text = "<br />";
            phAddForm.Controls.Add(br);

            // add textbox
            TextBox bx = new TextBox();
            bx.Text = string.Empty;
            bx.ID = par.Name;
            bx.Width = new Unit(250);
            bx.MaxLength = par.MaxLength;
            phAddForm.Controls.Add(bx);

            Literal br2 = new Literal();
            br2.Text = "<br />";
            phAddForm.Controls.Add(br2);
        }
    }

    /// <summary>
    /// Validate the form
    /// </summary>
    /// <returns>True if valid</returns>
    private bool IsValidForm()
    {
        bool rval = true;
        ErrorMsg.InnerHtml = string.Empty;
        foreach (Control ctl in phAddForm.Controls)
        {
            if (ctl.GetType().Name == "TextBox")
            {
                TextBox txt = (TextBox)ctl;
                if (_settings.IsRequiredParameter(txt.ID) && string.IsNullOrEmpty(txt.Text.Trim()))
                {
                    ErrorMsg.InnerHtml = "\"" + _settings.GetLabel(txt.ID) + "\" is a required field";
                    ErrorMsg.Visible = true;
                    rval = false;
                    break;
                }
                if (!_settings.IsScalar)
                {
                    if (_settings.KeyField == (txt.ID) && _settings.IsKeyValueExists(txt.Text.Trim()))
                    {
                        ErrorMsg.InnerHtml = "\"" + txt.Text + "\" is already exists";
                        ErrorMsg.Visible = true;
                        rval = false;
                        break;
                    }
                }
            }
        }
        return rval;
    }
    
    /// <summary>
    /// Gets a handle on grid data just before
    /// bound them to grid view
    /// </summary>
    /// <param name="sender">Grid view</param>
    /// <param name="e">Event args</param>
    protected void grid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        AddConfirmDelete((GridView)sender, e);
    }

    /// <summary>
    /// Adds confirmation box to delete buttons
    /// in the data grid
    /// </summary>
    /// <param name="gv">Data grid view</param>
    /// <param name="e">Event args</param>
    protected static void AddConfirmDelete(GridView gv, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow)
            return;

        foreach (DataControlFieldCell dcf in e.Row.Cells)
        {
            if (string.IsNullOrEmpty(dcf.Text.Trim()))
            {
                foreach (Control ctrl in dcf.Controls)
                {
                    LinkButton deleteButton = ctrl as LinkButton;
                    if (deleteButton != null && deleteButton.Text == "Delete")
                    {
                        deleteButton.Attributes.Add("onClick", "return confirm('Are you sure you want to delete this row?');");
                        break;
                    }
                }
                break;
            }
        }
    }
}
