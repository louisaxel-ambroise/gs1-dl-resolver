using Goto.Services.Data.Entities;

namespace Goto.Tests.Services.Data.Entities;

[TestClass]
public class MediaTypeTests
{
    [TestMethod]
    public void ToStringShouldWork()
    {
        var sut = new MediaType("application/json");
        var result = sut.ToString();

        Assert.AreEqual("application/json", result);
    }

    [TestMethod]
    [DataRow("application/json", "application/json")]
    [DataRow("text/html", "text/html")]
    [DataRow("text/html ", "text/html")]
    [DataRow("unknwn/type", "unknwn/type")]
    public void ParseShouldWork(string input, string expected)
    {
        var result = MediaType.Parse(input);

        Assert.AreEqual(expected, result.ToString());
    }

    [TestMethod]
    [DataRow("notEnoughParts")]
    [DataRow("too/many/parts")]
    public void CtorWithInvalidValueShouldThrow(string input)
    {
        var act = () => new MediaType(input);

        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.AreEqual($"Invalid media type: '{input}'. Shall use format 'type/subtype' (wildcard allowed)", ex.Message);
    }

    [TestMethod]
    [DataRow("notEnoughParts", "*/*")]
    [DataRow("too/many/parts", "*/*")]
    public void ParseInvalidValueShouldThrow(string input, string expected)
    {
        var act = () => MediaType.Parse(input);

        Assert.Throws<FormatException>(act);
    }
}
