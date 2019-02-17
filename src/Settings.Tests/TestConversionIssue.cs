using System;
using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestConversionIssue
    {
        [Test]
        public void Ensure_required_settings_object()
        {
            Assert.Throws<ArgumentNullException>(() => ConversionIssue.For(null));
        }

        [Test]
        public void Can_build_empty_conversion_issue()
        {
            var dummySettings = new object();
            var sut = ConversionIssue.For(dummySettings);

            Assert.AreEqual(expected:
                $"Settings: {SafeString.GetTypeName(dummySettings)}, " +
                $"Property: {SafeString.NullString}, " +
                $"Value: {SafeString.NullString}, " +
                $"Keys: {SafeString.NullString}, " +
                $"Exception: {SafeString.NullString}, " +
                $"Message: ",
                actual: sut.ToString());
        }

        [Test]
        public void Can_build_conversion_issue()
        {
            var dummySettings = new object();
            var sut = ConversionIssue.For(dummySettings)
                .WithProperty(A.Property)
                .WithValue("DummyValue")
                .WithKeys("Key1", "Key2")
                .WithException(new FormatException())
                .WithMessage("Dummy Message");

            Assert.AreEqual(expected:
                $"Settings: {SafeString.GetTypeName(dummySettings)}, " +
                $"Property: {Property.CreateNull()}, " +
                $"Value: DummyValue, " +
                $"Keys: Key1, Key2, " +
                $"Exception: {SafeString.GetTypeName(typeof(FormatException))}, " +
                $"Message: Dummy Message",
                actual: sut.ToString());
        }
    }
}