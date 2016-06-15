using System;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Wheatech.UnitTest
{
    public class AssemblyIdentityTest
    {
        [Fact]
        public void ParseShortNameOnly()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Version);
            Assert.Null(identity.Culture);
            Assert.Null(identity.PublicKeyToken);
        }

        [Fact]
        public void ParseVersion()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Culture);
            Assert.Null(identity.PublicKeyToken);
            Assert.Equal(Version.Parse("4.5.1.0"), identity.Version);
        }

        [Fact]
        public void ParseNeutralCulture()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Culture=neutral");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Version);
            Assert.Null(identity.Culture);
            Assert.Null(identity.PublicKeyToken);
        }

        [Fact]
        public void ParseSpecifiedCulture()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Culture=zh-Hans");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Version);
            Assert.Equal(CultureInfo.GetCultureInfo("zh-Hans"), identity.Culture);
            Assert.Null(identity.PublicKeyToken);
        }

        [Fact]
        public void ParseNotFoundCulture()
        {
            ExceptionAssert.Throws<ArgumentException>(() => AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Culture=zh-abc"));
        }

        [Fact]
        public void ParseNullPublicKeyToken()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, PublicKeyToken=null");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Version);
            Assert.Null(identity.Culture);
            Assert.Null(identity.PublicKeyToken);
        }

        [Fact]
        public void ParseSpecifiedPublicKeyToken()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, PublicKeyToken=31bf3856ad364e35");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.Null(identity.Version);
            Assert.Null(identity.Culture);
            Assert.NotNull(identity.PublicKeyToken);
            Assert.True(identity.PublicKeyToken.SequenceEqual(new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 }));
        }

        [Theory]
        [InlineData("31bf3856ad364e3")]
        [InlineData("31bf3856ad364e3x")]
        [InlineData("31bf3856ad364e357")]
        public void ParseInvalidPublicKeyToken(string token)
        {
            ExceptionAssert.Throws<ArgumentException>(() => AssemblyIdentity.Parse(string.Format("Wheatech.Framework.Modulize, PublicKeyToken={0}", token)));
        }

        [Fact]
        public void ParseFullIdentity()
        {
            var identity = AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35");
            Assert.NotNull(identity);
            Assert.Equal("Wheatech.Framework.Modulize", identity.ShortName);
            Assert.True(identity.PublicKeyToken.SequenceEqual(new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 }));
            Assert.Equal(CultureInfo.GetCultureInfo("zh-Hans"), identity.Culture);
            Assert.Equal(Version.Parse("4.5.1.0"), identity.Version);
        }

        [Fact]
        public void CompareShortName()
        {
            Assert.True(
                AssemblyIdentityComparer.ShortName.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize")));

            Assert.False(AssemblyIdentityComparer.ShortName.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulizer")));
        }

        [Fact]
        public void CompareVersion()
        {
            Assert.True(
                AssemblyIdentityComparer.Version.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0")));

            Assert.False(AssemblyIdentityComparer.Version.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.2.0")));
        }

        [Fact]
        public void CompareCulture()
        {
            Assert.True(
                AssemblyIdentityComparer.Culture.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0")));

            Assert.True(
                AssemblyIdentityComparer.Culture.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans")));

            Assert.False(AssemblyIdentityComparer.Culture.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-TW")));
        }

        [Fact]
        public void ComparePublicKeyToken()
        {
            Assert.True(
                AssemblyIdentityComparer.PublicKeyToken.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0")));

            Assert.True(
                AssemblyIdentityComparer.PublicKeyToken.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, PublicKeyToken=31bf3856ad364e35")));

            Assert.False(AssemblyIdentityComparer.PublicKeyToken.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e36")));
        }

        [Fact]
        public void CompareDefault()
        {
            Assert.True(
                AssemblyIdentityComparer.Default.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0")));

            Assert.True(
                AssemblyIdentityComparer.Default.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, PublicKeyToken=31bf3856ad364e35")));

            Assert.False(
                AssemblyIdentityComparer.Default.Equals(
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35"),
                    AssemblyIdentity.Parse("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e36")));
        }

        [Theory]
        [InlineData("Wheatech.Framework.Modulize")]
        [InlineData("Wheatech.Framework.Modulize, Version=4.5.1.0")]
        [InlineData("Wheatech.Framework.Modulize, Culture=neutral")]
        [InlineData("Wheatech.Framework.Modulize, Culture=zh-Hans")]
        [InlineData("Wheatech.Framework.Modulize, PublicKeyToken=null")]
        [InlineData("Wheatech.Framework.Modulize, PublicKeyToken=31bf3856ad364e35")]
        [InlineData("Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31bf3856ad364e35")]
        public void GenerateParseString(string assemblyString)
        {
            Assert.Equal(assemblyString, AssemblyIdentity.Parse(assemblyString).ToString());
        }

        [Theory]
        [InlineData("Wheatech.Framework.Modulize", null, null, null, "Wheatech.Framework.Modulize")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", null, null, "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", "zh-Hans", null, "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=null")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", "neutral", null, "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=null")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", "neutral", "31bf3856ad364e35", "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", null, "31bf3856ad364e35", "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35")]
        [InlineData("Wheatech.Framework.Modulize", "4.5.1.0", "zh-Hans", "31bf3856ad364e35", "Wheatech.Framework.Modulize, Version=4.5.1.0, Culture=zh-Hans, PublicKeyToken=31BF3856AD364E35")]
        public void GenerateConstructorString(string shortName, string version, string cultureName, string tokenString, string assemblyString)
        {
            CultureInfo culture;
            if (!AssemblyIdentity.TryParseCulture(cultureName, out culture))
            {
                throw new ArgumentException("Invalid culture name", "cultureName");
            }
            byte[] token;
            if (!AssemblyIdentity.TryParsepPublicKeyToken(tokenString, out token))
            {
                throw new ArgumentException("Invalid public key token", "tokenString");
            }
            Assert.Equal(assemblyString, new AssemblyIdentity(shortName, string.IsNullOrEmpty(version) ? null : Version.Parse(version), culture, token).ToString());
        }
    }
}
