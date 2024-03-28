using FutsalFusion.Application.Interfaces.Repositories.Base;
using FutsalFusion.Domain.Constants;
using FutsalFusion.Domain.Entities;
using FutsalFusion.Domain.Utilities;

namespace FutsalFusion.Infrastructure.Persistence.Seed;

public class DbInitializer(IGenericRepository genericRepository)
    : IDbInitializer
{
    public void Initialize()
    {
        try
        {
            if (!genericRepository.Get<AppRole>().Any())
            {
                var admin = new AppRole()
                {
                    Name = Constants.Roles.Admin
                };
                var futsalOwner = new AppRole()
                {
                    Name = Constants.Roles.Futsal
                };
                var player = new AppRole()
                {
                    Name = Constants.Roles.Player
                };

                genericRepository.Insert(admin);
                genericRepository.Insert(futsalOwner);
                genericRepository.Insert(player);
            }

            if (genericRepository.Get<AppUser>().Any()) return;
            
            var adminRole = genericRepository.GetFirstOrDefault<AppRole>(x => x.Name == Constants.Roles.Admin);
                
            var superAdminUser = new AppUser
            {
                FullName = "Super Admin",
                EmailAddress = "superadmin@superadmin.com",
                UserName = "superadmin@superadmin.com",
                Password = Password.CreatePasswordHash(Constants.Passwords.AdminPassword, Password.CreateSalt(Password.PasswordSalt)),
                RoleId = adminRole.Id,
                CreatedAt = DateTime.Now,
                MobileNo = "+977 9803364638",
                ImageURL = null
            };

            genericRepository.Insert(superAdminUser);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}