using System;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestSettingsSource
    {
        [Test]
        public void Can_get_from_environment()
        {
            Environment.SetEnvironmentVariable("MyClass.MyProperty", "DummyValue");

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var sut = configuration.CreateSettingsProvider();

            var settings = new MyClass();
            sut.Populate(settings);

            Assert.AreEqual("DummyValue", settings.MyProperty);
        }
        
        public class MyClass
        {
            public string MyProperty { get; set; }
        }
    }
}