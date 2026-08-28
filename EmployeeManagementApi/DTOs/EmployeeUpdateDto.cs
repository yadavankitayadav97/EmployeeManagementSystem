using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagementAPI.DTOs;

public class EmployeeUpdateDto
{
    [Required]
    public string EmployeeCode { get; set; } 

    [Required]
    public string Name { get; set; } 

    [Required]
    [EmailAddress]
    public string Email { get; set; } 

    [Required]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile number must contain exactly 10 digits.")]
    public string Mobile { get; set; } 

    [Required]
    public string Department { get; set; } 

    [Required]
    [DataType(DataType.Date)]
    public DateTime JoiningDate { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? ProfileImage { get; set; }
}