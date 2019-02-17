using System;

namespace Settings.Tests.TestDoubles
{
    internal class ConverterStub : IConverter
    {
        private readonly string _conversionResult;

        public ConverterStub(string conversionResult)
        {
            _conversionResult = conversionResult;
        }

        public bool Matches(Type type, Func<Type, bool> hasConverter)
        {
            return type == typeof(string);
        }

        public Func<string, object> CreateConverter(Type type, Func<Type, Func<string, object>> converters)
        {
            return s => _conversionResult;
        }
    }
}