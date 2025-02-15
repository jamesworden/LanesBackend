using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.Domain.Features.Classrooms.Extensions;

public static class StudentDetailExtensions
{
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
