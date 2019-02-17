using System;
using NUnit.Framework;

namespace Settings.Tests
{
    [TestFixture]
    public class TestConversionIssueReporter
    {
        [Test]
        public void Can_report_on_no_issues()
        {
            var sut = new ConversionIssueReporter();

            sut.ReportIssues();
        }

        [Test]
        public void Can_report_on_issues()
        {
            var dummyIssue1 = ConversionIssue.For(new object());
            var dummyIssue2 = ConversionIssue.For(new object());
            var sut = new ConversionIssueReporter();

            sut.AddIssue(dummyIssue1);
            sut.AddIssue(dummyIssue2);

            var invalidOperationException = Assert.Throws<InvalidOperationException>(() => sut.ReportIssues());

            Assert.AreEqual(expected: $@"{ConversionIssueReporter.ReportHeader}
{ConversionIssueReporter.GetIssueHeader(1)}
{ConversionIssueReporter.RenderConversionIssue(dummyIssue1)}
{ConversionIssueReporter.GetIssueHeader(2)}
{ConversionIssueReporter.RenderConversionIssue(dummyIssue2)}
",
                actual: invalidOperationException.Message);
        }

        [Test]
        public void Has_correct_report_header()
        {
            Assert.AreEqual("Could not convert all input values into their expected types:", ConversionIssueReporter.ReportHeader);
        }

        [Test]
        public void Can_get_issue_header()
        {
            Assert.AreEqual("---- Problem[1] --------------------", ConversionIssueReporter.GetIssueHeader(1));
        }

        [Test]
        public void Can_render_null_conversion_issue()
        {
            var dummySettings = new object();
            var dummyIssue = ConversionIssue.For(dummySettings);
            Assert.AreEqual(expected: $@"Settings Type:   {SafeString.GetTypeName(dummyIssue.Settings)}
Property:        {SafeString.NullString}
Property Type:   {SafeString.NullString}
Attempted Value: {SafeString.NullString}
Attempted Keys:  {SafeString.NullString}
Message:         {SafeString.NullString}
Exception:       {SafeString.NullString}",
                actual: ConversionIssueReporter.RenderConversionIssue(dummyIssue));
        }

        [Test]
        public void Can_render_conversion_issue()
        {
            var dummySettings = new object();
            var dummyProperty = A.Property.WithName("DummyProperty").WithType(typeof(bool)).Build();
            var dummyKeys = new[] {"Key1", "Key2"};
            var dummyProviders = new[] {"Provider1", "Provider2"};
            var dummyException = new Exception("Expected");
            var dummyIssue = ConversionIssue.For(dummySettings)
                .WithProperty(dummyProperty)
                .WithValue("DummyValue")
                .WithKeys(dummyKeys)
                .WithException(dummyException)
                .WithMessage("Dummy Message");

            Assert.AreEqual(expected: $@"Settings Type:   {SafeString.GetTypeName(dummyIssue.Settings)}
Property:        {dummyProperty.Name}
Property Type:   {SafeString.GetTypeName(dummyProperty.Type)}
Attempted Value: DummyValue
Attempted Keys:  {SafeString.GetStringList(dummyKeys)}
Message:         Dummy Message
Exception:       {dummyException}",
                actual: ConversionIssueReporter.RenderConversionIssue(dummyIssue));
        }
    }
}