namespace ClassroomGroups.Domain.Features.Authentication.Entities;

public class AccountView(Guid Id, string PrimaryEmail, SubscriptionView SubscriptionView)
{
  public string PrimaryEmail { get; private set; } = PrimaryEmail;

  public Guid Id { get; private set; } = Id;

  public SubscriptionView Subscription { get; private set; } = SubscriptionView;
}
