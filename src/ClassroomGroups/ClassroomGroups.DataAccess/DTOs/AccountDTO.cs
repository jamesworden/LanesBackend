using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Authentication.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class AccountDTO
{
  [Key]
  public int Key { get; set; }
  public string? GoogleNameIdentifier { get; set; }
  public string PrimaryEmail { get; set; } = "";

  [InverseProperty("AccountId")]
  public Guid Id { get; set; }

  public ICollection<ClassroomDTO> Classrooms { get; } = [];

  public SubscriptionDTO SubscriptionDTO { get; set; } = null!;
  public int SubscriptionKey { get; set; }
  public Guid SubscriptionId { get; set; }

  public Account ToAccount(Subscription Subscription)
  {
    return new Account(Id, PrimaryEmail, Key, Subscription);
  }
}
