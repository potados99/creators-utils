using Core;

namespace CoreTests;

public class AdderTest
{
    [Fact]
    public void Adder_InputIs1And2_Return3()
    {
        // Arrange
        var adder = new Adder();
        const int expected = 3;

        // Act
        var actual = adder.add(1, 2);

        // Assert
        Assert.Equal(expected, actual);
    }
}