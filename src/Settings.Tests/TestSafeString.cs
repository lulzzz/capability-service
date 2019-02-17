using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestSafeString
    {
        [Test]
        public void Ensure_expected_null_string()
        {
            Assert.AreEqual("(null)", SafeString.NullString);
        }

        [Test]
        public void Can_get_safe_null_string()
        {
            Assert.AreEqual(SafeString.NullString, SafeString.GetString(null));
        }

        [Test]
        public void Can_get_safe_string()
        {
            Assert.AreEqual("Dummy", SafeString.GetString("Dummy"));
        }

        [Test]
        public void Can_get_safe_null_type_name()
        {
            Assert.AreEqual(SafeString.NullString, SafeString.GetTypeName(null));
        }

        [Test]
        public void Can_get_safe_type_name()
        {
            Assert.AreEqual("System.Object", SafeString.GetTypeName(new object()));
        }

        [Test]
        public void Can_get_safe_string_list_for_empty_list_of_strings()
        {
            Assert.AreEqual(SafeString.NullString, SafeString.GetStringList());
        }

        [Test]
        public void Can_get_safe_string_list_for_a_single_string()
        {
            Assert.AreEqual("DummyKey", SafeString.GetStringList("DummyKey"));
        }

        [Test]
        public void Can_get_safe_string_list_for_list_of_strings()
        {
            Assert.AreEqual("DummyKey1, DummyKey2", SafeString.GetStringList("DummyKey1", "DummyKey2"));
        }
    }
}