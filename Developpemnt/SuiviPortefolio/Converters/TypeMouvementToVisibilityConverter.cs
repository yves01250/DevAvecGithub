using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using SuiviPortefolio.Transactions.MouvementModel;

namespace SuiviPortefolio.Converters;

public class TypeMouvementToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TypeMouvement typeMouvement)
            return Visibility.Collapsed;

        if (parameter is string expectedType && !string.IsNullOrEmpty(expectedType))
        {
            // Si un paramètre est passé (ex: "Virement"), on compare
            if (Enum.TryParse<TypeMouvement>(expectedType, out var expected))
            {
                return typeMouvement == expected ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        else
        {
            // Par défaut, afficher pour Dépôt et Retrait
            return typeMouvement is TypeMouvement.Depot or TypeMouvement.Retrait 
                ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
