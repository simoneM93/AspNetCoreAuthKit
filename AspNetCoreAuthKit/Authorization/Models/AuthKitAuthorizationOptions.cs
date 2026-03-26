using AspNetCoreAuthKit.ApiKey.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace AspNetCoreAuthKit.Authorization.Models
{
    public class AuthKitAuthorizationOptions
    {
        public string RoleClaimType { get; set; } = "role";

        public string AdminRoleValue { get; set; } = "admin";

        public string JwtScheme { get; set; } = JwtBearerDefaults.AuthenticationScheme;

        public string ApiKeyScheme { get; set; } = ApiKeyConstants.AuthenticationScheme;
    }
}
