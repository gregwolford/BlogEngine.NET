using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Collections.Generic;

/// <summary>
/// Serializable object that holds collection of
/// parameters and methods to manipulate them 
/// </summary>
public class ExtensionSettings
{
    #region Private members
    string _extensionName = string.Empty;
    string _settingsHelp = string.Empty;
    char[] _delimiter = null;
    List<ExtensionParameter> _params = null;
    string _keyField = string.Empty;
    bool _isScalar = false;
    StringCollection _requiredFields = new StringCollection();
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor requried for serialization
    /// </summary>
    public ExtensionSettings() { }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extensionName">Extension Name</param>
    public ExtensionSettings(string extensionName)
    {
        _extensionName = extensionName;
        _delimiter = ",".ToCharArray();
    }
    #endregion

    #region Public members
    /// <summary>
    /// Extension name, same as class name
    /// </summary>
    [XmlElement]
    public string ExtensionName { get { return _extensionName; } }
    /// <summary>
    /// Stores information extension author can save to describe 
    /// settings usage. If set, shows up in the settings page
    /// </summary>
    [XmlElement]
    public string Help { get { return _settingsHelp; } set { _settingsHelp = value; } }
    /// <summary>
    /// Used to split string into string array, comma by default
    /// </summary>
    [XmlElement]
    public char[] Delimiter { get { return _delimiter; } set { _delimiter = value; } }
    /// <summary>
    /// Collection of parameters defined by extension writer.
    /// </summary>
    [XmlElement(IsNullable = true)]
    public List<ExtensionParameter> Parameters { get { return _params; } set { _params = value; } }
    /// <summary>
    /// Field used as primary key for settings.
    /// If not defined, first parameter in the collection
    /// set as key field. Unique and required by default.
    /// </summary>
    [XmlElement]
    public string KeyField
    {
        get
        {
            string rval = string.Empty;
            foreach (ExtensionParameter par in _params)
            {
                if (par.KeyField == true)
                {
                    rval = par.Name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(rval))
            {
                rval = _params[0].Name;
            }
            return rval;
        }
        set
        {
            _keyField = value;
        }
    }
    /// <summary>
    /// Returns collection of required parameters
    /// </summary>
    [XmlIgnore]
    public StringCollection RequiredFields
    {
        get
        {
            foreach (ExtensionParameter par in _params)
            {
                if (par.Required == true && !_requiredFields.Contains(par.Name))
                    _requiredFields.Add(par.Name);
            }
            // key field is required by default
            if (!_requiredFields.Contains(KeyField))
                _requiredFields.Add(KeyField);

            return _requiredFields;
        }
    }
    /// <summary>
    /// If true, grid view will not show for settings
    /// and only single text box per parameter will
    /// be added to the form to update and store input
    /// </summary>
    [XmlElement]
    public bool IsScalar { get { return _isScalar; } set { _isScalar = value; } }
    #endregion

    #region Parameter methods
    /// <summary>
    /// Add parameter to settings object by name
    /// rest of the attributes will be set to defaults
    /// </summary>
    /// <param name="name">Parameter Name</param>
    public void AddParameter(string name)
    {
        AddParameter(name, name);
    }
    /// <summary>
    /// Add Parameter to settings
    /// </summary>
    /// <param name="name">Parameter Name</param>
    /// <param name="label">Parameter Label, used in the UI</param>
    public void AddParameter(string name, string label)
    {
        AddParameter(name, label, 100);
    }
    /// <summary>
    /// Add Parameter to settings
    /// </summary>
    /// <param name="name">Parameter Name</param>
    /// <param name="label">Parameter Label</param>
    /// <param name="maxLength">Maximum number of chars values will hold</param>
    public void AddParameter(string name, string label, int maxLength)
    {
        AddParameter(name, label, maxLength, false);
    }
    /// <summary>
    /// Add Parameter to settings
    /// </summary>
    /// <param name="name">Parameter Name</param>
    /// <param name="label">Parameter Label</param>
    /// <param name="maxLength">Maximum Length</param>
    /// <param name="required">Set if value in the parameter required when added/apdated</param>
    public void AddParameter(string name, string label, int maxLength, bool required)
    {
        AddParameter(name, label, maxLength, required, false);
    }
    /// <summary>
    /// Add Parameter
    /// </summary>
    /// <param name="name">Parameter Name</param>
    /// <param name="label">Parameter Label</param>
    /// <param name="maxLength">Maximum Length</param>
    /// <param name="required">Set if value in the parameter required when added/apdated</param>
    /// <param name="keyfield">Mark field as primary key, unique and required</param>
    public void AddParameter(string name, string label, int maxLength, bool required, bool keyfield)
    {
        if (_params == null)
            _params = new List<ExtensionParameter>();

        ExtensionParameter par = new ExtensionParameter(name);
        par.Label = label;
        par.MaxLength = maxLength;
        par.Required = required;
        par.KeyField = keyfield;

        _params.Add(par);
    }
    /// <summary>
    /// Returns true if values in the parameter required
    /// </summary>
    /// <param name="paramName">Parameter Name</param>
    /// <returns></returns>
    public bool IsRequiredParameter(string paramName)
    {
        if (RequiredFields.Contains(paramName))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Returns true if value that user entered
    /// exists in the parameter used as primary key
    /// </summary>
    /// <param name="newVal">Value entered</param>
    /// <returns>True if value exists</returns>
    public bool IsKeyValueExists(string newVal)
    {
        foreach (ExtensionParameter par in _params)
        {
            if (par.Name == KeyField)
            {
                foreach (string val in par.Values)
                {
                    if (val == newVal)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Compare value in the parameters collection
    /// with one in the grid. Return true if value 
    /// in the grid is the same (old value).
    /// </summary>
    /// <param name="parName">Parameter Name</param>
    /// <param name="val">Value in the grid view</param>
    /// <param name="rowIndex">Row in the grid view</param>
    /// <returns></returns>
    public bool IsOldValue(string parName, string val, int rowIndex)
    {
        string oldVal = string.Empty;

        foreach (ExtensionParameter par in _params)
        {
            if (par.Name.ToLower() == parName.ToLower())
            {
                if (par.Values != null)
                    if (rowIndex <= par.Values.Count)
                        oldVal = par.Values[rowIndex];

                if (oldVal == val)
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// Method to get description of the parameter
    /// </summary>
    /// <param name="parameterName">Parameter Name</param>
    /// <returns>Parameter Description(label)</returns>
    public string GetLabel(string parameterName)
    {
        foreach (ExtensionParameter par in _params)
        {
            if (par.Name.ToLower() == parameterName.ToLower())
            {
                return par.Label;
            }
        }
        return string.Empty;
    }
    #endregion

    #region Values Methods
    /// <summary>
    /// Appends value to parameter value collection
    /// </summary>
    /// <param name="parameterName">Parameter Name</param>
    /// <param name="val">Value</param>
    public void AddValue(string parameterName, string val)
    {
        foreach (ExtensionParameter par in _params)
        {
            if (par.Name == parameterName)
            {
                par.AddValue(val);
                break;
            }
        }
    }
    /// <summary>
    /// Update parameter that has only one value
    /// </summary>
    /// <param name="parameterName">Parameter Name</param>
    /// <param name="val">Value</param>
    public void UpdateScalarValue(string parameterName, string val)
    {
        foreach (ExtensionParameter par in _params)
        {
            if (par.Name == parameterName)
            {
                par.UpdateScalarValue(val);
                break;
            }
        }
    }
    /// <summary>
    /// Add values to parameter value collection
    /// </summary>
    /// <param name="values">Values as array of strings</param>
    public void AddValues(string[] values)
    {
        if (_params.Count > 0)
        {
            for (int i = 0; i < _params.Count; i++)
            {
                _params[i].AddValue(values[i]);
            }
        }
    }
    /// <summary>
    /// Add values to parameter value collection
    /// </summary>
    /// <param name="values">String collection of values</param>
    public void AddValues(StringCollection values)
    {
        if (_params.Count > 0)
        {
            for (int i = 0; i < _params.Count; i++)
            {
                _params[i].AddValue(values[i]);
            }
        }
    }
    /// <summary>
    /// Parameters with multiple values formatted
    /// as data table where column names are parameter
    /// names and collection of values are data rows.
    /// In the UI bound to the data grid view
    /// </summary>
    /// <returns>Data table</returns>
    public System.Data.DataTable GetDataTable()
    {
        System.Data.DataTable objDataTable = new System.Data.DataTable();
        foreach (ExtensionParameter p in _params)
        {
            objDataTable.Columns.Add(p.Name, string.Empty.GetType());
        }

        if (_params[0].Values != null)
        {
            for (int i = 0; i < _params[0].Values.Count; i++)
            {
                string[] row = new string[_params.Count];
                for (int j = 0; j < _params.Count; j++)
                {
                    row[j] = _params[j].Values[i];
                }
                objDataTable.Rows.Add(row);
            }
        }

        return objDataTable;
    }
    /// <summary>
    /// Method to get vaule for scalar parameter
    /// </summary>
    /// <param name="parameterName">Parameter Name</param>
    /// <returns>First value in the values collection</returns>
    public string GetSingleValue(string parameterName)
    {
        foreach (ExtensionParameter par in _params)
        {
            if (par.Name.ToLower() == parameterName.ToLower())
            {
                if (par.Values != null)
                    return par.Values[0];
            }
        }
        return string.Empty;
    }

    #endregion

}