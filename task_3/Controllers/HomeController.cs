using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using task_3.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace task_3.Controllers
{
    public class RefreshLoginAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await context.HttpContext.SignOutAsync();
    
            await next();
        }
    }
    
    public class HomeController : Controller
    {
        private UserContext _db;
        
        public HomeController(UserContext db)
        {
            _db = db;
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _db.Users.ToListAsync());
        }

        [HttpGet]
        [RefreshLogin]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    if (user.State == Status.Blocked)
                    {
                        ModelState.AddModelError("", "This account is blocked");
                        return View(model);
                    }
                    user.LastLoginDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                    await Authenticate(model.Email);

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Invalid login or(and) password");
            }
            return View(model);
        }
        
        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        [RefreshLogin]
        public async Task<IActionResult> Register()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    user = new User(model.Name, model.Email, model.Password);
                    await _db.Users.AddAsync(user);
                    await _db.SaveChangesAsync();
        
                    await Authenticate(model.Email);
        
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "This e-mail is already used");
            }
        
            return View(model);
        }

        [HttpPost]
        public IActionResult Block([FromBody]int[] users)
        {
            foreach (var id in users)
            {
                User user = _db.Users.FirstOrDefault(u => u.Id == id);
                
                user.State = Status.Blocked;

                if (User.Identity.Name == user.Email)
                {
                    HttpContext.SignOutAsync();
                }
            }
            _db.SaveChanges();
            
            return View("Index", _db.Users.ToList());
        }

        [HttpPost]
        public IActionResult Delete([FromBody]int[] users)
        {
            foreach (var id in users)
            {
                User user = _db.Users.FirstOrDefault(u => u.Id == id);
                _db.Users.Remove(user);
                
                if (User.Identity.Name == user.Email)
                {
                    HttpContext.SignOutAsync();
                }
            }

            _db.SaveChanges();
            return View("Index", _db.Users.ToList());
        }
        
        [HttpPost]
        public ActionResult Unblock([FromBody]int[] users)
        {
            foreach (var id in users)
            {
                User user = _db.Users.FirstOrDefault(u => u.Id == id);
                user.State = Status.Active;
            }
            _db.SaveChanges();
            
            return View("Index", _db.Users.ToList());
        }
    }
}