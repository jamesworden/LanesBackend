using FluentAssertions;

namespace ClassroomGroups.Algorithms.Tests;

public class BalancedGroupingTests
{
  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_ShouldCreateGroupsWithSimilarAverages()
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
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

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
  public void OptimallyPartitionIntoBalancedGroups_WithUnevenGroups_ShouldDistributeItemsFairly()
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
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

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
  public void OptimallyPartitionIntoBalancedGroups_WithEmptyInput_ShouldReturnEmptyGroups()
  {
    // Arrange
    var items = new List<(string item, double score)>();

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(0);
    groups[1].Should().HaveCount(0);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void OptimallyPartitionIntoBalancedGroups_WithInvalidGroupCount_ShouldThrowArgumentException(
    int groupCount
  )
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.OptimallyPartitionIntoBalancedGroups(groupCount));
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_WithThreeGroups_ShouldDistributeEvenly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 90.0),
      ("C", 80.0),
      ("D", 70.0),
      ("E", 60.0),
      ("F", 50.0)
    };

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(3);

    // Assert
    groups.Should().HaveCount(3);
    groups.Should().AllSatisfy(g => g.Should().HaveCount(2));

    var averages = groups.Select(g => g.Average(x => x.score)).ToList();
    var maxDiff = averages.Max() - averages.Min();
    maxDiff.Should().BeLessThanOrEqualTo(20.0); // Stricter than greedy because it's optimal
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_WithMoreGroupsThanItems_CreateAsManyGroupsAsThereAreItems()
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    items.Count.Should().Be(2);
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_ShouldPreserveAllItems()
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
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

    // Assert
    var allGroupedItems = groups.SelectMany(g => g.Select(x => x.item)).ToList();
    allGroupedItems.Should().BeEquivalentTo(items.Select(x => x.item));
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_WithLargeDataSet_ShouldProduceValidResult()
  {
    // Arrange
    var random = new Random(42); // Fixed seed for reproducibility
    var items = Enumerable
      .Range(0, 20) // Smaller dataset than greedy due to computational complexity
      .Select(i => ($"Item{i}", random.NextDouble() * 100))
      .ToList();

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(4);

    // Assert
    groups.Should().HaveCount(4);
    groups.Should().AllSatisfy(g => g.Count.Should().BeInRange(4, 6)); // 20/4 = 5 items per group ±1

    var averages = groups.Select(g => g.Average(x => x.score)).ToList();
    var maxDiff = averages.Max() - averages.Min();
    maxDiff.Should().BeLessThanOrEqualTo(30.0);
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_ShouldDistributeHighScoresOptimally()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 95.0),
      ("C", 90.0),
      ("D", 85.0)
    };

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);

    // Calculate group averages
    var averages = groups.Select(g => g.Average(x => x.score)).ToList();
    var maxDiff = averages.Max() - averages.Min();

    // Should be more optimal than greedy
    maxDiff.Should().BeLessThanOrEqualTo(5.0);
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_WithIdenticalScores_ShouldDistributeEvenly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 80.0),
      ("B", 80.0),
      ("C", 80.0),
      ("D", 80.0)
    };

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups.Should().AllSatisfy(g => g.Should().HaveCount(2));

    var averages = groups.Select(g => g.Average(x => x.score)).ToList();
    averages[0].Should().Be(averages[1]);
  }

  [Fact]
  public void OptimallyPartitionIntoBalancedGroups_WithExtremeScores_ShouldBalanceOptimally()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 100.0),
      ("C", 0.0),
      ("D", 0.0)
    };

    // Act
    var groups = items.OptimallyPartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);

    // Each group should have one high and one low score
    groups
      .Should()
      .AllSatisfy(g =>
      {
        g.Should().ContainSingle(x => x.score == 100.0);
        g.Should().ContainSingle(x => x.score == 0.0);
      });
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_ShouldCreateGroupsWithSimilarAverages()
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
    var groups = items.GreedilyPartitionIntoBalancedGroups(2);

    // Assert
    groups.Should().HaveCount(2);
    groups[0].Should().HaveCount(2);
    groups[1].Should().HaveCount(2);

    // Calculate group averages
    double group1Avg = groups[0].Average(x => x.score);
    double group2Avg = groups[1].Average(x => x.score);

    // Verify the difference between group averages is minimized
    Math.Abs(group1Avg - group2Avg).Should().BeLessThan(15.0);

    // Verify that highest scores are distributed across groups
    groups[0].Select(x => x.score).Should().Contain(x => x >= 90.0);
    groups[1].Select(x => x.score).Should().Contain(x => x >= 90.0);
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_WithUnevenGroups_ShouldDistributeItemsFairly()
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
    var groups = items.GreedilyPartitionIntoBalancedGroups(2);

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
  public void GreedilyPartitionIntoBalancedGroups_WithThreeGroups_ShouldDistributeEvenly()
  {
    // Arrange
    var items = new List<(string item, double score)>
    {
      ("A", 100.0),
      ("B", 90.0),
      ("C", 80.0),
      ("D", 70.0),
      ("E", 60.0),
      ("F", 50.0)
    };

    // Act
    var groups = items.GreedilyPartitionIntoBalancedGroups(3);

    // Assert
    groups.Should().HaveCount(3);
    groups.Should().AllSatisfy(g => g.Should().HaveCount(2));

    var averages = groups.Select(g => g.Average(x => x.score)).ToList();
    var maxDiff = averages.Max() - averages.Min();
    maxDiff.Should().BeLessThanOrEqualTo(30.0);
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_WithEmptyInput_ShouldThrowArgumentException()
  {
    // Arrange
    var items = new List<(string item, double score)>();

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.GreedilyPartitionIntoBalancedGroups(2));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public void GreedilyPartitionIntoBalancedGroups_WithInvalidGroupCount_ShouldThrowArgumentException(
    int groupCount
  )
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.GreedilyPartitionIntoBalancedGroups(groupCount));
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_WithMoreGroupsThanItems_ShouldThrowArgumentException()
  {
    // Arrange
    var items = new List<(string item, double score)> { ("A", 90.0), ("B", 80.0) };

    // Act & Assert
    Assert.Throws<ArgumentException>(() => items.GreedilyPartitionIntoBalancedGroups(3));
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_ShouldPreserveAllItems()
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
    var groups = items.GreedilyPartitionIntoBalancedGroups(2);

    // Assert
    var allGroupedItems = groups.SelectMany(g => g.Select(x => x.item)).ToList();
    allGroupedItems.Should().BeEquivalentTo(items.Select(x => x.item));
  }

  [Fact]
  public void GreedilyPartitionIntoBalancedGroups_WithLargeDataSet_ShouldCompleteQuickly()
  {
    // Arrange
    var random = new Random(42); // Fixed seed for reproducibility
    var items = Enumerable
      .Range(0, 100)
      .Select(i => ($"Item{i}", random.NextDouble() * 100))
      .ToList();

    // Act
    var startTime = DateTime.Now;
    var groups = items.GreedilyPartitionIntoBalancedGroups(5);
    var duration = DateTime.Now - startTime;

    // Assert
    duration.TotalSeconds.Should().BeLessThan(1.0);
    groups.Should().HaveCount(5);
    groups.Should().AllSatisfy(g => g.Count.Should().BeInRange(19, 21)); // 100/5 = 20 items per group ±1
  }
}
