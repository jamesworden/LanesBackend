namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class StudentDetail(
  Guid Id,
  Guid GroupId,
  int Ordinal,
  Guid StudentGroupId,
  Dictionary<Guid, string> FieldIdsToValues
)
{
  public Guid Id { get; private set; } = Id;

  public Guid GroupId { get; private set; } = GroupId;

  public int Ordinal { get; private set; } = Ordinal;

  public Guid StudentGroupId { get; private set; } = StudentGroupId;

  public Dictionary<Guid, string> FieldIdsToValues { get; private set; } = FieldIdsToValues;

  /// <summary>
  /// At some point, we may consider adding assignment weights and ranges
  /// as parameters in this function to calculate a weighted average.
  /// Until then, everything is equally weighted.
  /// </summary>
  public double CalculateAverage(IEnumerable<ColumnDetail> columnDetails)
  {
    var numericValues = new List<double>();

    foreach (var fieldPair in FieldIdsToValues)
    {
      var columnDetail = columnDetails.FirstOrDefault(c => c.FieldId == fieldPair.Key);

      if (columnDetail == null || columnDetail.Type != FieldType.NUMBER || !columnDetail.Enabled)
        continue;

      if (double.TryParse(fieldPair.Value, out double numericValue))
      {
        numericValues.Add(numericValue);
      }
    }

    return numericValues.Count == 0 ? 0 : Math.Round(numericValues.Average(), 2);
  }
}
