using FluentAssertions;

namespace ClassroomGroups.Algorithms.Tests;

public class BalancedGroupingTests
{
  [Fact]
  public void BalancedGrouping_ShouldCreateGroupsWithSimilarAverages()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 90.0),
      ("C", 80.0),
      ("D", 70.0)
    };

    // Act
    var groups = items.PartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(2);
    groups[1].Should().HaveCount(2);

    // Calculate group averages
    double group1Avg = groups[0].Average(x => x.score);
    double group2Avg = groups[1].Average(x => x.score);

    // Verify the difference between group averages is minimized
    Math.Abs(group1Avg - group2Avg).Should().BeLessThan(15.0);
  }

  [Fact]
  public void BalancedGrouping_WithUnevenGroups_ShouldDistributeItemsFairly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 90.0),
      ("C", 80.0),
      ("D", 70.0),
      ("E", 60.0)
    };

    // Act
    var groups = items.PartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);

    // One group should have 3 items, the other 2
    groups.Select(g => g.Count).Should().BeEquivalentTo([2, 3]);

    // Calculate group averages
    double group1Avg = groups[0].Average(x => x.score);
    double group2Avg = groups[1].Average(x => x.score);

    // Verify the difference between group averages is minimized
    Math.Abs(group1Avg - group2Avg).Should().BeLessThanOrEqualTo(25.0);
  }

  [Fact]
  public void BalancedGrouping_WithEmptyInput_ShouldReturnEmptyGroups()
  {
    // Arrange
    var items = new List<(string item, double score)>();

    // Act
    var groups = items.PartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(0);
    groups[1].Should().HaveCount(0);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void BalancedGrouping_WithInvalidGroupCount_ShouldThrowArgumentException(int groupCount)
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.PartitionIntoBalancedGroups(groupCount));
  }
}
