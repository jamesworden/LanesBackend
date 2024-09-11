namespace ClassroomGroups.Domain.Features.Classrooms.Entities.Account;

public class Account(Guid Id, string PrimaryEmail, int Key)
{
  public string PrimaryEmail { get; private set; } = PrimaryEmail;

  public Guid Id { get; private set; } = Id;

  public int Key { get; set; } = Key;

  public AccountView ToAccountView()
  {
    return new AccountView(Id, PrimaryEmail);
  }
}
