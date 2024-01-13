using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using SocialBookmarkingApp.Models;

namespace SocialBookmarkingApp; 

public static class HtmlHelperExtensions
{
    private const string _partialViewScriptItemPrefix = "scripts_";
    public static IHtmlContent PartialSectionScripts(this IHtmlHelper htmlHelper, Func<object, HelperResult> template)
    {
        htmlHelper.ViewContext.HttpContext.Items[_partialViewScriptItemPrefix + Guid.NewGuid()] = template;
        return new HtmlContentBuilder();
    }
    public static IHtmlContent RenderPartialSectionScripts(this IHtmlHelper htmlHelper)
    {
        var partialSectionScripts = htmlHelper.ViewContext.HttpContext.Items.Keys
            .Where(k => Regex.IsMatch(
                k.ToString(),
                "^" + _partialViewScriptItemPrefix + "([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$"));
        var contentBuilder = new HtmlContentBuilder();
        foreach (var key in partialSectionScripts)
        {
            var template = htmlHelper.ViewContext.HttpContext.Items[key] as Func<object, HelperResult>;
            if (template != null)
            {
                var writer = new System.IO.StringWriter();
                template(null).WriteTo(writer, HtmlEncoder.Default);
                contentBuilder.AppendHtml(writer.ToString());
            }
        }
        return contentBuilder;
    }
    
    public static async Task<bool> CanEditResource(this IHtmlHelper htmlHelper, 
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager,
        ApplicationUser? owner) {
        if (owner == null) return true;
        var user = await userManager.GetUserAsync(htmlHelper.ViewContext.HttpContext.User);
        if (user == null || !signInManager.IsSignedIn(htmlHelper.ViewContext.HttpContext.User)) return false;
        return user.Id == owner.Id || (await userManager.IsInRoleAsync(user, "Admin"));
    }
}