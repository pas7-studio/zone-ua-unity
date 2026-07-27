using NUnit.Framework;

namespace ZoneUA.EditorValidation.Tests
{
    public sealed class LegacyUsageScannerTests
    {
        [Test]
        public void ScanFile_LegacyInput_AddsBlockingFinding()
        {
            var report = new LegacyUsageReport();
            ZoneUALegacyUsageScanner.ScanFile(
                "Assets/Script/TestController.cs",
                new[] { "float x = Input.GetAxisRaw(\"Horizontal\");" },
                report);

            Assert.That(report.ErrorCount, Is.EqualTo(1));
            Assert.That(report.findings[0].code, Is.EqualTo("LEGACY_INPUT"));
            Assert.That(report.findings[0].line, Is.EqualTo(1));
        }

        [Test]
        public void ScanFile_AllowedEditorPath_IsIgnored()
        {
            var report = new LegacyUsageReport();
            ZoneUALegacyUsageScanner.ScanFile(
                "Assets/_ZoneUA/Editor/InputDebug.cs",
                new[] { "Input.GetKey(KeyCode.Space);" },
                report);

            Assert.That(report.findings, Is.Empty);
        }

        [Test]
        public void ScanFile_CommentedToken_IsIgnored()
        {
            var report = new LegacyUsageReport();
            ZoneUALegacyUsageScanner.ScanFile(
                "Assets/Script/TestController.cs",
                new[] { "// Input.GetKey(KeyCode.Space);" },
                report);

            Assert.That(report.findings, Is.Empty);
        }

        [Test]
        public void ScanFile_DirectHudDependency_IsBlocking()
        {
            var report = new LegacyUsageReport();
            ZoneUALegacyUsageScanner.ScanFile(
                "Assets/Script/TestWeapon.cs",
                new[] { "GlobalSystem.Instance.AmmoUI.Refresh();" },
                report);

            Assert.That(report.IsValid, Is.False);
            Assert.That(report.findings[0].code, Is.EqualTo("DIRECT_HUD_DEPENDENCY"));
        }
    }
}
