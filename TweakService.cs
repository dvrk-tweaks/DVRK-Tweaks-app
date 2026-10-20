using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace DvrkTweaks.Services;

public sealed class TweakService
{
    public bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        return new WindowsPrincipal(identity)
            .IsInRole(WindowsBuiltInRole.Administrator);
    }

    // ============================================================
    // MAIN APPLY METHOD
    // ============================================================

    public void Apply(string key, bool enabled)
    {
        try
        {
            switch (key)
            {
                // ------------------------------------------------
                // WINDOWS / GAMING
                // ------------------------------------------------

                case "HAGS":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                        "HwSchMode",
                        enabled ? 2 : 1);
                    break;

                case "GameMode":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\GameBar",
                        "AllowAutoGameMode",
                        enabled ? 1 : 0);
                    break;

                case "GameDVR":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"System\GameConfigStore",
                        "GameDVR_Enabled",
                        enabled ? 0 : 1);

                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
                        "AppCaptureEnabled",
                        enabled ? 0 : 1);
                    break;

                case "GameBarCapture":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
                        "HistoricalCaptureEnabled",
                        enabled ? 0 : 1);
                    break;

                case "BackgroundRecordingAction":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\GameDVR",
                        "HistoricalCaptureEnabled",
                        enabled ? 0 : 1);
                    break;

                case "GPUOverlayAction":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\GameBar",
                        "ShowStartupPanel",
                        enabled ? 0 : 1);
                    break;

                // ------------------------------------------------
                // POWER
                // ------------------------------------------------

                case "HighPerf":
                    PowerCommand("-setactive SCHEME_MIN");
                    break;

                case "UltimatePerf":
                    Run(
                        "cmd.exe",
                        "/c powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 >nul 2>&1");

                    PowerCommand(
                        "-setactive e9a42b02-d5df-448d-aa00-03f14749eb61");
                    break;

                case "PowerProfile":
                    PowerCommand("-setactive SCHEME_MIN");
                    break;

                case "CPUProfile":
                    PowerCommand("-setactive SCHEME_MIN");
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100");
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX 100");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "CpuMin100":
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "CpuMax100":
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX 100");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "CoreParking":
                    PowerCommand(
                        $"/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR CPMINCORES {(enabled ? 100 : 10)}");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "ProcessorIdle":
                    PowerCommand(
                        $"/setacvalueindex SCHEME_CURRENT SUB_PROCESSOR IDLEDISABLE {(enabled ? 1 : 0)}");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "USBSuspendPlan":
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT 2a737441-1930-4402-8d77-b2bebba308a3 48e6b7a6-50f5-4782-a5d4-53bb8f07e226 0");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "ASPM":
                case "PCIePower":
                    PowerCommand(
                        "/setacvalueindex SCHEME_CURRENT SUB_PCIEXPRESS ASPM 0");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "DiskIdle":
                case "DiskTimeout":
                    PowerCommand(
                        $"/setacvalueindex SCHEME_CURRENT SUB_DISK DISKIDLE {(enabled ? 0 : 20)}");
                    PowerCommand("/setactive SCHEME_CURRENT");
                    break;

                case "DisplayTimeout":
                    PowerCommand("-change monitor-timeout-ac 0");
                    break;

                case "SleepTimeout":
                    PowerCommand("-change standby-timeout-ac 0");
                    break;

                case "USBPower":
                    SetAdapterFeature("*Selective Suspend*", enabled ? "Disabled" : "Enabled");
                    break;

                // ------------------------------------------------
                // CPU / WINDOWS SCHEDULER
                // ------------------------------------------------

                case "PowerThrottling":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling",
                        "PowerThrottlingOff",
                        enabled ? 1 : 0);
                    break;

                case "SystemResponsiveness":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                        "SystemResponsiveness",
                        enabled ? 0 : 20);
                    break;

                case "NetworkThrottle":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                        "NetworkThrottlingIndex",
                        enabled ? unchecked((int)0xFFFFFFFF) : 10);
                    break;

                case "ForegroundPriority":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile",
                        "AlwaysOn",
                        enabled ? 1 : 0);
                    break;

                case "GamePriority":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                        "GPU Priority",
                        8);

                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games",
                        "Priority",
                        enabled ? 6 : 2);
                    break;

                case "GameScheduler":
                    Apply("GamePriority", enabled);
                    break;

                // ------------------------------------------------
                // WINDOWS VISUALS
                // ------------------------------------------------

                case "Transparency":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                        "EnableTransparency",
                        enabled ? 0 : 1);
                    break;

                case "Animations":
                case "MinAnimations":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Desktop\WindowMetrics",
                        "MinAnimate",
                        enabled ? "0" : "1");
                    break;

                case "MenuAnimations":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Desktop",
                        "MenuShowDelay",
                        enabled ? "0" : "400");
                    break;

                case "TaskbarAnimations":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                        "TaskbarAnimations",
                        enabled ? 0 : 1);
                    break;

                case "VisualEffects":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects",
                        "VisualFXSetting",
                        enabled ? 2 : 1);
                    break;

                // ------------------------------------------------
                // MOUSE / INPUT
                // ------------------------------------------------

                case "MouseAccel":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Mouse",
                        "MouseSpeed",
                        enabled ? "0" : "1");
                    break;

                case "MouseThresholds":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Mouse",
                        "MouseThreshold1",
                        enabled ? "0" : "6");

                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Mouse",
                        "MouseThreshold2",
                        enabled ? "0" : "10");
                    break;

                case "MouseTrails":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Mouse",
                        "MouseTrails",
                        enabled ? "0" : "1");
                    break;

                case "KeyboardRepeat":
                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Keyboard",
                        "KeyboardDelay",
                        enabled ? "0" : "1");

                    SetString(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Keyboard",
                        "KeyboardSpeed",
                        "31");
                    break;

                case "FilterKeys":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Accessibility\Keyboard Response",
                        "Flags",
                        enabled ? 0 : 126);
                    break;

                case "StickyKeys":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Accessibility\StickyKeys",
                        "Flags",
                        enabled ? 0 : 510);
                    break;

                case "ToggleKeys":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Accessibility\ToggleKeys",
                        "Flags",
                        enabled ? 0 : 62);
                    break;

                case "AccessHotkeys":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Control Panel\Accessibility\Keyboard Response",
                        "Flags",
                        enabled ? 0 : 126);
                    break;

                case "InputProfile":
                    Apply("MouseAccel", enabled);
                    Apply("MouseThresholds", enabled);
                    Apply("USBSelectiveSuspend", enabled);
                    break;

                // ------------------------------------------------
                // NETWORK
                // ------------------------------------------------

                case "TcpAutotune":
                    Run(
                        "netsh.exe",
                        "interface tcp set global autotuninglevel=normal");
                    break;

                case "TcpTimestamps":
                    Run(
                        "netsh.exe",
                        $"interface tcp set global timestamps={(enabled ? "disabled" : "enabled")}");
                    break;

                case "TcpEcn":
                    Run(
                        "netsh.exe",
                        $"interface tcp set global ecncapability={(enabled ? "disabled" : "default")}");
                    break;

                case "RSS":
                    Run(
                        "netsh.exe",
                        "interface tcp set global rss=enabled");
                    break;

                case "RSSQueues":
                    RunPowerShell(
                        "Get-NetAdapter | ForEach-Object { " +
                        "try { Enable-NetAdapterRss -Name $_.Name -NoRestart " +
                        "-ErrorAction SilentlyContinue } catch {} }");
                    break;

                case "LSO":
                    SetAdapterFeature("*LSO*", enabled ? "Enabled" : "Disabled");
                    break;

                case "RSC":
                    if (enabled)
                    {
                        RunPowerShell(
                            "Get-NetAdapterRsc | Enable-NetAdapterRsc " +
                            "-ErrorAction SilentlyContinue");
                    }
                    else
                    {
                        RunPowerShell(
                            "Get-NetAdapterRsc | Disable-NetAdapterRsc " +
                            "-ErrorAction SilentlyContinue");
                    }

                    break;

                case "InterruptModeration":
                    SetAdapterFeature(
                        "*InterruptModeration*",
                        enabled ? "Enabled" : "Disabled");
                    break;

                case "EEE":
                    SetAdapterFeature(
                        "*EEE*",
                        enabled ? "Enabled" : "Disabled");
                    break;

                case "GreenEthernet":
                    SetAdapterFeature(
                        "*Green Ethernet*",
                        enabled ? "Enabled" : "Disabled");
                    break;

                case "PowerSavingEthernet":
                    SetAdapterFeature(
                        "*Power Saving*",
                        enabled ? "Enabled" : "Disabled");
                    break;

                case "DNSCloudflare":
                    SetDns("1.1.1.1", "1.0.0.1");
                    break;

                case "DNSGoogle":
                    SetDns("8.8.8.8", "8.8.4.4");
                    break;

                case "FlushDNS":
                    Run("ipconfig.exe", "/flushdns");
                    break;

                case "NetworkProfile":
                    Apply("TcpTimestamps", enabled);
                    Apply("TcpEcn", enabled);
                    Apply("RSS", true);
                    Apply("LSO", enabled);
                    Apply("RSC", enabled);
                    Apply("InterruptModeration", enabled);
                    break;

                case "LatencyProfile":
                    Apply("HighPerf", enabled);
                    Apply("PowerThrottling", enabled);
                    Apply("SystemResponsiveness", enabled);
                    Apply("NetworkThrottle", enabled);
                    Apply("TcpTimestamps", enabled);
                    break;

                // ------------------------------------------------
                // BACKGROUND SERVICES
                // ------------------------------------------------

                case "DeliveryOptimization":
                    SetService("DoSvc", enabled ? "stop" : "start");
                    break;

                case "SearchService":
                    SetService("WSearch", enabled ? "stop" : "start");
                    break;

                case "SysMain":
                    SetService("SysMain", enabled ? "stop" : "start");
                    break;

                case "DiagTrack":
                    SetService("DiagTrack", enabled ? "stop" : "start");
                    break;

                case "ConnectedUser":
                    SetService("dmwappushservice", enabled ? "stop" : "start");
                    break;

                case "WER":
                    SetService("WerSvc", enabled ? "stop" : "start");
                    break;

                case "RemoteRegistry":
                    SetService("RemoteRegistry", enabled ? "stop" : "start");
                    break;

                case "RetailDemo":
                    SetService("RetailDemo", enabled ? "stop" : "start");
                    break;

                case "Fax":
                    SetService("Fax", enabled ? "stop" : "start");
                    break;

                case "MapsService":
                case "MapsUpdate":
                    SetService("MapsBroker", enabled ? "stop" : "start");
                    break;

                case "AppExperience":
                    SetService("AeLookupSvc", enabled ? "stop" : "start");
                    break;

                case "CompatAssistant":
                    SetService("PcaSvc", enabled ? "stop" : "start");
                    break;

                case "XboxServices":
                    foreach (string service in new[]
                    {
                        "XblAuthManager",
                        "XblGameSave",
                        "XboxNetApiSvc",
                        "XboxGipSvc"
                    })
                    {
                        SetService(service, enabled ? "stop" : "start");
                    }

                    break;

                case "Bluetooth":
                    SetService("bthserv", enabled ? "stop" : "start");
                    break;

                case "PrintSpooler":
                    SetService("Spooler", enabled ? "stop" : "start");
                    break;

                case "OneSync":
                    SetService("OneSyncSvc", enabled ? "stop" : "start");
                    break;

                case "Maintenance":
                    SetService("Schedule", enabled ? "stop" : "start");
                    break;

                case "BackgroundApps":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications",
                        "GlobalUserDisabled",
                        enabled ? 1 : 0);
                    break;

                case "BackgroundProfile":
                    Apply("DeliveryOptimization", enabled);
                    Apply("DiagTrack", enabled);
                    Apply("WER", enabled);
                    Apply("XboxServices", enabled);
                    break;

                // ------------------------------------------------
                // PRIVACY
                // ------------------------------------------------

                case "ClipboardCloud":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Clipboard",
                        "EnableCloudClipboard",
                        enabled ? 0 : 1);
                    break;

                case "ActivityHistory":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Policies\Microsoft\Windows\System",
                        "PublishUserActivities",
                        enabled ? 0 : 1);
                    break;

                case "AdvertisingID":
                    SetDword(
                        RegistryHive.LocalMachine,
                        @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo",
                        "DisabledByGroupPolicy",
                        enabled ? 1 : 0);
                    break;

                case "Feedback":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Siuf\Rules",
                        "NumberOfSIUFInPeriod",
                        enabled ? 0 : 1);
                    break;

                case "PrivacyProfile":
                    Apply("DiagTrack", enabled);
                    Apply("WER", enabled);
                    Apply("RemoteRegistry", enabled);
                    Apply("DeliveryOptimization", enabled);
                    break;

                // ------------------------------------------------
                // CLEANUP
                // ------------------------------------------------

                case "TempCleanup":
                    RunPowerShell(
                        "Remove-Item \"$env:TEMP\\*\" -Recurse -Force " +
                        "-ErrorAction SilentlyContinue");
                    break;

                case "DXCache":
                    Run(
                        "cmd.exe",
                        "/c del /q /f \"%LOCALAPPDATA%\\D3DSCache\\*\" 2>nul");
                    break;

                case "StartupCleanup":
                    SetDword(
                        RegistryHive.CurrentUser,
                        @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize",
                        "StartupDelayInMSec",
                        0);
                    break;

                // ------------------------------------------------
                // FORTNITE
                // ------------------------------------------------

                case "FortnitePerformance":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["bUseVSync"] = "False"
                    });
                    break;

                case "FortniteReplays":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["bEnableReplayRecording"] = "False"
                    });
                    break;

                case "FortniteEffects":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.EffectsQuality"] = "0"
                    });
                    break;

                case "FortniteShadows":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.ShadowQuality"] = "0"
                    });
                    break;

                case "FortniteViewDistance":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.ViewDistanceQuality"] = "0"
                    });
                    break;

                case "FortniteAA":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.AntiAliasingQuality"] = "0"
                    });
                    break;

                case "FortniteMotionBlur":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["bMotionBlur"] = "False"
                    });
                    break;

                case "FortniteVsync":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["bUseVSync"] = "False"
                    });
                    break;

                case "FortniteMultiThread":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["bAllowMultiThreadedRendering"] = "True"
                    });
                    break;

                case "FortniteShaders":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.ShadingQuality"] = "0"
                    });
                    break;

                case "FortniteCompetitive":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.EffectsQuality"] = "0",
                        ["sg.ShadowQuality"] = "0",
                        ["sg.PostProcessQuality"] = "0",
                        ["sg.FoliageQuality"] = "0",
                        ["sg.ShadingQuality"] = "0",
                        ["bUseVSync"] = "False",
                        ["bMotionBlur"] = "False"
                    });
                    break;

                case "FortniteInput":
                    Apply("MouseAccel", enabled);
                    Apply("MouseThresholds", enabled);
                    break;

                case "FortniteFPSStability":
                    FortniteSet(enabled, new Dictionary<string, string>
                    {
                        ["sg.EffectsQuality"] = "0",
                        ["sg.ShadowQuality"] = "0",
                        ["sg.PostProcessQuality"] = "0",
                        ["sg.FoliageQuality"] = "0",
                        ["sg.ViewDistanceQuality"] = "0"
                    });
                    break;

                default:
                    // Unknown tweak keys are safely ignored.
                    break;
            }
        }
        catch
        {
            // A failed optional tweak should not crash the application.
        }
    }

    // ============================================================
    // REGISTRY
    // ============================================================

    private static void SetDword(
        RegistryHive hive,
        string subKey,
        string name,
        int value)
    {
        using RegistryKey baseKey =
            RegistryKey.OpenBaseKey(hive, RegistryView.Default);

        using RegistryKey? key =
            baseKey.CreateSubKey(subKey);

        key?.SetValue(
            name,
            value,
            RegistryValueKind.DWord);
    }

    private static void SetString(
        RegistryHive hive,
        string subKey,
        string name,
        string value)
    {
        using RegistryKey baseKey =
            RegistryKey.OpenBaseKey(hive, RegistryView.Default);

        using RegistryKey? key =
            baseKey.CreateSubKey(subKey);

        key?.SetValue(
            name,
            value,
            RegistryValueKind.String);
    }

    // ============================================================
    // PROCESS / COMMANDS
    // ============================================================

    private static void Run(
        string executable,
        string arguments)
    {
        try
        {
            using Process? process =
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = executable,
                        Arguments = arguments,
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });

            process?.WaitForExit();
        }
        catch
        {
            // Ignore optional command failures.
        }
    }

    private static void RunPowerShell(string command)
    {
        Run(
            "powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"");
    }

    private static void PowerCommand(string arguments)
    {
        Run("powercfg.exe", arguments);
    }

    // ============================================================
    // SERVICES
    // ============================================================

    private static void SetService(
        string serviceName,
        string action)
    {
        Run(
            "sc.exe",
            $"{action} \"{serviceName}\"");
    }

    // ============================================================
    // NETWORK ADAPTER
    // ============================================================

    private static void SetAdapterFeature(
        string featurePattern,
        string state)
    {
        string command =
            "Get-NetAdapterAdvancedProperty -Name '*' " +
            "-ErrorAction SilentlyContinue | " +
            $"Where-Object {{$_.DisplayName -like '{featurePattern}'}} | " +
            "ForEach-Object { " +
            $"Set-NetAdapterAdvancedProperty -Name $_.Name " +
            $"-DisplayName $_.DisplayName -DisplayValue '{state}' " +
            "-NoRestart -ErrorAction SilentlyContinue }";

        RunPowerShell(command);
    }

    private static void SetDns(
        string primary,
        string secondary)
    {
        string command =
            "Get-NetAdapter | " +
            "Where-Object {$_.Status -eq 'Up'} | " +
            "ForEach-Object { " +
            $"Set-DnsClientServerAddress -InterfaceIndex $_.ifIndex " +
            $"-ServerAddresses @('{primary}','{secondary}') " +
            "-ErrorAction SilentlyContinue }";

        RunPowerShell(command);
    }

    // ============================================================
    // FORTNITE CONFIG
    // ============================================================

    private static void FortniteSet(
        bool enabled,
        Dictionary<string, string> values)
    {
        string? path = GetFortniteIni();

        if (path == null)
            return;

        string backup = path + ".dvrk.bak";

        try
        {
            // Create backup the first time the tweak is enabled.
            if (enabled && !File.Exists(backup))
            {
                File.Copy(path, backup, true);
            }

            // Disable = restore original configuration.
            if (!enabled)
            {
                if (File.Exists(backup))
                {
                    File.Copy(backup, path, true);
                    File.Delete(backup);
                }

                return;
            }

            List<string> lines =
                new List<string>(File.ReadAllLines(path));

            foreach (KeyValuePair<string, string> pair in values)
            {
                int index = FindIniValue(lines, pair.Key);

                string newLine =
                    pair.Key + "=" + pair.Value;

                if (index >= 0)
                    lines[index] = newLine;
                else
                    lines.Add(newLine);
            }

            File.WriteAllLines(path, lines);
        }
        catch
        {
            // Fortnite config may be unavailable or read-only.
        }
    }

    private static int FindIniValue(
        List<string> lines,
        string key)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i].Trim();

            if (line.StartsWith(
                    key + "=",
                    StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private static string? GetFortniteIni()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "FortniteGame",
            "Saved",
            "Config",
            "WindowsClient",
            "GameUserSettings.ini");

        return File.Exists(path)
            ? path
            : null;
    }
}