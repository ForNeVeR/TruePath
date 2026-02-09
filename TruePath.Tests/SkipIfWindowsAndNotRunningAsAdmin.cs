using System.Security.Principal;

namespace TruePath.Tests;

internal class SkipIfWindowsAndNotRunningAsAdmin : FactAttribute
{
    public SkipIfWindowsAndNotRunningAsAdmin()
    {
        if (IsWindowsAndNotRunningAsAdmin())
        {
            Skip = "Skipped due to elevated rights requirement on Windows";
        }
    }

    static bool IsWindowsAndNotRunningAsAdmin()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            bool runAsAdmin = !principal.IsInRole(WindowsBuiltInRole.Administrator);
            return runAsAdmin;
        }
        return true;
    }
}
