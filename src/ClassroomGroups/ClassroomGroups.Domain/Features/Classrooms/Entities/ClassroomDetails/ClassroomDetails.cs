namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class ClassroomDetails(
  List<Classroom> Classrooms,
  List<Student> Students,
  List<Field> Fields,
  List<Column> Columns,
  List<StudentGroup> StudentGroups,
  List<StudentField> StudentFields,
  List<Group> Groups,
  List<Configuration> Configurations
)
{
  public List<Classroom> Classrooms { get; set; } = Classrooms;

  public List<Student> Students { get; set; } = Students;

  public List<Field> Fields { get; set; } = Fields;

  public List<Column> Columns { get; set; } = Columns;

  public List<StudentGroup> StudentGroups { get; set; } = StudentGroups;

  public List<StudentField> StudentFields { get; set; } = StudentFields;

  public List<Group> Groups { get; set; } = Groups;

  public List<Configuration> Configurations { get; set; } = Configurations;
}
