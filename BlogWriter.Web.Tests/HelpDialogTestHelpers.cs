using Bunit;

namespace BlogWriter.Web.Tests;

internal static class HelpDialogTestHelpers
{
    public static void AllowDialogOpen(BunitContext context) =>
        context.JSInterop.SetupVoid("blogWriterDialog.show", _ => true);

}
