using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wheatech.Properties;

namespace Wheatech
{
    public static class VersionComparatorFactory
    {
        private static bool TryParseByBrakets(string value, out VersionComparator lowerBound, out VersionComparator upperBound)
        {
            lowerBound = null;
            upperBound = null;
            if (value[0] != '(' && value[0] != '[') return false;
            bool isMinInclusive, isMaxInclusive;
            // The first character must be [ to (
            switch (value[0])
            {
                case '[':
                    isMinInclusive = true;
                    break;
                case '(':
                    isMinInclusive = false;
                    break;
                default:
                    return false;
            }
            // The last character must be ] ot )
            switch (value[value.Length - 1])
            {
                case ']':
                    isMaxInclusive = true;
                    break;
                case ')':
                    isMaxInclusive = false;
                    break;
                default:
                    return false;
            }
            // Get rid of the two brackets
            value = value.Substring(1, value.Length - 2);
            // Split by comma, and make sure we don't get more than two pieces
            var parts = value.Split(',');
            if (parts.Length > 2) return false;
            // If all parts are empty, then neither of upper or lower bounds were specified. Version spec is of the format (,]
            if (parts.All(string.IsNullOrEmpty)) return false;
            var lowerBoundString = parts[0];
            var upperBoundString = (parts.Length == 2) ? parts[1] : parts[0];
            if (!string.IsNullOrWhiteSpace(lowerBoundString))
            {
                if (!VersionComparator.TryParse(lowerBoundString, isMinInclusive ? VersionOperator.GreaterThanEqual : VersionOperator.GreaterThan, out lowerBound))
                {
                    return false;
                }
                lowerBound._originalString = null;
            }
            if (!string.IsNullOrWhiteSpace(upperBoundString))
            {
                if (!VersionComparator.TryParse(upperBoundString, isMaxInclusive ? VersionOperator.LessThanEqual : VersionOperator.LessThan, out upperBound))
                {
                    return false;
                }
                upperBound._originalString = null;
            }
            return true;
        }

        private static bool TryParseByHyphen(string value, out VersionComparator lowerBound, out VersionComparator upperBound)
        {
            lowerBound = null;
            upperBound = null;
            var seperator = " - ";
            var hyphenIndex = value.IndexOf(seperator, StringComparison.Ordinal);
            if (hyphenIndex == -1)
            {
                seperator = "-";
                if (value.EndsWith(seperator))
                {
                    hyphenIndex = value.Length - 1;
                }
                else if (value.StartsWith(seperator))
                {
                    hyphenIndex = 0;
                }
            }
            if (hyphenIndex == -1) return false;
            var lowerBoundString = value.Substring(0, hyphenIndex).Trim();
            var upperBoundString = value.Substring(hyphenIndex + seperator.Length).Trim();
            if (!string.IsNullOrWhiteSpace(lowerBoundString))
            {
                if (!VersionComparator.TryParse(lowerBoundString, VersionOperator.GreaterThanEqual, out lowerBound))
                {
                    return false;
                }
                lowerBound._originalString = null;
            }
            if (!string.IsNullOrWhiteSpace(upperBoundString))
            {
                if (!VersionComparator.TryParse(upperBoundString, VersionOperator.LessThanEqual, out upperBound))
                {
                    return false;
                }
                upperBound._originalString = null;
            }
            return true;
        }

        private static bool TryParseByTilde(string value, out IVersionComparator comparator)
        {
            comparator = null;
            if (value[0] != '~') return false;
            var versionString = value.Substring(1);
            if (string.IsNullOrWhiteSpace(versionString)) return false;
            var hyphenIndex = versionString.IndexOf("-", StringComparison.Ordinal);
            var realVersion = hyphenIndex == -1 ? versionString : versionString.Substring(0, hyphenIndex);
            if (hyphenIndex == -1)
            {
                var parts = realVersion.Split('.');
                if (parts.Length >= 3)
                {
                    VersionComparator lowerBound;
                    if (!VersionComparator.TryParse(realVersion, VersionOperator.GreaterThanEqual, out lowerBound)) return false;
                    lowerBound._originalString = null;
                    var upperBound = new VersionComparator(new Version(lowerBound.Version.Major, lowerBound.Version.Minor, 0), VersionFloatBehavior.Patch,
                        VersionOperator.LessThanEqual);
                    comparator = new VersionCompositeComparator(new IVersionComparator[] { lowerBound, upperBound }, VersionCompositor.And, value);
                }
                else
                {
                    VersionComparator versionComparator;
                    if (!VersionComparator.TryParse(realVersion + ".*", out versionComparator)) return false;
                    versionComparator._originalString = value;
                    comparator = versionComparator;
                }
                return true;
            }
            else
            {
                Version version;
                if (!Version.TryParse(versionString, out version)) return false;
                var releaseComparator = new VersionCompositeComparator(
                    new VersionComparator(version, VersionOperator.GreaterThanEqual),
                    new VersionComparator(new Version(version.Major, version.Minor, version.Patch, version.Revision),
                        VersionOperator.LessThan));
                IVersionComparator versionComparator;
                if (TryParseByTilde("~" + realVersion, out versionComparator))
                {
                    comparator = new VersionCompositeComparator(new[] { releaseComparator, versionComparator }, VersionCompositor.Or, value);
                    return true;
                }
                return false;
            }
        }

