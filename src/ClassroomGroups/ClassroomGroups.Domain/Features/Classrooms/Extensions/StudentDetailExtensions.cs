using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.Domain.Features.Classrooms.Extensions
{
  public static class StudentDetailExtensions
  {
    public static double CalculateAverage(this StudentDetail student, IEnumerable<Field> fields)
    {
      var numericValues = new List<double>();

      foreach (var fieldPair in student.FieldIdsToValues)
      {
        var field = fields.FirstOrDefault(f => f.Id == fieldPair.Key);

        // Skip if field not found or not numeric
        if (field == null || field.Type != FieldType.NUMBER)
          continue;

        // Try to parse the value
        if (double.TryParse(fieldPair.Value, out double numericValue))
        {
          numericValues.Add(numericValue);
        }
      }

      // Return 0 if no valid numeric values found
      return numericValues.Any() ? numericValues.Average() : 0;
    }

    public static IOrderedEnumerable<StudentDetail> OrderByAverage(
      this IEnumerable<StudentDetail> students,
      IEnumerable<Field> fields,
      bool descending = true
    )
    {
      return (
        descending
          ? students.OrderByDescending(s => s.CalculateAverage(fields))
          : students.OrderBy(s => s.CalculateAverage(fields))
      )
        .Select(
          (s, i) =>
          {
            s.Ordinal = i;
            return s;
          }
        )
        .OrderBy(s => s.Ordinal);
    }
  }
}
