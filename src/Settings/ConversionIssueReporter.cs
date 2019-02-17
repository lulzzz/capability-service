using System;
using System.Collections.Generic;
using System.Text;

namespace Settings
{
    public class ConversionIssueReporter
    {
        public const string ReportHeader = "Could not convert all input values into their expected types:";

        protected IList<ConversionIssue> Issues { get; } = new List<ConversionIssue>();

        public void AddIssue(ConversionIssue problem)
        {
            Issues.Add(problem);
        }

        public virtual void ReportIssues()
        {
            if (Issues.Count == 0)
            {
                return;
            }

            var counter = 0;
            var builder = new StringBuilder();

            builder.AppendFormat(ReportHeader);
            builder.AppendLine();

            foreach (var issue in Issues)
            {
                builder.AppendFormat(GetIssueHeader(++counter));
                builder.AppendLine();
                RenderConversionIssue(builder, issue);
                builder.AppendLine();
            }

            throw new InvalidOperationException(builder.ToString());
        }

        public static string GetIssueHeader(int issueNo)
        {
            return $"---- Problem[{issueNo}] --------------------";
        }

        private static StringBuilder RenderConversionIssue(StringBuilder stringBuilder, ConversionIssue issue)
        {
            var builder = stringBuilder;

            void PrintLine(string fmt, bool appendLine = true)
            {
                builder.Append(fmt);
                if (appendLine)
                {
                    builder.AppendLine();
                }
            }

            PrintLine($"Settings Type:   {SafeString.GetTypeName(issue.Settings)}");
            PrintLine($"Property:        {SafeString.GetString(issue.Property?.Name)}");
            PrintLine($"Property Type:   {SafeString.GetTypeName(issue.Property?.Type)}");
            PrintLine($"Attempted Value: {SafeString.GetString(issue.Value)}");
            PrintLine($"Attempted Keys:  {SafeString.GetStringList(issue.Keys)}");
            PrintLine($"Message:         {SafeString.GetString(issue.Message)}");
            PrintLine($"Exception:       {SafeString.GetString(issue.Exception)}", false);
     
            return builder;
        }

        internal static string RenderConversionIssue(ConversionIssue issue)
        {
            return RenderConversionIssue(new StringBuilder(), issue).ToString();
        }
    }
}