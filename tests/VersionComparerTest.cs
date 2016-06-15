using System.Collections.Generic;
using Xunit;

namespace Wheatech.UnitTest
{
    public class VersionComparerTest
    {
        private static int Compare(VersionComparer comparer, string version1, string version2)
        {
            // Act
            var x = CompareOneWay(comparer, version1, version2);
            var y = CompareOneWay(comparer, version2, version1) * -1;

            // Assert
            Assert.Equal(x, y);

            return x;
        }

        private static int CompareOneWay(VersionComparer comparer, string version1, string version2)
        {
            // Arrange
            var a = Version.Parse(version1);
            var b = Version.Parse(version2);
            var c = Version.Parse(version1);
            var d = Version.Parse(version2);

            // Act
            var results = new List<int>();
            results.Add(comparer.Compare(a, b));
            results.Add(comparer.Compare(a, d));
            results.Add(comparer.Compare(c, b));
            results.Add(comparer.Compare(c, d));

            // Assert
            Assert.True(results.FindAll(x => x == results[0]).Count == results.Count);

            return results[0];
        }

        private static bool Equals(VersionComparer comparer, string version1, string version2)
        {
            return EqualsOneWay(comparer, version1, version2) && EqualsOneWay(comparer, version2, version1);
        }

        private static bool EqualsOneWay(VersionComparer comparer, string version1, string version2)
        {
            // Arrange
            var a = Version.Parse(version1);
            var b = Version.Parse(version2);
            Version c = Version.Parse(version1);
            Version d = Version.Parse(version2);

            // Act
            var match = Compare(comparer, version1, version2) == 0;
            match &= comparer.Equals(a, b);
            match &= comparer.Equals(a, d);
            match &= comparer.Equals(c, d);
            match &= comparer.Equals(c, b);

            return match;
        }

        [Theory]
        [InlineData("1.0.0", "1.0.0")]
        [InlineData("1.0.0-BETA", "1.0.0-beta")]
        [InlineData("1.0.0-BETA+AA", "1.0.0-beta+aa")]
        [InlineData("1.0.0-BETA+AA", "1.0.0-beta+aa")]
        [InlineData("1.0.0-BETA.X.y.5.77.0+AA", "1.0.0-beta.x.y.5.77.0+aa")]
        public void VersionComparisonDefaultEqual(string version1, string version2)
        {
            // Arrange & Act
            var match = Equals(VersionComparer.Default, version1, version2);

            // Assert
            Assert.True(match);
        }

        [Theory]
        [InlineData("0.0.0", "1.0.0")]
        [InlineData("1.1.0", "1.0.0")]
        [InlineData("1.0.1", "1.0.0")]
        [InlineData("1.0.1", "1.0.0")]
        [InlineData("1.0.0-BETA", "1.0.0-beta2")]
        [InlineData("1.0.0+AA", "1.0.0-beta+aa")]
        [InlineData("1.0.0-BETA+AA", "1.0.0-beta")]
        [InlineData("1.0.0-BETA.X.y.5.77.0+AA", "1.0.0-beta.x.y.5.79.0+aa")]
        public void VersionComparisonDefaultNotEqual(string version1, string version2)
        {
            // Arrange & Act
            var match = !Equals(version1, version2);

            // Assert
            Assert.True(match);
        }

        [Theory]
        [InlineData("0.0.0", "1.0.0")]
        [InlineData("1.0.0", "1.1.0")]
        [InlineData("1.0.0", "1.0.1")]
        [InlineData("1.999.9999", "2.1.1")]
        [InlineData("1.0.0-BETA", "1.0.0-beta2")]
        [InlineData("1.0.0-beta+AA", "1.0.0+aa")]
        [InlineData("1.0.0-BETA", "1.0.0-beta.1+AA")]
        [InlineData("1.0.0-BETA.X.y.5.77.0+AA", "1.0.0-beta.x.y.5.79.0+aa")]
        [InlineData("1.0.0-BETA.X.y.5.79.0+AA", "1.0.0-beta.x.y.5.790.0+abc")]
        public void VersionComparisonDefaultLess(string version1, string version2)
        {
            // Arrange & Act
            var result = Compare(VersionComparer.Default, version1, version2);

            // Assert
            Assert.True(result < 0);
        }
    }
}
