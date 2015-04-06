using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace PortableJson
{
    public static class JsonSerializationHelper
    {
        /// <summary>
        /// Goes through all targetType's base types to see if one of them match baseType. In other words, to see if targetType inherits from baseType.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        private static bool IsInheritedBy(Type targetType, Type baseType)
        {
            //if targetType is a generic type (for instance, List<T>), then extract List<> without the generic parameter and use it for comparison.
            if (targetType.IsGenericParameter)
            {
                targetType = targetType.GetGenericTypeDefinition();
            }

            //if baseType is a generic type (for instance, List<T>), then extract List<> without the generic parameter and use it for comparison.
            if (baseType.IsGenericParameter)
            {
                baseType = baseType.GetGenericTypeDefinition();
            }

            //do we have a match?
            if (targetType == baseType)
            {
                return true;
            }
            else
            {
                var typeToAnalyzeInformation = targetType.GetTypeInfo();
                if (typeToAnalyzeInformation.BaseType == null)
                {
                    return false;
                }
                else
                {
                    //check targetType's base type, and see if that's equal to baseType instead.
                    return IsInheritedBy(typeToAnalyzeInformation.BaseType, baseType);
                }
            }
        }

        public static string Serialize<T>(T element) where T : class
        {
            var result = string.Empty;

            var type = element.GetType();
            var isArray = IsInheritedBy(type, typeof(IEnumerable));
            if (isArray)
            {
                result += "[";
            }
            else
            {
                result += "{";
            }

            if (isArray)
            {
                var subElements = element as IEnumerable;
                var count = 0;
                foreach (var subElement in subElements)
                {
                    result += Serialize(subElement) + ",";
                    count++;
                }

                if (count > 0)
                {
                    //remove the extra comma.
                    result = result.Substring(0, result.Length - 1);
                }
            }
            else
            {
                var properties = type
                    .GetRuntimeProperties()
                    .Where(p => p.CanRead);
                foreach (var property in properties)
                {
                    result += property.Name + ":";

                    var value = property.GetValue(element);
                    if (value is string)
                    {
                        result += "\"";

                        result += value
                            .ToString()
                            .Replace("\\", "\\\\")
                            .Replace("\"", "\\\"");

                        result += "\"";
                    }
                    else if (value is int || value is long || value is short)
                    {
                        result += value.ToString();
                    }
                    else if (value is float || value is decimal || value is double)
                    {
                        result += value
                            .ToString()
                            .Replace(",", ".");
                    }
                    else if (value == null)
                    {
                        result += "null";
                    }
                    else
                    {
                        result += Serialize(value);
                    }
                }
            }

            if (isArray)
            {
                result += "]";
            }
            else
            {
                result += "}";
            }

            return result;
        }

        public static T Deserialize<T>(string input) where T : new()
        {
            var instance = new T();
            var type = typeof(T);

            var properties = type
                .GetRuntimeProperties()
                .Where(r => r.CanWrite);

            var inString = false;
            var inEscapeSequence = false;
            var inObject = false;
            var inArray = false;
            var inDeclaration = false;
            var inAssignment = true;

            var propertyAssigning = (PropertyInfo)null;

            var data = string.Empty;
            foreach (var character in input)
            {
                if (inString)
                {
                    if (character == '\\' && !inEscapeSequence)
                    {
                        inEscapeSequence = true;
                    }
                    else
                    {
                        if (inEscapeSequence || character != '\"')
                        {
                            data += character;
                        }
                        else
                        {
                            //we're outside of the string now.
                            inString = false;
                        }

                        //escape the escape sequence again.
                        inEscapeSequence = false;
                    }
                }
                else
                {
                    //ignore whitespaces.
                    if (character == ' ') continue;

                    if (inDeclaration)
                    {
                        if (character == '{')
                        {
                            inObject = true;
                            inDeclaration = true;
                        }
                        else if (character == ':')
                        {
                            inDeclaration = false;
                            inAssignment = true;

                            var propertyName = data;
                            data = string.Empty;

                            //do we have a property with this name?
                            var property = properties.SingleOrDefault(p => p.Name == propertyName);
                            if (property != null)
                            {
                                propertyAssigning = property;
                            }
                        }
                    }

                    data += character;
                }
            }

            throw new NotImplementedException();
        }
    }
}
