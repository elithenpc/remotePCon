using System.Runtime.InteropServices;

namespace RemotePCon.FullTrust;

internal static class Program
{
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_CONTINUOUS = 0x80000000;

    private const string ReleaseEventName = "Local\\RemotePCon.Release";
    private const string MutexName = "Local\\RemotePCon.KeepAwake";

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private static int Main(string[] args)
    {
        var command = args.FirstOrDefault()?.Trim().ToLowerInvariant() ?? string.Empty;

        if (command == "/release")
        {
            try
            {
                using var release = EventWaitHandle.OpenExisting(ReleaseEventName);
                release.Set();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // Nothing is keeping the PC awake.
            }
            return 0;
        }

        if (command != "/wake")
            return 0;

        using var mutex = new Mutex(true, MutexName, out var ownsMutex);
        if (!ownsMutex)
            return 0;

        using var releaseEvent = new EventWaitHandle(false, EventResetMode.ManualReset, ReleaseEventName);

        // ES_CONTINUOUS + ES_SYSTEM_REQUIRED keeps the system working while this
        // process remains alive. It deliberately does not include ES_DISPLAY_REQUIRED.
        if (SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED) == 0)
            return 2;

        try
        {
            releaseEvent.WaitOne();
        }
        finally
        {
            // Clear the execution requirement on the same thread that set it.
            SetThreadExecutionState(ES_CONTINUOUS);
        }

        return 0;
    }
}
