using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestStarkovGroup.Database.Models;

[Index(nameof(Name), IsUnique = true)]
public class DepartmentDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public DepartmentDb? Parent { get; set; }
    public EmployeeDb? Manager { get; set; }
    public List<EmployeeDb> Employees { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    
}