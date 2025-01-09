using Client.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Client.Services.CoockiePolicy
{
    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCookie(string name)
        {
            return _httpContextAccessor.HttpContext.Request.Cookies[name];
        }

        public void SetCookie(string name, string value, DateTime expiry)
        {
            var options = new CookieOptions
            {
                Expires = expiry,
                HttpOnly = true
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(name, value, options);
        }
    }
}
