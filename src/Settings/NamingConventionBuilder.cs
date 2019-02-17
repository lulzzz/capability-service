using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Settings.Tests")]

namespace Settings
{
    public interface INamingConventionBuilder
    {
        void WithCustom(Func<Property, string> converter);
        void WithPrefix(string prefix);
        void WithPrefix(Func<string, string> prefix);
        void PascalCaseToUpperCaseWithUnderscores();
    }

    internal class NamingConventionBuilder : INamingConventionBuilder
    {
        private Func<string> _separator = () => ".";
        private Func<Property, string> _converter;

        public NamingConventionBuilder()
        {
            _converter = property => $"{property.DeclaringTypeName}{_separator()}{property.Name}";
        }

        public void WithCustom(Func<Property, string> converter)
        {
            _converter = converter;
        }

        public void PascalCaseToUpperCaseWithUnderscores()
        {
            _separator = () => "_";

            With(last => Regex.Replace(last(), "(?<lc>[a-z0-9])(?<uc>[A-Z])", match => match.Groups["lc"] + _separator() + match.Groups["uc"]));
            ToUpper();
        }

        private void With(Func<Func<string>, string> converter)
        {
            var last = _converter;
            _converter = property => converter(() => last(property));
        }

        private void ToUpper()
        {
            With(last => last().ToUpper());
        }

        public void WithPrefix(string prefix)
        {
            WithPrefix(separator => prefix + separator);
        }

        public void WithPrefix(Func<string, string> prefix)
        {
            With(last => prefix(_separator()) + last());
        }

        public NamingConvention Build()
        {
            return new NamingConventionImpl(_converter);
        }

        private class NamingConventionImpl : NamingConvention
        {
            private readonly Func<Property, string> _converter;

            public NamingConventionImpl(Func<Property, string> converter)
            {
                _converter = converter;
            }

            public override string GetKey(Property property)
            {
                return _converter(property);
            }
        }
    }
}