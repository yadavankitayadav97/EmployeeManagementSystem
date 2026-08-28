using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // POST: api/employees
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] EmployeeCreateDto dto)
    {
        var employee = await _employeeService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new
        {
            message = "Employee created successfully.",
            data = employee
        });
    }

    // GET: api/employees/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        return Ok(new { data = employee });
    }

    // GET: api/employees?pageNumber=1&pageSize=10&search=john
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _employeeService.GetAllAsync(pageNumber, pageSize, search);
        return Ok(result);
    }

    // PUT: api/employees/1
    [HttpPut("{id:int}")]
    [HttpPost("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] EmployeeUpdateDto dto)
    {
        var employee = await _employeeService.UpdateAsync(id, dto);
        if (employee == null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        return Ok(new
        {
            message = "Employee updated successfully.",
            data = employee
        });
    }

    // DELETE: api/employees/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _employeeService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Employee not found." });
        }

        return Ok(new { message = "Employee deleted successfully." });
    }
}