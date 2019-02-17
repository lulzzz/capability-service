using System.Collections.Generic;

namespace Settings.Tests.TestDoubles
{
    internal class ConversionIssueReporterSpy : ConversionIssueReporter
    {
        public override void ReportIssues()
        {
        }

        public IEnumerable<ConversionIssue> ReportedIssues => Issues;
    }
}