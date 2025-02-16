namespace ClassroomGroups.Algorithms;

public static class SimilarityGroupingExtensions
{
  /// <summary>
  /// Partitions items into groups based on score similarity by sorting and chunking.
  /// Items with similar scores will be grouped together.
  /// </summary>
  /// <typeparam name="T">The type of items being grouped.</typeparam>
  /// <param name="items">The collection of item-score tuples to partition.</param>
  /// <param name="numGroups">The number of groups to partition the items into.</param>
  /// <returns>A list of lists, where each inner list represents a group of similar-scoring items.</returns>
  public static List<List<T>> PartitionIntoSimilarGroups<T>(
    this IEnumerable<(T item, double score)> items,
    int numGroups
  )
  {
    var sortedItems = items.OrderByDescending(x => x.score).Select(x => x.item).ToList();

    int itemsPerGroup = (int)Math.Ceiling(sortedItems.Count / (double)numGroups);

    return sortedItems.Chunk(itemsPerGroup).Select(chunk => chunk.ToList()).ToList();
  }
}
