using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TTCCashRegister.Data;

namespace TTCCashRegister.Areas.Identity.Pages.Account.Manage;

public class PasskeysModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;

    public PasskeysModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
    }

    public IList<UserPasskeyInfo>? CurrentPasskeys { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public PasskeyInputModel Input { get; set; } = new();

    [BindProperty]
    public string? CredentialId { get; set; }

    public string AntiforgeryTokenName { get; private set; } = "";
    public string AntiforgeryTokenValue { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        CurrentPasskeys = await _userManager.GetPasskeysAsync(user);

        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        AntiforgeryTokenName = tokens.HeaderName!;
        AntiforgeryTokenValue = tokens.RequestToken!;

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!string.IsNullOrEmpty(Input.Error))
        {
            StatusMessage = $"Error: {Input.Error}";
            return RedirectToPage();
        }

        if (string.IsNullOrEmpty(Input.CredentialJson))
        {
            StatusMessage = "Error: The browser did not provide a passkey.";
            return RedirectToPage();
        }

        var currentPasskeys = await _userManager.GetPasskeysAsync(user);
        if (currentPasskeys.Count >= 100)
        {
            StatusMessage = "Error: You have reached the maximum number of allowed passkeys.";
            return RedirectToPage();
        }

        var attestationResult = await _signInManager.PerformPasskeyAttestationAsync(Input.CredentialJson);
        if (!attestationResult.Succeeded)
        {
            StatusMessage = $"Error: Could not add the passkey: {attestationResult.Failure.Message}";
            return RedirectToPage();
        }

        var addPasskeyResult = await _userManager.AddOrUpdatePasskeyAsync(user, attestationResult.Passkey);
        if (!addPasskeyResult.Succeeded)
        {
            StatusMessage = "Error: The passkey could not be added to your account.";
            return RedirectToPage();
        }

        // Redirect to rename page so user can name the passkey
        var credentialIdBase64Url = Convert.ToBase64String(attestationResult.Passkey.CredentialId)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return RedirectToPage("RenamePasskey", new { id = credentialIdBase64Url });
    }

    public async Task<IActionResult> OnPostRenameAsync()
    {
        return RedirectToPage("RenamePasskey", new { id = CredentialId });
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        byte[] credentialIdBytes;
        try
        {
            credentialIdBytes = Base64UrlDecode(CredentialId!);
        }
        catch (FormatException)
        {
            StatusMessage = "Error: The specified passkey ID had an invalid format.";
            return RedirectToPage();
        }

        var result = await _userManager.RemovePasskeyAsync(user, credentialIdBytes);
        if (!result.Succeeded)
        {
            StatusMessage = "Error: The passkey could not be deleted.";
            return RedirectToPage();
        }

        StatusMessage = "Passkey deleted successfully.";
        return RedirectToPage();
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
