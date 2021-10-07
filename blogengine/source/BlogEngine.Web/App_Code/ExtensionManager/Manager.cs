using System;
using System.Web;
using System.Web.Hosting;
using System.Web.Caching;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BlogEngine.Core;
using BlogEngine.Core.Web.Controls;

/// <summary>
/// Extension Manager - top level object in the hierarchy
/// Holds collection of extensions and methods to manipulate
/// extensions
/// </summary>
[XmlRoot]
public class ExtensionManager
{
    #region Constructor
    /// <summary>
    /// Default constructor, requred for serialization to work
    /// </summary>
    public ExtensionManager() { }
    #endregion

    #region Private members
    private static string _fileName = HostingEnvironment.MapPath(BlogSettings.Instance.StorageLocation + "extensions.xml");
    private static List<ManagedExtension> _extensions = new List<ManagedExtension>();
    #endregion

    #region Public members
    /// <summary>
    /// Used to hold exeption thrown when extension can not be serialized because of
    /// file access permission. Not serializable, used by UI to show error message.
    /// </summary>
    [XmlIgnore]
    public static Exception FileAccessException = null;
    /// <summary>
    /// Collection of extensions
    /// </summary>
    [XmlElement]
    public static List<ManagedExtension> Extensions { get { return _extensions; } }
    /// <summary>
    /// Enabled / Disabled
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns>True if enabled</returns>
    public static bool ExtensionEnabled(string extensionName)
    {
        bool val = true;
        LoadExtensions();
        _extensions.Sort(delegate(ManagedExtension p1, ManagedExtension p2) { return String.Compare(p1.Name, p2.Name); });

        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == extensionName)
            {
                if (x.Enabled == false)
                {
                    val = false;
                }
                break;
            }
        }
        return val;
    }
    /// <summary>
    /// Method to change extension status
    /// </summary>
    /// <param name="extension">Extensio Name</param>
    /// <param name="enabled">If true, enables extension</param>
    public static void ChangeStatus(string extension, bool enabled)
    {
        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == extension)
            {
                x.Enabled = enabled;
                SaveToXML();
                SaveToCache();
                break;
            }
        }
    }
    /// <summary>
    /// A way to let extension author to use custom
    /// admin page. Will show up as link on extensions page
    /// </summary>
    /// <param name="extension">Extension Name</param>
    /// <param name="url">Path to custom admin page</param>
    public static void SetAdminPage(string extension, string url)
    {
        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == extension)
            {
                x.AdminPage = url;
                SaveToXML();
                SaveToCache();
                break;
            }
        }
    }
    /// <summary>
    /// Will serialize and cache ext. mgr. object
    /// </summary>
    public static void Save()
    {
        SaveToXML();
        SaveToCache();
    }
    /// <summary>
    /// Tell if manager already has this extension
    /// </summary>
    /// <param name="type">Extension Type</param>
    /// <returns>True if already has</returns>
    public static bool Contains(Type type)
    {
        foreach (ManagedExtension extension in _extensions)
        {
            if (extension.Name == type.Name)
                return true;
        }

        return false;
    }
    /// <summary>
    /// Adds extension to ext. collection in the manager
    /// </summary>
    /// <param name="type">Extension type</param>
    /// <param name="attribute">Extension attribute</param>
    public static void AddExtension(Type type, object attribute)
    {
        ExtensionAttribute xa = (ExtensionAttribute)attribute;
        ManagedExtension x = new ManagedExtension(type.Name, xa.Version, xa.Description, xa.Author);
        _extensions.Add(x);
    }
    #endregion

    #region Private methods
    /// <summary>
    /// If extensions not in the cache will load
    /// from the XML file. If file not exists
    /// will load from assembly using reflection
    /// </summary>
    static void LoadExtensions()
    {   // initialize on application load
        if (HttpContext.Current.Cache["Extensions"] == null)
        {
            if (File.Exists(_fileName))
            {
                LoadFromXML();
                AddNewExtensions();
            }
            else  // very first run
            {
                LoadFromAssembly();
            }
            SaveToXML();
            SaveToCache();
        }
    }
    /// <summary>
    /// Populates extensions collection with
    /// information loaded from assembly
    /// </summary>
    static void LoadFromAssembly()
    {
        string assemblyName = "__code";

        if (Utils.IsMono)
            assemblyName = "App_Code";

        Assembly a = Assembly.Load(assemblyName);
        Type[] types = a.GetTypes();

        foreach (Type type in types)
        {
            object[] attributes = type.GetCustomAttributes(typeof(ExtensionAttribute), false);

            foreach (object attribute in attributes)
            {
                AddExtension(type, attribute);
            }
        }
    }
    /// <summary>
    /// After loading from XML file, checks if
    /// there were new extensions added to application
    /// </summary>
    static void AddNewExtensions()
    {
        string assemblyName = "__code";

        if (Utils.IsMono)
            assemblyName = "App_Code";

        Assembly a = Assembly.Load(assemblyName);
        Type[] types = a.GetTypes();

        foreach (Type type in types)
        {
            object[] attributes = type.GetCustomAttributes(typeof(ExtensionAttribute), false);

            foreach (object attribute in attributes)
            {
                if (!Contains(type))
                    AddExtension(type, attribute);
            }
        }
    }
    #endregion

    #region Settings
    /// <summary>
    /// Method to get settings collection
    /// </summary>
    /// <param name="extensionName">Extension Name</param>
    /// <returns>Collection of settings</returns>
    public static ExtensionSettings GetSettings(string extensionName)
    {
        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == extensionName)
                return x.Settings;
        }

        return null;
    }
    /// <summary>
    /// Will save settings (add to extension object, then
    /// cache and serialize all object hierarhy to XML)
    /// </summary>
    /// <param name="extensionName">Extension Name</param>
    /// <param name="settings">Settings object</param>
    public static void SaveSettings(string extensionName, ExtensionSettings settings)
    {
        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == extensionName)
            {
                x.Settings = settings;
                break;
            }
        }
        Save();
    }
    /// <summary>
    /// Do initial import here.
    /// If already imported, let extension manager take care of settings
    /// To reset, blogger has to delete all settings in the manager
    /// </summary>
    public static bool ImportSettings(ExtensionSettings settings)
    {
        foreach (ManagedExtension x in _extensions)
        {
            if (x.Name == settings.ExtensionName)
            {
                if (x.Settings == null)
                {
                    x.SaveSettings(settings);
                }
                break;
            }
        }

        SaveToCache();

        return SaveToXML();
    }
    #endregion

    #region Serialization
    /// <summary>
    /// Saves ext. manager object to XML file
    /// </summary>
    /// <returns>True if successful</returns>
    public static bool SaveToXML()
    {
        try
        {
            using (TextWriter writer = new StreamWriter(_fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ManagedExtension>));
                serializer.Serialize(writer, _extensions);
                return true;
            }
        }
        catch (Exception e)
        {
            FileAccessException = e;
            return false;
        }
    }
    /// <summary>
    /// Deserializes XML file back to ext. manager object
    /// </summary>
    private static void LoadFromXML()
    {
        TextReader reader = null;
        try
        {
            reader = new StreamReader(_fileName);
            XmlSerializer serializer = new XmlSerializer(typeof(List<ManagedExtension>));
            _extensions = (List<ManagedExtension>)serializer.Deserialize(reader);

            string assemblyName = "__code";
            if (Utils.IsMono)
                assemblyName = "App_Code";

            Assembly a = Assembly.Load(assemblyName);

            for (int i = _extensions.Count - 1; i >= 0; i--)
            {
                if (a.GetType(_extensions[i].Name, false) == null)
                {
                    _extensions.Remove(_extensions[i]);
                }
            }
        }
        catch (Exception)
        {
            // to avoid runtime error. In the
            // worse case defaults will be loaded
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }
    }
    /// <summary>
    /// Caches for performance. If manager cached
    /// and not updates done, chached copy always 
    /// returned
    /// </summary>
    static void SaveToCache()
    {
        HttpContext.Current.Cache.Remove("Extensions");
        HttpContext.Current.Cache["Extensions"] = _extensions;
    }
    #endregion
}
