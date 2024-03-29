﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using webAplication.Domain;
using webAplication.Domain.Interfaces;
using webAplication.Domain.Persons;
using webAplication.Persons;
using webAplication.Service.Interfaces;
using webAplication.Service.Models;
using AplicationDbContext = webAplication.DAL.AplicationDbContext;

namespace webAplication.Service;

public class AccountService : IAccountService
{
    private AplicationDbContext db;

    private readonly ILogger<AccountService> _logger;

    public AccountService(ILogger<AccountService> logger, AplicationDbContext context)
    {
        db = context;
        _logger = logger;
    }

    public async Task<BaseResponse<JwtSecurityTokenHandler>> RefreshToken()
    {
        throw new NotImplementedException();
    }

    [Authorize(Roles = "admin")]
    public async Task<BaseResponse<User>> Register(RegisterViewModel model)
    {
        try
        {
            User user;

            switch (model.role)
            {
                case "admin":
                    user = new User(
                            new Admin(model.role, model.name));
                    break;
                case "trustee":
                    user = new User(
                        new Trustee(model.role, model.name));
                    break;
                case "canteenEmployee":
                    user = new User(
                        new CanteenEmployee(model.role, model.name));
                    break;
                case "teacher":
                    user = new User(
                        new Teacher(model.role, model.name));
                    break;
                default:
                    return new BaseResponse<User>()
                    {
                        StatusCode = StatusCode.BAD,
                        Description = $"not avalible role: {model.role}"
                    };
            }
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();
            return new BaseResponse<User>()
            {
                Description = "User added",
                StatusCode = StatusCode.OK,
                Data = user,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[Register]: {exception.Message}");
            return new BaseResponse<User>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }

    }

    [Authorize(Roles = "admin")]
    public async Task<BaseResponse<Trustee>> PutSchoolKidIntoTrustee(string trusteeId, string[] schoolKidIds)
    {
        try
        {
            var trustee = await db.Trustees.FirstOrDefaultAsync(x => x.Id == trusteeId);
            //todo 
            trustee.schoolKidIds.Clear();
            foreach (var schoolKidId in schoolKidIds)
            {
                if (schoolKidId == null || schoolKidId.Length == 0)
                    continue;
                var schoolKid = db.SchoolKids.FirstOrDefault(sc => sc.Id == schoolKidId);
                if (schoolKid == null)
                {
                    return new BaseResponse<Trustee>()
                    {
                        StatusCode = StatusCode.BAD,
                        Description = $"there is no schoolKid with that id: {schoolKidId}"
                    };
                }
                trustee.schoolKidIds.Add(schoolKidId);
            }

            db.SaveChanges();

            return new BaseResponse<Trustee>()
            {
                StatusCode = StatusCode.OK,
                Data = trustee,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[Register]: {exception.Message}");
            return new BaseResponse<Trustee>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    [Authorize(Roles = "admin")]
    public async Task<BaseResponse<IEnumerable<SchoolKid>>> GetTrustesSchoolKids(string trusteeId)
    {
        try
        {
            Trustee trustee = await db.Trustees.FirstOrDefaultAsync(x => x.Id == trusteeId);

            if (trustee == null)
            {
                return new BaseResponse<IEnumerable<SchoolKid>>()
                {
                    StatusCode = StatusCode.BAD,
                    Description = "There is no Trustee with that id"
                };
            }

            var schoolKids = new List<SchoolKid>();
            foreach (var schoolKidId in trustee.schoolKidIds)
            {
                var schoolKid = db.SchoolKids.FirstOrDefault(x => x.Id == schoolKidId);
                schoolKids.Add(schoolKid);
            }

            return new BaseResponse<IEnumerable<SchoolKid>>
            {
                Data = schoolKids
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[Register]: {exception.Message}");
            return new BaseResponse<IEnumerable<SchoolKid>>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    [Authorize(Roles = "admin")]
    public async Task<BaseResponse<SchoolKid>> CreateSchoolKid(SchoolKid schoolKid)
    {
        //todo add validation
        try
        {
            db.SchoolKids.AddAsync(schoolKid);
            db.Attendances.AddAsync(new SchoolKidAttendance(schoolKid));
            await db.SaveChangesAsync();

            return new BaseResponse<SchoolKid>()
            {
                StatusCode = StatusCode.OK,
                Data = schoolKid,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[Register]: {exception.Message}");
            return new BaseResponse<SchoolKid>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model)
    {
        try
        {
            User? user = await db.Users.Include(x => x.Person).FirstOrDefaultAsync(x => x.Login == model.Login);

            if (user == null)
            {
                return new BaseResponse<ClaimsIdentity>
                {
                    Description = "User not found"
                };
            }

            if (user.Password != model.Password)
            {
                return new BaseResponse<ClaimsIdentity>
                {
                    Description = "Wrong password"
                };
            }

            var result = Authenticate(user);

            return new BaseResponse<ClaimsIdentity>
            {
                Data= result,
                StatusCode= StatusCode.OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Login]: {ex.Message}");
            return new BaseResponse<ClaimsIdentity>
            {
                Description= ex.Message,
                StatusCode= StatusCode.BAD
            };
        }
    }
    private ClaimsIdentity Authenticate(User user)
    {
        var claims = new List<Claim>
        {
            //new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
            //new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Person.role)
            new Claim("name", user.Login),
            new Claim("role", user.Person.role),
            new Claim("id", user.Person.Id)
        };
        return new ClaimsIdentity(claims); //
    }
    public Task<BaseResponse<JwtSecurityTokenHandler>> RefreshToken(RegisterViewModel model)
    {
        throw new NotImplementedException();
    }
    public async Task<BaseResponse<IEnumerable<SchoolKid>>> GetSchoolKids()
    {
        try
        {
            var schoolKids = db.SchoolKids.ToList();

            return new BaseResponse<IEnumerable<SchoolKid>>()
            {
                StatusCode = StatusCode.OK,
                Data = schoolKids,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[GetSchoolKids]: {exception.Message}");
            return new BaseResponse<IEnumerable<SchoolKid>>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<IEnumerable<Teacher>>> GetTeachers()
    {
        try
        {
            var teachers = db.Teachers.ToList();

            return new BaseResponse<IEnumerable<Teacher>>()
            {
                StatusCode = StatusCode.OK,
                Data = teachers,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[GetTeachers]: {exception.Message}");
            return new BaseResponse<IEnumerable<Teacher>>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<IEnumerable<CanteenEmployee>>> GetCanteenEmployees()
    {
        try
        {
            var employees = db.CanteenEmployees.ToList();

            return new BaseResponse<IEnumerable<CanteenEmployee>>()
            {
                StatusCode = StatusCode.OK,
                Data = employees,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[GetTeachers]: {exception.Message}");
            return new BaseResponse<IEnumerable<CanteenEmployee>>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<Teacher>> UpdateTeacher(Teacher teacher, string id)
    {
        try
        {
            var teacherOld = db.Teachers.FirstOrDefault(x => x.Id == id);
            if (teacherOld == null)
                return new BaseResponse<Teacher>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Teacher with that id: {id}"
                };
            teacherOld.Update(teacher);
            db.Teachers.Update(teacherOld);
            db.SaveChanges();
            return new BaseResponse<Teacher>()
            {
                StatusCode = StatusCode.OK,
                Data = teacher,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[UpdateTeacher]: {exception.Message}");
            return new BaseResponse<Teacher>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<CanteenEmployee>> UpdateCanteenEmployee(CanteenEmployee canteenEmployee, string id)
    {
        try
        {
            var canteenEmployeeOld = db.CanteenEmployees.FirstOrDefault(x => x.Id == id);
            if (canteenEmployeeOld == null)
                return new BaseResponse<CanteenEmployee>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Teacher with that id: {id}"
                };
            canteenEmployeeOld.Update(canteenEmployee);
            db.CanteenEmployees.Update(canteenEmployeeOld);
            db.SaveChanges();
            return new BaseResponse<CanteenEmployee>()
            {
                StatusCode = StatusCode.OK,
                Data = canteenEmployeeOld,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[UpdateCanteenEmployee]: {exception.Message}");
            return new BaseResponse<CanteenEmployee>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<Trustee>> UpdateTrustee(Trustee trustee, string id)
    {
        try
        {
            var trusteeOld = db.Trustees.FirstOrDefault(x => x.Id == id);
            if (trusteeOld == null)
                return new BaseResponse<Trustee>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Trustee with that id: {id}"
                };
            trusteeOld.Update(trustee);
            db.Trustees.Update(trusteeOld);
            db.SaveChanges();
            return new BaseResponse<Trustee>()
            {
                StatusCode = StatusCode.OK,
                Data = trustee,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[UpdateTrustee]: {exception.Message}");
            return new BaseResponse<Trustee>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<SchoolKid>> UpdateSchoolKid(SchoolKid schoolKid, string id)
    {
        try
        {
            var schoolKidOld = db.SchoolKids.FirstOrDefault(x => x.Id == id);
            if (schoolKidOld == null)
                return new BaseResponse<SchoolKid>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Trustee with that id: {id}"
                };
            schoolKidOld.Update(schoolKid);
            db.SchoolKids.Update(schoolKidOld);
            db.SaveChanges();
            return new BaseResponse<SchoolKid>()
            {
                StatusCode = StatusCode.OK,
                Data = schoolKid,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[UpdateSchoolKid]: {exception.Message}");
            return new BaseResponse<SchoolKid>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<Teacher>> DeleteTeacher(string id)
    {
        try
        {
            var teacher = db.Teachers.FirstOrDefault(x => x.Id == id);
            if (teacher == null)
                return new BaseResponse<Teacher>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Teacher with that id: {id}"
                };
            db.Teachers.Remove(teacher);
            db.SaveChanges();
            return new BaseResponse<Teacher>()
            {
                StatusCode = StatusCode.OK,
                Data = teacher,
            };
        }
        catch(Exception exception)
        {
            _logger.LogError(exception, $"[DeleteTeacher]: {exception.Message}");
            return new BaseResponse<Teacher>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
    public async Task<BaseResponse<CanteenEmployee>> DeleteCanteenEmployee(string id)
    {
        try
        {
            var canteenEmployee = db.CanteenEmployees.FirstOrDefault(x => x.Id == id);
            if (canteenEmployee == null)
                return new BaseResponse<CanteenEmployee>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Teacher with that id: {id}"
                };
            db.CanteenEmployees.Remove(canteenEmployee);
            db.SaveChanges();
            return new BaseResponse<CanteenEmployee>()
            {
                StatusCode = StatusCode.OK,
                Data = canteenEmployee,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[DeleteCanteenEmployees]: {exception.Message}");
            return new BaseResponse<CanteenEmployee>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<Trustee>> DeleteTrustee(string id)
    {
        try
        {
            var trustee = db.Trustees.FirstOrDefault(x => x.Id == id);
            if (trustee == null)
                return new BaseResponse<Trustee>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no Trustee with that id: {id}"
                };
            db.Trustees.Remove(trustee);
            db.SaveChanges();
            return new BaseResponse<Trustee>()
            {
                StatusCode = StatusCode.OK,
                Data = trustee,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[DeleteTrustee]: {exception.Message}");
            return new BaseResponse<Trustee>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<SchoolKid>> DeleteSchoolKid(string id)
    {
        try
        {
            var schoolKid = db.SchoolKids.FirstOrDefault(x => x.Id == id);
            if (schoolKid == null)
                return new BaseResponse<SchoolKid>()
                {
                    StatusCode = StatusCode.OK,
                    Description = $"there is no SchoolKid with that id: {id}"
                };
            db.SchoolKids.Remove(schoolKid);
            db.SaveChanges();
            return new BaseResponse<SchoolKid>()
            {
                StatusCode = StatusCode.OK,
                Data = schoolKid,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[DeleteSchoolKid]: {exception.Message}");
            return new BaseResponse<SchoolKid>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }


    public async Task<BaseResponse<IEnumerable<Trustee>>> GetTrustees()
    {
        try
        {
            var trustees = db.Trustees.ToList();

            return new BaseResponse<IEnumerable<Trustee>>()
            {
                StatusCode = StatusCode.OK,
                Data = trustees,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[GetTrustees]: {exception.Message}");
            return new BaseResponse<IEnumerable<Trustee>>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }

    public async Task<BaseResponse<Person>> PutImage(string personId, string imageId)
    {
        try
        {
            var person = await db.Person.FirstOrDefaultAsync(p => p.Id == personId);
            var file = await db.Files.FirstOrDefaultAsync(f => f.Id == imageId);

            if (person == null)
                return new BaseResponse<Person>()
                {
                    StatusCode = StatusCode.BAD,
                    Description= $"there is no person with that id: {personId}"
                };
            if (file == null)
                return new BaseResponse<Person>()
                {
                    StatusCode = StatusCode.BAD,
                    Description= $"there is no file with that id: {imageId}"
                };

            person.imageId = imageId;
            db.SaveChanges();
            return new BaseResponse<Person>()
            {
                StatusCode = StatusCode.OK,
            };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"[PutImage]: {exception.Message}");
            return new BaseResponse<Person>()
            {
                Description = exception.Message,
                StatusCode = StatusCode.BAD
            };
        }
    }
}