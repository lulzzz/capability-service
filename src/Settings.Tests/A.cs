using Settings.Tests.TestBuilders;

namespace Settings.Tests
{
    internal static class A
    {
        public static PropertyBuilder Property => new PropertyBuilder();
        public static ConfigurationBuilder Configuration => new ConfigurationBuilder();
    }
}