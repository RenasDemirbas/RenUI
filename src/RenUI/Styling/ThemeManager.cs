namespace RenUI.Styling;

public sealed class ThemeManager
{
    private static ThemeManager? _instance;
    private static readonly object _lock = new();

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ThemeManager();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, Theme> _themes = new();
    private Theme _currentTheme;

    public Theme CurrentTheme => _currentTheme;

    public event Action<Theme>? ThemeChanged;

    private ThemeManager()
    {
        _currentTheme = Theme.Dark;
        RegisterTheme(Theme.Dark);
        RegisterTheme(Theme.Light);
    }

    public void RegisterTheme(Theme theme)
    {
        _themes[theme.Name] = theme;
    }

    public void UnregisterTheme(string name)
    {
        if (name != "Dark" && name != "Light")
        {
            _themes.Remove(name);
        }
    }

    public void SetTheme(string name)
    {
        if (_themes.TryGetValue(name, out var theme))
        {
            _currentTheme = theme;
            ThemeChanged?.Invoke(theme);
        }
    }

    public void SetTheme(Theme theme)
    {
        _currentTheme = theme;
        if (!_themes.ContainsKey(theme.Name))
        {
            RegisterTheme(theme);
        }
        ThemeChanged?.Invoke(theme);
    }

    public Theme? GetTheme(string name)
    {
        return _themes.TryGetValue(name, out var theme) ? theme : null;
    }

    public IEnumerable<string> GetAvailableThemes()
    {
        return _themes.Keys;
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
