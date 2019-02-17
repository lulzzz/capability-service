using System;
using System.Diagnostics;
using System.Reflection;

namespace Settings
{
    [DebuggerDisplay("{ToString}")]
    public class Property
    {
        private static readonly Action<object, object> NoOp = (obj, value) => { };
        private static readonly Func<bool> NotOptional = () => false;

        private readonly Func<bool> _isOptionalEval;
        private readonly Action<object, object> _setValueAction;

        internal static Property Create(PropertyInfo propertyInfo)
        {
            return new Property(
                declaringTypeName: propertyInfo.DeclaringType?.Name,
                name: propertyInfo.Name,
                type: propertyInfo.PropertyType,
                isOptionalEval: () => propertyInfo.IsDefined(typeof(OptionalSettingAttribute), true),
                setValueAction: propertyInfo.SetValue);
        }

        internal static Property CreateNull(string declaringTypeName = null, string name = null, Type type = null)
        {
            return new Property(
                declaringTypeName: declaringTypeName,
                name: name,
                type: type,
                isOptionalEval: NotOptional,
                setValueAction: NoOp);
        }

        private Property(string declaringTypeName, string name, Type type, Func<bool> isOptionalEval, Action<object, object> setValueAction)
        {
            Name = name;
            Type = type;
            DeclaringTypeName = declaringTypeName;
            _isOptionalEval = isOptionalEval;
            _setValueAction = setValueAction;
        }

        public string DeclaringTypeName { get; }
        public string Name { get; }
        public Type Type { get; }
        public bool IsOptional => _isOptionalEval();

        internal void SetValue(object obj, object value)
        {
            _setValueAction(obj, value);
        }

        public sealed override string ToString()
        {
            return $"{DeclaringTypeName}.{Name}{(IsOptional ? "?" : "")}:{Type?.FullName}";
        }
    }
}