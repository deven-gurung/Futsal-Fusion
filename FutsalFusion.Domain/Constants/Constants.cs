namespace FutsalFusion.Domain.Constants;

public class Constants
{
    public class Roles
    {
        public const string Admin = "Admin";
        public const string Futsal = "Futsal";
        public const string Player = "Player";
    }

    public class Passwords
    {
        public const string AdminPassword = "radi0V!oleta";
        public const string FutsalPassword = "radi0V!oleta";
    }
    
    public class FilePath
    {
        public static string FutsalImagesFilePath => @"futsal-images\";
        
        public static string CourtImagesFilePath => @"court-images\";
        
        public static string ProductImagesFilePath => @"product-images\";

        public static string UsersImagesFilePath => @"user-images\";
    }
}