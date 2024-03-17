using System.Text.RegularExpressions;
using TestStarkovGroup.DTO;

namespace TestStarkovGroup.Utility;

public static class FormattedData
{
    public static DepartmentDTO FormattedDepartmentDtoData(DepartmentDTO departmentDTO)
    {
        if (!string.IsNullOrEmpty(departmentDTO.ParentDepartment))
        {
            departmentDTO.ParentDepartment = String.Join("", departmentDTO.ParentDepartment.Split('-', ',', '(', ')'));
            departmentDTO.ParentDepartment = Regex.Replace(departmentDTO.ParentDepartment, "[ ]+", " ").Trim();
            departmentDTO.ParentDepartment = char.ToUpper(departmentDTO.ParentDepartment[0]) + departmentDTO.ParentDepartment.Substring(1);
        }

        if (!string.IsNullOrEmpty(departmentDTO.Phone))
        {
            departmentDTO.Phone = String.Join("", departmentDTO.Phone.Split(' ', '-', ',', '(', ')'));
            departmentDTO.Phone = Regex.Replace(departmentDTO.Phone, "[ ]+", " ").Trim();
        }

       
        if (!string.IsNullOrEmpty(departmentDTO.ManagerName))
        {
            departmentDTO.ManagerName = String.Join("", departmentDTO.ManagerName.Split(',', '(', ')'));
            departmentDTO.ManagerName = Regex.Replace(departmentDTO.ManagerName, "[ ]+", " ").Trim();
            departmentDTO.ManagerName = char.ToUpper(departmentDTO.ManagerName[0]) + departmentDTO.ManagerName.Substring(1);
        }
        
        if (!string.IsNullOrEmpty(departmentDTO.Name))
        {
            departmentDTO.Name = String.Join("", departmentDTO.Name.Split('-', ',', '(', ')'));
            departmentDTO.Name = Regex.Replace(departmentDTO.Name, "[ ]+", " ").Trim();   
            departmentDTO.Name = char.ToUpper(departmentDTO.Name[0]) + departmentDTO.Name.Substring(1);
        }
        return departmentDTO;
    }

    public static EmployeeDTO FormattedEmployeeDtoData(EmployeeDTO employeeDTO)
    {
        if (!string.IsNullOrEmpty(employeeDTO.DepartmentName))
        {
            employeeDTO.DepartmentName = String.Join("", employeeDTO.DepartmentName.Split('-', ',', '(', ')'));
            employeeDTO.DepartmentName = Regex.Replace(employeeDTO.DepartmentName, "[ ]+", " ").Trim();   
            employeeDTO.DepartmentName = char.ToUpper(employeeDTO.DepartmentName[0]) + employeeDTO.DepartmentName.Substring(1);
        }
        if (!string.IsNullOrEmpty(employeeDTO.FullName))
        {
            employeeDTO.FullName = String.Join("", employeeDTO.FullName.Split(',', '(', ')'));
            employeeDTO.FullName = Regex.Replace(employeeDTO.FullName, "[ ]+", " ").Trim();   
            employeeDTO.FullName = char.ToUpper(employeeDTO.FullName[0]) + employeeDTO.FullName.Substring(1);
        }
        if (!string.IsNullOrEmpty(employeeDTO.JobTitleName))
        {
            employeeDTO.JobTitleName = String.Join("", employeeDTO.JobTitleName.Split(',', '(', ')'));
            employeeDTO.JobTitleName = Regex.Replace(employeeDTO.JobTitleName, "[ ]+", " ").Trim();
            employeeDTO.JobTitleName = char.ToUpper(employeeDTO.JobTitleName[0]) + employeeDTO.JobTitleName.Substring(1);
        }
        return employeeDTO;
    }

    public static JobTitleDTO FormattedJobTitleDtoData(JobTitleDTO jobTitleDTO)
    {
        if (!string.IsNullOrEmpty(jobTitleDTO.Name))
        {
            jobTitleDTO.Name = String.Join("", jobTitleDTO.Name.Split(',', '(', ')'));
            jobTitleDTO.Name = Regex.Replace(jobTitleDTO.Name, "[ ]+", " ").Trim();   
            jobTitleDTO.Name = char.ToUpper(jobTitleDTO.Name[0]) + jobTitleDTO.Name.Substring(1);
        }
        return jobTitleDTO;
    }
}