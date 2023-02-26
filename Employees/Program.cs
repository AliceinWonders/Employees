using System;
using System.Linq;
using System.Text;
using Employees.Data.Models;

namespace Employees
{
    class Program
    {
        static private EmployeesContext _context = new EmployeesContext();

        static void Main(string[] args)
        {
            //Console.WriteLine(HighlyPaidEmp());
            //Console.WriteLine(RelocateEmp("452 Vault Avenue"));
            //Console.WriteLine(ProjectAudit());
            //Dossier(8);
            //SmallDep();
            //SalaryIncrease(1,10);
            //Tax(1);
            City404("Seattle");
        }

        //
        static string GetEmployeesInformation()
        {
            var employees = _context.Employees
                .OrderBy(e => e.EmployeeId)
                .Select(e => new
                {
                    e.AddressId,
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle
                })
                .ToList();
            var sb = new StringBuilder();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} | {e.MiddleName} | {e.JobTitle} | {e.AddressId}");
            }

            return sb.ToString().TrimEnd();
        }
        //
        
        static string HighlyPaidEmp()
        {
            var employees = _context.Employees
                .Where(e => e.Salary > 48000)
                .OrderBy(e => e.FirstName)
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.MiddleName,
                    e.JobTitle
                })
                .ToList();
            var sb = new StringBuilder();
            foreach (var e in employees)
            {
                sb.AppendLine($"{e.EmployeeId}|{e.FirstName} {e.LastName} | {e.MiddleName} | {e.JobTitle}");
            }

            return sb.ToString().TrimEnd();
        }
        
        static string RelocateEmp(string address)
        {
            var empAdress = new Addresses()
            {
                AddressText = address
            };
            _context.Addresses.Add(empAdress);
            _context.SaveChanges();
            var lastName = _context.Employees
                .Where (e =>e.LastName == "Brown")
                .ToList();
            foreach (var e in lastName)
            {
                e.Address = empAdress;
            }
            var sb = new StringBuilder();
            _context.SaveChanges();
            foreach (var e in lastName)
            {
                sb.AppendLine($"{e.EmployeeId} | {e.FirstName} {e.LastName} | {e.MiddleName} | {e.Address.AddressText} | {e.Department} | {e.Salary} | {e.HireDate}");
            }

            return sb.ToString().TrimEnd();
        }
        
        static string ProjectAudit()
        {
            var  startDate = new DateTime(2002, 1, 1, 00, 00, 00);
            var endDate = new DateTime(2006, 1, 1, 00, 00, 00);
            var projectYears = _context.EmployeesProjects
                .Where(ep => DateTime.Compare( ep.Project.StartDate, startDate) >=0 
                && DateTime.Compare( ep.Project.StartDate, endDate) < 0 )
                
                .Select(ep => new
            {
                EmployeeName = ep.Employee.FirstName + " " + ep.Employee.LastName,
                ManagerName = ep.Employee.Manager.FirstName + " " + ep.Employee.Manager.LastName,
                ProjectName = ep.Project.Name,
                StartD = ep.Project.StartDate,
                EndD = ep.Project.EndDate
            })
                .Take(5)
                .ToList();
            var sb = new StringBuilder();
            foreach (var ep in projectYears)
            {
                sb.AppendLine($"{ep.EmployeeName} | {ep.ManagerName} \n {ep.ProjectName} | {ep.StartD} | {(ep.EndD == null? "НЕ ЗАВЕРШЕН": ep.EndD)}");
                
            }

            return sb.ToString().TrimEnd();

        }
        
        static void Dossier(int id)
        {

            var employee = _context.Employees
                .Where(e => e.EmployeeId == id);


            var project = _context.EmployeesProjects
                .Where(ep => ep.EmployeeId == id)
                .Select(ep => new
                {
                    ProjectName = ep.Project.Name 
                });

            
            foreach (var e in employee)
            {
                Console.WriteLine($"{e.LastName} {e.FirstName} {e.MiddleName} - {e.JobTitle}");
            }
            
            
            foreach (var ep in project)
            {
                Console.WriteLine(ep.ProjectName);
            }
            
            
        }

        static void SmallDep()
        {
            var departament = _context.Departments
                .Join(_context.Employees,
                    d => d.DepartmentId,
                    e => e.DepartmentId,
                    (d, e) => new
                    {
                        d.Name,
                        e.EmployeeId
                    })
                .GroupBy(e => e.Name)
                .Select(e => new
                {
                    Name = e.Key,
                    CountEmployees = e.Count()
                })
                .Where(e => e.CountEmployees < 5);
            
            foreach (var d in departament)
            {
                Console.WriteLine($"{d.Name} {d.CountEmployees}");
            }

        }

        static void SalaryIncrease(int deptId,int percent)
        {
            
            var department = _context.Employees
                .Where(e => e.Department.DepartmentId == deptId);
            foreach (var e in department)
            {
                Console.WriteLine($"{e.EmployeeId.ToString()} {e.FirstName} {e.LastName} {e.Salary}");
            }
            Console.WriteLine("----------------");
            var increaseDepartment = _context.Employees
                .Where(e => e.Department.DepartmentId == deptId);
            foreach (var e in department)
            {
                e.Salary += e.Salary * percent/100;
                Console.WriteLine($"{e.EmployeeId.ToString()} {e.FirstName} {e.LastName} {e.Salary}");
            }

            _context.SaveChanges();
        }

        static void Tax(int id)
        {
            var reserveDept = _context.Departments.First(d => 
            d.DepartmentId != id);
            var departament = _context.Employees
                .Where(e => e.DepartmentId == id)
                .Select(e => new
                {
                    EmpId = e.EmployeeId.ToString(),
                    EmpName = e.FirstName,
                    DeptName = e.Department.Name,
                    DeptId = e.DepartmentId
                });
            foreach (var e in departament)
            {
                Console.WriteLine($"{e.EmpId} {e.EmpName} {e.DeptName}");
                
            }
            
            foreach (var employeeNewDept in _context.Employees
                .Where(e => e.DepartmentId == id))
            {
                employeeNewDept.DepartmentId = reserveDept.DepartmentId;
                employeeNewDept.ManagerId = reserveDept.ManagerId;
            }
            _context.Remove(_context.Departments
                .First(d => d.DepartmentId == id));
            _context.SaveChanges();
            
            
        }

        static void City404(string name)
        {
            var town = _context.Towns
                .Single(t => t.Name == name);
            
            foreach (var e in  _context.Addresses
                .Where(a => a.TownId == town.TownId))
            
            {
                e.TownId = null;
            }

            _context.Remove(town);
            _context.SaveChanges();
        }
    }
}
