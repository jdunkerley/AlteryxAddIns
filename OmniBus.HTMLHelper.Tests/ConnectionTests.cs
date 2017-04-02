using System;
using System.Xml;

using Xunit;

namespace OmniBus.HTMLHelper.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void CanMakeWithAName()
        {
            var result = new Connection("Input");
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("\t")]
        [InlineData("  ")]
        [InlineData(null)]
        public void RequireAName(string name)
        {
            Assert.Throws<ArgumentNullException>(() => new Connection(name));
        }

        [Fact]
        public void MakesValidXml()
        {
            var result = new Connection("Name").MakeXml();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);

            Assert.NotNull(xmlDoc.DocumentElement);
        }

        [Theory]
        [InlineData("Input")]
        [InlineData("Output")]
        public void SerialisesNameToXml(string name)
        {
            var result = new Connection(name).MakeXml();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(result);

            Assert.NotNull(xmlDoc.DocumentElement.SelectSingleNode("@Name"));
            Assert.Equal(name, xmlDoc.DocumentElement.SelectSingleNode("@Name")?.InnerText);
        }
    }
}
