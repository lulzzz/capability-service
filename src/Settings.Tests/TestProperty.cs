using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestProperty
    {
        [Test]
        public void Has_correct_string_representation_of_null_property()
        {
            Property sut = A.Property;

            Assert.AreEqual(".:", sut.ToString());
        }

        [Test]
        public void Has_correct_string_representation_of_property()
        {
            var propertyInfo = ReflectionHelper.Property<DummyClass>(x => x.DummyProperty);

            var sut = Property.Create(propertyInfo);

            Assert.AreEqual("DummyClass.DummyProperty:System.String", sut.ToString());
        }

        [Test]
        public void Has_correct_string_representation_of_optional_property()
        {
            var propertyInfo = ReflectionHelper.Property<DummyClass>(x => x.DummyOptionalProperty);

            var sut = Property.Create(propertyInfo);

            Assert.AreEqual("DummyClass.DummyOptionalProperty?:System.Boolean", sut.ToString());
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class DummyClass
        {
            public string DummyProperty { get; set; }

            [OptionalSetting]
            public bool DummyOptionalProperty { get; set; }
        }
    }
}