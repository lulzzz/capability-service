using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Settings.Tests.TestBuilders
{
    internal class ConfigurationBuilder
    {
        private readonly IDictionary<string, string> _settings = new Dictionary<string, string>();

        public ConfigurationBuilder With(string key, string value)
        {
            _settings[key] = value;
            return this;
        }

        public IConfiguration Build()
        {
            return new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddInMemoryCollection(_settings)
                .Build();
        }

    }
}