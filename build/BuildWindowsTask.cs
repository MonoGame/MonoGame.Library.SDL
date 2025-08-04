using Cake.Common.Tools.VSWhere.Latest;

namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        var vswhere = new VSWhereLatest(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools);

        string cmake = "cmake";
        string msbuild = "msbuild";

        // If processes are not on PATH, we want to retrieve the Visual Studio installation
        if (!IsOnPATH(cmake))
            cmake = vswhere.Latest(new VSWhereLatestSettings()).FullPath + @"\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe";
        if (!IsOnPATH(msbuild))
            msbuild = vswhere.Latest(new VSWhereLatestSettings()).FullPath + @"\MSBuild\Current\Bin\MSBuild.exe";

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

        // Check if process exist on PATH env

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
}
