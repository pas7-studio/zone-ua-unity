using NUnit.Framework;
using ZoneUA.EditorValidation;

namespace ZoneUA.EditorValidation.Tests
{
    public sealed class ProductionCompositionTests
    {
        [Test]
        public void Catalog_contains_core_production_rules()
        {
            Assert.That(ProductionCompositionCatalog.Rules.Count, Is.GreaterThanOrEqualTo(6));
        }

        [Test]
        public void Report_is_clean_only_without_missing_or_errors()
        {
            var report = new CompositionMigrationReport();
            Assert.That(report.IsClean, Is.True);
            report.Add(CompositionMigrationStatus.Added, "a", "b", "c");
            Assert.That(report.IsClean, Is.True);
            report.Add(CompositionMigrationStatus.Missing, "a", "b", "c");
            Assert.That(report.IsClean, Is.False);
        }

        [Test]
        public void Runtime_type_resolver_finds_unity_component()
        {
            Assert.That(RuntimeTypeResolver.Resolve("UnityEngine.Transform"), Is.Not.Null);
        }
    }
}
