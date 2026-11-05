namespace ECMS
{
    internal static class Program
    {
        /// <summary>The application starts at the sign-in window.</summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}
