using FileHelpers;
using Microsoft.EntityFrameworkCore;
using TestStarkovGroup.Database;
using TestStarkovGroup.Database.Models;
using TestStarkovGroup.DTO;
using TestStarkovGroup.Utility;

namespace TestStarkovGroup;

public class ImportDataService
{
    private readonly StarkovDbContext _starkovDbContext;

    public ImportDataService(StarkovDbContext starkovDbContext)
    {
        _starkovDbContext = starkovDbContext;
    }

    public async Task JobTitle(string filePath)
    {
        var jobTitles = await _starkovDbContext.JobTitles.ToListAsync();

        _starkovDbContext.JobTitles.UpdateRange(GetUpdatedJobTitles(jobTitles, filePath));
        await _starkovDbContext.SaveChangesAsync();
    }

    public async Task Employees(string filePath)
    {
        var departments = await _starkovDbContext.Departments.ToListAsync();
        var employees = await _starkovDbContext.Employees.ToListAsync();
        var jobTitles = await _starkovDbContext.JobTitles.ToListAsync();

        var engine = new FileHelperEngine<EmployeeDTO>();
        engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;
        var employeeDTOs = engine.ReadFile(filePath)
            .Skip(1) // Пропуск заголовков
            .Select(FormattedData.FormattedEmployeeDtoData)
            .ToList();
        
        if (engine.ErrorManager.Errors.Length > 0)
        {
            var strError = "Не удалось десериализовать записи:\n";
            foreach (var error in engine.ErrorManager.Errors)
                strError += "Message: " + error.ExceptionInfo.Message;
            Console.Error.WriteLine(strError);
        }
        
        foreach (var formattedEmployeeDTO in employeeDTOs)
        {
            var errors = formattedEmployeeDTO.Validate();
            if (errors.Count > 0)
            {
                var strError = $"Работник не добален в бд. Данные о работнике: " +
                               $"ФИО: {formattedEmployeeDTO.FullName}, " +
                               $"Наименование подразделения: {formattedEmployeeDTO.DepartmentName}, " +
                               $"Наименование должности: {formattedEmployeeDTO.JobTitleName}, " +
                               $"Логин: {formattedEmployeeDTO.Login}.\n";
                foreach (var error in errors)
                    strError += $"\t{error}";
                Console.Error.WriteLine(strError);
                continue;
            }
            
            if (jobTitles.All(x => x.Name != formattedEmployeeDTO.JobTitleName))
            {
                Console.Error.WriteLine($"Профессии с наименованием - {formattedEmployeeDTO.JobTitleName} не существует. " +
                                        $"Работник с именем - {formattedEmployeeDTO.FullName} не добавлен в бд.");
                continue;
            }
            if (departments.All(x => x.Name != formattedEmployeeDTO.DepartmentName))
            {
                Console.Error.WriteLine($"Подразделения с наименованием  - {formattedEmployeeDTO.DepartmentName} не существует. " +
                                        $"Работник с именем - {formattedEmployeeDTO.FullName} не добавлен в бд.");
                continue;
            }
            
            var jobTitle = jobTitles.SingleOrDefault(x => x.Name == formattedEmployeeDTO.JobTitleName);
            var employee = employees.SingleOrDefault(x => x.FullName == formattedEmployeeDTO.FullName);
            var department = departments.SingleOrDefault(x => x.Name == formattedEmployeeDTO.DepartmentName);
            if (employee == null)
            {
                employee = new EmployeeDb()
                {
                    FullName = formattedEmployeeDTO.FullName,
                    Login = formattedEmployeeDTO.Login,
                    Passwrod = formattedEmployeeDTO.Password,
                    JobTitle = jobTitle
                };
            }
            else
            {
                employee.Login = formattedEmployeeDTO.Login;
                employee.Passwrod = formattedEmployeeDTO.Password;
                employee.JobTitle = jobTitle;
            }

            _starkovDbContext.Employees.Update(employee);
            
            if (department.Employees != null)
            {
                if (department.Manager != null)
                {
                    if (department.Manager.FullName == employee.FullName) 
                        continue;
                    if (department.Employees.All(x => x.FullName != employee.FullName))
                        department.Employees.Add(employee);

                }
                else
                {
                    if (department.Employees.All(x => x.FullName != employee.FullName))
                        department.Employees.Add(employee);   
                }
            }
            else
            {
                department.Employees = new List<EmployeeDb>() { employee };
            }
        }
        _starkovDbContext.Departments.UpdateRange(departments);
        await _starkovDbContext.SaveChangesAsync();
    }

    public async Task Departments(string filePath)
    {
        var departments = await _starkovDbContext.Departments.Include(x => x.Employees).ToListAsync();
        var employees = await _starkovDbContext.Employees.ToListAsync();

        var engine = new FileHelperEngine<DepartmentDTO>();
        engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;
        var departmentDTOs = engine.ReadFile(filePath)
            .Skip(1) // Пропуск заголовков
            .Select(FormattedData.FormattedDepartmentDtoData)
            .ToList();
        if (engine.ErrorManager.Errors.Length > 0)
        {
            var strError = "Не удалось десериализовать записи:\n";
            foreach (var error in engine.ErrorManager.Errors)
                strError += "Message: " + error.ExceptionInfo.Message;
            Console.Error.WriteLine(strError);
        }

        foreach (var formattedDepartmentDTO in departmentDTOs.Where(x => string.IsNullOrEmpty(x.ParentDepartment)))
        {
            var errors = formattedDepartmentDTO.Validate();
            if (errors.Count > 0)
            {
                var strError = "Подразделение не добалено в бд.\n";
                foreach (var error in errors)
                    strError += $"\t{error}";
                Console.Error.WriteLine(strError);
                continue;
            }
            var department = GetDepartment(employees, departments, formattedDepartmentDTO, departmentDTOs);
        }
        await _starkovDbContext.SaveChangesAsync();
        
        foreach (var formattedDepartmentDTO in departmentDTOs)
        {
            var errors = formattedDepartmentDTO.Validate();
            if (errors.Count > 0)
            {
                var strError = "Подразделение не добалено в бд.\n";
                foreach (var error in errors)
                    strError += $"\t{error}";
                Console.Error.WriteLine(strError);
                continue;
            }
            var department = GetDepartment(employees, departments, formattedDepartmentDTO, departmentDTOs);
        }

        await _starkovDbContext.SaveChangesAsync();
    }

