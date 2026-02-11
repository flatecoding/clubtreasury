using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TTCCashRegister.Data;

namespace TTCCashRegister.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;
    private readonly IAntiforgery _antiforgery;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginModel> logger,
        IAntiforgery antiforgery)
    {
        _signInManager = signInManager;
        _logger = logger;
        _antiforgery = antiforgery;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public string AntiforgeryTokenName { get; private set; } = "";
    public string AntiforgeryTokenValue { get; private set; } = "";

    public async Task OnGetAsync(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ReturnUrl = returnUrl;

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        AntiforgeryTokenName = tokens.HeaderName!;
        AntiforgeryTokenValue = tokens.RequestToken!;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // Populate antiforgery tokens for re-render
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        AntiforgeryTokenName = tokens.HeaderName!;
        AntiforgeryTokenValue = tokens.RequestToken!;
        ReturnUrl = returnUrl;

        if (!string.IsNullOrEmpty(Input.Passkey?.Error))
        {
            ErrorMessage = $"Error: {Input.Passkey.Error}";
            return Page();
        }

        Microsoft.AspNetCore.Identity.SignInResult result;
        if (!string.IsNullOrEmpty(Input.Passkey?.CredentialJson))
        {
            // Passkey sign-in — skip form validation
            result = await _signInManager.PasskeySignInAsync(Input.Passkey.CredentialJson);
        }
        else
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        }

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in.");
            return LocalRedirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
        {
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public PasskeyInputModel? Passkey { get; set; }
    }
}
