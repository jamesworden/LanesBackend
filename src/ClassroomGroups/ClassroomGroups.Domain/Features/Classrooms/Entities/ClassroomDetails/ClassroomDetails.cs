namespace ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

public class ClassroomDetails
{
  public List<Classroom> Classrooms { get; set; } = [];

  public List<Student> Students { get; set; } = [];

  public List<Field> Fields { get; set; } = [];

  public List<Column> Columns { get; set; } = [];

  public List<StudentGroup> StudentGroups { get; set; } = [];

  public List<StudentField> StudentFields { get; set; } = [];

  public List<Group> Group { get; set; } = [];

  public List<Configuration> Configurations { get; set; } = [];
}
