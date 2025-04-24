using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Hosting.Internal;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    private readonly CVContext _cvContext;
    // private readonly IWebHostEnvironment _hostingEnvironment;
    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, CVContext cvContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cvContext = cvContext;
        // _hostingEnvironment = _hostingEnvironment;
    }

    [HttpPost]
    public async Task<IActionResult> Rega(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                ProfileType = model.ProfileType, // Lägg till detta
                                                 // ProfilePictureUrl = "defaultProfilePicUrl.jpg" // Lägg till standard URL här
            };



            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                var cv = new CV
                {
                    UserId = _userManager.GetUserId(User)
                };
                _cvContext.CVs.Add(cv);
                _cvContext.SaveChanges();
                return RedirectToAction("index", "home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }


    public IActionResult UserCV()
    {
        var cvs = _cvContext.CVs.ToList();
        CV myCV = new CV();
        foreach (var cv in cvs)
        {
            if (cv.UserId == _userManager.GetUserId(User))
            {
                myCV = cv;
                break;
            }
        }
        return View(myCV);
    }














    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    [HttpGet]
    public IActionResult login()
    {
        LoginViewModel loginViewModel = new LoginViewModel();
        return View(loginViewModel);
    }
    // Användare loggar in
    [HttpPost]
    public async Task<IActionResult> login(LoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("index", "home");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Ogiltigt inloggningsförsök.");
            }
        }

        return View(model);
    }

    // ... Andra metoder (t.ex. Logga ut)


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> UpdateUser()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new UpdateUserViewModel
        {
            Email = user.Email,
            Name = user.Name,
            ProfileType = user.ProfileType
            // Sätt andra fält här från användarens data
        };
        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateUser(UpdateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.Email = model.Email;
                user.Name = model.Name;
                user.ProfileType = model.ProfileType;
                // Uppdatera andra fält här

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
        }
        return View(model);
    }





    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Loggar ut användaren
        await _signInManager.SignOutAsync();

        // Återvänder till startsidan
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(); // Skapa en ny instans av ViewModel
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login");
        }

        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    public async Task<IActionResult> ShowSearchForm(SearchModel searchModel)
    {
        return View(searchModel);
    }
    public ActionResult Index(SearchModel searchModel)
    {
        var searchResults = _cvContext.Users
             .Where(e => e.Name.Contains(searchModel.SearchText)) //|| e.Description.Contains(searchModel.SearchText))
             .ToList();

        // Skicka sökresultaten till vyn

        return View("~/Views/Account/HittaCV.cshtml", searchResults);
    }






    [HttpGet]
    public IActionResult AddKompetens()
    {
        return View(new Kompetens());
    }

    [HttpPost]
    public async Task<IActionResult> AddKompetens(Kompetens model)
    {
        var userId = _userManager.GetUserId(User);
        var cv = _cvContext.CVs.FirstOrDefault(c => c.UserId == userId);
        if (cv != null)
        {
            var cvKompetens = new CVKompetens { CVId = cv.Id, Kompetens = model };
            _cvContext.CVKompetenser.Add(cvKompetens);
            await _cvContext.SaveChangesAsync();

            return RedirectToAction("UserCV");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult AddErfarenhet()
    {
        return View(new Erfarenhet());
    }

    [HttpPost]
    public async Task<IActionResult> AddErfarenhet(Erfarenhet model)
    {
        var userId = _userManager.GetUserId(User);
        var cv = _cvContext.CVs.FirstOrDefault(c => c.UserId == userId);
        if (cv != null)
        {
            var cvErfarenhet = new CVErfarenhet { CVId = cv.Id, Erfarenhet = model };
            _cvContext.CVErfarenheter.Add(cvErfarenhet);
            await _cvContext.SaveChangesAsync();

            return RedirectToAction("UserCV");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult AddUtbildning()
    {
        return View(new Utbildning());
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> AddUtbildning(Utbildning model)
    {
        var userId = _userManager.GetUserId(User);
        var cv = _cvContext.CVs.FirstOrDefault(c => c.UserId == userId);
        if (cv != null)
        {
            var cvUtbildning = new CVUtbildning { CVId = cv.Id, Utbildning = model };
            _cvContext.CVUtbildningar.Add(cvUtbildning);
            await _cvContext.SaveChangesAsync();

            return RedirectToAction("UserCV");
        }

        return View(model);
    }




    public IActionResult UploadProfilePicture()
    {
        return View();
    }


    [HttpPost]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return View("Error", "Please select a file.");
        }

        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");

        // Skapa mappen om den inte finns
        Directory.CreateDirectory(uploadPath);

        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadPath, fileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Efter uppladdningen, återvänd till en sida som bekräftar uppladdningen eller till profilsidan
            return RedirectToAction("Profile");
        }
        catch
        {
            return View("Error", "An error occurred while uploading the file.");
        }

    }





}