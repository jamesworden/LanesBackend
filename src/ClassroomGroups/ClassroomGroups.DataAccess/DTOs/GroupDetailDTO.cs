using ClassroomGroups.Domain.Features.Classrooms.Entities.ClassroomDetails;

namespace ClassroomGroups.DataAccess.DTOs;

public class GroupDetailDTO(Guid Id, Guid ConfigurationId, string Label, int GroupOrdinal)
{
  public Guid Id = Id;

  public Guid ConfigurationId = ConfigurationId;

  public string Label = Label;

  public int GroupOrdinal = GroupOrdinal;

  public GroupDetail ToGroupDetail(List<StudentDetail> StudentDetails)
  {
    return new GroupDetail(Id, ConfigurationId, Label, GroupOrdinal, StudentDetails);
  }
}
