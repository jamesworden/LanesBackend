using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClassroomGroups.Domain.Features.Classrooms.Entities.Account;

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

  public Account ToAccount()
  {
    return new Account(Id, PrimaryEmail);
  }
}
