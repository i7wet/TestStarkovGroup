using FileHelpers;

namespace TestStarkovGroup.DTO;

[DelimitedRecord("\t")]
public class EmployeeDTO
{
    public string? DepartmentName { get; set; }
    public string? FullName { get; set; }
    public string? Login { get; set; }
    public string? Password { get; set; }
    public string? JobTitleName { get; set; }
    
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(JobTitleName) || string.IsNullOrEmpty(JobTitleName))
            errors.Add("Наименование должности не указано.");
        
        if (string.IsNullOrWhiteSpace(DepartmentName) || string.IsNullOrEmpty(DepartmentName))
            errors.Add("Наименование подразделения не указано.");
        
        if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrEmpty(Password))
            errors.Add("Пароль не указан.");
        
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrEmpty(Login))
            errors.Add("Логин не указан.");
        
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrEmpty(FullName))
            errors.Add("Имя не указано.");

        return errors;
    }
}