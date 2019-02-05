namespace AuthWebApi.Helpers
{
    public static class Constants
    {
        public static class Strings
        {
            public static class JwtClaimIdentifiers
            {
                public const string Rol = "rol", Id = "id";
            }

            public static class JwtClaims
            {
                public const string ApiAccess = "api_access";
                public const string Admin = "api_control";
                public const string SuperAdmin = "full_control";
            }
        }
    }
}
