using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml.Serialization;

namespace HangFire.Demo1.Models.commom
{
    public static class JsonHelper 
    {
        public static DataTable SetDataTableFromQT<T>(object values, string TableName) where T : class
        {
            DataTable dataTable = new DataTable(TableName);
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                PropertyInfo[] array = properties;
                for (int i = 0; i < array.Length; i++)
                {
                    PropertyInfo propertyInfo = array[i];
                    string text = propertyInfo.Name.ToString();
                    string columnName = (propertyInfo.GetCustomAttributes(false)[0] as XmlElementAttribute).ElementName.ToString();
                    Type propertyType = propertyInfo.PropertyType;
                    dataTable.Columns.Add(columnName, propertyType);
                }
                if (values is List<T>)
                {
                    foreach (T current in (values as List<T>))
                    {
                        DataRow dataRow = dataTable.NewRow();
                        dataRow.BeginEdit();
                        array = properties;
                        for (int i = 0; i < array.Length; i++)
                        {
                            PropertyInfo propertyInfo = array[i];
                            string columnName = (propertyInfo.GetCustomAttributes(false)[0] as XmlElementAttribute).ElementName.ToString();
                            dataRow[columnName] = propertyInfo.GetValue(current, null);
                        }
                        dataRow.EndEdit();
                        dataTable.Rows.Add(dataRow);
                    }
                }
                else if (values is T)
                {
                    DataRow dataRow = dataTable.NewRow();
                    dataRow.BeginEdit();
                    array = properties;
                    for (int i = 0; i < array.Length; i++)
                    {
                        PropertyInfo propertyInfo = array[i];
                        string columnName = (propertyInfo.GetCustomAttributes(false)[0] as XmlElementAttribute).ElementName.ToString();
                        dataRow[columnName] = propertyInfo.GetValue(values, null);
                    }
                    dataRow.EndEdit();
                    dataTable.Rows.Add(dataRow);
                }
            }
            catch (Exception ex)
            {
            }
            return dataTable;
        }
    }
}
