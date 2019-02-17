using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Settings
{
    public static class ConfigurationExtensions
    {
        public static SettingsProvider CreateSettingsProvider(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return configuration.CreateSettingsProvider(new SettingsProviderOptions());
        }

        public static SettingsProvider CreateSettingsProvider(this IConfiguration configuration, string applicationPrefix)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(applicationPrefix))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(applicationPrefix));
            }

            return configuration.CreateSettingsProvider(builder => builder.WithConvention(NamingConvention.DefaultEnvironment(applicationPrefix)));
        }

        public static SettingsProvider CreateSettingsProvider(this IConfiguration configuration, Action<ISettingsProviderOptionsBuilder> optionsBuilder)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            var settingsProviderOptions = new SettingsProviderOptions();
            optionsBuilder(settingsProviderOptions);

            return configuration.CreateSettingsProvider(settingsProviderOptions);
        }

        private static SettingsProvider CreateSettingsProvider(this IConfiguration configuration, SettingsProviderOptions options)
        {
            return new SettingsProvider(configuration, options.ObjectConverter, options.IssueReporter, options.NamingConventions);
        }
    }

    #region Builders

    public interface ISettingsProviderOptionsBuilder
    {
        void RegisterConverter(IConverter converter);
        void WithConvention(NamingConvention convention);
        void WithConvention(Action<INamingConventionBuilder> conventionBuilder);
        void WithIssueReporter(ConversionIssueReporter conversionIssueReporter);
    }

    internal class SettingsProviderOptions : ISettingsProviderOptionsBuilder
    {
        private readonly List<NamingConvention> _namingConventions = new List<NamingConvention>
        {
            NamingConvention.Default
        };

        public ObjectConverter ObjectConverter { get; } = new ObjectConverter();

        public ConversionIssueReporter IssueReporter { get; private set; } = new ConversionIssueReporter();

        public NamingConvention[] NamingConventions => _namingConventions.ToArray();

        public void RegisterConverter(IConverter converter)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            ObjectConverter.RegisterConverter(converter);
        }

        public void WithConvention(NamingConvention convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            _namingConventions.Insert(0, convention);
        }

        public void WithConvention(Action<INamingConventionBuilder> conventionBuilder)
        {
            if (conventionBuilder == null)
            {
                throw new ArgumentNullException(nameof(conventionBuilder));
            }

            var builder = new NamingConventionBuilder();
            conventionBuilder(builder);
            WithConvention(builder.Build());
        }

        public void WithIssueReporter(ConversionIssueReporter conversionIssueReporter)
        {
            IssueReporter = conversionIssueReporter ?? throw new ArgumentNullException(nameof(conversionIssueReporter));
        }
    }

    #endregion

    public class SettingsProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ObjectConverter _objectConverter;
        private readonly ConversionIssueReporter _issueReporter;
        private readonly NamingConvention[] _namingConventions;

        internal SettingsProvider(IConfiguration configuration, ObjectConverter objectConverter, ConversionIssueReporter issueReporter, NamingConvention[] namingConventions)
        {
            _configuration = configuration;
            _objectConverter = objectConverter;
            _issueReporter = issueReporter;
            _namingConventions = namingConventions;
        }

        public T Populate<T>(T settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var properties = GetWritableProperties(settings);

            foreach (var property in properties)
            {
                var stringValue = GetSettingStringValue(property);

                var conversionIssue = ConversionIssue.For(settings)
                        .WithProperty(property)
                        .WithValue(stringValue)
                        .WithKeys(GetAttemptedKeys(property))
                    ;

                if (string.IsNullOrEmpty(stringValue))
                {
                    if (!property.IsOptional)
                    {
                        _issueReporter.AddIssue(conversionIssue.WithMessage("Required value missing"));
                    }

                    continue;
                }

                try
                {
                    if (!_objectConverter.HasConverter(property.Type))
                    {
                        _issueReporter.AddIssue(conversionIssue.WithMessage($"Unable to parse type {property.Type}"));
                        continue;
                    }

                    var value = _objectConverter.FromString(stringValue, property.Type);
                    property.SetValue(settings, value);
                }
                catch (Exception ex)
                {
                    _issueReporter.AddIssue(conversionIssue.WithException(ex).WithMessage("Failed to convert"));
                }
            }

            _issueReporter.ReportIssues();

            return settings;
        }

        private static IEnumerable<Property> GetWritableProperties(object settings)
        {
            var properties = settings
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(propertyInfo => propertyInfo.CanWrite)
                .Select(Property.Create);

            return properties;
        }

        private string GetSettingStringValue(Property property)
        {
            return _namingConventions
                .Select(convention => _configuration[convention.GetKey(property)])
                .FirstOrDefault(stringValue => !string.IsNullOrEmpty(stringValue));
        }

        private string[] GetAttemptedKeys(Property property)
        {
            return _namingConventions
                .Select(namingConvention => namingConvention.GetKey(property))
                .ToArray();
        }
    }

//    public interface ISettingsProviderBuilder
//    {
//        void WithConversionIssueReporter(ConversionIssueReporter conversionIssueReporter);
//        void RegisterConverter(IConverter converter);
//        ISettingsProviderBuilder WithConvention(NamingConvention convention);
//    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionalSettingAttribute : Attribute
    {
    }
}