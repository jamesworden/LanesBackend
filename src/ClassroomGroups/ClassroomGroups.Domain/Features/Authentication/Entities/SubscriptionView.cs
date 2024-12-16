namespace ClassroomGroups.Domain.Features.Authentication.Entities;

public class SubscriptionView(
  Guid Id,
  string DisplayName,
  SubscriptionType subscriptionType,
  int MaxClassrooms,
  int MaxStudentsPerClassroom,
  int MaxFieldsPerClassroom,
  int MaxConfigurationsPerClassroom
)
{
  public string DisplayName { get; private set; } = DisplayName;

  public Guid Id { get; private set; } = Id;

  public SubscriptionType SubscriptionType { get; private set; } = subscriptionType;

  public int MaxClassrooms { get; set; } = MaxClassrooms;

  public int MaxStudentsPerClassroom { get; set; } = MaxStudentsPerClassroom;

  public int MaxFieldsPerClassroom { get; set; } = MaxFieldsPerClassroom;

  public int MaxConfigurationsPerClassroom { get; set; } = MaxConfigurationsPerClassroom;
}
