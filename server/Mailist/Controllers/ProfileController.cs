using ChurchTools;
using Mailist.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mailist.Controllers;

[ApiController]
public class ProfileController : ControllerBase
{
    private readonly TokenService tokenService;

    public ProfileController(TokenService tokenService)
    {
        this.tokenService = tokenService;
    }

    [HttpPost("~/api/token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponse>> GetAccessToken([FromBody] TokenRequest request)
    {
        using var churchToolsFromRequest = ChurchToolsApi.CreateWithToken(new Uri(request.ChurchToolsUrl), request.LoginToken);
        var user = await churchToolsFromRequest.GetPerson();
        var permissions = await churchToolsFromRequest.GetGlobalPermissions();

        if (permissions.Mailist == null)
            return BadRequest("User does not have permissions for Mailist or the Mailist plugin is not installed in ChurchTools");

        var module = await churchToolsFromRequest.GetCustomModule("mailist");
        var categories = await churchToolsFromRequest.GetCustomDataCategories(module.Id);
        var configCategory = categories.FirstOrDefault(c => c.Shorty == "config");
        if (configCategory == null)
            return BadRequest("Extension is not initialized. Custom data category 'config' was not found");

        bool isManager = permissions.Mailist.CreateCustomCategory;
        bool isAdmin = permissions.Mailist.EditCustomCategory.Contains(configCategory.Id);

        string tokenString = tokenService.CreateToken(user.Id.ToString(), isManager, isAdmin);

        return new TokenResponse { AccessToken = tokenString };
    }

    public class TokenRequest
    {
        public required string ChurchToolsUrl { get; init; }
        public required string LoginToken { get; init; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
