using NUnit.Framework;

namespace ZoneUA.EditorValidation.Tests
{
    public sealed class ContentStandardsTests
    {
        [Test]
        public void EmptyReport_IsValid()
        {
            ContentStandardsReport report = new ContentStandardsReport();

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.ErrorCount, Is.Zero);
            Assert.That(report.WarningCount, Is.Zero);
        }

        [Test]
        public void Warning_DoesNotInvalidateReport()
        {
            ContentStandardsReport report = new ContentStandardsReport();
            report.Add(ValidationSeverity.Warning, "TEST_WARNING", "warning");

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.WarningCount, Is.EqualTo(1));
        }

        [Test]
        public void Error_InvalidatesReport()
        {
            ContentStandardsReport report = new ContentStandardsReport();
            report.Add(ValidationSeverity.Error, "TEST_ERROR", "error", "Assets/Test.prefab");

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.ErrorCount, Is.EqualTo(1));
            Assert.That(report.issues[0].assetPath, Is.EqualTo("Assets/Test.prefab"));
        }
    }
}
