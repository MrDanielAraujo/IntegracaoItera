using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace IntegracaoItera.Common;

public static class Extensions
{
    public static string FormatarCnpj(this string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj)) return "";
        cnpj = cnpj.PadLeft(14, '0'); // Preenche com zeros à esquerda se tiver menos de 14 dígitos

        if (cnpj.Length != 14)
            throw new ArgumentException("O CNPJ deve ter 14 dígitos.");

        return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
    }

    /// <summary>
    /// Remove todos os caracteres especiais do CNPJ.
    /// </summary>
    /// <param name="cnpj"> Número de CNPJ formatado com caracteres. </param>
    /// <returns></returns>
    public static string StripCnpj(this string cnpj)
    {
        return cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
    }

    /// <summary>
    /// Converte uma string para formato double, limpando a string antes da conversão.
    /// </summary>
    /// <param name="value"> Valor em formato texto. </param>
    /// <param name="decimalPlaces"> Quantidade de casas decimais a serem consideradas. Default: 4. </param>
    /// <returns> double value </returns>
    public static double ToDoubleSecure(this string value, int decimalPlaces = 4)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        Console.WriteLine($"Convertendo: {value}");

        // Valores sem milhar.
        if (value.Contains('.') && !value.Contains(','))
        {
            value = value.Trim();
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out double resultUs))
            {
                return Math.Round(resultUs, decimalPlaces);
            }
            else return 0.0;
        }

        if (double.TryParse(value, NumberStyles.Any, new CultureInfo("pt-BR"), out double result))
        {
            return Math.Round(result, decimalPlaces);
        }
        else if (double.TryParse(value, NumberStyles.Any, new CultureInfo("en-US"), out double resultUs))
        {
            return Math.Round(resultUs, decimalPlaces);
        }
        else
        {
            return 0.0;
        }
    }

    /// <summary>
    /// Converte uma string para formato double, limpando a string antes da conversão.
    /// </summary>
    /// <param name="value"> Valor em formato texto. </param>
    /// <param name="decimalPlaces"> Quantidade de casas decimais a serem consideradas. Default: 4. </param>
    /// <returns> double value </returns>
    public static double FromExcelToDouble(this string value, int decimalPlaces = 4)
    {
        if (string.IsNullOrEmpty(value)) return 0;

        var sanitized = value.Trim().Replace(".", "").Replace(",", ".");

        if (double.TryParse(sanitized, NumberStyles.Number, CultureInfo.InvariantCulture, out double result))
        {
            return Math.Round(result, decimalPlaces);
        }
        else
        {
            return 0;
        }
    }

    public static string ToBrazilianDate(this string date)
    {
        return DateTime.TryParse(date, out DateTime result) ? result.ToString("dd/MM/yyyy") : "01/01/1900";
    }

    public static string ToBrazilianDate(this DateTime date)
    {
        string d = string.Empty;
        try
        {
            d = date.ToString("dd/MM/yyyy");
        }
        catch
        {
            d = "01/01/1900";
        }
        return d;
    }

    public static bool Between(this int num, int min, int max)
    {
        return num >= min && num <= max;
    }

    public static bool Between(this DateTime date, DateTime init, DateTime end)
    {
        return date >= init && date <= end;
    }

    public static double ProtectZero(this double num)
    {
        if (num == 0) return 1;
        else return num;
    }

    public static DateTime AddNetworkDays(this DateTime date, int days)
    {
        if (days == 0)
            return date;

        int direction = days > 0 ? 1 : -1;
        int remainingDays = Math.Abs(days);

        while (remainingDays > 0)
        {
            date = date.AddDays(direction);

            if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
            {
                remainingDays--;
            }
        }

        return date;
    }

    public static string DateToTimestamp(this string date)
    {
        var d = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.GetCultureInfo("pt-BR")).ToString("s");
        return d;
    }

    public static string ClearString(this string text)
    {
        return text.Replace("-", "").Replace("/", "").Replace("(", "").Replace(")", "").Replace(".", "").Trim();
    }

    public static string GetDisplayName(this Enum value)
    {
        return value
            .GetType()
            .GetMember(value.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()?
            .Name ?? value.ToString();
    }

    public static bool IsNullOrEmptyOrZero(this string value)
    {
        if (value is null) return true;
        if (value.Equals('0')) return true;
        if (value.Equals("")) return true;
        return false;
    }
}
