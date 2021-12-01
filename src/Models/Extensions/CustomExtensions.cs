using Models.CustomAttributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Models.Extensions
{
    public static class CustomExtensions
    {
        public static PropertyInfo[] GetFilteredProperties(this Type type)
        {
            return type.GetProperties().Where(pi => pi.GetCustomAttributes(typeof(SkipPropertyAttribute), true).Length == 0).ToArray();
        }

        public static string GetPropertyDisplayName(this PropertyInfo property)
        {
            return property.GetCustomAttribute<DisplayAttribute>() != null ? property.GetCustomAttribute<DisplayAttribute>().Name : property.Name;
        }
    }
}
