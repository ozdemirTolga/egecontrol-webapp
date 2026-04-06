namespace EgeControlWebApp.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string SatisTemsilcisi = "SatisTemsilcisi";
        
        public static readonly string[] AllRoles = new[]
        {
            Admin,
            SatisTemsilcisi
        };
        
        public static readonly Dictionary<string, string> RoleDescriptions = new()
        {
            [Admin] = "Sistem yöneticisi - Tüm yetkilere sahip",
            [SatisTemsilcisi] = "Satış temsilcisi - Sadece kendi tekliflerini görebilir ve düzenleyebilir"
        };
        
        public static bool CanCreateQuotes(IEnumerable<string> userRoles)
        {
            return userRoles.Intersect(new[] { Admin, SatisTemsilcisi }).Any();
        }
        
        public static bool CanEditQuote(IEnumerable<string> userRoles, string quoteOwnerUserId, string currentUserId)
        {
            if (userRoles.Contains(Admin))
                return true;
                
            if (userRoles.Contains(SatisTemsilcisi) && quoteOwnerUserId == currentUserId)
                return true;
                
            return false;
        }
        
        public static bool CanSendQuotes(IEnumerable<string> userRoles)
        {
            return userRoles.Intersect(new[] { Admin, SatisTemsilcisi }).Any();
        }
        
        public static bool CanManageUsers(IEnumerable<string> userRoles)
        {
            return userRoles.Contains(Admin);
        }
        
        /// <summary>
        /// Admin her şeyi görür, SatisTemsilcisi sadece kendi verilerini görür
        /// </summary>
        public static bool CanSeeAllData(IEnumerable<string> userRoles)
        {
            return userRoles.Contains(Admin);
        }
    }
}
