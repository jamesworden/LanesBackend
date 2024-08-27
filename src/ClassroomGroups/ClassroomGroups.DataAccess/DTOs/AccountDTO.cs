using System.ComponentModel.DataAnnotations;
using ClassroomGroups.Domain.Features.Classrooms.Entities;

namespace ClassroomGroups.DataAccess.DTOs;

public class AccountDTO
{
  public string? GoogleNameIdentifier { get; set; }

  public string PrimaryEmail { get; set; } = "";

  public Guid AccountId { get; set; }

  [Key]
  public int AccountKey { get; set; }

  public Account ToAccount()
  {
    return new Account
    {
      GoogleNameIdentifier = GoogleNameIdentifier,
      PrimaryEmail = PrimaryEmail,
      AccountId = AccountId
    };
  }
}
