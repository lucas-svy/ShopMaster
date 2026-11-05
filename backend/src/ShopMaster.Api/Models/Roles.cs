namespace ShopMaster.Api.Models;

/// <summary>
/// Rôles applicatifs. "Admin" est attribué dans Auth0 ; tout autre compte est "Customer".
/// </summary>
public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
}
