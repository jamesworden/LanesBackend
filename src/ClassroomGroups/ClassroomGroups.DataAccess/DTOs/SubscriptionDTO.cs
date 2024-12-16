using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Authentication.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class SubscriptionDTO
{
  [Key]
  public int Key { get; set; }
  public string DisplayName { get; set; } = "";

  public SubscriptionType SubscriptionType = SubscriptionType.FREE;

  [InverseProperty("SubscriptionId")]
  public Guid Id { get; set; }

  public ICollection<AccountDTO> Accounts { get; set; } = [];

  public int MaxClassrooms { get; set; }

  public int MaxStudentsPerClassroom { get; set; }

  public int MaxFieldsPerClassroom { get; set; }

  public int MaxConfigurationsPerClassroom { get; set; }

  public Subscription ToSubscription()
  {
    return new Subscription(
      Id,
      DisplayName,
      Key,
      SubscriptionType,
      MaxClassrooms,
      MaxStudentsPerClassroom,
      MaxFieldsPerClassroom,
      MaxConfigurationsPerClassroom
    );
  }
}
