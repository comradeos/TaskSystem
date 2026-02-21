namespace TaskSystem.Tests.Integration;

public class DummyIntegrationTests
{
    // це тест заглушка щоб переконатись шо механізм тестів працює
    [Fact]
    public void Two_Plus_Two_Should_Equal_Four()
    {
        const int result = 2 + 2;

        Assert.Equal(4, result);
    }
}