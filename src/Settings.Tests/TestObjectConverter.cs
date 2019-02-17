using System;
using System.Collections.Generic;
using NUnit.Framework;
using Settings.Tests.TestDoubles;

namespace Settings.Tests
{
    [TestFixture]
    public class TestObjectConverter
    {
        [Test]
        public void Can_convert_string()
        {
            Assert.AreEqual("dummy", new ObjectConverter().FromString<string>("dummy"));
        }

        [Test]
        public void Has_expected_null_string()
        {
            Assert.AreEqual("NULL", ObjectConverter.NullString);
        }

        [Test]
        public void Can_convert_null_string()
        {
            Assert.IsNull(new ObjectConverter().FromString<string>("NULL"));
        }

        [Test]
        public void Has_expected_empty_string()
        {
            Assert.AreEqual("EMPTY", ObjectConverter.EmptyString);
        }

        [Test]
        public void Can_convert_empty_string()
        {
            Assert.IsEmpty(new ObjectConverter().FromString<string>("EMPTY"));
        }

        [Test]
        public void Can_convert_nullable_null_int()
        {
            Assert.IsNull(new ObjectConverter().FromString<int?>("NULL"));
        }

        [Test]
        public void Can_convert_nullable_int()
        {
            Assert.AreEqual(1, new ObjectConverter().FromString<int?>("1"));
        }

        [Test]
        public void Can_convert_enum()
        {
            Assert.AreEqual(ConsoleColor.Black, new ObjectConverter().FromString<ConsoleColor>("BLACK"));
        }

        [Test]
        public void Can_convert_array()
        {
            Assert.AreEqual(new[] {1, 2, 3}, new ObjectConverter().FromString<int[]>("1,2,3"));
        }

        [Test]
        public void Can_convert_null_array()
        {
            Assert.IsNull(new ObjectConverter().FromString<int[]>("NULL"));
        }

        [Test]
        public void Can_convert_empty_array()
        {
            Assert.IsEmpty(new ObjectConverter().FromString<int[]>("EMPTY"));
        }

        [Test]
        public void Can_convert_array_of_empty_and_null_strings()
        {
            Assert.AreEqual(new[] {null, string.Empty}, new ObjectConverter().FromString<string[]>("NULL,EMPTY"));
        }

        [Test]
        public void Can_convert_enumerable()
        {
            Assert.AreEqual(new[] {1, 2, 3}, new ObjectConverter().FromString<IList<int>>("1,2,3"));
        }

        [Test]
        public void Throws_unsupported_type()
        {
            Assert.Throws<InvalidOperationException>(() => new ObjectConverter().FromString<object>("SomeObject"));
        }

        [Test]
        public void Can_add_customer_converter()
        {
            var sut = new ObjectConverter();
            sut.RegisterConverter(new ConverterStub("bar"));

            var result = sut.FromString<string>("foo");

            Assert.AreEqual(result, "bar");
        }
    }
}