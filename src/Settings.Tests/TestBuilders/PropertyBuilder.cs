using System;

namespace Settings.Tests.TestBuilders
{
    internal class PropertyBuilder
    {
        private string _name;
        private Type _type;
        private string _declaringTypeName;

        public PropertyBuilder WithDeclaringTypeName(string declaringTypeName)
        {
            _declaringTypeName = declaringTypeName;
            return this;
        }

        public PropertyBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public PropertyBuilder WithType(Type type)
        {
            _type = type;
            return this;
        }

        public Property Build()
        {
            return Property.CreateNull(_declaringTypeName, _name, _type);
        }

        public static implicit operator Property(PropertyBuilder builder)
        {
            return builder.Build();
        }
    }
}