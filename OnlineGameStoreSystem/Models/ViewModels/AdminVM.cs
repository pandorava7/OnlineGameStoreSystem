using System.ComponentModel.DataAnnotations;

namespace OnlineGameStoreSystem.Models.ViewModels;


public class StatusManageVM
{
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;
}
