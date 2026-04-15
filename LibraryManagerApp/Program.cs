namespace LibraryManagerApp;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ShowFatalError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            ShowFatalError(e.ExceptionObject as Exception ?? new Exception("Unknown fatal error."));

        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            ShowFatalError(ex);
        }
    }

    private static void ShowFatalError(Exception ex)
    {
        MessageBox.Show(
            $"Erreur critique: {ex.Message}\n\nVerifie la configuration de l'application (appsettings.json).",
            "Erreur",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}