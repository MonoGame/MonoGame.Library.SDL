
namespace BuildScripts;

[TaskName("Build Windows")]
[IsDependentOn(typeof(PrepTask))]
[IsDependeeOf(typeof(BuildLibraryTask))]
public sealed class BuildWindowsTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        // Prepare artifacts
        context.CreateDirectory($"{context.ArtifactsDir}/win-x64");
        context.CreateDirectory($"{context.ArtifactsDir}/win-arm64");

        // Build sdl2-compat x64
        var buildDir = "sdl2-compat/cmake-build/x64";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../../ -A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded -DSDL3_INCLUDE_DIRS=../../../sdl3/include" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "sdl2_compat.sln /p:Configuration=Release" });
        context.CopyFile($"{buildDir}/Release/SDL2.dll", $"{context.ArtifactsDir}/win-x64/SDL2.dll");

        // Build sdl3 x64
        buildDir = "sdl3/build-x64";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL3.sln /p:Configuration=Release" });
        context.CopyFile($"{buildDir}/Release/SDL3.dll", $"{context.ArtifactsDir}/win-x64/SDL3.dll");

        // Build sdl2-compat ARM64
        buildDir = "sdl2-compat/cmake-build/arm64";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../../ -A ARM64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded -DSDL3_INCLUDE_DIRS=../../../sdl3/include" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "sdl2_compat.sln /p:Configuration=Release" });
        context.CopyFile($"{buildDir}/Release/SDL2.dll", $"{context.ArtifactsDir}/win-arm64/SDL2.dll");

        // Build sdl3 ARM64
        buildDir = "sdl3/build-arm64";
        context.CreateDirectory(buildDir);
        context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-S ../ -A ARM64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded" });
        context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL3.sln /p:Configuration=Release" });
        context.CopyFile($"{buildDir}/Release/SDL3.dll", $"{context.ArtifactsDir}/win-arm64/SDL3.dll");
    }
}
