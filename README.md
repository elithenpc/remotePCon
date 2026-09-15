# remotePCon

Turn on / keep the PC reachable remotely without Wake-on-LAN or a smart plug.

## How it works

`Gmail Windows notification -> UserNotificationChangedTrigger -> exact title "wake" -> full-trust helper -> ES_SYSTEM_REQUIRED`

The app listens to Windows notifications rather than logging into Gmail. It only reacts when:

- the notification comes from an app whose Windows display name contains `Gmail`;
- the toast title is exactly `wake`, ignoring case and surrounding whitespace;
- the notification was created within the last 15 seconds.

The notification listener is an event-driven Windows background task. Windows requires the user to grant notification-listener access before notifications can be read.

The full-trust helper then stays alive and uses `SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED)`. It does **not** request `ES_DISPLAY_REQUIRED`, so the display is allowed to turn off while the system remains awake.

## Project layout

- `src/RemotePCon` - UWP app and notification background task.
- `src/RemotePCon.FullTrust` - persistent Win32 keep-awake helper.
- `src/RemotePCon.Package` - MSIX packaging project containing both executables.
- `RemotePCon.sln` - Visual Studio solution.

## Build

Install Visual Studio with the **Universal Windows Platform development** workload and the Windows 10/11 SDK. The packaging project is intended to be built as **x64**.

Open `RemotePCon.sln`, select `Release | x64`, then build `RemotePCon.Package`.

A command-line build can be done with the Visual Studio MSBuild executable:

```powershell
& "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" .\src\RemotePCon.Package\RemotePCon.Package.wapproj /p:Configuration=Release /p:Platform=x64
```

For Visual Studio editions/versions installed elsewhere, use the matching MSBuild path.

## Install for local testing

The project deliberately does not commit a signing certificate. Windows supports installing an unsigned development MSIX with `Add-AppxPackage -AllowUnsigned`; executable unsigned packages normally require an elevated PowerShell session.

After building, locate the generated `.msix` under the packaging project's `AppPackages` directory and run:

```powershell
Add-AppxPackage -Path "C:\path\to\RemotePCon.msix" -AllowUnsigned
```

Launch **Remote PC On** once. On first launch Windows will ask for notification-listener permission. Allow it.

## Test

Use the **Test keep-awake** button in the app. The helper should remain running until **Release keep-awake** is pressed.

Then send a Windows Gmail notification whose title is exactly:

```text
wake
```

The body of the Gmail notification is ignored.

## Important power behaviour

`SetThreadExecutionState` cannot override a deliberate user sleep action, such as closing the lid or pressing the power button. The helper only prevents idle sleep while it is running. This is intentional Windows power-management behaviour.
