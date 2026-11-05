namespace ECMS;

/// <summary>Arguments for opening the checkout page.</summary>
public record CheckoutRequest(int CourseId, int? BatchId);

/// <summary>
/// A tiny bridge between pages and the main window.
/// Pages call <see cref="Go"/> to change screen and <see cref="Toast"/> to show a message.
/// </summary>
public static class Navigator
{
    public static Action<string, object?>? Handler;
    public static Action<string, bool>? ToastHandler;

    public static void Go(string key, object? argument = null) => Handler?.Invoke(key, argument);

    public static void Toast(string message, bool success = true)
        => ToastHandler?.Invoke(message, success);
}
