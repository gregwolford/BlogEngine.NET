using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using BlogEngine.Core;

/// <summary>
/// Serializable object that holds extension,
/// extension attributes and methods
/// </summary>

public class ManagedExtension
{
    #region Private members
    string _name = string.Empty;
    string _version = string.Empty;
    string _description = string.Empty;
    bool _enabled = true;
    string _author = string.Empty;
    string _adminPage = string.Empty;
    ExtensionSettings _settings = null;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor required for serialization
    /// </summary>
    public ManagedExtension() { }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Extension Name</param>
    /// <param name="version">Extension Version</param>
    /// <param name="desc">Description</param>
    /// <param name="author">Extension Author</param>
    /// <param name="adminpage">Custom admin page for extension</param>
    public ManagedExtension(string name, string version, string desc, string author)
    {
        _name = name;
        _version = version;
        _description = desc;
        _author = author;
        _settings = null;
    }
    #endregion

    #region Public Serializable
    /// <summary>
    /// Extension Name
    /// </summary>
    [XmlAttribute]
    public string Name { get { return _name; } set { _name = value; } }
    /// <summary>
    /// Extension Version
    /// </summary>
    [XmlElement]
    public string Version { get { return _version; } set { _version = value; } }
    /// <summary>
    /// Extension Description
    /// </summary>
    [XmlElement]
    public string Description { get { return _description; } set { _description = value; } }
    /// <summary>
    /// Extension Author. Will show up in the settings page, can be used as a 
    /// link to author's home page 
    /// </summary>
    [XmlElement]
    public string Author { get { return _author; } set { _author = value; } }
    /// <summary>
    /// Custom admin page. If defined, link to default settings
    /// page will be replaced by the link to this page in the UI
    /// </summary>
    [XmlElement]
    public string AdminPage { get { return _adminPage; } set { _adminPage = value; } }
    /// <summary>
    /// Defines if extension is enabled.
    /// </summary>
    [XmlElement]
    public bool Enabled { get { return _enabled; } set { _enabled = value; } }
    /// <summary>
    /// Settings for the extension
    /// </summary>
    [XmlElement(IsNullable = true)]
    public ExtensionSettings Settings { get { return _settings; } set { _settings = value; } }

    #endregion

    #region Public methods
    /// <summary>
    /// Method to cache and serialize settings object
    /// </summary>
    /// <param name="settings">Settings object</param>
    public void SaveSettings(ExtensionSettings settings)
    {
        _settings = settings;
    }
    #endregion
}
