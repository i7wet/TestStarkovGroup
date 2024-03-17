using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestStarkovGroup.Database.Models;

[Index(nameof(FullName), IsUnique = true)]
public class EmployeeDb
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Login { get; set; }
    public string Passwrod { get; set; }
    public JobTitleDb JobTitle { get; set; }
}