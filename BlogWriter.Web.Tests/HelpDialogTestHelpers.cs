using Bunit;
using Microsoft.JSInterop;

namespace BlogWriter.Web.Tests;

internal static class HelpDialogTestHelpers
{
    public static void AllowDialogOpen(BunitContext context) =>
        context.JSInterop.SetupVoid("blogWriterDialog.show", _ => true);

    public static void FailClipboardCopy(BunitContext context) =>
        context.JSInterop.SetupVoid("navigator.clipboard.writeText", _ => true)
            .SetException(new JSException("Clipboard permission denied."));
}
