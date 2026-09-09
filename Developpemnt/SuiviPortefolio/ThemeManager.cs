using System.Windows;

public static class ThemeManager
{
    private const string LightThemePath = "Ressources/Theme.xaml";
    private const string DarkThemePath = "Ressources/DarkTheme.xaml";

    public static void ApplyTheme(Uri themeUri)
    {
        for (int index = Application.Current.Resources.MergedDictionaries.Count - 1; index >= 0; index--)
        {
            ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[index];
            if (IsThemeDictionary(dictionary))
            {
                Application.Current.Resources.MergedDictionaries.RemoveAt(index);
            }
        }

        ResourceDictionary theme = new ResourceDictionary { Source = themeUri };
        Application.Current.Resources.MergedDictionaries.Insert(0, theme);
    }

    private static bool IsThemeDictionary(ResourceDictionary dictionary)
    {
        string? source = dictionary.Source?.OriginalString;
        if (string.IsNullOrWhiteSpace(source))
            return false;

        string normalizedSource = source.Replace('\\', '/').TrimStart('/');
        return normalizedSource.EndsWith(LightThemePath, StringComparison.OrdinalIgnoreCase) ||
               normalizedSource.EndsWith(DarkThemePath, StringComparison.OrdinalIgnoreCase);
    }

    public static void ApplyLightTheme()
    {
        ApplyTheme(new Uri(LightThemePath, UriKind.Relative));
    }

    public static void ApplyDarkTheme()
    {
        ApplyTheme(new Uri(DarkThemePath, UriKind.Relative));
    }
}