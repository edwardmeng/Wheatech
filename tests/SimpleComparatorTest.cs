using Xunit;

namespace Wheatech.UnitTest
{
    public class SimpleComparatorTest
    {
        private VersionOperator ParseOperator(string @operator)
        {
            switch (@operator)
            {
                case "<>":
                case "!=":
                    return VersionOperator.NotEqual;
                case ">":
                    return VersionOperator.GreaterThan;
                case ">=":
                    return VersionOperator.GreaterThanEqual;
                case "<":
                    return VersionOperator.LessThan;
                case "<=":
                    return VersionOperator.LessThanEqual;
                default:
                    return VersionOperator.Equal;
            }
        }

        [Fact]
        public void ParseMajorFloating()
        {
            var comparator = VersionComparator.Parse("*");
            Assert.NotNull(comparator);
            Assert.Equal(VersionFloatBehavior.Major, comparator.FloatBehavior);
            Assert.Equal(VersionOperator.Equal, comparator.Operator);
            Assert.Equal(new Version(0, 0, 0), comparator.Version);
        }

        [Theory]
        [InlineData("1.*", "1.0")]
        [InlineData("1.x", "1.0")]
        [InlineData("v1.*", "1.0")]
        [InlineData("V1.*", "1.0")]
        [InlineData("v1.x", "1.0")]
        [InlineData("V1.x", "1.0")]
        public void ParseMinorFloating(string versionString, string version)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(VersionFloatBehavior.Minor, comparator.FloatBehavior);
            Assert.Equal(Version.Parse(version), comparator.Version);
        }

        [Theory]
        [InlineData("1.234.*", "1.234")]
        [InlineData("1.5.x", "1.5")]
        [InlineData("v1.2.x", "1.2")]
        [InlineData("V1.2.x", "1.2")]
        public void ParsePatchFloating(string versionString, string version)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(VersionFloatBehavior.Patch, comparator.FloatBehavior);
            Assert.Equal(Version.Parse(version), comparator.Version);
        }

        [Theory]
        [InlineData("1.234.0.*", "1.234.0")]
        [InlineData("1.5.3.x", "1.5.3")]
        [InlineData("v1.2.2.x", "1.2.2")]
        [InlineData("V1.2.2.x", "1.2.2")]
        public void ParseRevisionFloating(string versionString, string version)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(VersionFloatBehavior.Revision, comparator.FloatBehavior);
            Assert.Equal(Version.Parse(version), comparator.Version);
        }

        [Theory]
        [InlineData("1.0-alpha*", "1.0", "alpha")]
        [InlineData("1.0-alpha.*", "1.0", "alpha.")]
        [InlineData("1.0-*", "1.0", "")]
        public void ParseReleaseFloating(string versionString, string version, string releasePrefix)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(VersionFloatBehavior.Prerelease, comparator.FloatBehavior);
            Assert.Equal(releasePrefix, comparator.ReleasePrefix);
        }

        [Theory]
        [InlineData("1.*", "=")]
        [InlineData("=1.*", "=")]
        [InlineData("==v1.*", "=")]
        [InlineData(">1.*", ">")]
        [InlineData(">=1.*", ">=")]
        [InlineData("<1.*", "<")]
        [InlineData("<=1.*", "<=")]
        [InlineData("<>1.*", "!=")]
        [InlineData("!=1.*", "!=")]
        public void ParseOperators(string versionString, string op)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(Version.Parse("1.0"), comparator.Version);
            Assert.Equal(ParseOperator(op), comparator.Operator);
        }

        [Theory]
        [InlineData("1.0+", ">=")]
        [InlineData("1.0-", "<=")]
        public void ParseTailOperators(string versionString, string op)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(Version.Parse("1.0"), comparator.Version);
            Assert.Equal(ParseOperator(op), comparator.Operator);
        }

        [Theory]
        [InlineData("*", "0.0.0", true)]
        [InlineData("*", "123.456.789", true)]
        [InlineData("*", "123.456.789-alpha", false)]
        [InlineData("*", "123.456.789-alpha+abcd", false)]
        public void MajorSatisfies(string versionString, string version, bool result)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(result, comparator.Match(Version.Parse(version)));
        }

        [Theory]
        [InlineData("1.x", "1.0.0", true)]
        [InlineData("1.x", "0.9.0", false)]
        [InlineData("1.x", "1.0.0-alpha", false)]
        [InlineData("1.x", "2.1.0", false)]
        [InlineData("1.x", "1.5", true)]
        public void MinorSatisfies(string versionString, string version, bool result)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(result, comparator.Match(Version.Parse(version)));
        }

        [Theory]
        [InlineData("1.0.x", "1.0.0", true)]
        [InlineData("1.0.x", "0.9.0", false)]
        [InlineData("1.0.x", "1.0.0-alpha", false)]
        [InlineData("1.0.x", "2.1.0", false)]
        [InlineData("1.0.x", "1.1", false)]
        [InlineData("1.0.x", "1.0.5", true)]
        public void PatchSatisfies(string versionString, string version, bool result)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(result, comparator.Match(Version.Parse(version)));
        }

        [Theory]
        [InlineData("1.0.5.x", "1.0.5.0", true)]
        [InlineData("1.0.5.x", "1.0.4.0", false)]
        [InlineData("1.0.5.x", "0.9.5.0", false)]
        [InlineData("1.0.5.x", "1.0.5.0-alpha", false)]
        [InlineData("1.0.5.x", "2.1.5.0", false)]
        [InlineData("1.0.5.x", "1.0.6", false)]
        [InlineData("1.0.5.x", "1.0.5.5", true)]
        public void RevisionSatisfies(string versionString, string version, bool result)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(result, comparator.Match(Version.Parse(version)));
        }

        [Theory]
        [InlineData("1.0-alpha.x", "1.0-alpha.0", true)]
        [InlineData("1.0-alpha.x", "1.0-alpha.beta", true)]
        [InlineData("1.0-alpha.x", "1.0-beta", false)]
        [InlineData("1.0-alpha.x", "1.1-alpha.1", false)]
        [InlineData("1.0-alpha.*", "1.0-alpha.0", true)]
        [InlineData("1.0-alpha.*", "1.0-alpha.beta", true)]
        [InlineData("1.0-alpha.*", "1.0-beta", false)]
        [InlineData("1.0-alpha.*", "1.1-alpha.1", false)]
        [InlineData("1.0-alpha*", "1.0-alpha1", true)]
        [InlineData("1.0-alpha*", "1.0-alpha.1", true)]
        [InlineData("1.0-alpha*", "1.0-beta.1", false)]
        [InlineData("1.0-alpha*", "1.1-alpha", false)]
        [InlineData("1.0-*", "1.0-alpha", true)]
        [InlineData("1.0-*", "1.0-beta", true)]
        [InlineData("1.0-*", "1.0", false)]
        [InlineData("1.0-*", "1.1-alpha", false)]
        public void ReleaseSatisfies(string versionString, string version, bool result)
        {
            var comparator = VersionComparator.Parse(versionString);
            Assert.NotNull(comparator);
            Assert.Equal(result, comparator.Match(Version.Parse(version)));
        }
    }
}
