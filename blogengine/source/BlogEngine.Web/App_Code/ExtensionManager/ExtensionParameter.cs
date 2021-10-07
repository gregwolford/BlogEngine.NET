using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Collections.Generic;

/// <summary>
/// Extension Parameter - serializable object
/// that holds parameter attributes and collection
/// of values
/// </summary>
public class ExtensionParameter
{
    #region Private members
    string _name = string.Empty;
    string _label = string.Empty;
    int _maxLength = 100;
    bool _required = false;
    bool _keyField = false;
    StringCollection _values = null;
    #endregion

    #region Public Serializable
    /// <summary>
    /// Parameter Name, often used as ID in the UI
    /// </summary>
    [XmlElement]
    public string Name { get { return _name; } set { _name = value.Trim().Replace(" ", ""); } }
    /// <summary>
    /// Used as label in the UI controls
    /// </summary>
    [XmlElement]
    public string Label { get { return _label; } set { _label = value; } }
    /// <summary>
    /// Maximum number of characters stored in the value fields
    /// </summary>
    [XmlElement]
    public int MaxLength { get { return _maxLength; } set { _maxLength = value; } }
    /// <summary>
    /// Specifies if values for parameter required
    /// </summary>
    [XmlElement]
    public bool Required { get { return _required; } set { _required = value; } }
    /// <summary>
    /// Primary Key field
    /// </summary>
    [XmlElement]
    public bool KeyField { get { return _keyField; } set { _keyField = value; } }
    /// <summary>
    /// Collection of values for given parameter
    /// </summary>
    [XmlElement]
    public StringCollection Values { get { return _values; } set { _values = value; } }
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor required for serialization
    /// </summary>
    public ExtensionParameter() { }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name">Parameter Name</param>
    public ExtensionParameter(string name)
    {
        _name = name;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Add single value to value collection
    /// </summary>
    /// <param name="val">Value</param>
    public void AddValue(string val)
    {
        if (_values == null)
            _values = new StringCollection();

        _values.Add(val);
    }
    /// <summary>
    /// Update value for scalar (single value) parameter
    /// </summary>
    /// <param name="val">Value</param>
    public void UpdateScalarValue(string val)
    {
        if (_values == null)
            _values = new StringCollection();

        _values[0] = val;
    }
    /// <summary>
    /// Delete value in parameter value collection
    /// </summary>
    /// <param name="rowIndex">Index</param>
    public void DeleteValue(int rowIndex)
    {
        _values.RemoveAt(rowIndex);
    }
    #endregion
}
