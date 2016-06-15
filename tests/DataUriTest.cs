using Xunit;

namespace Wheatech.UnitTest
{
    public class DataUriTest
    {
        [Fact]
        public void ParseSimpleText()
        {
            DataUri uri;
            Assert.True(DataUri.TryParse("data:,A%20brief%20note",out uri));
            var textUri = Assert.IsType<TextDataUri>(uri);
            Assert.Equal(string.Empty, uri.Encoding);
            Assert.Equal("text/plain", uri.MediaType);
            Assert.Equal("A brief note", textUri.Content);
        }
    }
}
