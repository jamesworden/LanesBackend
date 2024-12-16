namespace ClassroomGroups.Domain.Features.Authentication.Entities;

public class Account(Guid Id, string PrimaryEmail, int Key, Subscription Subscription)
{
  public string PrimaryEmail { get; private set; } = PrimaryEmail;

  public Guid Id { get; private set; } = Id;

  public int Key { get; set; } = Key;

  public Subscription Subscription = Subscription;

  public AccountView ToAccountView()
  {
    return new AccountView(Id, PrimaryEmail, Subscription.ToSubscriptionView());
  }
}
