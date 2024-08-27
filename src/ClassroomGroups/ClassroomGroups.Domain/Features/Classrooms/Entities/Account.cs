namespace ClassroomGroups.Domain.Features.Classrooms.Entities;

public class Account
{
  public string? GoogleNameIdentifier { get; set; }

  public string PrimaryEmail { get; set; } = "";

  public Guid AccountId { get; set; }
}
