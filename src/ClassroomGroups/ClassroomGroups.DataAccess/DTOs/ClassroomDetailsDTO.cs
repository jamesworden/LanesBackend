namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class ClassroomDetailsDTO
{
  public List<Classroom> Classrooms { get; set; } = [];

  public List<Student> Students { get; set; } = [];

  public List<Field> Fields { get; set; } = [];

  public List<Column> Columns { get; set; } = [];

  public List<StudentGroup> StudentGroups { get; set; } = [];

  public List<StudentField> StudentFields { get; set; } = [];

  public List<Group> Groups { get; set; } = [];

  public List<Configuration> Configurations { get; set; } = [];

  public ClassroomDetails ToClassroomDetails()
  {
    return new ClassroomDetails(
      Classrooms,
      Students,
      Fields,
      Columns,
      StudentGroups,
      StudentFields,
      Groups,
      Configurations
    );
  }
}
