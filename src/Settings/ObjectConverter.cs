using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Settings
{
    public interface IConverter
    {
        bool Matches(Type type, Func<Type, bool> hasConverter);
        Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters);
    }

    internal class ObjectConverter
    {
        public const string NullString = "NULL";
        public const string EmptyString = "EMPTY";

        private readonly IList<IConverter> _converters = new List<IConverter>
        {
            new StringConverter(),
            new ArrayConverter(),
            new NullableConverter(),
            new TypeDescriptorConverter()
        };

        public bool HasConverter(Type type)
        {
            return _converters.Any(family => family.Matches(type, HasConverter));
        }

        public T FromString<T>(string stringValue)
        {
            return (T) FromString(stringValue, typeof(T));
        }

        public object FromString(string stringValue, Type type)
        {
            return FindConverter(type)(stringValue);
        }

        private Func<string, object> FindConverter(Type type)
        {
            var matchingConverter = _converters.FirstOrDefault(converter => converter.Matches(type, HasConverter));
            if (matchingConverter != null)
            {
                return matchingConverter.CreateConverter(type, FindConverter);
            }

            throw new InvalidOperationException($"Unable to find a converter implementation for {type.FullName}");
        }

        #region Built-in Converters

        private class StringConverter : IConverter
        {
            public bool Matches(Type type, Func<Type, bool> hasConverter)
            {
                return type == typeof(string);
            }

            public Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters)
            {
                return s =>
                {
                    if (s == EmptyString)
                    {
                        return string.Empty;
                    }
                    else if (s == NullString)
                    {
                        return null;
                    }
                    else
                    {
                        return s;
                    }
                };
            }
        }

        private class NullableConverter : IConverter
        {
            public bool Matches(Type type, Func<Type, bool> hasConverter)
            {
                var isNullable = type.IsGenericType &&
                    !type.IsGenericTypeDefinition &&
                    ReferenceEquals(type.GetGenericTypeDefinition(), typeof(Nullable<>));

                return isNullable && hasConverter(Nullable.GetUnderlyingType(type));
            }

            public Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters)
            {
                return stringValue =>
                {
                    if (stringValue == NullString || stringValue == null)
                    {
                        return null;
                    }

                    var inner = converters(Nullable.GetUnderlyingType(type));

                    return inner(stringValue);
                };
            }
        }

        private class TypeDescriptorConverter : IConverter
        {
            public bool Matches(Type type, Func<Type, bool> hasConverter)
            {
                var typeDescriptor = TypeDescriptor.GetConverter(type);
                return typeDescriptor.CanConvertFrom(typeof(string));
            }

            public Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters)
            {
                var typeDescriptor = TypeDescriptor.GetConverter(type);
                return typeDescriptor.ConvertFromString;
            }
        }

        private class ArrayConverter : IConverter
        {
            public bool Matches(Type type, Func<Type, bool> hasConverter)
            {
                if (type.IsArray && hasConverter(type.GetElementType()))
                {
                    return true;
                }

                return IsGenericEnumerable(type) && hasConverter(type.GetGenericArguments()[0]);
            }

            public Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters)
            {
                var elementType = IsGenericEnumerable(type) ? type.GetGenericArguments()[0] : type.GetElementType();
                if (elementType == null)
                {
                    throw new InvalidOperationException($"Unable to determine element type for {type.Name}");
                }

                var converter = converters(elementType);

                return stringValue =>
                {
                    if (stringValue == NullString || stringValue == null)
                    {
                        return null;
                    }

                    if (stringValue.Equals(EmptyString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return Array.CreateInstance(elementType, 0);
                    }

                    var strings = stringValue.Split(',');
                    var array = Array.CreateInstance(elementType, strings.Length);

                    for (var i = 0; i < strings.Length; i++)
                    {
                        var value = converter(strings[i]);
                        array.SetValue(value, i);
                    }

                    return array;
                };
            }

            private static bool IsGenericEnumerable(Type type)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length != 1)
                {
                    return false;
                }

                var genericType = typeof(IEnumerable<>).MakeGenericType(genericArgs);
                return genericType == type || genericType.IsAssignableFrom(type);
            }
        }

        #endregion

        public void RegisterConverter(IConverter converter)
        {
            _converters.Insert(0, converter);
        }
    }
}