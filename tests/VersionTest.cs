using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Wheatech.UnitTest
{
    public class VersionTest
    {
        [Theory]
        [InlineData("1.0.0")]
        [InlineData("0.0.1")]
        [InlineData("1.2.3")]
        [InlineData("1.2.3-alpha")]
        [InlineData("1.2.3-X.yZ.3.234.243.32423423.4.23423.4324.234.234.3242")]
        [InlineData("1.2.3-X.yZ.3.234.243.32423423.4.23423+METADATA")]
        [InlineData("1.2.3-X.y3+0")]
        [InlineData("1.2.3-X+0")]
        [InlineData("1.2.3+0")]
        [InlineData("1.2.3-0")]
        public void ParseSemanticVersionStrict(string versionString)
        {
            // Act
            Version semVer;
            Version.TryParse(versionString, out semVer);

            // Assert
            Assert.Equal<string>(versionString, semVer.ToString());
        }

        [Theory]
        [InlineData("1.2.3")]
        [InlineData("1.2.3+0")]
        [InlineData("1.2.3+321")]
        [InlineData("1.2.3+XYZ")]
        public void SemanticVersionStrictEquality(string versionString)
        {
            // Act
            Version main;
            Version.TryParse("1.2.3", out main);

            Version semVer;
            Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(main.Equals(semVer));
            Assert.True(semVer.Equals(main));

            Assert.True(main.GetHashCode() == semVer.GetHashCode());
        }

        [Theory]
        [InlineData("1.2.3-alpha")]
        [InlineData("1.2.3-alpha+0")]
        [InlineData("1.2.3-alpha+10")]
        [InlineData("1.2.3-alpha+beta")]
        public void SemanticVersionStrictEqualityPreRelease(string versionString)
        {
            // Act
            Version main;
            Version.TryParse("1.2.3-alpha", out main);

            Version semVer;
            Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(main.Equals(semVer));
            Assert.True(semVer.Equals(main));

            Assert.True(main.GetHashCode() == semVer.GetHashCode());
        }

        [Theory]
        [InlineData("1.3 .4")]
        [InlineData("1.2.3-A..B")]
        [InlineData("01.2.3")]
        [InlineData("1.02.3")]
        [InlineData("1.2.03")]
        [InlineData(".2.03")]
        [InlineData("1.2.")]
        [InlineData("1.2.3-a$b")]
        [InlineData("a.b.c")]
        [InlineData("1.2.3-00")]
        [InlineData("1.2.3-A.00.B")]
        public void TryParseStrictReturnsFalseIfVersionIsNotStrictSemVer(string version)
        {
            // Act 
            Version semanticVersion;
            var result = Version.TryParse(version, out semanticVersion);

            // Assert
            Assert.False(result);
            Assert.Null(semanticVersion);
        }

        // A normal version number MUST take the form X.Y.Z
        [Theory]
        [InlineData("1", true)]
        [InlineData("1.2", true)]
        [InlineData("1.2.3", true)]
        [InlineData("10.2.3", true)]
        [InlineData("13234.223.32222", true)]
        [InlineData("1.2.3.4", true)]
        [InlineData("1.2. 3", false)]
        [InlineData("1. 2.3", false)]
        [InlineData("X.2.3", false)]
        [InlineData("1.2.Z", false)]
        [InlineData("X.Y.Z", false)]
        public void SemVerVersionMustBe3Parts(string version, bool expected)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(version, out semVer);

            // Assert
            Assert.Equal(expected, valid);
        }

        // X, Y, and Z are non-negative integers
        [Theory]
        [InlineData("-1.2.3")]
        [InlineData("1.-2.3")]
        [InlineData("1.2.-3")]
        public void SemVerVersionNegativeNumbers(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // X, Y, and Z MUST NOT contain leading zeroes
        [Theory]
        [InlineData("01.2.3")]
        [InlineData("1.02.3")]
        [InlineData("1.2.03")]
        [InlineData("00.2.3")]
        [InlineData("1.2.0030")]
        public void SemVerVersionLeadingZeros(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Major version zero (0.y.z) is for initial development
        [Theory]
        [InlineData("0.1.2")]
        [InlineData("1.0.0")]
        [InlineData("0.0.0")]
        public void SemVerVersionValidZeros(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
        }

        // valid release labels
        [Theory]
        [InlineData("0.1.2-Alpha")]
        [InlineData("0.1.2-Alpha.2.34.5.453.345.345.345.345.A.B.bbbbbbb.Csdfdfdf")]
        [InlineData("0.1.2-Alpha-2-5Bdd")]
        [InlineData("0.1.2--")]
        [InlineData("0.1.2--B-C-")]
        [InlineData("0.1.2--B2.-.C.-A0-")]
        [InlineData("0.1.2+NoReleaseLabel")]
        public void SemVerVersionValidReleaseLabels(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
        }

        // Release label identifiers MUST NOT be empty
        [Theory]
        [InlineData("0.1.2-Alpha..2")]
        [InlineData("0.1.2-Alpha.")]
        [InlineData("0.1.2-.AA")]
        [InlineData("0.1.2-")]
        public void SemVerVersionInvalidReleaseId(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Identifiers MUST comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]
        [Theory]
        [InlineData("0.1.2-alp=ha")]
        [InlineData("0.1.2-alp┐jj")]
        [InlineData("0.1.2-a&444")]
        [InlineData("0.1.2-a.&.444")]
        public void SemVerVersionInvalidReleaseLabelChars(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Numeric identifiers MUST NOT include leading zeroes
        [Theory]
        [InlineData("0.1.2-02")]
        [InlineData("0.1.2-2.02")]
        [InlineData("0.1.2-2.A.02")]
        [InlineData("0.1.2-02.A")]
        public void SemVerVersionReleaseLabelZeros(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Numeric identifiers MUST NOT include leading zeroes
        [Theory]
        [InlineData("0.1.2-02A")]
        [InlineData("0.1.2-2.02B")]
        [InlineData("0.1.2-2.A.02-")]
        [InlineData("0.1.2-A02.A")]
        public void SemVerVersionReleaseLabelValidZeros(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
        }

        // Identifiers MUST comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]
        [Theory]
        [InlineData("0.1.2+02A")]
        [InlineData("0.1.2+A")]
        [InlineData("0.1.2+20349244.233.344.0")]
        [InlineData("0.1.2+203-49244.23-3.34-4.0-.-.-")]
        [InlineData("0.1.2+AAaaaaAAAaaaa")]
        [InlineData("0.1.2+-")]
        [InlineData("0.1.2+----.-.-.-")]
        [InlineData("0.1.2----+----")]
        public void SemVerVersionMetadataValidChars(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
        }

        // Identifiers MUST comprise only ASCII alphanumerics and hyphen [0-9A-Za-z-]
        [Theory]
        [InlineData("0.1.2+ÄÄ")]
        [InlineData("0.1.2+22.2ÄÄ")]
        [InlineData("0.1.2+2+A")]
        public void SemVerVersionMetadataInvalidChars(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Identifiers MUST NOT be empty
        [Theory]
        [InlineData("0.1.2+02A.")]
        [InlineData("0.1.2+02..A")]
        [InlineData("0.1.2+")]
        public void SemVerVersionMetadataNonEmptyParts(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.False(valid);
        }

        // Leading zeros are fine for metadata
        [Theory]
        [InlineData("0.1.2+02.02-02")]
        [InlineData("0.1.2+02")]
        [InlineData("0.1.2+02A")]
        [InlineData("0.1.2+000000")]
        public void SemVerVersionMetadataLeadingZeros(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
        }

        [Theory]
        [InlineData("0.1.2+AA-02A")]
        [InlineData("0.1.2+A.-A-02A")]
        public void SemVerVersionMetadataOrder(string versionString)
        {
            // Arrange & act
            Version semVer;
            var valid = Version.TryParse(versionString, out semVer);

            // Assert
            Assert.True(valid);
            Assert.False(semVer.IsPrerelease);
        }

        // Precedence is determined by the first difference when comparing each of these identifiers from left to right as follows: Major, minor, and patch versions are always compared numerically
        [Theory]
        [InlineData("1.2.3", "1.2.4")]
        [InlineData("1.2.3", "2.0.0")]
        [InlineData("9.9.9", "10.1.1")]
        public void SemVerSortVersion(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // a pre-release version has lower precedence than a normal version
        [Theory]
        [InlineData("1.2.3-alpha", "1.2.3")]
        public void SemVerSortRelease(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // identifiers consisting of only digits are compared numerically
        [Theory]
        [InlineData("1.2.3-2", "1.2.3-3")]
        [InlineData("1.2.3-1.9", "1.2.3-1.50")]
        public void SemVerSortReleaseNumeric(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // identifiers with letters or hyphens are compared lexically in ASCII sort order
        [Theory]
        [InlineData("1.2.3-2A", "1.2.3-3A")]
        [InlineData("1.2.3-1.50A", "1.2.3-1.9A")]
        public void SemVerSortReleaseAlpha(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // Numeric identifiers always have lower precedence than non-numeric identifiers
        [Theory]
        [InlineData("1.2.3-999999", "1.2.3-Z")]
        [InlineData("1.2.3-A.999999", "1.2.3-A.56-2")]
        public void SemVerSortNumericAlpha(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // A larger set of pre-release fields has a higher precedence than a smaller set
        [Theory]
        [InlineData("1.2.3-a", "1.2.3-a.2")]
        [InlineData("1.2.3-a.2.3.4", "1.2.3-a.2.3.4.5")]
        public void SemVerSortReleaseLabelCount(string lower, string higher)
        {
            // Arrange & act
            Version lowerSemVer, higherSemVer;
            Version.TryParse(lower, out lowerSemVer);
            Version.TryParse(higher, out higherSemVer);

            // Assert
            Assert.True(VersionComparer.Default.Compare(lowerSemVer, higherSemVer) < 0);
        }

        // ignore release label casing
        [Theory]
        [InlineData("1.2.3-a", "1.2.3-A")]
        [InlineData("1.2.3-A-b2-C", "1.2.3-a-B2-c")]
        public void SemVerSortIgnoreReleaseCasing(string a, string b)
        {
            // Arrange & act
            Version semVerA, semVerB;
            Version.TryParse(a, out semVerA);
            Version.TryParse(b, out semVerB);

            // Assert
            Assert.True(VersionComparer.Default.Equals(semVerA, semVerB));
        }

        [Fact]
        public void SemVerConstructors()
        {
            // Arrange
            var versions = new HashSet<Version>(VersionComparer.Default);

            // act
            versions.Add(new Version("4.3.0"));
            versions.Add(new Version(Version.Parse("4.3.0")));
            versions.Add(new Version(new System.Version(4, 3, 0)));
            versions.Add(new Version(new System.Version(4, 3, 0), string.Empty, string.Empty));
            versions.Add(new Version(4, 3, 0));
            versions.Add(new Version(4, 3, 0, string.Empty));
            versions.Add(new Version(4, 3, 0, null));
            versions.Add(new Version(4, 3, 0, 0));

            versions.Add(new Version(4, 3, 0));
            versions.Add(new Version(4, 3, 0, string.Empty));
            versions.Add(new Version(4, 3, 0, null));

            // Assert
            Assert.Equal(1, versions.Count);
        }

        [Theory]
        [InlineData("1.0.0", "1.0.0.0", "")]
        [InlineData("2.3-alpha", "2.3.0.0", "alpha")]
        [InlineData("3.4.0.3-RC-3", "3.4.0.3", "RC-3")]
        [InlineData("1.0.0-beta.x.y.5.79.0+aa", "1.0.0.0", "beta.x.y.5.79.0")]
        [InlineData("1.0.0-beta.x.y.5.79.0+AA", "1.0.0.0", "beta.x.y.5.79.0")]
        public void StringConstructorParsesValuesCorrectly(string version, string versionValueString, string specialValue)
        {
            // Arrange
            var versionValue = new System.Version(versionValueString);

            // Act
            var semanticVersion = Version.Parse(version);

            // Assert
            Assert.Equal(versionValue, new System.Version(semanticVersion.Major, semanticVersion.Minor, semanticVersion.Patch, semanticVersion.Revision));
            Assert.Equal(specialValue, semanticVersion.Release);
        }

        [Fact]
        public void ParseThrowsIfStringIsNullOrEmpty()
        {
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Version.Parse(null), "value");
            ExceptionAssert.ThrowsArgNullOrEmpty(() => Version.Parse(String.Empty), "value");
        }

        [Theory]
        [InlineData("1beta")]
        [InlineData("1.2Av^c")]
        [InlineData("1.2..")]
        [InlineData("1.2.3.4.5")]
        [InlineData("1.2.3.Beta")]
        [InlineData("1.2.3.4This version is full of awesomeness!!")]
        [InlineData("So.is.this")]
        [InlineData("1.34.2Alpha")]
        [InlineData("1.34.2Release Candidate")]
        [InlineData("1.4.7-")]
        public void ParseThrowsIfStringIsNotAValidSemVer(string versionString)
        {
            ExceptionAssert.ThrowsArgumentException(() => Version.Parse(versionString),
                "value",
                String.Format(CultureInfo.InvariantCulture, "'{0}' is not a valid version string.", versionString));
        }

        [Theory]
        [InlineData("1.22", "1.22.0.0")]
        [InlineData("23.2.3", "23.2.3.0")]
        [InlineData("1.3.42.10133", "1.3.42.10133")]
        public void ParseReadsLegacyStyleVersionNumbers(string versionString, string expectedString)
        {
            // Arrange
            var expected = new Version(new System.Version(expectedString));

            // Act
            var actual = Version.Parse(versionString);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1.22-Beta", "1.22.0.0", "Beta")]
        [InlineData("23.2.3-Alpha", "23.2.3.0", "Alpha")]
        [InlineData("1.3.42.10133-PreRelease", "1.3.42.10133", "PreRelease")]
        [InlineData("1.3.42.200930-RC-2", "1.3.42.200930", "RC-2")]
        public void ParseReadsSemverAndHybridSemverVersionNumbers(string versionString, string expectedString, string releaseString)
        {
            // Arrange
            var expected = new Version(new System.Version(expectedString), releaseString);

            // Act
            var actual = Version.Parse(versionString);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("  1.22-Beta", "1.22.0.0", "Beta")]
        [InlineData("23.2.3-Alpha  ", "23.2.3.0", "Alpha")]
        [InlineData("    1.3.42.10133-PreRelease  ", "1.3.42.10133", "PreRelease")]
        public void ParseIgnoresLeadingAndTrailingWhitespace(string versionString, string expectedString, string releaseString)
        {
            // Arrange
            var expected = new Version(new System.Version(expectedString), releaseString);

            // Act
            var actual = Version.Parse(versionString);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1.0", "1.0.1")]
        [InlineData("1.23", "1.231")]
        [InlineData("1.4.5.6", "1.45.6")]
        [InlineData("1.4.5.6", "1.4.5.60")]
        [InlineData("1.1", "1.10")]
        [InlineData("1.1-alpha", "1.10-beta")]
        [InlineData("1.1.0-RC-1", "1.10.0-rc-2")]
        [InlineData("1.1-RC-1", "1.1")]
        [InlineData("1.1", "1.2-preview")]
        public void SemVerLessThanAndGreaterThanOperatorsWorks(string versionA, string versionB)
        {
            // Arrange
            var itemA = Version.Parse(versionA);
            var itemB = Version.Parse(versionB);
            object objectB = itemB;

            // Act and Assert
            Assert.True(itemA < itemB);
            Assert.True(itemA <= itemB);
            Assert.True(itemB > itemA);
            Assert.True(itemB >= itemA);
            Assert.False(itemA.Equals(itemB));
            Assert.False(itemA.Equals(objectB));
        }

        [Theory]
        [InlineData(new object[] { 1 })]
        [InlineData(new object[] { "1.0.0" })]
        [InlineData(new object[] { new object[0] })]
        public void EqualsReturnsFalseIfComparingANonSemVerType(object other)
        {
            // Arrange
            var semVer = Version.Parse("1.0.0");

            // Act and Assert
            Assert.False(semVer.Equals(other));
        }

        [Theory]
        [InlineData("1.0", "1.0.0.0")]
        [InlineData("1.23.1", "1.23.1")]
        [InlineData("1.45.6", "1.45.6.0")]
        [InlineData("1.45.6-Alpha", "1.45.6-Alpha")]
        [InlineData("1.6.2-BeTa", "1.6.2-beta")]
        [InlineData("22.3.7     ", "22.3.7")]
        public void SemVerEqualsOperatorWorks(string versionA, string versionB)
        {
            // Arrange
            var itemA = Version.Parse(versionA);
            var itemB = Version.Parse(versionB);
            object objectB = itemB;

            // Act and Assert
            Assert.True(itemA == itemB);
            Assert.True(itemA.Equals(itemB));
            Assert.True(itemA.Equals(objectB));
            Assert.True(itemA <= itemB);
            Assert.True(itemB == itemA);
            Assert.True(itemB >= itemA);
        }

        [Fact]
        public void SemVerEqualityComparisonsWorkForNullValues()
        {
            // Arrange
            Version itemA = null;
            Version itemB = null;

            // Act and Assert
            Assert.True(itemA == itemB);
            Assert.True(itemB == itemA);
            Assert.True(itemA <= itemB);
            Assert.True(itemB <= itemA);
            Assert.True(itemA >= itemB);
            Assert.True(itemB >= itemA);
        }

        [Theory]
        [InlineData("1.0.0")]
        [InlineData("1.0.0-b")]
        [InlineData("3.0.1.2")]
        [InlineData("2.1.4.3-pre-1")]
        public void ToStringReturnsOriginalValue(string version)
        {
            // Act
            var semVer = Version.Parse(version);

            // Assert
            Assert.Equal(version, semVer.ToString());
        }

        [Theory]
        [InlineData("1.0.0", null, "1.0.0")]
        [InlineData("1.0.3.120", "", "1.0.3.120")]
        [InlineData("1.0.3.120", "alpha", "1.0.3.120-alpha")]
        [InlineData("1.0.3.120", "rc-2", "1.0.3.120-rc-2")]
        public void ToStringConstructedFromVersionAndSpecialVersionConstructor(string versionString, string specialVersion, string expected)
        {
            // Arrange 
            var version = new System.Version(versionString);

            // Act
            var semVer = new Version(version, specialVersion);

            // Assert
            Assert.Equal(expected, semVer.ToString());
        }

        [Theory]
        [InlineData("1.0.0", null, "1.0.0")]
        [InlineData("1.0.3.120", "", "1.0.3.120")]
        [InlineData("1.0.3.120", "alpha", "1.0.3.120-alpha")]
        [InlineData("1.0.3.120", "rc-2", "1.0.3.120-rc-2")]
        public void ToStringFromStringFormat(string versionString, string specialVersion, string expected)
        {
            // Arrange 
            var version = new System.Version(versionString);

            // Act
            var semVer = new Version(version, specialVersion);

            // Assert
            Assert.Equal(expected, String.Format("{0}", semVer));
        }

        [Fact]
        public void TryParseStrictParsesStrictVersion()
        {
            // Arrange
            var versionString = "1.3.2-CTP-2-Refresh-Alpha";

            // Act
            Version version;
            var result = Version.TryParse(versionString, out version);

            // Assert
            Assert.True(result);
            Assert.Equal(new System.Version("1.3.2.0"), new System.Version(version.Major, version.Minor, version.Patch, version.Revision));
            Assert.Equal("CTP-2-Refresh-Alpha", version.Release);
        }
    }
}
