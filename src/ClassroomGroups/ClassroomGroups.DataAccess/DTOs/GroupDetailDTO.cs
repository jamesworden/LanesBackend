using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class GroupDetailDTO(
  Guid Id,
  Guid ConfigurationId,
  string Label,
  int GroupOrdinal,
  bool IsLocked
)
{
  public Guid Id = Id;

  public Guid ConfigurationId = ConfigurationId;

  public string Label = Label;

  public int GroupOrdinal = GroupOrdinal;

  public bool IsLocked = IsLocked;

  public GroupDetail ToGroupDetail(List<StudentDetail> StudentDetails)
  {
    return new GroupDetail(Id, ConfigurationId, Label, GroupOrdinal, StudentDetails, IsLocked);
  }
}
