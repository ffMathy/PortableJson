using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;

namespace PortableJson.Xamarin
{
    public static class JsonSerializationHelper
    {
        private static readonly CultureInfo serializationCulture;

        static JsonSerializationHelper()
        {
            serializationCulture = new CultureInfo("en-US");
        }

        /// <summary>
        /// Goes through all targetType's base types to see if one of them match baseType. In other words, to see if targetType inherits from baseType.
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        private static bool IsInheritedBy(Type targetType, Type baseType)
        {
            //if targetType is a generic type (for instance, List<T>), then extract List<> without the generic parameter and use it for comparison.
            if (targetType.IsConstructedGenericType)
            {
                targetType = targetType.GetGenericTypeDefinition();
            }

            //if baseType is a generic type (for instance, List<T>), then extract List<> without the generic parameter and use it for comparison.
            if (baseType.IsConstructedGenericType)
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

        private static bool IsArrayType(Type type)
        {
            return type.IsArray
                    || IsInheritedBy(type, typeof(IEnumerable<>))
                    || IsInheritedBy(type, typeof(IList<>))
                    || IsInheritedBy(type, typeof(ICollection<>))
                    || IsInheritedBy(type, typeof(IReadOnlyCollection<>))
                    || IsInheritedBy(type, typeof(List<>))
                    || IsInheritedBy(type, typeof(HashSet<>))
                    || IsInheritedBy(type, typeof(ReadOnlyCollection<>));
        }

        public static string Serialize<T>(T element)
        {
            var result = string.Empty;

            if (element is string)
            {
                result += "\"";

                result += element
                    .ToString()
                    .Replace("\\", "\\\\")
                    .Replace("\"", "\\\"");

                result += "\"";
            }
            else if (element is int || element is long || element is short)
            {
                result += element.ToString();
            }
            else if (element is float || element is decimal || element is double)
            {
                result += element
                    .ToString()
                    .Replace(",", ".");
            }
            else if (element == null)
            {
                result += "null";
            }
            else
            {
                var type = element.GetType();

                var isArray = IsArrayType(type);
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
                        result += Serialize(value) + ",";
                    }

                    if (properties.Any())
                    {
                        result = result.Substring(0, result.Length - 1);
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
            }

            return result;
        }

        public static T Deserialize<T>(string input)
        {
            return (T)(Deserialize(input, typeof(T)) ?? default(T));
        }

        public static object Deserialize(string input, Type type)
        {

            var instance = (object)null;

            var properties = type
                .GetRuntimeProperties()
                .Where(r => r.CanWrite);

            var inString = false;
            var inEscapeSequence = false;
            var inObject = false;
            var inArray = false;
            var inDeclaration = false;
            var inAssignment = true;

            var nestingLevel = 0;

            var propertyAssigning = (PropertyInfo)null;

            var data = string.Empty;
            foreach (var character in input)
            {
                //ignore some input.
                if (character == ' ') continue;
                if (character == '\"' && inDeclaration) continue;

                //are we in a declaration, or an assignment?
                if (inDeclaration)
                {
                    //are we in an object?
                    if (inObject)
                    {
                        if (character == ':')
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
                }
                else if (inAssignment)
                {
                    var isEndOfAssignment = false;

                    //are we in a string?
                    if (inString)
                    {
                        if (character == '\\' && !inEscapeSequence)
                        {
                            //ignore escape sequences.
                            inEscapeSequence = true;
                        }
                        else
                        {
                            if (inEscapeSequence || character != '\"')
                            {
                                //if the string is not terminating, or we're in an escape sequence, just append the character.
                                data += character;
                            }
                            else
                            {
                                //we're outside of the string now.
                                inString = false;
                            }

                            //let's get out of the escape sequence again. an escape sequence only counts for one character ahead anyway.
                            inEscapeSequence = false;
                        }
                    }
                    else
                    {
                        if (character == '\"')
                        {
                            //if we are not in a string and we encounter a quotation mark, then we're moving into a string.
                            inString = true;
                        }
                        else if (character == '{')
                        {
                            if (nestingLevel == 0)
                            {
                                inObject = true;
                            }

                            nestingLevel++;
                        }
                        else if (character == '[')
                        {
                            if (nestingLevel == 0)
                            {
                                inArray = true;
                            }

                            nestingLevel++;
                        }
                        else if (character == '}')
                        {
                            nestingLevel--;

                            if (nestingLevel == 0)
                            {
                                isEndOfAssignment = true;
                            }
                        }
                        else if (character == ']')
                        {
                            nestingLevel--;

                            if (nestingLevel == 0)
                            {
                                isEndOfAssignment = true;
                            }

                        }
                        else if (character == ',')
                        {
                            if (nestingLevel == 0)
                            {
                                if (inArray)
                                {
                                    inDeclaration = false;
                                    inAssignment = true;
                                }
                                else
                                {
                                    inDeclaration = true;
                                    inAssignment = false;
                                }

                                isEndOfAssignment = true;
                            }
                        }
                    }

                    //should we assign the value?
                    if (isEndOfAssignment)
                    {
                        inAssignment = false;

                        //gather values differently depending on type.
                        if (inObject)
                        {
                            inObject = false;

                            //try to convert the type if possible.
                            var targetType = propertyAssigning.PropertyType;
                            var element = Deserialize(data, targetType);

                            //create a new instance if not already set.
                            instance = instance ?? Activator.CreateInstance(type);
                            propertyAssigning.SetValue(instance, element);

                            //reset the property.
                            propertyAssigning = null;
                        }
                        else if (inArray)
                        {
                            inArray = false;

                            if (character == ']')
                            {
                                //TODO: return list.
                            }
                            else
                            {
                                //TODO: add to list.
                            }
                            throw new NotImplementedException();
                        }
                        else
                        {
                            //we are in no object, and in no array. just return the element.
                            if (type == typeof(string))
                            {
                                return data;
                            }
                            else if (type == typeof(int))
                            {
                                return int.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                            else if (type == typeof(long))
                            {
                                return long.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                            else if (type == typeof(short))
                            {
                                return short.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                            else if (type == typeof(float))
                            {
                                return float.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                            else if (type == typeof(decimal))
                            {
                                return decimal.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                            else if (type == typeof(double))
                            {
                                return double.Parse(data, NumberStyles.Any, serializationCulture);
                            }
                        }

                        data = string.Empty;
                    }
                }
            }

            return null;
        }
    }
}
