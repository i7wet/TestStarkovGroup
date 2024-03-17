using FileHelpers;

namespace TestStarkovGroup.DTO;

[DelimitedRecord("\t")]
public class DepartmentDTO
{
    [FieldNotEmpty] public string? Name { get; set; }
    public string? ParentDepartment { get; set; }
    public string? ManagerName { get; set; }
    public string? Phone { get; set; }

    public List<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrEmpty(Name))
            errors.Add("Наименование не указано.");

        if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrEmpty(Phone))
            errors.Add("Номер не указан.");
        
        return errors;
    }
}