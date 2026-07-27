using NUnit.Framework;

namespace ZoneUA.EditorValidation.Tests
{
    public sealed class ValidationReportTests
    {
        [Test]
        public void EmptyReportIsValid()
        {
            var report = new ValidationReport();

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.ErrorCount, Is.Zero);
            Assert.That(report.WarningCount, Is.Zero);
        }

        [Test]
        public void ErrorMakesReportInvalid()
        {
            var report = new ValidationReport();

            report.Add(ValidationSeverity.Error, "TEST_ERROR", "Failure");

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void WarningDoesNotInvalidateReport()
        {
            var report = new ValidationReport();

            report.Add(ValidationSeverity.Warning, "TEST_WARNING", "Warning");

            Assert.That(report.IsValid, Is.True);
            Assert.That(report.WarningCount, Is.EqualTo(1));
        }

        [Test]
        public void IssuePreservesAssetPathAndCode()
        {
            var report = new ValidationReport();

            report.Add(ValidationSeverity.Info, "TEST_INFO", "Information", "Assets/Test.asset");

            Assert.That(report.issues, Has.Count.EqualTo(1));
            Assert.That(report.issues[0].code, Is.EqualTo("TEST_INFO"));
            Assert.That(report.issues[0].assetPath, Is.EqualTo("Assets/Test.asset"));
        }
    }
}
