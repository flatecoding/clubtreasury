using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TTCCashRegister.Data;

namespace TTCCashRegister.Areas.Identity.Pages.Account.Manage;

public class RenamePasskeyModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RenamePasskeyModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = "";

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? CurrentPasskeyName { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64UrlDecode(Id);
        }
        catch (FormatException)
        {
            StatusMessage = "Error: The specified passkey ID had an invalid format.";
            return RedirectToPage("Passkeys");
        }

        var passkey = await _userManager.GetPasskeyAsync(user, credentialId);
        if (passkey is null)
        {
            StatusMessage = "Error: The specified passkey could not be found.";
            return RedirectToPage("Passkeys");
        }

        CurrentPasskeyName = passkey.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        byte[] credentialId;
        try
        {
            credentialId = Base64UrlDecode(Id);
        }
        catch (FormatException)
        {
            StatusMessage = "Error: The specified passkey ID had an invalid format.";
            return RedirectToPage("Passkeys");
        }

        var passkey = await _userManager.GetPasskeyAsync(user, credentialId);
        if (passkey is null)
        {
            StatusMessage = "Error: The specified passkey could not be found.";
            return RedirectToPage("Passkeys");
        }

        passkey.Name = Input.Name;
        var result = await _userManager.AddOrUpdatePasskeyAsync(user, passkey);
        if (!result.Succeeded)
        {
            StatusMessage = "Error: The passkey could not be updated.";
            return RedirectToPage("Passkeys");
        }

        StatusMessage = "Passkey updated successfully.";
        return RedirectToPage("Passkeys");
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

    public class InputModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Passkey names must be no longer than {1} characters.")]
        public string Name { get; set; } = "";
    }
}
