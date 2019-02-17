using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestNamingConvention
    {
        [Test]
        public void Has_correct_name_for_default_conventions()
        {
            var sut = NamingConvention.Default;

            var result = sut.GetKey(A.Property.WithName("DummyProperty").WithDeclaringTypeName("DummyClass"));

            Assert.AreEqual("DummyClass.DummyProperty", result);
        }

        [Test]
        public void Can_convert_from_pascal_case_to_upper_case_with_underscores()
        {
            var builder = new NamingConventionBuilder();
            builder.PascalCaseToUpperCaseWithUnderscores();
            var sut = builder.Build();

            var result = sut.GetKey(A.Property.WithDeclaringTypeName("DummyClass").WithName("DummyProperty"));

            Assert.AreEqual("DUMMY_CLASS_DUMMY_PROPERTY", result);
        }

        [Test]
        public void Can_prepend_prefix()
        {
            var builder = new NamingConventionBuilder();
            builder.WithPrefix("Prefix");
            var sut = builder.Build();

            var result = sut.GetKey(A.Property.WithDeclaringTypeName("DummyClass").WithName("DummyProperty"));

            Assert.AreEqual("Prefix.DummyClass.DummyProperty", result);
        }

        [Test]
        public void Can_prepend_prefix_with_custom_separator()
        {
            var builder = new NamingConventionBuilder();
            builder.WithPrefix(separator => "Prefix:");
            var sut = builder.Build();

            var result = sut.GetKey(A.Property.WithDeclaringTypeName("DummyClass").WithName("DummyProperty"));

            Assert.AreEqual("Prefix:DummyClass.DummyProperty", result);
        }

        [Test]
        public void Can_use_custom_naming()
        {
            var builder = new NamingConventionBuilder();
            builder.WithCustom(property => property.Name);
            var sut = builder.Build();

            var result = sut.GetKey(A.Property.WithDeclaringTypeName("DummyClass").WithName("DummyProperty"));

            Assert.AreEqual("DummyProperty", result);
        }

        [Test]
        public void Can_use_multiple_naming_conventions()
        {
            var builder = new NamingConventionBuilder();
            builder.WithCustom(property => property.Name);
            builder.WithPrefix("prefix");
            builder.PascalCaseToUpperCaseWithUnderscores();
            var sut = builder.Build();

            var result = sut.GetKey(A.Property.WithDeclaringTypeName("DummyClass").WithName("DummyProperty"));

            Assert.AreEqual("PREFIX_DUMMY_PROPERTY", result);
        }
    }
}