    private List<JobTitleDb> GetUpdatedJobTitles(List<JobTitleDb> jobTitles, string filePath)
    {
        var engine = new FileHelperEngine<JobTitleDTO>();
        engine.ErrorManager.ErrorMode = ErrorMode.SaveAndContinue;
        var jobTitleDTOs = engine.ReadFile(filePath)
            .Skip(1) // Пропуск заголовков
            .Select(FormattedData.FormattedJobTitleDtoData)
            .ToList();
        if (engine.ErrorManager.Errors.Length > 0)
        {
            var strError = "Не удалось десериализовать записи:\n";
            foreach (var error in engine.ErrorManager.Errors)
                strError += "Message: " + error.ExceptionInfo.Message;
            Console.Error.WriteLine(strError);
        }

        foreach (var formattedJobTitleDtO in jobTitleDTOs)
        {
            var errors = formattedJobTitleDtO.Validate();
            if (errors.Count > 0)
            {
                var strError = "Должность не добалена в бд.\n";
                foreach (var error in errors)
                    strError += $"\t{error}";
                Console.Error.WriteLine(strError);
                continue;
            }

            if (jobTitles.Any(x => x.Name == formattedJobTitleDtO.Name) && jobTitles.Count > 0)
                continue;
            
            var newJobTitle = new JobTitleDb()
            {
                Name = formattedJobTitleDtO.Name
            };
            jobTitles.Add(newJobTitle);
            
        }

        return jobTitles;
    }

    private DepartmentDb GetDepartment(List<EmployeeDb> employees, List<DepartmentDb> departments,
        DepartmentDTO normalizedDepartmentDTO, List<DepartmentDTO> departmentDTOs)
    {
        var department = departments.SingleOrDefault(x => x.Name == normalizedDepartmentDTO.Name);
        if (department == null)
        {
            var departmentParent = departments.SingleOrDefault(x => x.Name == normalizedDepartmentDTO.ParentDepartment);
            if (!string.IsNullOrEmpty(normalizedDepartmentDTO.ParentDepartment) && !string.IsNullOrWhiteSpace(normalizedDepartmentDTO.ParentDepartment))
            {
                if (departmentParent == null)
                {
                    foreach (var departmentDTO in departmentDTOs)
                    {
                        if (departmentDTO.Name ==
                            normalizedDepartmentDTO.ParentDepartment)
                        {
                            departmentParent = GetDepartment(employees, departments, normalizedDepartmentDTO,
                                departmentDTOs);
                        }
                    }
                }
            }

            department = new DepartmentDb()
            {
                Name = normalizedDepartmentDTO.Name,
                Phone = normalizedDepartmentDTO.Phone,
                Parent = departmentParent
            };
            departments.Add(department);
            _starkovDbContext.Departments.Add(department);
        }
        else
        {
            var departmentParent = departments.SingleOrDefault(x => x.Name == normalizedDepartmentDTO.ParentDepartment);
            if (!string.IsNullOrEmpty(normalizedDepartmentDTO.ParentDepartment) && !string.IsNullOrWhiteSpace(normalizedDepartmentDTO.ParentDepartment))
            {
                if (departmentParent == null)
                {
                    foreach (var departmentDTO in departmentDTOs)
                    {
                        if (FormattedData.FormattedDepartmentDtoData(departmentDTO).Name ==
                            normalizedDepartmentDTO.ParentDepartment)
                        {
                            departmentParent =
                                GetDepartment(employees, departments, normalizedDepartmentDTO, departmentDTOs);
                        }
                    }
                }
            }

            department.Phone = normalizedDepartmentDTO.Phone;
            department.Name = normalizedDepartmentDTO.Name;
            department.Parent = departmentParent;
            _starkovDbContext.Departments.Update(department);
        }

        var employee = employees.SingleOrDefault(x => x.FullName == normalizedDepartmentDTO.ManagerName);
        if (employee == null)
        {
            Console.WriteLine($"Не удалось прикрепить пользователя: {normalizedDepartmentDTO.ManagerName} к подразделению: {department.Name}. Возможно он не добвален в бд. Добавьте его в бд и поторите операцию");
        }
        else
        {
            if (department.Manager != null)
            {
                if (employee.FullName != department.Manager.FullName)
                {
                    if (!department.Employees.Contains(employee))
                        department.Employees.Add(employee);
                }
                else
                {
                    department.Employees.Remove(employee);
                }
            }
            else
            {
                if (normalizedDepartmentDTO.ManagerName == employee.FullName)
                {
                    department.Manager = employee;
                    department.Employees.Remove(employee);
                }
                else
                {
                    if(!department.Employees.Contains(employee))
                        department.Employees.Add(employee);
                }
            }
            _starkovDbContext.Departments.Update(department);
        }
        
        return department;
    }
}