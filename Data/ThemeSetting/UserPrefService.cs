using System.ComponentModel;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ClubTreasury.Data.ThemeSetting;

public sealed class UserPrefService : INotifyPropertyChanged
{
    private bool _isDarkMode;
    private IJSObjectReference? _module;
    private IJSRuntime? _jsRuntime;
    private readonly ILogger<UserPrefService> _logger;
    public const string StorageKey = "ClubTreasury.UserPrefData";

    public UserPrefService(ILogger<UserPrefService> logger)
    {
        _logger = logger;
    }

    public UserPrefService(string userPrefString, ILogger<UserPrefService> logger)
    {
        _logger = logger;
        
        if (!string.IsNullOrEmpty(userPrefString) &&
            JsonSerializer.Deserialize<UserPrefData>(userPrefString) is { } pref)
        {
            _isDarkMode = pref.IsDarkMode;
        }
    }

    public async Task Init(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        try
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/darkmode.js");
            
            var cookieValue = await _module.InvokeAsync<string>("getCookie", StorageKey);
            if (!string.IsNullOrEmpty(cookieValue) &&
                JsonSerializer.Deserialize<UserPrefData>(cookieValue) is { } pref)
            {
                _isDarkMode = pref.IsDarkMode;
                OnPropertyChanged(nameof(IsDarkMode));
            }
            
            if (string.IsNullOrEmpty(cookieValue))
            {
                await SetPrefs();
            }
        }
        catch (TaskCanceledException)
        {
            // Can be ignored
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "An error occured while initializing darkmode.js - Cookie-function isn't working properly");
        }
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        SetPrefs().CatchAndLog(_logger);
        UpdateThemeBackground().CatchAndLog(_logger);
    }

    private async ValueTask UpdateThemeBackground()
    {
        if (_jsRuntime != null)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("setThemeBackground", _isDarkMode);
            }
            catch (TaskCanceledException)
            {
                // Can be ignored
            }
        }
    }

    private async ValueTask SetPrefs()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("setCookie", StorageKey, 
                    JsonSerializer.Serialize(new UserPrefData(_isDarkMode)), 365);
            }
            catch (TaskCanceledException)
            {
                // Can be ignored
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while storing user-preferences");
            }
        }
    }
}

public static class TaskExtensions
{
    public static async void CatchAndLog(this ValueTask task, ILogger logger)
    {
        try
        {
            await task;
        }
        catch (TaskCanceledException)
        {
            // can be ignored
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in async Task");
        }
    }
}