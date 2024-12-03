using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ConfigurationDetailDTO(
  Guid Id,
  Guid ClassroomId,
  Guid DefaultGroupId,
  string Label,
  string Description
)
{
  public Guid Id = Id;

  public Guid ClassroomId = ClassroomId;

  public Guid DefaultGroupId = DefaultGroupId;

  public string Label = Label;

  public string Description = Description;

  public ConfigurationDetail ToConfigurationDetail(
    List<GroupDetail> GroupDetails,
    List<ColumnDetail> ColumnDetails
  )
  {
    return new ConfigurationDetail(
      Id,
      ClassroomId,
      DefaultGroupId,
      Label,
      Description,
      GroupDetails,
      ColumnDetails
    );
  }
}
