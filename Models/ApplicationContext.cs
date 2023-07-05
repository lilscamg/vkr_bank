using Microsoft.EntityFrameworkCore;
using vkr_bank.Models;

public class ApplicationContext : DbContext
{
    public DbSet<UserInfo> UserInfos { get; set; } = null!;
    public DbSet<UserSRP> UserSRPs { get; set; } = null!;
    public DbSet<Credit> Credits { get; set; } = null!;
    public DbSet<Organization> Organizations { get; set; } = null!;
    public DbSet<EmploymentRegister> EmploymentRegisters { get; set; } = null!;
    public DbSet<OrganizationInfo> OrganizationInfos { get; set; } = null!;
    public DbSet<CreditProccessing> CreditProccessings { get; set; } = null!;   


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=vkr_bank;Username=postgres;Password=2312");
    }
}
