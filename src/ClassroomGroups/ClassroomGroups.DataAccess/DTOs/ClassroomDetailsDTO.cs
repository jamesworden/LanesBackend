using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class ClassroomDetailsDTO
{
  public List<ClassroomDTO> Classrooms { get; set; } = [];

  public List<StudentDTO> Students { get; set; } = [];

  public List<FieldDTO> Fields { get; set; } = [];

  public List<ColumnDTO> Columns { get; set; } = [];

  public List<StudentGroupDTO> StudentGroups { get; set; } = [];

  public List<StudentFieldDTO> StudentFields { get; set; } = [];

  public List<GroupDTO> Groups { get; set; } = [];

  public List<ConfigurationDTO> Configurations { get; set; } = [];
}
