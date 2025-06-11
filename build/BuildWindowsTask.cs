
using Microsoft.VisualStudio.Setup.Configuration;
using System.Runtime.InteropServices;

namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        string cmake = "cmake";
        string msbuild = "msbuild";

        // If processes are not on PATH, we want to retrieve the Visual Studio installation
        if (!IsOnPATH(cmake))
            cmake = System.IO.Path.Combine(GetVisualStudioPath(), "Common7\\IDE\\CommonExtensions\\Microsoft\\CMake\\CMake\\bin\\cmake.exe");
        if (!IsOnPATH(msbuild))
            msbuild = System.IO.Path.Combine(GetVisualStudioPath(), "MSBuild\\Current\\Bin\\MSBuild.exe");

        // Build
        var buildDir = "sdl/build_x64";
        context.CreateDirectory(buildDir);
        context.CreateDirectory($"{context.ArtifactsDir}/win-x64");
        context.StartProcess(cmake, new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
        context.StartProcess(msbuild, new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build_x64/Release/SDL2.dll", $"{context.ArtifactsDir}/win-x64/SDL2.dll");

        buildDir = "sdl/build_arm64";
        context.CreateDirectory(buildDir);
        context.CreateDirectory($"{context.ArtifactsDir}/win-arm64");
        context.StartProcess(cmake, new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A ARM64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
        context.StartProcess(msbuild, new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release" });

        // Copy artifact
        context.CreateDirectory(context.ArtifactsDir);
        context.CopyFile("sdl/build_arm64/Release/SDL2.dll", $"{context.ArtifactsDir}/win-arm64/SDL2.dll");
    }

    private bool IsOnPATH(string process)
    {
        if (string.IsNullOrEmpty(process))
            return false;

        if (!process.EndsWith(".exe"))
            process += ".exe";

        // check if process exist on PATH env

        var split = Environment.GetEnvironmentVariable("PATH")?.Split(';');
        if (split != null)
        {
            foreach (var path in split)
            {
                string processPath = System.IO.Path.Combine(path, process);
                if (File.Exists(processPath))
                    return true;
            }
        }

        return false;
    }

    private string GetVisualStudioPath()
    {
        // This code to retrieve where Visual Studio is installed is adapted from
        // https://github.com/microsoft/MSBuildLocator
        // Surprisingly, this package has code to retrieve a VS installation but
        // this feature is disabled from the public nuget... so adapting it from source

        // This will only detect Visual Studio 2015 and above

        string path = string.Empty;
        Version version = new Version();

        try
        {
            // This code is not obvious. See the sample (link above) for reference.
            var query = (ISetupConfiguration2)GetQuery();
            var e = query.EnumAllInstances();

            int fetched;
            var instances = new ISetupInstance[1];
            do
            {
                // Call e.Next to query for the next instance (single item or nothing returned).
                e.Next(1, instances, out fetched);
                if (fetched <= 0) continue;

                var instance = (ISetupInstance2)instances[0];
                InstanceState state = instance.GetState();

                // If the install was complete
                if (state == InstanceState.Complete ||
                    (state.HasFlag(InstanceState.Registered) && state.HasFlag(InstanceState.NoRebootRequired)))
                {
                    if (!Version.TryParse(instance.GetInstallationVersion(), out var current))
                        continue;

                    // We want the highest version installed
                    if (current <= version)
                        continue;

                    version = current;
                    path = instance.GetInstallationPath();
                }
            }
            while (fetched > 0);
        }
        catch (COMException)
        {
        }
        catch (DllNotFoundException)
        {
            // This is OK, VS "15" or greater likely not installed.
        }

        return path;
    }

    private static ISetupConfiguration GetQuery()
    {
        const int REGDB_E_CLASSNOTREG = unchecked((int)0x80040154);

        try
        {
            // Try to CoCreate the class object.
            return new SetupConfiguration();
        }

        catch (COMException ex) when (ex.ErrorCode == REGDB_E_CLASSNOTREG)
        {
            // Try to get the class object using app-local call.
            ISetupConfiguration query;
            var result = GetSetupConfiguration(out query, IntPtr.Zero);

            if (result < 0)
                throw new COMException($"Failed to get {nameof(query)}", result);

            return query;
        }
    }

    [DllImport("Microsoft.VisualStudio.Setup.Configuration.Native.dll", ExactSpelling = true, PreserveSig = true)]
    private static extern int GetSetupConfiguration(
        [MarshalAs(UnmanagedType.Interface)][Out] out ISetupConfiguration configuration,
        IntPtr reserved);
}
