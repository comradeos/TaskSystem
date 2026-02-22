namespace TaskSystem.IntegrationTests;

public class HelloWorldTests
{
    [Fact]
    public void TwoPlusTwo_EqualsFour()
    {
        const int result = 2 + 2; // потужний тест для перевірки

        Assert.Equal(4, result);
    }
}