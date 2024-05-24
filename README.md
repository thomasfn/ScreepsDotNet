# Screeps DotNet

A toolset and API to build bots for [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) and [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) using .Net 8.0.

* [What is Screeps DotNet?](#what-is-screeps-dotnet)
* [Quickstart](#quickstart)
* [Migration from .Net 7](#migration)
* [API](#api)
* [Native](#native)
* [Project structure](#project-structure)
* [Limitations, Issues and Implications](#limitations-issues-and-implications)
* [Troubleshooting](#troubleshooting)
* [License](#license)

## What is Screeps DotNet?

Screeps DotNet allows you to write bots for Screeps in any language that targets .Net 7.0, for example C#, and provides tooling to compile your bot to wasm ready to be deployed to the Screeps environment.

- [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) - :heavy_check_mark: full support
- [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) - :heavy_check_mark: full support

A managed API is provided that handles the interop with the Screeps javascript API, meaning you only need to write code against a set of generic interfaces. For some examples, please see the [example Arena project](ScreepsDotNet.ExampleArenaBot/) which contains example solutions for all 10 tutorials of Screeps Arena, or the [example World project](ScreepsDotNet.ExampleWorldBot/) which contains a barebones Screeps World bot.

## Quickstart

To get started making your first bot for Screeps in C#, follow these steps. You'll need a working dotnet environment as we're using terminal commands here. If you're using Visual Studio, you can use the Package Manager Console to run them.

### Workload
Install the experimental wasi workload if you haven't done already.
```
dotnet workload install wasi-experimental
```

### Setup project
Create a new wasm project.
```
dotnet new wasiconsole
```

Edit the csproj to contain the following property groups:

```XML
<PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>wasi-wasm</RuntimeIdentifier>
    <OutputType>Exe</OutputType>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>enable</Nullable>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
    <TrimmerRemoveSymbols>true</TrimmerRemoveSymbols>
    <InvariantGlobalization>true</InvariantGlobalization>
    <WasmSingleFileBundle>true</WasmSingleFileBundle>
    <EventSourceSupport>false</EventSourceSupport>
    <UseSystemResourceKeys>true</UseSystemResourceKeys>
    <InvariantTimezone>true</InvariantTimezone>
    <WasiSdkVersion>15</WasiSdkVersion>
</PropertyGroup>

<PropertyGroup>
    <ScreepsCompressWasm>false</ScreepsCompressWasm>
    <ScreepsEncoding>b64</ScreepsEncoding>
</PropertyGroup>
```

Note that the trimming, compression and encoding settings here have [implications](#limitations-and-issues), you may need to do some research into these and play around to get settings that work for you.

Add nuget references to the following packages:
```
dotnet add (MyProjectName) package ScreepsDotNet.API
dotnet add (MyProjectName) package ScreepsDotNet.Bundler
```

### Entrypoint (Arena)

Replace your `Program.cs` with the following code:
```CS
using System;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static IGame? game;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
                game = new Native.Arena.NativeGame();
                // TODO: Add startup logic here!
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Loop()
        {
            if (game == null) { return; }
            try
            {
                game.Tick();
                // TODO: Add loop logic here!
                Console.WriteLine($"Hello world from C#, the current tick is {game.Utils.GetTicks()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
```

Notice the use of the `DynamicDependency` and the `SupportedOSPlatform` attributes on the entrypoint methods.
The `DynamicDependency` attribute informs the IL trimmer that the `Init` and `Loop` methods are used and should not be removed.
The `SupportedOSPlatform` attribute doesn't explicitly do something but will cause a warning if you accidentally try and call the method outside of wasm, for example in a unit test.

Do not change the namespace of the entrypoint as the native calls used to look it up cannot be configured to use a different namespace. You can use any namespace you like for code other than the entrypoint.

### Entrypoint (World)

Replace your `Program.cs` with the following code:
```CS
using System;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static IGame? game;

        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Program))]
        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Init()
        {
            try
            {
                game = new Native.World.NativeGame();
                // TODO: Add startup logic here!
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [System.Runtime.Versioning.SupportedOSPlatform("wasi")]
        public static void Loop()
        {
            if (game == null) { return; }
            try
            {
                game.Tick();
                // TODO: Add loop logic here!
                Console.WriteLine($"Hello world from C#, the current tick is {game.Time}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
```

Notice the use of the `DynamicDependency` and the `SupportedOSPlatform` attributes on the entrypoint methods.
The `DynamicDependency` attribute informs the IL trimmer that the `Init` and `Loop` methods are used and should not be removed.
The `SupportedOSPlatform` attribute doesn't explicitly do something but will cause a warning if you accidentally try and call the method outside of wasm, for example in a unit test.

Do not change the namespace of the entrypoint as the native calls used to look it up cannot be configured to use a different namespace. You can use any namespace you like for code other than the entrypoint.

### Building

Build the project in publish mode.
```
dotnet publish -c Debug
-- or --
dotnet publish -c Release
```
A standard build will not suffice as it does not optimise the assembly size and the generated bundle will be way too big. Debug builds normally create larger bundle sizes than release builds but should still fall within the 5mb script size limit. Release builds do take quite a bit longer to build though, so Debug builds are recommended for quick iteration.

The first ever build using ScreepsDotNet 2.x will take much longer as it has to download the binaryen sdk in order to make use of wasm-opt. This is a one-off cost per version of the bundler tool.

### Build artifacts

The build artifacts can be found at `MyProjectName/bin/(Debug|Release)/net8.0/wasi-wasm/AppBundle/(arena|world)`. All will need to be copied to your Screeps environment to work properly, however generally only the bundle file will change between builds.

#### Arena
- `bootloader.d.ts` - type definitions for the bootloader. Not strictly needed, but helpful when making modifications to `main.mjs`. Does not change between builds.
- `bootloader.mjs` - js for initialising and running the dotnet runtime. Does not change between builds.
- `bundle.mjs` - contains compressed and encoded dotnet wasm and assemblies. Changes every build, as it contains your code.
- `main.mjs` - entrypoint for the bot. You can customise this if you want to, for example, add custom js functions that you want to call from C#.

#### World
- `bootloader.js` - js for initialising and running the dotnet runtime. Does not change between builds but is different between Debug and Release builds.
- `ScreepsDotNet.wasm` - contains dotnet wasm and assemblies. Changes every build, as it contains your code.
- `main.js` - entrypoint for the bot. You can customise this if you want to, for example, add custom js functions that you want to call from C#.

If all has gone well, you should now have a working basic bot that runs successfully in your Screeps environment. Note that the simulator is not supported - if you have issues with the simulator, try deploying your code to either the mmo or an up-to-date private server.

### Next steps

You can architect your bot however you like. Generally it is recommended to keep the `Program.cs` as a slim bootstrap entrypoint and have it instantiate another class which will run the bot. You could use inversion of control to pass the instance of `IGame` to your code to keep it nice and separated from the specifics of the JS interop (which also makes it much easier to mock during unit testing), but the choice really is yours. Let your imagination run wild!

## Migration

If you have an existing project on .Net 7 using ScreepsDotNet 1.x, migrating to .Net 8 using ScreepsDotNet 2.x should be fairly easy. The following migration checklist should cover all needed changes for migration.

- Check that you have the workload `wasi-experimental` installed, as per the quickstart guide (ScreepsDotNet 1.x used a different workload)
- Change the project SDK in the `csproj` file to `<Project Sdk="Microsoft.NET.Sdk">`, the `TargetFramework` property to `net8.0` and the `RuntimeIdentifier` property to `wasi-wasm`
- Check that the other properties in `csproj` to match those listed in the quickstart guide, there may be some old ones to be removed or new ones to be added
- Change the nuget references for `ScreepsDotNet.Bundler` and `ScreepsDotNet.API` to target `2.0.0` (you might need to restart VS after the new dependencies are restored as it likes to cache the bundler build task)
- Change your entrypoint to match that of the quickstart guide - especially the attributes, the access modifiers (`internal` to `public`) and the namespace (must be `ScreepsDotNet`)
- For Screeps Arena, add a call to `game.Tick()` in your program's `Loop` function before running any of your own loop logic.

The API also contains some minor breaking changes but they should only need to be addressed if they create compiler errors (for example, some cases of properties changing from `int` to `int?` to properly represent when the JS API may return null). Also of note is a change to the meaning of the `X` and `Y` properties in `RoomCoord`, so take care if you've serialised these properties to memory.

## API

The Screeps .Net API has been designed to be as close to the JS API as possible, with only minor alterations to adopt standard C# idioms. If you're familiar with the JS API, you will automatically be familiar with the .Net API. 

The API is exposed as a set of interfaces and a few support structures and is contained within the `ScreepsDotNet.API` namespace. Any part of the API common to both Arena and World lives directly in this namespace. Anything more specific lives in either `ScreepsDotNet.API.Arena` or `ScreepsDotNet.API.World`.

There is currently only one exposed concrete implementation. For Arena this is `ScreepsDotNet.Native.Arena.NativeGame` implementing `IGame` and for World this is `ScreepsDotNet.Native.World.NativeGame` implementing `IGame`. At the start of your program you can instantiate this directly. You should avoid creating multiple instances of this throughout the lifetime of your program, and instead just reuse the same instance. Note that Arena and World have very different APIs so you won't be able to write code that targets both, unless you wrap the APIs in your own layer or do alot of switching.

All other objects follow the same inheritance hierarchy as the JS API. For example - `IStructureTower : IOwnedStructure : IStructure : IGameObject : IPosition`.

More details and documentation for the API is planned.

### Notes & Tips

- Instead of X and Y properties on game objects, you can access the `Position` of a game object, which is a struct encapsulating both X and Y. Positions can also be constructed in your own code, including from tuples, e.g. `Position myPos = (30, 40);`
- Many Screeps Arena methods accept both a `Position` and an `IPosition`, to reflect that you can use a game object in the place of a position in the JS API. In some places you may need to convert an `IPosition` to a `Position` by using `gameObject.Position` where accepting an `IPosition` is impractical in the API.
- Unlike with the JS API, keeping references to objects between ticks is fully supported and encouraged. The API will automatically refresh any stale instances as needed. Don't forget to test `IGameObject.Exists` to check that a reference is still valid. If you try to use an object that no longer exists, it will throw a `NativeObjectNoLongerExists` exception. If an object starts existing again (e.g. a room or object that regains visibility), you can safely reuse the same instance again.
- JS interop is expensive. The first access of a property on a game object will always involve an interop call (two if this is the first time the object has been used in a tick, as it has to refresh the stale instance first). However, the API will cache the property for the remainder of the tick, so subsequent accesses of that property will be cheap.
- You can't store properties directly on objects like in the JS API, nor can you extend the objects yourself. You can, however, use `IGameObject` as the key of a `Dictionary` or safely store it in any other collection like `HashSet`. Don't forget to clean up the collection when the game object is destroyed.
- You can associate user data with any game object, using `gameObject.SetUserData<T>(T instance)` and other sibling user data methods. Only reference types can be stored in this way, and the generic type parameter itself is used as the key. You should consider user data stored in this manner to be ephemeral, e.g. it might go away at any time, so always handle the case where user data is not set. You can store as many instances of different types as you like on a game object but only one instance per type. User data lookups are more efficient than a dictionary lookup.

## Native

It is possible to write some of your bot in native C and call into it from C# via icalls. Native C compiles directly to wasm whereas C# compiles to CIL which is executed by the Mono IL interpreter. The Mono IL interpreter is generally fast enough for most workloads you'll end up running in Screeps but for some compute-heavy algorithms such as pathfinding, mincut or distance transforms, every instruction counts. The following guide will demonstrate how to write a compute-heavy algorithm in native C and call it from C#.

- Add a C file to your project. The name of the file and location within the project tree does not matter, but it should have the `.c` extension.
- For the purposes of this guide we'll add two simple methods that sum either two values or `n` values. Add the following code:

    ```C
    #include <mono/metadata/loader.h>

    int AddTwo(int a, int b)
    {
        return a + b;
    }

    int Sum(int* values, int n)
    {
        int result = 0;
        for (int i = 0; i < n; i++)
        {
            result += values[i];
        }
        return result;
    }

    __attribute__((export_name("myproject_initnative")))
    void myproject_initnative()
    {
        mono_add_internal_call("MyProject_Native::AddTwo", AddTwo);
        mono_add_internal_call("MyProject_Native::Sum", Sum);
    }
    ```
- Add the following item group to your `.csproj` file:
    ```xml
    <ItemGroup>
        <_WasmRuntimePackSrcFile Include="$(MSBuildThisFileDirectory)MyNativeCFile.c" />
        <UpToDateCheckInput Include="MyNativeCFile.c" />
        <ScreepsCustomInitExportNames Include="myproject_initnative" />
    </ItemGroup>
    ```
- Add a C# file to your project to bind the native code. It should contain the following code:
    ```CSharp
    using System.Runtime.CompilerServices;

    internal static class MyProject_Native
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern int AddTwo(int a, int b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern unsafe int Sum(int* values, int n);
    }
    ```
    Note that this binding class must not be contained in any namespace and must be both internal and static. The name and signature of the icalls must match that of the native C functions exactly.
- Call the native method somewhere:
    ```CSharp
    Console.WriteLine($"1 + 2 = {AddTwo(1, 2)}!");

    Span<int> values = [1, 2, 3];
    unsafe
    {
        fixed (int* valuesPtr = values)
        {
            Console.WriteLine($"1 + 2 + 3 = {Sum(valuesPtr, values.Length)}!");
        }
    }
    ```
  Note that any code with dependencies on a native method will only run within a wasm environment and will no longer work when being unit tested. You can solve this by encapsulating the icalls in an api and providing both managed and native implementations of your code, switching as needed. For example:
    ```CSharp
    using System.Runtime.InteropServices;

    public void AddTwo(int a, int b)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
        {
            return MyProject_Native.AddTwo(a, b);
        }
        else
        {
            return a + b;
        }
    }

    public void Sum(ReadOnlySpan<int> values)
    {
        if (RuntimeInformation.OSArchitecture == Architecture.Wasm)
        {
            unsafe
            {
                fixed (int* valuesPtr = values)
                {
                    return MyProject_Native.Sum(valuesPtr, values.Length);
                }
            }
            
        }
        else
        {
            int result = 0;
            foreach (int value in values)
            {
                result += value;
            }
            return result;
        }
    }
    ```

All the usual rules of using unsafe code apply when you're passing pointers to managed data to native code. All the safety barriers are lifted and there's alot you can do wrong to very badly break the runtime. Don't be afraid to use `assert` liberally to check everything until you're confident your native code is working properly.

## Project Structure

Screeps DotNet is made up of the following pieces:

- [Managed API](ScreepsDotNet.API/) - class library providing interfaces and JS interop glue code for Screeps Arena and Screeps World.
- [Managed API Tests](ScreepsDotNet.API.Tests/) - xunit tests for [Managed API](ScreepsDotNet.API/)
- [Bundler](ScreepsDotNet.Bundler/) - msbuild extension that bundles the compiled wasm and assemblies during build, ready to be consumed by Screeps
- [Bootloader](Bootloader/) - typescript project containing the glue code and api bindings that loads the wasm into the Screeps environment
- [Example Arena Bot](ScreepsDotNet.ExampleArenaBot/) - working example project containing tutorial solutions for Screeps Arena
- [Example World Bot](ScreepsDotNet.ExampleWorldBot/) - working example project containing basic bot for Screeps World
- [SourceGen](ScreepsDotNet.SourceGen/) - source generator for js interop layer

## Limitations, Issues and Implications

### .Net
- Currently only .Net 8 is supported - targeting other versions of .Net with this version of ScreepsDotNet will not work.
- The whole .Net 8 runtime feature set is supported, at least as far as .Net 8's wasm support goes.
- Most features that involve the OS or external APIs will not work, for example file system IO, networking or http requests. There is a virtual filesystem provided by the wasi interop layer that might support reading and writing files but realisticly it's going to cost too much cpu to be worthwhile trying to use. Just keep stuff in memory.
- Usage of external libraries is supported, but bear in mind that this will inflate the bundle size by quite alot.

### Trimming
- Trimming is necessary to keep the bundle size down. This is basically like tree shaking in js - the trimmer removes all unreferenced types and methods during the build step.
- Trimming should not cause your code any trouble unless you're using reflection. Methods invoked by reflection do not count as a reference and so the trimmer will not know to keep that method. There are workarounds for this - do some research on .Net trimming to find out more.
- Any external libraries will be trimmed too and they might not be designed properly to consider this - check the library for trimming support before using.

### Bundle Size
- The Screeps script size limit is 5mb for both Screeps Arena and Screeps World. This limit is for all of your scripts total, not individual files.
- The bootloader js takes up around ~140kb and the main js contributes too, so you should aim for a rough bundle size limit of 4.5mb.
- For Screeps World, the bundle is packed into a single binary wasm file that can be deployed directly to Screeps. For Screeps Arena, binary wasm files are not supported and so the bundle is encoded to base64 and packed into a js file as a string instead. If necessary, you can override this behaviour using the `ScreepsEncoding` property in the csproj:
  ```XML
  <PropertyGroup>
    <ScreepsEncoding>b64</ScreepsEncoding>
  </PropertyGroup>
  ```
  Possible encodings are `bin`, `b64` and `b32768`.
- Compression can be enabled to reduce the size of the bundle at the cost of more CPU during startup. This can be enabled using the `ScreepsCompressWasm` property in the csproj:
  ```XML
  <PropertyGroup>
    <ScreepsCompressWasm>true</ScreepsEncoding>
  </PropertyGroup>
  ```

### CPU time
- Screeps Arena gives you 1000ms of cpu during the first tick and 50ms for every subsequent tick. Screeps World gives you 500ms hard limit every tick and a bucket system to limit average cpu usage based on GCL. If the script takes too long, it will be forcefully terminated, which could have disastrous consequences for the .Net runtime, especially if it happens during GC.
- Code runs slower the first time as the Mono IL interpreter is still figuring out the best way to execute your code. Over time your code will actually gain performance as all the common pathways are hit and optimised internally.
- There is also a small up-front cost to using any methods that involve JS interop, similar to a JIT, as it has to import and bind code to the JS api. This cannot be avoided.
- This means you need to be very careful about calling a bunch of code for the first time after the first tick as this might incur alot of startup overhead and run over the 50ms/500ms limit.
- It is recommended to track CPU usage (via Arena's `IGame.Utils.GetCpuTime()` or World's `IGame.Cpu.GetUsed()`) throughout your main loop and early-out if it's getting too close to the 50ms/500ms limit.
- For Screeps World, this is less of a problem as the bucket should hopefully absorb any CPU spikes caused by binding imports or JS interop, but remember you still have a 500ms tick limit and this is still quite easy to hit, especially during the runtime startup phase.

## Troubleshooting

ScreepsDotNet is still very young and you're likely to run into all sorts of problems, including ones nobody has ever had before. Unfortunately this means this section is very small and not likely to be too useful. Still, if you're having trouble, here are some things you can try.

- For Screeps Arena, any calls to `console.log` are ignored during the startup phase. The bootloader deals with this by storing all logs in a buffer and printing them all during the next loop instead. If you need to log something during startup, you'll need to implement something similar.
- For script execution timeouts, these are quite common during the initial startup phase when the wasm module is being compiled but should be recoverable. If not, try to avoid running too much code during `Init` or the first `Loop`.

## Local Development

If you wish to use latest changes that have not yet been released to NuGet, you will need to build the project locally.

- Whenever you make or pull a change to either the API or the Bundler (including bundled assets like the bootloader js), you will need to manually increment the version number in the csproj.
- Once the version number is changed, right click on the project in Visual Studio and click Pack. This will generate a NuGet package to the output folder.
- You can copy the generated NuGet package to your bot's local folder and have your bot's csproj reference that instead of the one from the NuGet repository. This allows your bot to compile against latest changes.
- Currently this is necessary for Screeps World support as this has not yet been published to NuGet.

## License

Licensed under the [MIT](LICENSE) license.
