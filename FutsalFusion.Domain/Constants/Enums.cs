using System.ComponentModel.DataAnnotations;

namespace FutsalFusion.Domain.Constants;

public enum Roles
{
    Admin = 1,
    Futsal = 2,
    Player = 3
}

public enum CourtType
{
    [Display(Name="Indoor")] Indoor = 1,
    [Display(Name="Outdoor")] Outdoor = 2
}