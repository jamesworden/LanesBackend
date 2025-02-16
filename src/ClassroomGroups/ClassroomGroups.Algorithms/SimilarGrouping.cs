namespace ClassroomGroups.Algorithms;

public static class SimilarityGroupingExtensions
{
  /// <summary>
  /// Partitions items into groups based on score similarity by sorting and chunking.
  /// Items with similar scores will be grouped together.
  /// If numGroups is greater than the number of items, the additional groups will be empty.
  /// </summary>
  /// <typeparam name="T">The type of items being grouped.</typeparam>
  /// <param name="items">The collection of item-score tuples to partition.</param>
  /// <param name="numGroups">The number of groups to partition the items into.</param>
  /// <returns>A list of lists, where each inner list represents a group of item-score tuples with similar scores.</returns>
  public static List<List<(T item, double score)>> PartitionIntoSimilarGroups<T>(
    this IEnumerable<(T item, double score)> items,
    int numGroups
  )
  {
    if (numGroups <= 0)
    {
      throw new ArgumentException("The desired number of groups must be a positive integer.");
    }

    var sortedItems = items.OrderByDescending(x => x.score).ToList();

    if (sortedItems.Count == 0)
    {
      // Return the requested number of empty groups
      return Enumerable
        .Range(0, numGroups)
        .Select(_ => new List<(T item, double score)>())
        .ToList();
    }

    int itemsPerGroup = Math.Max(1, (int)Math.Ceiling(sortedItems.Count / (double)numGroups));
    var initialGroups = sortedItems.Chunk(itemsPerGroup).Select(chunk => chunk.ToList()).ToList();

    // Add empty groups if we have fewer groups than requested
    while (initialGroups.Count < numGroups)
    {
      initialGroups.Add(new List<(T item, double score)>());
    }

    return initialGroups;
  }
}
