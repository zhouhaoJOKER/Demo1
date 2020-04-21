using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace HangFire.Demo1.Models.CenterPlatform
{
    public class RequestBuilder
    {
        protected static T GetEntity<T>(XmlNode node) where T : class
        {
            Type t = typeof(T);
            T entity = t.Assembly.CreateInstance(t.FullName) as T;
            XmlNodeList childNodes = node.ChildNodes;
            foreach (XmlNode childNode in childNodes)
            {
                PropertyInfo propInfo = t.GetProperty(childNode.Name);
                if (propInfo != null)
                {
                    object value = GetValue(propInfo.PropertyType, childNode.InnerText);
                    if (value != null)
                    {
                        propInfo.SetValue(entity, value, null);
                    }
                }
            }
            return entity;
        }

        private static object GetValue(Type type, string text)
        {
            if (type.FullName == "System.Guid")
            {
                try
                {
                    return new Guid(text);
                }
                catch
                {
                    return Guid.NewGuid();
                }
            }
            try
            {
                return Convert.ChangeType(text, type);
            }
            catch
            {
                return null;
            }
        }
        public static Dictionary<string, object> getProperties<T>(T t, string ObjectName)
        {
            Dictionary<string, object> list = new Dictionary<string, object>();
            string tStr = string.Empty;
            if (t == null)
            {
                return list;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (properties.Length <= 0)
            {
                return list;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                if (name == "InvoicesMX")
                {
                    continue;
                } 
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    if (value != null)
                    {
                        if (name.ToLower().Equals("bz"))
                        {
                            if (value.ToString().Length > 150)
                            {
                                value = value.ToString().Substring(0, 150);
                            }
                        } 
                        if (name.ToLower().Equals("rq_4"))
                        {
                            value = DateTime.Parse(value.ToString()).ToString("yyyy-MM-dd");
                        }
                        list.Add(name, value);
                    }
                }
                else
                {
                    getProperties(value, ObjectName);
                }
            }
            return list;
        }
    }
}
