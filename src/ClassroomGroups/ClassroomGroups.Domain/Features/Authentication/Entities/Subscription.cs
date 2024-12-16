namespace ClassroomGroups.Domain.Features.Authentication.Entities;

public class Subscription(
  Guid Id,
  string DisplayName,
  int Key,
  SubscriptionType subscriptionType,
  int MaxClassrooms,
  int MaxStudentsPerClassroom,
  int MaxFieldsPerClassroom,
  int MaxConfigurationsPerClassroom
)
{
  public string DisplayName { get; private set; } = DisplayName;

  public Guid Id { get; private set; } = Id;

  public int Key { get; set; } = Key;

  public SubscriptionType SubscriptionType { get; set; } = subscriptionType;

  public int MaxClassrooms { get; set; } = MaxClassrooms;

  public int MaxStudentsPerClassroom { get; set; } = MaxStudentsPerClassroom;

  public int MaxFieldsPerClassroom { get; set; } = MaxFieldsPerClassroom;

  public int MaxConfigurationsPerClassroom { get; set; } = MaxConfigurationsPerClassroom;

  public SubscriptionView ToSubscriptionView()
  {
    return new SubscriptionView(
      Id,
      DisplayName,
      SubscriptionType,
      MaxClassrooms,
      MaxStudentsPerClassroom,
      MaxFieldsPerClassroom,
      MaxConfigurationsPerClassroom
    );
  }
}
