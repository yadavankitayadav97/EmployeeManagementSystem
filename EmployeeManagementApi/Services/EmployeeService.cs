using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementAPI.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
    private const long MaxFileSize = 2 * 1024 * 1024; // 2 MB

    public EmployeeService(
        AppDbContext context,
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto)
    {
        ValidateJoiningDate(dto.JoiningDate);

        var existingEmployee = await _context.Employees
            .FirstOrDefaultAsync(x => (x.EmployeeCode == dto.EmployeeCode || x.Email == dto.Email) && x.IsActive);

        if (existingEmployee != null)
        {
            var updated = await UpdateAsync(existingEmployee.Id, new EmployeeUpdateDto
            {
                EmployeeCode = dto.EmployeeCode,
                Name = dto.Name,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Department = dto.Department,
                JoiningDate = dto.JoiningDate,
                IsActive = dto.IsActive,
                ProfileImage = dto.ProfileImage
            });

            return updated!;
        }

        string? imagePath = null;
        if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
        {
            imagePath = await SaveImageAsync(dto.ProfileImage);
        }

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode,
            Name = dto.Name,
            Email = dto.Email,
            Mobile = dto.Mobile,
            Department = dto.Department,
            JoiningDate = dto.JoiningDate,
            ProfileImage = imagePath,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return MapToDto(employee);
    }

    public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeListResponseDto> GetAllAsync(int pageNumber, int pageSize, string? search)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

        var query = _context.Employees
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Name.Contains(search) || x.EmployeeCode.Contains(search));
        }

        var totalRecords = await query.CountAsync();
        var employees = await query
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        return new EmployeeListResponseDto
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            Employees = employees.Select(MapToDto).ToList()
        };
    }

    public async Task<EmployeeResponseDto?> UpdateAsync(int id, EmployeeUpdateDto dto)
    {
        ValidateJoiningDate(dto.JoiningDate);

        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (employee == null)
        {
            employee = await _context.Employees.FirstOrDefaultAsync(x => (x.EmployeeCode == dto.EmployeeCode || x.Email == dto.Email) && x.IsActive);
            if (employee == null)
            {
                return null;
            }
        }

        var employeeCodeExists = await _context.Employees.AnyAsync(x => x.EmployeeCode == dto.EmployeeCode && x.Id != employee.Id && x.IsActive);
        if (employeeCodeExists)
        {
            throw new Exception("EmployeeCode already exists for another active employee.");
        }

        var emailExists = await _context.Employees.AnyAsync(x => x.Email == dto.Email && x.Id != employee.Id && x.IsActive);
        if (emailExists)
        {
            throw new Exception("Email already exists for another active employee.");
        }

        employee.EmployeeCode = dto.EmployeeCode;
        employee.Name = dto.Name;
        employee.Email = dto.Email;
        employee.Mobile = dto.Mobile;
        employee.Department = dto.Department;
        employee.JoiningDate = dto.JoiningDate;
        employee.IsActive = dto.IsActive;
        employee.UpdatedAt = DateTime.UtcNow;

        if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
        {
            DeleteImage(employee.ProfileImage);
            employee.ProfileImage = await SaveImageAsync(dto.ProfileImage);
        }

        await _context.SaveChangesAsync();
        return MapToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (employee == null)
        {
            return false;
        }

        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static void ValidateJoiningDate(DateTime joiningDate)
    {
        if (joiningDate.Date > DateTime.UtcNow.Date)
        {
            throw new Exception("JoiningDate cannot be a future date.");
        }
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        if (file.Length > MaxFileSize)
        {
            throw new Exception("Profile image size cannot exceed 2 MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            throw new Exception("Only JPG, JPEG and PNG images are allowed.");
        }

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadFolder = Path.Combine(webRoot, "uploads");
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{fileName}";
    }

    private void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        var fileName = Path.GetFileName(imagePath);
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var filePath = Path.Combine(webRoot, "uploads", fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private EmployeeResponseDto MapToDto(Employee employee)
    {
        var request = _httpContextAccessor.HttpContext?.Request;
        string? imageUrl = null;

        if (!string.IsNullOrEmpty(employee.ProfileImage))
        {
            imageUrl = $"{request?.Scheme}://{request?.Host}{employee.ProfileImage}";
        }

        return new EmployeeResponseDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            Name = employee.Name,
            Email = employee.Email,
            Mobile = employee.Mobile,
            Department = employee.Department,
            JoiningDate = employee.JoiningDate,
            ProfileImage = imageUrl,
            IsActive = employee.IsActive
        };
    }
}