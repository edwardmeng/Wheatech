using System;
using System.Linq;
using Xunit;

namespace Wheatech.UnitTest
{
    public class CompositeComparatorTest
    {
        [Theory]
        [InlineData("[1.0.x, 2.9.x]", "1.0.x", "2.9.x", true, true)]
        [InlineData("(1.0.x, 2.9.x]", "1.0.x", "2.9.x", false, true)]
        [InlineData("[1.0.x, 2.9.x)", "1.0.x", "2.9.x", true, false)]
        [InlineData("(1.0.x, 2.9.x)", "1.0.x", "2.9.x", false, false)]
        public void ParseByBrakets(string comparatorString, string lowerBoundary, string upperBoundary, bool includeLowerBound, bool includeUpperBound)
        {
            var comparator = Assert.IsType<VersionCompositeComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(2, comparator.Count);
            var lowerComparator = Assert.IsType<VersionComparator>(comparator.First());
            var upperComparator = Assert.IsType<VersionComparator>(comparator[1]);
            var lowerVersion = VersionComparator.Parse(lowerBoundary);
            var upperVersion = VersionComparator.Parse(upperBoundary);
            Assert.Equal(lowerVersion.Version, lowerComparator.Version);
            Assert.Equal(upperVersion.Version, upperComparator.Version);
            Assert.Equal(lowerVersion.FloatBehavior, lowerComparator.FloatBehavior);
            Assert.Equal(upperVersion.FloatBehavior, upperComparator.FloatBehavior);
            Assert.Equal(lowerVersion.ReleasePrefix, lowerComparator.ReleasePrefix);
            Assert.Equal(upperVersion.ReleasePrefix, upperComparator.ReleasePrefix);

            Assert.Equal(includeLowerBound ? VersionOperator.GreaterThanEqual : VersionOperator.GreaterThan, lowerComparator.Operator);
            Assert.Equal(includeUpperBound ? VersionOperator.LessThanEqual : VersionOperator.LessThan, upperComparator.Operator);
        }

        [Theory]
        [InlineData("[1.0.x, ]", "1.0.x", true, false)]
        [InlineData("[1.0.x, )", "1.0.x", true, false)]
        [InlineData("(1.0.x, )", "1.0.x", false, false)]
        [InlineData("( 1.0.x, ]", "1.0.x", false, false)]
        [InlineData("[ , 1.0.x ]", "1.0.x", true, true)]
        [InlineData("[ , 1.0.x)", "1.0.x", false, true)]
        [InlineData("( , 1.0.x)", "1.0.x", false, true)]
        [InlineData("( ,  1.0.x]", "1.0.x", true, true)]
        public void ParseMissingBoundaryByBrakets(string comparatorString, string boundaryString, bool includeBound, bool missingLower)
        {
            var comparator = Assert.IsType<VersionComparator>(VersionComparatorFactory.Parse(comparatorString));
            var boundary = VersionComparator.Parse(boundaryString);
            Assert.Equal(boundary.Version, comparator.Version);
            Assert.Equal(boundary.FloatBehavior, comparator.FloatBehavior);
            Assert.Equal(boundary.ReleasePrefix, comparator.ReleasePrefix);
            Assert.Equal(missingLower ?
                includeBound ? VersionOperator.LessThanEqual : VersionOperator.LessThan :
                includeBound ? VersionOperator.GreaterThanEqual : VersionOperator.GreaterThan, comparator.Operator);
        }

        [Theory]
        [InlineData("1.0.x - 2.9.x", "1.0.x", "2.9.x")]
        [InlineData("1.0.x - 2.9.x", "1.0.x", "2.9.x")]
        public void ParseByHyphen(string comparatorString, string lowerBoundary, string upperBoundary)
        {
            var comparator = Assert.IsType<VersionCompositeComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(2, comparator.Count);
            var lowerComparator = Assert.IsType<VersionComparator>(comparator.First());
            var upperComparator = Assert.IsType<VersionComparator>(comparator[1]);
            var lowerVersion = VersionComparator.Parse(lowerBoundary);
            var upperVersion = VersionComparator.Parse(upperBoundary);
            Assert.Equal(lowerVersion.Version, lowerComparator.Version);
            Assert.Equal(upperVersion.Version, upperComparator.Version);
            Assert.Equal(lowerVersion.FloatBehavior, lowerComparator.FloatBehavior);
            Assert.Equal(upperVersion.FloatBehavior, upperComparator.FloatBehavior);
            Assert.Equal(lowerVersion.ReleasePrefix, lowerComparator.ReleasePrefix);
            Assert.Equal(upperVersion.ReleasePrefix, upperComparator.ReleasePrefix);

            Assert.Equal(VersionOperator.GreaterThanEqual, lowerComparator.Operator);
            Assert.Equal(VersionOperator.LessThanEqual, upperComparator.Operator);
        }

        [Theory]
        [InlineData("1.0.x -", "1.0.x", false)]
        [InlineData("1.0.x-", "1.0.x", false)]
        [InlineData("- 1.0.x", "1.0.x", true)]
        [InlineData("-1.0.x", "1.0.x", true)]
        public void ParseMissingBoundaryByHyphen(string comparatorString, string boundaryString, bool missingLower)
        {
            var comparator = Assert.IsType<VersionComparator>(VersionComparatorFactory.Parse(comparatorString));
            var boundary = VersionComparator.Parse(boundaryString);
            Assert.Equal(boundary.Version, comparator.Version);
            Assert.Equal(boundary.FloatBehavior, comparator.FloatBehavior);
            Assert.Equal(boundary.ReleasePrefix, comparator.ReleasePrefix);
            Assert.Equal(missingLower ? VersionOperator.LessThanEqual : VersionOperator.GreaterThanEqual, comparator.Operator);
        }

