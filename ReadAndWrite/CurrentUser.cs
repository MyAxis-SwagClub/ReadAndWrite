namespace ReadAndWrite
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Login { get; set; }
        public static string DisplayName { get; set; }
        public static string Email { get; set; }
        public static int RoleId { get; set; }
        public static bool IsFrozen { get; set; }
        public static bool IsAdmin => RoleId == 3;
        public static bool IsAuthor => RoleId == 2;
        public static bool IsReader => RoleId == 1;

        public static void Clear()
        {
            UserId = 0;
            Login = null;
            DisplayName = null;
            Email = null;
            RoleId = 0;
            IsFrozen = false;
        }
    }
}