using System.ComponentModel.DataAnnotations;

namespace EgeControlWebApp.Models;

public class SiteSetting
{
    [Key]
    public string Key { get; set; } = default!;

    [Required]
    public string Value { get; set; } = default!;

    public string? Description { get; set; }

    // Well-known keys
    public const string StatProjects = "stat_projects";
    public const string StatCustomers = "stat_customers";
    public const string StatExperience = "stat_experience";
    public const string StatSupport = "stat_support";
}
