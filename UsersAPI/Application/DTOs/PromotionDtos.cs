using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Application.DTOs;

public class CreatePromotionDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxUses { get; set; }
}

public class UpdatePromotionDto
{
    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal DiscountPercentage { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxUses { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

public class PromotionResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
}
