using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using Settings.Tests.TestDoubles;

namespace Settings.Tests
{
    [TestFixture]
    public class TestSettingsProvider
    {
        [Test]
        public void Ensure_required_settings_object()
        {
            var configuration = A.Configuration.Build();
            var sut = configuration.CreateSettingsProvider();

            Assert.Throws<ArgumentNullException>(() => sut.Populate<TestSettings>(null));
        }

        [Test]
        public void Can_populate_settings_object()
        {
            var configuration = A.Configuration
                .With("TestSettings.MyBool", "True")
                .With("TestSettings.MyString", "String")
                .With("TestSettings.State", "Undefined")
                .With("TestSettings.MyStrings", "A,B,C,D,E,F")
                .Build();
            var sut = configuration.CreateSettingsProvider();

            var settings = sut.Populate(new TestSettings());

            Assert.IsTrue(settings.MyBool);
            Assert.AreEqual("String", settings.MyString);
            Assert.AreEqual(StateType.Undefined, settings.State);
            Assert.AreEqual(new[] {"A", "B", "C", "D", "E", "F"}, settings.MyStrings);
        }

        [Test]
        public void Allow_for_optional_settings()
        {
            var configuration = A.Configuration
                .With("TestSettings.MyBool", "True")
                .With("TestSettings.State", "Undefined")
                .With("TestSettings.MyStrings", "A,B,C,D,E,F")
                .Build();
            var sut = configuration.CreateSettingsProvider();

            var settings = sut.Populate(new TestSettings());

            Assert.IsTrue(settings.MyBool);
            Assert.IsNull(settings.MyString);
            Assert.AreEqual(StateType.Undefined, settings.State);
            Assert.AreEqual(new[] {"A", "B", "C", "D", "E", "F"}, settings.MyStrings);
        }

        [Test]
        public void Report_issues_when_required_values_are_missing()
        {
            var configuration = A.Configuration.Build();
            var spy = new ConversionIssueReporterSpy();
            var sut = configuration.CreateSettingsProvider(options => options.WithIssueReporter(spy));

            var settings = sut.Populate(new TestSettings());

            var conversionIssue = spy.ReportedIssues.First();
            AssertConversionIssue(
                expected: ConversionIssue.For(settings)
                    .WithProperty(A.Property
                        .WithDeclaringTypeName("TestSettings")
                        .WithName("MyBool")
                        .WithType(typeof(bool)))
                    .WithKeys("TestSettings.MyBool")
                    .WithMessage("Required value missing"),
                actual: conversionIssue);
        }

        private static void AssertConversionIssue(ConversionIssue expected, ConversionIssue actual)
        {
            Assert.AreEqual(expected?.ToString(), actual?.ToString());
        }

        [Test]
        public void Report_issues_when_converting_unknown_types()
        {
            var configuration = A.Configuration
                .With("ComplexTestSettings.Nested", "???")
                .Build();
            var spy = new ConversionIssueReporterSpy();
            var sut = configuration.CreateSettingsProvider(options => options.WithIssueReporter(spy));

            var settings = sut.Populate(new ComplexTestSettings());

            var conversionIssue = spy.ReportedIssues.First();
            AssertConversionIssue(
                expected: ConversionIssue.For(settings)
                    .WithProperty(A.Property
                        .WithDeclaringTypeName("ComplexTestSettings")
                        .WithName("Nested")
                        .WithType(typeof(TestSettings)))
                    .WithValue("???")
                    .WithKeys("ComplexTestSettings.Nested")
                    .WithMessage($"Unable to parse type {typeof(TestSettings)}"),
                actual: conversionIssue);
        }

        [Test]
        public void Report_issues_when_failing_to_convert()
        {
            var configuration = A.Configuration
                .With("TestSettings.MyBool", "NotBoolean")
                .With("TestSettings.MyString", "String")
                .With("TestSettings.State", "Undefined")
                .With("TestSettings.MyStrings", "A,B,C,D,E,F")
                .Build();
            var spy = new ConversionIssueReporterSpy();
            var sut = configuration.CreateSettingsProvider(options => options.WithIssueReporter(spy));

            var settings = sut.Populate(new TestSettings());

            var conversionIssue = spy.ReportedIssues.First();
            AssertConversionIssue(
                expected: ConversionIssue.For(settings)
                    .WithProperty(A.Property
                        .WithDeclaringTypeName("TestSettings")
                        .WithName("MyBool")
                        .WithType(typeof(bool)))
                    .WithValue("NotBoolean")
                    .WithKeys("TestSettings.MyBool")
                    .WithException(conversionIssue.Exception)
                    .WithMessage("Failed to convert"),
                actual: conversionIssue);
        }

        [Test]
        public void Can_populate_settings_object_with_multiple_naming_conventions()
        {
            var configuration = A.Configuration
                .With("APP_MY_STRING", "String")
                .With("APP_STATE", "Undefined")
                .With("APP_MY_STRINGS", "A,B,C,D,E,F")
                .With("TestSettings.MyBool", "True")
                .Build();

            var sut = configuration.CreateSettingsProvider(options =>
            {
                options.WithConvention(NamingConvention.DefaultEnvironment("APP"));
            });

            var settings = sut.Populate(new TestSettings());

            Assert.IsTrue(settings.MyBool);
            Assert.AreEqual("String", settings.MyString);
            Assert.AreEqual(StateType.Undefined, settings.State);
            Assert.AreEqual(new[] {"A", "B", "C", "D", "E", "F"}, settings.MyStrings);
        }

        [Test]
        public void Can_use_custom_converter()
        {
            var configuration = A.Configuration.With("SimpleSettings.Value", "foo").Build();
            var sut = configuration.CreateSettingsProvider(options => options.RegisterConverter(new ConverterStub("bar")));
            var simpleSettings = new SimpleSettings();

            sut.Populate(simpleSettings);

            Assert.AreEqual(simpleSettings.Value, "bar");
        }

        public class SimpleSettings
        {
            public string Value { get; set; }
        }

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class TestSettings
        {
            public bool MyBool { get; set; }

            [OptionalSetting]
            public string MyString { get; set; }

            public StateType State { get; set; }
            public string[] MyStrings { get; set; }
        }

        private enum StateType
        {
            Undefined
        }

        private class ComplexTestSettings
        {
            // ReSharper disable once UnusedMember.Local
            public TestSettings Nested { get; set; }
        }
    }
}