        [Theory]
        [InlineData("~1.2", "1.2.x")]
        [InlineData("~1", "1.x")]
        [InlineData("~0.2", "0.2.x")]
        [InlineData("~0", "0.x")]
        public void ParseMajorAndMinorByTilde(string comparatorString, string boundaryString)
        {
            var comparator = Assert.IsType<VersionComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(VersionComparator.Parse(boundaryString), comparator);
        }

        [Theory]
        [InlineData("~1.2.3", ">=1.2.3", "<=1.2.x")]
        [InlineData("~0.2.3", ">=0.2.3", "<=0.2.x")]
        public void ParsePatchByTilde(string comparatorString, string lowerBoundary, string upperBoundary)
        {
            var comparator = Assert.IsType<VersionCompositeComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(2, comparator.Count);
            var lowerComparator = Assert.IsType<VersionComparator>(comparator.First());
            var upperComparator = Assert.IsType<VersionComparator>(comparator[1]);
            Assert.Equal(VersionComparator.Parse(lowerBoundary), lowerComparator);
            Assert.Equal(VersionComparator.Parse(upperBoundary), upperComparator);
        }

        [Fact]
        public void ParsePrereleaseByTilde()
        {
            var comparator = VersionComparatorFactory.Parse("~1.2.3-beta.2");
            Assert.True(comparator.Match(Version.Parse("1.2.3")));
            Assert.True(comparator.Match(Version.Parse("1.2.3-beta.4")));
            Assert.False(comparator.Match(Version.Parse("1.2.4-beta.2")));
            Assert.False(comparator.Match(Version.Parse("1.3.0")));
            Assert.False(comparator.Match(Version.Parse("1.2.2")));
        }

        [Theory]
        [InlineData("^1.2.3", ">=1.2.3", "<=1.x")]
        [InlineData("^0.2.3", ">=0.2.3", "<=0.2.x")]
        [InlineData("^0.0.3", ">=0.0.3", "<=0.0.3.x")]
        public void ParseVersionByCaret(string comparatorString, string lowerBoundary, string upperBoundary)
        {
            var comparator = Assert.IsType<VersionCompositeComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(2, comparator.Count);
            var lowerComparator = Assert.IsType<VersionComparator>(comparator.First());
            var upperComparator = Assert.IsType<VersionComparator>(comparator[1]);
            Assert.Equal(VersionComparator.Parse(lowerBoundary), lowerComparator);
            Assert.Equal(VersionComparator.Parse(upperBoundary), upperComparator);
        }

        [Fact]
        public void ParsePrereleaseByCaret()
        {
            var comparator = VersionComparatorFactory.Parse("^1.2.3-beta.2");
            Assert.True(comparator.Match(Version.Parse("1.2.3")));
            Assert.True(comparator.Match(Version.Parse("1.2.3-beta.4")));
            Assert.False(comparator.Match(Version.Parse("1.2.4-beta.2")));
            Assert.True(comparator.Match(Version.Parse("1.3.0")));
            Assert.False(comparator.Match(Version.Parse("2.0.0")));

            comparator = VersionComparatorFactory.Parse("^0.0.3-beta");
            Assert.True(comparator.Match(Version.Parse("0.0.3")));
            Assert.True(comparator.Match(Version.Parse("0.0.3-pr.2")));
            Assert.False(comparator.Match(Version.Parse("0.0.4")));
        }

        [Theory]
        [InlineData("1.2.3")]
        [InlineData("=1.2.3")]
        [InlineData("==1.2.3")]
        [InlineData(">1.2.3")]
        [InlineData(">=1.2.3")]
        [InlineData("<1.2.3")]
        [InlineData("<=1.2.3")]
        [InlineData("<>1.2.3")]
        [InlineData("!=1.2.3")]
        public void ParseSimpleComparator(string comparatorString)
        {
            var comparator = Assert.IsType<VersionComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(VersionComparator.Parse(comparatorString), comparator);
        }

        private VersionCompositor ParseCompositor(string compositor)
        {
            switch (compositor)
            {
                case "||":
                    return VersionCompositor.Or;
                case "&&":
                    return VersionCompositor.And;
                default:
                    throw new NotSupportedException();
            }
        }

        [Theory]
        [InlineData("1.2.3 || >1.2.4", "1.2.3", ">1.2.4", "||")]
        [InlineData("1.2.3 && >1.2.4", "1.2.3", ">1.2.4", "&&")]
        [InlineData("(1.0.x, 2.9.x] || [1.0.x, 2.9.x)", "(1.0.x, 2.9.x]", "[1.0.x, 2.9.x)", "||")]
        public void ParseCompositeComparator(string comparatorString, string lowerBoundary, string upperBoundary, string compositor)
        {
            var comparator = Assert.IsType<VersionCompositeComparator>(VersionComparatorFactory.Parse(comparatorString));
            Assert.Equal(2, comparator.Count);
            Assert.Equal(ParseCompositor(compositor), comparator.Compositor);
            Assert.Equal(VersionComparatorFactory.Parse(lowerBoundary), comparator.First());
            Assert.Equal(VersionComparatorFactory.Parse(upperBoundary), comparator[1]);
        }
    }
}
