using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class User_controls_xmanager_SourceEditor : System.Web.UI.UserControl
{
    static protected string _errorMsg = string.Empty;
    static protected string _extensionName = string.Empty;

    /// <summary>
    /// Handles page load event
    /// </summary>
    /// <param name="sender">Page</param>
    /// <param name="e">Event args</param>
    protected void Page_Load(object sender, EventArgs e)
    { if(!string.IsNullOrEmpty(Request.QueryString["readfile"])) { Response.Write(ReadFile(Request.QueryString["readfile"])); }
        btnSave.Enabled = true;
        _extensionName = Request.QueryString["ext"];
        txtEditor.Text = ReadFile(GetExtFileName());
    }
    
    /// <summary>
    /// Buttons save hanler
    /// </summary>
    /// <param name="sender">Button</param>
    /// <param name="e">Event args</param>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        string ext = Request.QueryString["ext"];

        if (WriteFile(GetExtFileName(), txtEditor.Text))
        {
            Response.Redirect("Default.aspx");
        }
        else
        {
            txtEditor.Text = _errorMsg;
            txtEditor.ForeColor = System.Drawing.Color.Red;
            btnSave.Enabled = false;
        }
    }

    /// <summary>
    /// Returns extension file name
    /// </summary>
    /// <returns>File name</returns>
    string GetExtFileName()
    {
        string fileName = HttpContext.Current.Request.PhysicalApplicationPath;
        fileName += "App_Code\\Extensions\\" + _extensionName + ".cs";
        return fileName;
    }

    /// <summary>
    /// Read extension source file from disk
    /// </summary>
    /// <param name="fileName">File Name</param>
    /// <returns>Source file text</returns>
    string ReadFile(string fileName)
    {
        string val = "Source for [" + fileName + "] not found";
        try
        {
            val = File.ReadAllText(fileName);
        }
        catch (Exception)
        {
            btnSave.Enabled = false;
        }
        return val;
    }

    /// <summary>
    /// Writes file to the disk
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="val">File source (text)</param>
    /// <returns>True if successful</returns>
    static bool WriteFile(string fileName, string val)
    {
        try
        {
            StreamWriter sw = File.CreateText(fileName);
            sw.Write(val);
            sw.Close();
        }
        catch (Exception e)
        {
            _errorMsg = e.Message;
            return false;
        }
        return true;
    }
}
