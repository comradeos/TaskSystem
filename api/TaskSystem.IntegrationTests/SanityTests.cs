namespace TaskSystem.IntegrationTests;

public class SanityTests
{
    [Fact]
    public void TwoPlusTwo_EqualsFour()
    {
        var result = 2 + 2;

        Assert.Equal(4, result);
    }
}