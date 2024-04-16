namespace FutsalFusion.Domain.Utilities;

public static class ExtensionMethod
{
    public static string SetUniqueFileName(this string fileExtension)
    {
        var renamedFileName = DateTime.Now.Year.ToString() + 
                              DateTime.Now.Month.ToString() + 
                              DateTime.Now.Day.ToString() + 
                              DateTime.Now.Hour.ToString() + 
                              DateTime.Now.Minute.ToString() + 
                              DateTime.Now.Millisecond.ToString();
        
        return renamedFileName + fileExtension;
    }
    
    public static string GenerateOTP()
    {
        const string capitalAlphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string smallAlphabets = "abcdefghijklmnopqrstuvwxyz";
        const string numbers = "1234567890";

        var characters = numbers;
        characters += capitalAlphabets + numbers + smallAlphabets;

        var otp = string.Empty;
        
        for (var i = 0; i < 6; i++)
        {
            string character;
            do
            {
                var index = new Random().Next(0, characters.Length);
                
                character = characters.ToCharArray()[index].ToString();
            } while (otp.Contains(character));
            
            otp += character;
        }
        
        return otp;
    }
}