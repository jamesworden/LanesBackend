namespace ClassroomGroups.Domain.Features.Classrooms.Entities.Account;

public class Account(Guid Id, string PrimaryEmail)
{
  public string PrimaryEmail { get; private set; } = PrimaryEmail;

  public Guid Id { get; private set; } = Id;
}