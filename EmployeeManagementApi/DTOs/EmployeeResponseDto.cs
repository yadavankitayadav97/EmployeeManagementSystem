namespace EmployeeManagementAPI.DTOs;

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } 
    public string Name { get; set; } 
    public string Email { get; set; } 
    public string Mobile { get; set; } 
    public string Department { get; set; } 
    public DateTime JoiningDate { get; set; }
    public string? ProfileImage { get; set; }
    public bool IsActive { get; set; }
}