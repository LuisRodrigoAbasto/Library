using A.Dynamic.Core.Paginate.Model;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;

namespace A.Dynamic.Core.Paginate.Tools
{
    public static class TypePropertyDescriptor
    {
        public static TypeProperty GetTypeProperty(this PropertyDescriptorCollection property, string column)
        {
            TypeProperty typeProperty = new TypeProperty();
            var propertyType = property[column].PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null)
            {
                typeProperty.isNullable = true;
                typeProperty.type = underlyingType;
            }
            else
            {
                typeProperty.isNullable = false;
                typeProperty.type = propertyType;
            }
            typeProperty.isNumeric = propertyType.GetTypeIsNumeric();
            return typeProperty;
        }
        public static Type GetProperty(this PropertyDescriptorCollection property, string column)
        {
            var propertyType = property[column].PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null) return underlyingType;
            return propertyType;
        }
        public static object GetConvertValue(this PropertyDescriptorCollection property, string column, object value)
        {
            return property.GetProperty(column).GetConvertValue(value);
        }
        public static object GetConvertValue(this Type type, object value)
        {
            try
            {
                if (value == null) return null;
                var valueType = value.GetType();
                if (typeof(JValue) == valueType)
                {
                    value = ((JValue)value).Value;
                }
                if (value == null) return null;

                object newValue = null;
                if (type != valueType)
                {
                    if (type == typeof(decimal)) newValue = Convert.ToDecimal(value);
                    else if (type == typeof(double)) newValue = Convert.ToDouble(value);
                    else if (type == typeof(float)) newValue = Convert.ToSingle(value);
                    else if (type == typeof(long)) newValue = Convert.ToInt64(value);
                    else if (type == typeof(int)) newValue = Convert.ToInt32(value);
                    else if (type == typeof(DateTime)) newValue = Convert.ToDateTime(value);
                    else if (type == typeof(bool)) newValue = Convert.ToBoolean(value);
                    else if (type == typeof(string)) newValue = Convert.ToString(value);
                    else newValue = value;
                }
                else newValue = value;
                return newValue;
            }
            catch
            {
                return null;
            }
        }
        public static object GetConvertValueTruncate(this PropertyDescriptorCollection property, string column, object value)
        {
            Type propertyType = property.GetProperty(column);
            Type valueType = value.GetType();
            object newValue = value;
            if (propertyType != valueType)
            {
                if (propertyType == typeof(decimal)) newValue = Convert.ToDecimal(value);
                else if (propertyType == typeof(double)) newValue = Convert.ToDouble(value);
                else if (propertyType == typeof(float)) newValue = Convert.ToSingle(value);
                else if (propertyType == typeof(int) || propertyType != typeof(long)) newValue = Convert.ToDecimal(value);
            }
            else if (propertyType == typeof(int) || propertyType != typeof(long)) newValue = Convert.ToDecimal(value);
            return newValue;
        }
        public static bool GetTypeIsNullable(this PropertyDescriptorCollection property, string column)
        {
            var propertyType = property[column].PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            if (underlyingType != null) return true;
            return false;
        }
        public static bool GetTypeIsNumeric(this PropertyDescriptorCollection property, string column)
        {
            var propertyType = property.GetProperty(column);
            return propertyType.GetTypeIsNumeric();
        }
        public static bool GetTypeIsNumeric(this Type type)
        {
            if (type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(long)
            || type == typeof(int)) return true;
            else return false;
        }
    }
}
