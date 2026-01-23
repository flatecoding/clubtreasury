using System.ComponentModel;
using System.Text.Json;
using Microsoft.JSInterop;

namespace TTCCashRegister.Data.ThemeSetting;

public sealed class UserPrefService : INotifyPropertyChanged
{
    private bool _isDarkMode;
    private bool _isRtl;
    private IJSObjectReference? _module;
    private readonly ILogger<UserPrefService> _logger;
    public const string StorageKey = "ClubCash.UserPrefData";

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
            _isRtl = pref.IsRtl;
        }
    }

    public async Task Init(IJSRuntime jsRuntime)
    {
        try
        {
            _module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/darkmode.js");
            await SetPrefs();
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

    public bool IsRtl
    {
        get => _isRtl;
        set
        {
            if (_isRtl != value)
            {
                _isRtl = value;
                OnPropertyChanged(nameof(IsRtl));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        SetPrefs().CatchAndLog(_logger);
    }

    private async ValueTask SetPrefs()
    {
        if (_module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("setCookie", StorageKey, 
                    JsonSerializer.Serialize(new UserPrefData(_isDarkMode, _isRtl)), 365);
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