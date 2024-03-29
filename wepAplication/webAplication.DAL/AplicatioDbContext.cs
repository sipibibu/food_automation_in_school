﻿using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Text;
using wepAplication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using webAplication.Domain;
using webAplication.Domain.Persons;
using webAplication.Persons;

namespace webAplication.DAL;
/// <summary>
/// This class usses for interaction with data base
/// </summary>
public class AplicationDbContext : DbContext
{
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Menu> Menuse { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Person> Person { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Trustee> Trustees { get; set; }
    public DbSet<SchoolKid> SchoolKids { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<FileModel> Files { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<SchoolKidAttendance> Attendances { get; set; }
    public DbSet<CanteenEmployee> CanteenEmployees { get; set; }

    public DbSet<Teacher> Teachers { get; set; }

    public AplicationDbContext(DbContextOptions<AplicationDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
        if (Users.Count() == 0)
        {
            var user = new User(new Admin("admin", "string"), "string");

            Users.AddAsync(user);


            var trusteePerson = new Trustee("trustee", "Andrew");
            var trustee = new User(trusteePerson, "Andrew");

            Users.AddAsync(trustee);

            SaveChanges();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(order => order.Id)
            .HasName("PK_OrderId");

        modelBuilder.Entity<SchoolKidAttendance>()
            .HasKey(at => at.schoolKidId)
            .HasName("PK_SchoolKidAttendanceId");

        modelBuilder.Entity<User>()
            .HasKey(d => d.Id)
            .HasName("PK_UserId");

        modelBuilder.Entity<DishMenu>()
    .HasKey(t => new { t.DishId, t.MenuId});

        modelBuilder.Entity<DishMenu>()
            .HasOne(dm => dm.dish)
            .WithMany(d => d.dishMenus)
            .HasForeignKey(dm => dm.DishId);

        modelBuilder.Entity<DishMenu>()
            .HasOne(dm => dm.menu)
            .WithMany(m => m.dishMenus)
            .HasForeignKey(dm => dm.MenuId);

        modelBuilder.Entity<SchoolKidAttendance>()
            .HasKey(k => k.schoolKidId)
            .HasName("Id");


    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        JObject json;
        using (var file = File.OpenText("..\\webAplication.DAL\\Properties\\dbConnectionSettings.json"))
        using (var reader = new JsonTextReader(file))
        {
            json = (JObject)JToken.ReadFrom(reader);
        }

        optionsBuilder.EnableSensitiveDataLogging(true);
        optionsBuilder.UseNpgsql($"Host={json["Host"]};Port={json["Port"]};Database={json["Database"]};Username={json["Username"]};Password={json["Password"]}");
    }
}