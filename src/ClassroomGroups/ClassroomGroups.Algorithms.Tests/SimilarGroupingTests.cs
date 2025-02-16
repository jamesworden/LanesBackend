using FluentAssertions;

namespace ClassroomGroups.Algorithms.Tests;

public class SimilarGroupingTests
{
  [Fact]
  public void SimilarityGrouping_WithEvenGroups_ShouldPartitionCorrectly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 95.0),
      ("B", 90.0),
      ("C", 85.0),
      ("D", 80.0)
    };

    // Act
    var groups = items.PartitionIntoSimilarGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(2);
    groups[1].Should().HaveCount(2);

    // Verify highest scores are in first group
    groups[0].Should().Contain(x => x.item == "A" && x.score == 95.0);
    groups[0].Should().Contain(x => x.item == "B" && x.score == 90.0);

    // Verify lower scores are in second group
    groups[1].Should().Contain(x => x.item == "C" && x.score == 85.0);
    groups[1].Should().Contain(x => x.item == "D" && x.score == 80.0);
  }

  [Fact]
  public void SimilarityGrouping_WithUnevenGroups_ShouldPartitionCorrectly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 95.0),
      ("B", 90.0),
      ("C", 85.0),
      ("D", 80.0),
      ("E", 75.0)
    };

    // Act
    var groups = items.PartitionIntoSimilarGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(3); // First group gets extra item
    groups[1].Should().HaveCount(2);

    // Verify scores are in descending order
    groups[0].Select(x => x.score).Should().BeInDescendingOrder();
    groups[1].Select(x => x.score).Should().BeInDescendingOrder();
  }

  [Fact]
  public void SimilarityGrouping_WithEmptyInput_ShouldReturnEmptyGroups()
  {
    // Arrange
    var items = new List<(string item, double score)>();

    // Act
    var groups = items.PartitionIntoSimilarGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(0);
    groups[1].Should().HaveCount(0);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void SimilarityGrouping_WithInvalidGroupCount_ShouldThrowArgumentException(int groupCount)
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.PartitionIntoSimilarGroups(groupCount));
  }
}
