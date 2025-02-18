using System.Diagnostics;
using Google.OrTools.LinearSolver;

namespace ClassroomGroups.Algorithms;

public static class BalancedGroupingExtensions
{
  /// <summary>
  /// Partitions a collection of items into balanced groups using Integer Linear Programming (ILP),
  /// ensuring each group has a similar average score by minimizing the difference in total scores between groups.
  /// </summary>
  /// <typeparam name="T">The type of items being grouped.</typeparam>
  /// <param name="items">The collection of item-score tuples to partition.</param>
  /// <param name="numGroups">The number of groups to partition the items into.</param>
  /// <returns>A list of lists, where each inner list represents a group of item-score tuples.</returns>
  /// <exception cref="InvalidOperationException">Thrown when an optimal solution cannot be found.</exception>
  public static List<List<(T item, double score)>> OptimallyPartitionIntoBalancedGroups<T>(
    this IEnumerable<(T item, double score)> items,
    int numGroups
  )
  {
    var itemsList = items.ToList();
    int numItems = itemsList.Count;

    if (numGroups <= 0)
    {
      throw new ArgumentException("The desired number of groups must be a positive integer.");
    }

    var solver =
      Solver.CreateSolver("SCIP")
      ?? throw new UnreachableException("Solver initialization failed.");

    // Decision variables: x[i,j] = 1 if item i is in group j
    Variable[,] x = new Variable[numItems, numGroups];
    for (int i = 0; i < numItems; i++)
    for (int j = 0; j < numGroups; j++)
      x[i, j] = solver.MakeIntVar(0, 1, $"x_{i}_{j}");

    // Variables for group sums
    Variable[] groupSums = new Variable[numGroups];
    for (int j = 0; j < numGroups; j++)
      groupSums[j] = solver.MakeNumVar(0, double.PositiveInfinity, $"group_sum_{j}");

    // Constraint: Each item must be assigned to exactly one group
    for (int i = 0; i < numItems; i++)
    {
      var constraint = solver.MakeConstraint(1, 1);
      for (int j = 0; j < numGroups; j++)
        constraint.SetCoefficient(x[i, j], 1);
    }

    // Constraint: Group size limits
    double itemsPerGroup = numItems / (double)numGroups;
    var maxSize = Math.Ceiling(itemsPerGroup);
    var minSize = Math.Floor(itemsPerGroup);

    for (int j = 0; j < numGroups; j++)
    {
      var constraint = solver.MakeConstraint(minSize, maxSize);
      for (int i = 0; i < numItems; i++)
        constraint.SetCoefficient(x[i, j], 1);
    }

    // Calculate group sums
    for (int j = 0; j < numGroups; j++)
    {
      var constraint = solver.MakeConstraint(0, 0);
      constraint.SetCoefficient(groupSums[j], 1);
      for (int i = 0; i < numItems; i++)
        constraint.SetCoefficient(x[i, j], -itemsList[i].score);
    }

    // Variable to track maximum difference between any two groups
    Variable maxDiff = solver.MakeNumVar(0, double.PositiveInfinity, "max_diff");

    // Constrain maxDiff to be at least as large as any difference between groups
    for (int j1 = 0; j1 < numGroups; j1++)
    {
      for (int j2 = j1 + 1; j2 < numGroups; j2++)
      {
        // |group1 - group2| ≤ maxDiff
        var constraint1 = solver.MakeConstraint(double.NegativeInfinity, 0);
        constraint1.SetCoefficient(groupSums[j1], 1);
        constraint1.SetCoefficient(groupSums[j2], -1);
        constraint1.SetCoefficient(maxDiff, -1);

        var constraint2 = solver.MakeConstraint(double.NegativeInfinity, 0);
        constraint2.SetCoefficient(groupSums[j1], -1);
        constraint2.SetCoefficient(groupSums[j2], 1);
        constraint2.SetCoefficient(maxDiff, -1);
      }
    }

    // Objective: Minimize the maximum difference between any two groups
    var objective = solver.Objective();
    objective.SetCoefficient(maxDiff, 1);
    objective.SetMinimization();

    // Solve ILP
    Solver.ResultStatus resultStatus = solver.Solve();
    if (resultStatus != Solver.ResultStatus.OPTIMAL)
      throw new InvalidOperationException("No optimal solution found.");

    // Extract groups with scores
    var groups = new List<List<(T item, double score)>>(numGroups);
    for (int j = 0; j < numGroups; j++)
      groups.Add([]);

    for (int i = 0; i < numItems; i++)
    for (int j = 0; j < numGroups; j++)
      if (x[i, j].SolutionValue() == 1)
        groups[j].Add(itemsList[i]);

    return groups;
  }

  /// <summary>
  /// Partitions a collection of items into balanced groups using a greedy algorithm,
  /// ensuring each group has a similar average score by alternating between highest and lowest scores.
  /// </summary>
  /// <typeparam name="T">The type of items being grouped.</typeparam>
  /// <param name="items">The collection of item-score tuples to partition.</param>
  /// <param name="numGroups">The number of groups to partition the items into.</param>
  /// <returns>A list of lists, where each inner list represents a group of item-score tuples.</returns>
  public static List<List<(T item, double score)>> GreedilyPartitionIntoBalancedGroups<T>(
    this IEnumerable<(T item, double score)> items,
    int numGroups
  )
  {
    if (numGroups <= 0)
      throw new ArgumentException("The desired number of groups must be a positive integer.");

    var itemsList = items.ToList();
    int numItems = itemsList.Count;

    if (numItems < numGroups)
      throw new ArgumentException(
        "Number of items must be greater than or equal to number of groups."
      );

    // Initialize groups
    var groups = new List<List<(T item, double score)>>();
    var groupSums = new double[numGroups];
    for (int i = 0; i < numGroups; i++)
      groups.Add([]);

    // Sort items by score in descending order
    itemsList.Sort((a, b) => b.score.CompareTo(a.score));

    // Calculate items per group
    int minItemsPerGroup = numItems / numGroups;
    int extraItems = numItems % numGroups;

    // First pass: distribute highest scoring items evenly
    int currentGroup = 0;
    for (int i = 0; i < numGroups; i++)
    {
      groups[i].Add(itemsList[currentGroup]);
      groupSums[i] += itemsList[currentGroup].score;
      currentGroup++;
    }

    // Second pass: distribute remaining items to minimize score differences
    while (currentGroup < numItems)
    {
      // Find group with lowest total score
      int targetGroup = Array.IndexOf(groupSums, groupSums.Min());

      // Check if this group can accept more items
      if (groups[targetGroup].Count < minItemsPerGroup + (extraItems > targetGroup ? 1 : 0))
      {
        groups[targetGroup].Add(itemsList[currentGroup]);
        groupSums[targetGroup] += itemsList[currentGroup].score;
        currentGroup++;
      }
      else
      {
        // If this group is full, find the next eligible group
        var eligibleGroups = Enumerable
          .Range(0, numGroups)
          .Where(g => groups[g].Count < minItemsPerGroup + (extraItems > g ? 1 : 0))
          .OrderBy(g => groupSums[g]);

        if (!eligibleGroups.Any())
          break;

        targetGroup = eligibleGroups.First();
        groups[targetGroup].Add(itemsList[currentGroup]);
        groupSums[targetGroup] += itemsList[currentGroup].score;
        currentGroup++;
      }
    }

    return groups;
  }
}
