namespace ECMS;

/// <summary>Small formatting helpers used by every page.</summary>
public static class Fmt
{
    public static string Money(decimal amount) => "EGP " + amount.ToString("N0");
    public static string Date(DateTime date) => date.ToString("dd MMM yyyy");
    public static string ShortDate(DateTime date) => date.ToString("dd MMM");
    public static string Time(DateTime date) => date.ToString("HH:mm");
    public static string Percent(double value) => $"{value:0}%";
    public static string Percent(decimal value) => $"{value:0}%";
    public static string Minutes(int minutes) => minutes + " min";
}
