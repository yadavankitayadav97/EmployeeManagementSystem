using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementApi.Entities;

public class Employee
{
    public int Id { get; set; }

    [Required]
    public string EmployeeCode { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Mobile { get; set; }

    [Required]
    public string Department { get; set; }

    public DateTime JoiningDate { get; set; }

    public string? ProfileImage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