        private static bool TryParseByCaret(string value, out IVersionComparator comparator)
        {
            comparator = null;
            if (value[0] != '^') return false;
            var versionString = value.Substring(1);
            if (string.IsNullOrEmpty(versionString)) return false;
            var hyphenIndex = versionString.IndexOf("-", StringComparison.Ordinal);
            var realVersion = hyphenIndex == -1 ? versionString : versionString.Substring(0, hyphenIndex);
            var parts = realVersion.Split('.');
            var major = parts.Length > 0 ? parts[0] : null;
            var minor = parts.Length > 1 ? parts[1] : null;
            var patch = parts.Length > 2 ? parts[2] : null;
            var revision = parts.Length > 3 ? parts[3] : null;
            if (hyphenIndex == -1)
            {
                VersionComparator lowerBound, upperBound = null;
                if (!VersionComparator.TryParse(realVersion, VersionOperator.GreaterThanEqual, out lowerBound)) return false;
                lowerBound._originalString = null;
                if (!string.IsNullOrEmpty(major) && (major != "0" || string.IsNullOrEmpty(minor) || minor == "x" || minor == "*"))
                {
                    upperBound = new VersionComparator(new Version(lowerBound.Version.Major, 0, 0), VersionFloatBehavior.Minor, VersionOperator.LessThanEqual);
                }
                else if (!string.IsNullOrEmpty(minor) && (minor != "0" || string.IsNullOrEmpty(patch) || patch == "x" || patch == "*"))
                {
                    upperBound = new VersionComparator(new Version(lowerBound.Version.Major, lowerBound.Version.Minor, 0), VersionFloatBehavior.Patch,
                        VersionOperator.LessThanEqual);
                }
                else if (!string.IsNullOrEmpty(patch) && (patch != "0" || string.IsNullOrEmpty(revision) || revision == "x" || revision == "*"))
                {
                    upperBound = new VersionComparator(new Version(lowerBound.Version.Major, lowerBound.Version.Minor, lowerBound.Version.Patch),
                        VersionFloatBehavior.Revision, VersionOperator.LessThanEqual);
                }
                else if (!string.IsNullOrEmpty(revision))
                {
                    comparator = new VersionComparator(lowerBound.Version);
                    return true;
                }
                if (upperBound != null)
                {
                    comparator = new VersionCompositeComparator(lowerBound, upperBound);
                    return true;
                }
            }
            else
            {
                Version version;
                if (!Version.TryParse(versionString, out version)) return false;
                var releaseComparator = new VersionCompositeComparator(
                    new VersionComparator(version, VersionOperator.GreaterThanEqual),
                    new VersionComparator(new Version(version.Major, version.Minor, version.Patch, version.Revision),
                        VersionOperator.LessThan));
                IVersionComparator versionComparator;
                if (TryParseByCaret("^" + realVersion, out versionComparator))
                {
                    comparator = new VersionCompositeComparator(new[] { releaseComparator, versionComparator }, VersionCompositor.Or, value);
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseSection(string value, out IVersionComparator comparator)
        {
            comparator = null;
            if (string.IsNullOrEmpty(value)) return false;
            if (value == "*")
            {
                comparator = new VersionComparator(new Version(0, 0, 0), VersionFloatBehavior.Major, value);
                return true;
            }
            VersionComparator lowerBound, upperBound;
            if (TryParseByBrakets(value, out lowerBound, out upperBound) || TryParseByHyphen(value, out lowerBound, out upperBound))
            {
                if (lowerBound == null && upperBound == null) return false;
                if (lowerBound == null || upperBound == null)
                {
                    comparator = lowerBound ?? upperBound;
                    return true;
                }
                comparator = new VersionCompositeComparator(new IVersionComparator[] { lowerBound, upperBound }, VersionCompositor.And, value);
                return true;
            }
            if (TryParseByTilde(value, out comparator) || TryParseByCaret(value, out comparator))
            {
                return true;
            }
            VersionComparator versionComparator;
            if (VersionComparator.TryParse(value, out versionComparator))
            {
                comparator = versionComparator;
                return true;
            }
            return false;
        }

        private static bool TryParseExpression(string value, out IVersionComparator comparator)
        {
            value = value.Trim();
            comparator = null;
            if (string.IsNullOrEmpty(value)) return false;
            if (TryParseCompositeExpression(value, "||", VersionCompositor.Or, out comparator))
            {
                return true;
            }
            if (TryParseCompositeExpression(value, "&&", VersionCompositor.And, out comparator))
            {
                return true;
            }
            if (TryParseSection(value, out comparator))
            {
                return true;
            }
            return false;
        }

        private static bool TryParseCompositeExpression(string value, string operatorString, VersionCompositor compositor, out IVersionComparator comparator)
        {
            comparator = null;
            if (value.IndexOf(operatorString, StringComparison.Ordinal) == -1) return false;
            Func<bool, IVersionComparator> parse = ignoreParanthesis =>
            {
                string[] expressions;
                if (!SplitByOperator(value, operatorString, ignoreParanthesis, out expressions)) return null;
                if (expressions.Length <= 1) return null;
                var comparators = new List<IVersionComparator>();
                bool result = true;
                foreach (var expression in expressions)
                {
                    IVersionComparator childComparator;
                    if (!TryParseExpression(expression, out childComparator))
                    {
                        result = false;
                        break;
                    }
                    comparators.Add(childComparator);
                }
                return result ? new VersionCompositeComparator(comparators, compositor) : null;
            };
            comparator = parse(false) ?? parse(true);
            if (comparator != null) return true;
            if (value[0] == '(')
            {
                if (value[value.Length - 1] != ')') return false;
                return TryParseCompositeExpression(value.Substring(1, value.Length - 2).Trim(), operatorString, compositor, out comparator);
            }
            return false;
        }

        private static bool SplitByOperator(string value, string logicalOperator, bool ignoreParanthesis, out string[] expressions)
        {
            expressions = null;
            int parenthesisCount = 0;
            int sectionStartIndex = 0;
            int currentIndex = 0;
            var list = new List<string>();
            while (currentIndex < value.Length)
            {
                var ch = value[currentIndex];
                if (ch == '(')
                {
                    parenthesisCount++;
                }
                else if (ch == ')')
                {
                    if (parenthesisCount > 0)
                    {
                        parenthesisCount--;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((ignoreParanthesis || parenthesisCount == 0) && ch == logicalOperator[0])
                {
                    bool matchOperator = true;
                    for (int i = 1; i < logicalOperator.Length; i++)
                    {
                        if (value[currentIndex + i] != logicalOperator[i])
                        {
                            matchOperator = false;
                            break;
                        }
                    }
                    if (matchOperator)
                    {
                        var expression = value.Substring(sectionStartIndex, currentIndex - sectionStartIndex).Trim();
                        if (string.IsNullOrEmpty(expression)) return false;
                        list.Add(expression);
                        currentIndex += logicalOperator.Length;
                        sectionStartIndex = currentIndex;

                    }
                }
                currentIndex++;
            }
            {
                var expression = value.Substring(sectionStartIndex, currentIndex - sectionStartIndex).Trim();
                if (string.IsNullOrEmpty(expression)) return false;
                list.Add(expression);
                expressions = list.ToArray();
                return true;
            }
        }

        public static bool TryParse(string value, out IVersionComparator comparator)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            value = value.Trim();
            comparator = null;
            if (string.IsNullOrEmpty(value)) return false;
            return TryParseExpression(value, out comparator);
        }

        public static IVersionComparator Parse(string value)
        {
            IVersionComparator comparator;
            if (!TryParse(value, out comparator))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Strings.InvalidVersion, value), "value");
            }
            return comparator;
        }
    }
}
