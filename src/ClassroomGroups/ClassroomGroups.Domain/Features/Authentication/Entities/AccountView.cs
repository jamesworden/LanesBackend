namespace ClassroomGroups.Domain.Features.Authentication.Entities;

public class AccountView(Guid Id, string PrimaryEmail)
{
  public string PrimaryEmail { get; private set; } = PrimaryEmail;

  public Guid Id { get; private set; } = Id;
}
