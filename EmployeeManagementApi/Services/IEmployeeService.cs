using EmployeeManagementAPI.DTOs;

namespace EmployeeManagementAPI.Services;

public interface IEmployeeService
{
    Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto);
    Task<EmployeeResponseDto?> GetByIdAsync(int id);
    Task<EmployeeListResponseDto> GetAllAsync(int pageNumber, int pageSize, string? search);
    Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}