using System;

namespace Settings
{
    internal static class SafeString
    {
        public const string NullString = "(null)";

        public static string GetTypeName(object obj)
        {
            return GetTypeName(obj?.GetType());
        }

        public static string GetTypeName(Type type)
        {
            return GetString(type?.FullName);
        }

        public static string GetString(object obj)
        {
            if (obj == null)
            {
                return NullString;
            }

            if (obj is string render)
            {
                return render;
            }

            return obj.ToString();
        }

        public static string GetStringList(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return NullString;
            }

            return string.Join(", ", keys);
        }
    }
}