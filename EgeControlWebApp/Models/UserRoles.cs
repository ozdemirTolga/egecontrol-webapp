namespace EgeControlWebApp.Models
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string QuoteCreator = "QuoteCreator";
        public const string QuoteEditor = "QuoteEditor";
        public const string QuoteSender = "QuoteSender";
        public const string Viewer = "Viewer";
        
        public static readonly string[] AllRoles = new[]
        {
            Admin,
            Manager,
            QuoteCreator,
            QuoteEditor,
            QuoteSender,
            Viewer
        };
        
        public static readonly Dictionary<string, string> RoleDescriptions = new()
        {
            [Admin] = "Sistem yöneticisi - Tüm yetkilere sahip",
            [Manager] = "Yönetici - Tüm teklifleri yönetebilir",
            [QuoteCreator] = "Teklif oluşturabilir ve kendi tekliflerini düzenleyebilir",
            [QuoteEditor] = "Tüm teklifleri düzenleyebilir",
            [QuoteSender] = "Teklifleri gönderebilir (e-posta)",
            [Viewer] = "Sadece görüntüleme yetkisi"
        };
        
        public static bool CanCreateQuotes(IEnumerable<string> userRoles)
        {
            return userRoles.Intersect(new[] { Admin, Manager, QuoteCreator }).Any();
        }
        
        public static bool CanEditQuote(IEnumerable<string> userRoles, string quoteOwnerUserId, string currentUserId)
        {
            if (userRoles.Contains(Admin) || userRoles.Contains(Manager) || userRoles.Contains(QuoteEditor))
                return true;
                
            if (userRoles.Contains(QuoteCreator) && quoteOwnerUserId == currentUserId)
                return true;
                
            return false;
        }
        
        public static bool CanSendQuotes(IEnumerable<string> userRoles)
        {
            return userRoles.Intersect(new[] { Admin, Manager, QuoteSender, QuoteEditor }).Any();
        }
        
        public static bool CanManageUsers(IEnumerable<string> userRoles)
        {
            return userRoles.Contains(Admin);
        }
    }
}
