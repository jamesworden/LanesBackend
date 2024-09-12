using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class ConfigurationDetailDTO(Guid Id, Guid ClassroomId, string Label, string? Description)
{
  public Guid Id = Id;

  public Guid ClassroomId = ClassroomId;

  public string Label = Label;

  public string? Description = Description;

  public ConfigurationDetail ToConfigurationDetail(
    List<GroupDetail> GroupDetails,
    List<ColumnDetail> ColumnDetails
  )
  {
    return new ConfigurationDetail(
      Id,
      ClassroomId,
      Label,
      Description,
      GroupDetails,
      ColumnDetails
    );
  }
}
