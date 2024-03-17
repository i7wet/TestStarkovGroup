using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestStarkovGroup;
using TestStarkovGroup.Database;using TestStarkovGroup.Database.Models;

var builder = new ConfigurationBuilder();
builder.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
IConfiguration config = builder.Build();
IServiceCollection services = new ServiceCollection();

services.AddDbContext<StarkovDbContext>(options =>
    options.UseNpgsql(config.GetConnectionString("Default")));

services.AddScoped<ImportDataService>(x => new ImportDataService(x.GetRequiredService<StarkovDbContext>()));
var serviceProvider = services.BuildServiceProvider();
await using var scope = serviceProvider.CreateAsyncScope();

Console.WriteLine("Данные нужно вводить без лишних проблеов, спец символов и учетом регистра");
Console.WriteLine("1 - Импор данных | 2 - Вывод данных");
Console.Write("Введите цифру: ");
var actionNumberStr = Console.ReadLine();
if (actionNumberStr == "1")
{
    Console.Write("Введите имя файла с расширением(Пример: departments.tsv): ");
    var fileName = Console.ReadLine();
    var filePath = Directory.GetCurrentDirectory() + "\\Data\\" + fileName;
    if (!File.Exists(filePath))
        throw new Exception($"Файл с именем - {fileName} не найден.");

    var importDataService = scope.ServiceProvider.GetRequiredService<ImportDataService>();
    if (fileName == "jobtitle.tsv")
    {
        await importDataService.JobTitle(filePath);
        var starkovDbContext = serviceProvider.GetRequiredService<StarkovDbContext>();
        var jobTitles = await starkovDbContext.JobTitles.ToListAsync();
        foreach (var jobTitle in jobTitles)
            Console.WriteLine($"{nameof(jobTitle.Id)}: {jobTitle.Id}  |  {nameof(jobTitle.Name)}: {jobTitle.Name}");
    }
    else if (fileName == "employees.tsv")
    {
        await importDataService.Employees(filePath);
        var starkovDbContext = serviceProvider.GetRequiredService<StarkovDbContext>();
        var employees = await starkovDbContext.Employees.Include(x => x.JobTitle).ToListAsync();
        foreach (var employee in employees)
            Console.WriteLine($"{nameof(employee.Id)}: {employee.Id}  |  " +
                              $"{nameof(employee.FullName)}: {employee.FullName}  |  " +
                              $"{nameof(employee.JobTitle.Name)}: {employee.JobTitle.Name}  |  " +
                              $"{nameof(employee.Login)}: {employee.Login}  |  " +
                              $"{nameof(employee.Passwrod)}: {employee.Passwrod}");
    }
    else if (fileName == "departments.tsv")
    {
        await importDataService.Departments(filePath);
        var starkovDbContext = serviceProvider.GetRequiredService<StarkovDbContext>();
        var departments = await starkovDbContext.Departments
            .Include(x => x.Employees)
            .Include(x => x.Manager)
            .Include(x => x.Parent)
            .ToListAsync();
        foreach (var department in departments)
        {
            var employeesStr = "[ ";
            foreach (var employee in department.Employees)
                employeesStr += employee.FullName + ", ";
            employeesStr += "]";

            int indexComma = employeesStr.LastIndexOf(',');
            if(employeesStr.Contains(','))
                employeesStr = employeesStr.Remove(indexComma, 1);

            var managerName = department.Manager == null ? null : department.Manager.FullName;
            var parentName = department.Parent == null ? null : department.Parent.Name;

            Console.WriteLine($"{nameof(department.Id)}: {department.Id}  |  " +
                              $"{nameof(department.Name)}: {department.Name}  |  " +
                              $"{nameof(department.Manager)}: {managerName}  |  " +
                              $"{nameof(department.Parent)}: {parentName}  |  " +
                              $"{nameof(department.Phone)}: {department.Phone}  |  " +
                              $"{nameof(department.Employees)}: {employeesStr}");
        }
    }
}
else if (actionNumberStr == "2")
{
    Console.Write("Введите Id подразделения: ");
    var departmentIdStr = Console.ReadLine();
    var starkovDbContext = serviceProvider.GetRequiredService<StarkovDbContext>();
    
    if (string.IsNullOrWhiteSpace(departmentIdStr) || string.IsNullOrEmpty(departmentIdStr))
    {
        var departments = await starkovDbContext.Departments
            .Include(x => x.Employees)
            .Include(x => x.Manager)
            .GroupBy(x => x.Parent)
            .ToListAsync();
        if (departments == null || departments.Count == 0)
        {
            Console.WriteLine("Данных нет");
            return;
        }
        var sortedParentDepartments = new List<DepartmentDb>();
        foreach (var groupDepartments in departments.Where(groupDepart => groupDepart.Key == null))
            sortedParentDepartments.AddRange(groupDepartments.OrderBy(parentDepart => parentDepart.Name));
        DisplayData(0, sortedParentDepartments, departments);
    }
    else
    {
        int departmentId;
        try
        {
            departmentId = Int32.Parse(departmentIdStr);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        var departments = await starkovDbContext.Departments
            .Include(x => x.Employees)
            .Include(x => x.Manager)
            .Include(x => x.Parent)
            .ToListAsync();
        var department = departments.Where(x => x.Id == departmentId).SingleOrDefault();
        if (department == null)
        {
            Console.WriteLine($"Не удалось получить подразделение по Id - {departmentId}");
            return;
        }
        
        DisplayDataById(departmentId, SortedDepartments(department, new List<DepartmentDb>()));
    }
}
else
{
    Console.WriteLine($"Не верно указан номер - {actionNumberStr}");
}

List<DepartmentDb> SortedDepartments(DepartmentDb? department, List<DepartmentDb> departments)
{
    if (department == null)
    {
        return departments;
    }
    departments.Insert(0, department);
    SortedDepartments( department.Parent, departments);

    return departments;
}

void DisplayDataById(int id, List<DepartmentDb> sortedDepartments)
{
    int nestingIndex = 0;
    foreach (var department in sortedDepartments)
    {
        nestingIndex++;
        var symbolsForDepartment = "=";
        for (var i = 1; i < nestingIndex; i++)
            symbolsForDepartment += "=";
        var symbolsForEmployee = "-";
        for (var i = 1; i < nestingIndex; i++)
            symbolsForEmployee = " " + symbolsForEmployee;
        var symbolsForManager = "*";
        for (var i = 1; i < nestingIndex; i++)
            symbolsForManager = " " + symbolsForManager;

        var managerName = department.Manager == null ? null : department.Manager.FullName;

        Console.WriteLine($"{symbolsForDepartment} " + department.Name);
        if (department.Id == id)
        {
            Console.WriteLine($"{symbolsForManager} " + managerName);
            foreach (var employee in department.Employees)
                Console.WriteLine($"{symbolsForEmployee} " + employee.FullName);
        }
    }
}

void DisplayData(int nestingIndex, List<DepartmentDb> sortedParentDepartments, List<IGrouping<DepartmentDb?,DepartmentDb>> departments)
{
    nestingIndex++;
    foreach (var parentDepartment in sortedParentDepartments)
    {
        var symbolsForDepartment = "=";
        for (var i = 1; i<nestingIndex; i++)
            symbolsForDepartment += "=";
        var symbolsForEmployee = "-";
        for (var i = 1; i<nestingIndex; i++)
            symbolsForEmployee = " " + symbolsForEmployee;
        var symbolsForManager = "*";
        for (var i = 1; i<nestingIndex; i++)
            symbolsForManager = " " + symbolsForManager;
        
        var managerName =  parentDepartment.Manager == null ? null : parentDepartment.Manager.FullName;
        
        Console.WriteLine($"{symbolsForDepartment} " + parentDepartment.Name);
        Console.WriteLine($"{symbolsForManager} " + managerName);
        foreach (var employee in parentDepartment.Employees)
            Console.WriteLine($"{symbolsForEmployee} " + employee.FullName);

        foreach (var depart in departments)
        {
            if (depart.Key == null)
                continue;
            if (parentDepartment.Name == depart.Key.Name)
            {
                DisplayData(nestingIndex, depart.OrderBy(parentDepart => parentDepart.Name).ToList(), departments);
            }
        }
    }
}

Console.WriteLine("OK");