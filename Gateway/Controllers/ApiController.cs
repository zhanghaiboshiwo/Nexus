using Aiursoft.Gateway.Data;
using Aiursoft.Gateway.Models;
using Aiursoft.Gateway.Models.ApiViewModels;
using Aiursoft.Pylon;
using Aiursoft.Pylon.Attributes;
using Aiursoft.Pylon.Models;
using Aiursoft.Pylon.Models.API;
using Aiursoft.Pylon.Models.API.ApiViewModels;
using Aiursoft.Pylon.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Aiursoft.Gateway.Controllers
{
    public class ApiController : Controller
    {
        private readonly UserManager<GatewayUser> _userManager;
        private readonly GatewayDbContext _dbContext;
        private readonly ACTokenManager _tokenManager;

        public ApiController(
            UserManager<GatewayUser> userManager,
            GatewayDbContext context,
            ACTokenManager tokenManager)
        {
            _userManager = userManager;
            _dbContext = context;
            _tokenManager = tokenManager;
        }

        private void _ApplyCultureCookie(string culture)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    HttpOnly = true,
                });
        }

        [HttpGet]
        public IActionResult Setlang(SetlangAddressModel model)
        {
            return View(new SetlangViewModel
            {
                Host = model.Host,
                Path = model.Path
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetLang(SetlangViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                _ApplyCultureCookie(model.Culture);
            }
            catch (CultureNotFoundException)
            {
                return Json(new AiurProtocol { Message = "Not a language.", Code = ErrorType.InvalidInput });
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                user.PreferedLanguage = model.Culture;
                await _userManager.UpdateAsync(user);
            }
            string toGo = new AiurUrl(model.Host, "Api", "SetSonLang", new
            {
                model.Culture,
                ReturnUrl = model.Path
            }).ToString();
            return Redirect(toGo);
        }

        [APIExpHandler]
        [APIModelStateChecker]
        [APIProduces(typeof(AllUserGrantedViewModel))]
        public async Task<IActionResult> AllUserGranted([Required]string accessToken)
        {
            var appid = _tokenManager.ValidateAccessToken(accessToken);
            var grants = await _dbContext.LocalAppGrant.Include(t => t.User).Where(t => t.AppID == appid).Take(400).ToListAsync();
            var model = new AllUserGrantedViewModel
            {
                AppId = appid,
                Grants = new List<Grant>(),
                Code = ErrorType.Success,
                Message = "Successfully get all your users"
            };
            model.Grants.AddRange(grants);
            return Json(model);
        }

        [HttpPost]
        [APIExpHandler]
        [APIModelStateChecker]
        public async Task<IActionResult> DropGrants([Required]string accessToken)
        {
            var appid = _tokenManager.ValidateAccessToken(accessToken);
            _dbContext.LocalAppGrant.Delete(t => t.AppID == appid);
            await _dbContext.SaveChangesAsync();
            return this.Protocol(ErrorType.Success, "Successfully droped all users granted!");
        }

        private Task<GatewayUser> GetCurrentUserAsync()
        {
            return _dbContext
                .Users
                .Include(t => t.Emails)
                .SingleOrDefaultAsync(t => t.UserName == User.Identity.Name);
        }
    }
}
