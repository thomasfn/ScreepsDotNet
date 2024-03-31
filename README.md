# Screeps DotNet

A toolset and API to build bots for [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) and [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) using .Net 7.0.

* [What is Screeps DotNet?](#what-is-screeps-dotnet)
* [Quickstart](#quickstart)
* [API](#api)
* [Project structure](#project-structure)
* [Limitations, Issues and Implications](#limitations-issues-and-implications)
* [Troubleshooting](#troubleshooting)
* [License](#license)

## What is Screeps DotNet?

Screeps DotNet allows you to write bots for Screeps in any language that targets .Net 7.0, for example C#, and provides tooling to compile your bot to wasm ready to be deployed to the Screeps environment.

- [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) - :heavy_check_mark: full support
- [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) - :heavy_check_mark: full support

A managed API is provided that handles the interop with the Screeps javascript API, meaning you only need to write code against a set of generic interfaces. For some examples, please see the [example Arena project](ScreepsDotNet/Arena) which contains example solutions for all 10 tutorials of Screeps Arena, or the [example World project](ScreepsDotNet/World) which contains a barebones Screeps World bot.

## Quickstart

To get started making your first bot for Screeps in C#, follow these steps. You'll need a working dotnet environment as we're using terminal commands here. If you're using Visual Studio, you can use the Package Manager Console to run them.

### Workload
Install the wasm workload if you haven't done already.
```
dotnet workload install wasm-tools
dotnet workload install wasm-experimental
```
If the build process reports issues with the workload even after running the above commands, you may also need to install the following workload:
```
dotnet workload install wasm-tools-net7
```
If the above command reports that the workload ID is not recognized, update your installation of dotnet to 8.0. This should allow `wasm-tools-net7` to be recognized.

### Setup project
Create a new wasm project.
```
dotnet new wasmbrowser
```

Delete the default `index.html` that was created, and empty out `main.js` as these files are not used. The `main.js` file unfortunately cannot be removed as it is required by the wasm workload build process, but we won't use it for anything so we can just leave behind a stub.

```js
// stub main.js file - this is not used
```

Edit the csproj to contain the following property groups:

```XML
<PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <RuntimeIdentifier>browser-wasm</RuntimeIdentifier>
    <WasmMainJSPath>main.js</WasmMainJSPath>
    <OutputType>Exe</OutputType>

    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>enable</Nullable>
    <PublishTrimmed>true</PublishTrimmed>
    <TrimMode>full</TrimMode>
    <TrimmerRemoveSymbols>true</TrimmerRemoveSymbols>
    <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>

<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <RunAOTCompilation>true</RunAOTCompilation>
</PropertyGroup>
```

Also remove any mention of `index.html`. Note that the trimming and AOT settings here have [implications](#limitations-and-issues), you may need to do some research into these and play around to get settings that work for you.

Add nuget references to the following packages:
```
dotnet add (MyProjectName) package ScreepsDotNet.API
dotnet add (MyProjectName) package ScreepsDotNet.Bundler
```

### Entrypoint (Arena)

Replace your `Program.cs` with the following code:
```CS
using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.Arena;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static IGame? game;

        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [JSExport]
        internal static void Init()
        {
            game = new Native.Arena.NativeGame();
        }

        [JSExport]
        internal static void Loop()
        {
            if (game == null) { return; }
            Console.WriteLine($"Hello world from C#, the current tick is {game.Utils.GetTicks()}");
        }
    }
}
```

### Entrypoint (World)

Replace your `Program.cs` with the following code:
```CS
using System;
using System.Runtime.InteropServices.JavaScript;

using ScreepsDotNet.API.World;

namespace ScreepsDotNet
{
    public static partial class Program
    {
        private static IGame? game;

        public static void Main()
        {
            // Keep the entrypoint platform independent and let Init (which is called from js) create the game instance
            // This keeps the door open for unit testing later down the line
        }

        [JSExport]
        internal static void Init()
        {
            game = new Native.World.NativeGame();
        }

        [JSExport]
        internal static void Loop()
        {
            if (game == null) { return; }
            game.Tick();
            Console.WriteLine($"Hello world from C#, the current tick is {game.Time}");
        }
    }
}
```

### Building

You can change the namespace if you wish, but make a note that you will also need to change the `main.(m)js` file emitted by the bundler, so that the bootloader can find the entrypoint.

Build the project in publish mode.
```
dotnet publish -c Debug
-- or --
dotnet publish -c Release
```
A standard build will not suffice as it does not optimise the assembly size and the generated bundle will be way too big. Debug builds normally create larger bundle sizes than release builds but should still fall within the 5mb script size limit. Release builds will be larger if AOT is turned on, but should consume less CPU when loading and running. Release builds do take quite a bit longer to build though, so Debug builds are recommended for quick iteration.

### Build artifacts

The build artifacts can be found at `MyProjectName/bin/(Debug|Release)/AppBundle/(arena|world)`. All will need to be copied to your Screeps environment to work properly, however generally only the bundle file will change between builds.

#### Arena
- `bootloader.d.ts` - type definitions for the bootloader. Not strictly needed, but helpful when making modifications to `main.mjs`. Does not change between builds.
- `bootloader.mjs` - js for initialising and running the dotnet runtime. Does not change between builds but is different between Debug and Release builds.
- `bundle.mjs` - contains compressed and encoded dotnet wasm and assemblies. Changes every build, as it contains your code.
- `main.mjs` - entrypoint for the bot. Also contains Screeps API bindings. You can customise this if you want to, for example, add custom js functions that you want to call from C#. You also need to change the namespace of the `loop` function at the bottom to match your own namespace.

#### World
- `bootloader.js` - js for initialising and running the dotnet runtime. Does not change between builds but is different between Debug and Release builds.
- `bundle.js` - contains compressed and encoded dotnet wasm and assemblies. Changes every build, as it contains your code.
- `main.js` - entrypoint for the bot. Also contains Screeps API bindings. You can customise this if you want to, for example, add custom js functions that you want to call from C#. You also need to change the namespace of the `loop` function at the bottom to match your own namespace.

If all has gone well, you should now have a working basic bot that runs successfully in your Screeps environment.

### Next steps

You can architect your bot however you like. Generally it is recommended to keep the `Program.cs` as a slim bootstrap entrypoint and have it instantiate another class which will run the bot. You could use dependency injection to inject the instance of `IGame` to your code to keep it nice and separated from the specifics of the JS interop (which also makes it much easier to mock during unit testing), but the choice really is yours. Let your imagination run wild!

## API

The Screeps .Net API has been designed to be as close to the JS API as possible, with only minor alterations to adopt standard C# idioms. If you're familiar with the JS API, you will automatically be familiar with the .Net API. 

The API is exposed as a set of interfaces and a few support structures and is contained within the `ScreepsDotNet.API` namespace. Any part of the API common to both Arena and World lives directly in this namespace. Anything more specific lives in either `ScreepsDotNet.API.Arena` or `ScreepsDotNet.API.World`.

There is currently only one exposed concrete implementation. For Arena this is `ScreepsDotNet.Native.Arena.NativeGame` implementing `IGame` and for World this is `ScreepsDotNet.Native.World.NativeGame` implementing `IGame`. At the start of your program you can instantiate this directly. You should avoid creating multiple instances of this throughout the lifetime of your program, and instead just reuse the same instance. Note that Arena and World have very different APIs so you won't be able to write code that targets both, unless you wrap the APIs in your own layer or do alot of switching.

All other objects follow the same inheritance hierarchy as the JS API. For example - `IStructureTower : IOwnedStructure : IStructure : IGameObject : IPosition`.

More details and documentation for the API is planned.

### Notes & Tips

- Instead of X and Y properties on game objects, you can access the `Position` of a game object, which is a struct encapsulating both X and Y. Positions can also be constructed in your own code, including from tuples, e.g. `Position myPos = (30, 40);`
- Many Screeps Arena methods accept both a `Position` and an `IPosition`, to reflect that you can use a game object in the place of a position in the JS API. In some places you may need to convert an `IPosition` to a `Position` by using `gameObject.Position` where accepting an `IPosition` is impractical in the API.
- JS interop is expensive. If you're using a property that involves JS interop frequently within the same method, for example `ICreep.Body` or `IOwnedStructure.My`, consider caching the evaluation in a local variable instead to reduce JS calls.
- You can't store properties directly on objects like in the JS API, nor can you extend the objects yourself. You can, however, use `IGameObject` as the key of an `IDictionary` - this is the recommended way to associate data with an object. Don't forget to clean up the dictionary when the game object is destroyed!
- Keeping objects alive between ticks is supported. If you retrieve the same object again from the API you might get multiple managed instances for the same JS object, but they will all be considered equal and have the same hash code, so this shouldn't cause any problems. Don't forget to test `IGameObject.Exists` to check that a reference is still valid. If you try to use an object that no longer exists, it will throw a `NativeObjectNoLongerExists` exception. If an object starts existing again (e.g. a room or object that regains visibility), you can safely reuse the same instance again.

## Project Structure

Screeps DotNet is made up of the following pieces:

- [Managed API](ScreepsDotNet.API/) - class library providing interfaces and JS interop glue code for Screeps Arena and Screeps World.
- [Managed API Tests](ScreepsDotNet.API.Tests/) - xunit tests for [Managed API](ScreepsDotNet.API/)
- [Bundler](ScreepsDotNet.Bundler/) - msbuild extension that bundles the compiled wasm and assemblies during build, ready to be consumed by Screeps
- [Bootloader](Bootloader/) - modifies and wraps the `dotnet.js` glue code emitted by the .Net wasm workload to make it work in the Screeps environment
- [Example Project](ScreepsDotNet/) - working example project containing tutorial solutions for Screeps Arena

## Limitations, Issues and Implications

### .Net
- Currently only .Net 7 is supported - targeting other versions of .Net (including .Net 8) is very unlikely to work.
- The whole .Net 7 runtime feature set is supported, at least as far as .Net 7's wasm support goes.
- Most features that involve the OS or external APIs will not work, for example file system IO, networking or http requests. There is a virtual filesystem provided by emscripten that might support reading and writing files but realisticly it's going to cost too much cpu to be worthwhile trying to use. Just keep stuff in memory.
- Usage of external libraries is supported, but bear in mind that this will inflate the bundle size by quite alot.

### Trimming
- Trimming is necessary to keep the bundle size down. This is basically like tree shaking in js - the trimmer removes all unreferenced types and methods during the build step.
- Trimming should not cause your code any trouble unless you're using reflection. Methods invoked by reflection do not count as a reference and so the trimmer will not know to keep that method. There are workarounds for this - do some research on .Net trimming to find out more.
- Any external libraries will be trimmed too and they might not be designed properly to consider this - check the library for trimming support before using.

### Bundle Size
- The Screeps script size limit is 5mb for both Screeps Arena and Screeps World. This limit is for all of your scripts total, not individual files.
- The bootloader js takes up around ~530kb and the main js contributes too, so you should aim for a rough bundle size limit of 4mb.
- If you need to get your bundle size down, you can try disabling AOT and making a release build. In my tests you can the bundle size down to almost 1mb in this case.
- By default the bundle js is encoded to UTF8 contains the wasm and assemblies encoded as base64 strings. You can try the base32768 encoding that will create a UTF16 bundle js file which will cut down the size a little bit, but some Screeps clients (including the official steam one) seem to have issues deploying UTF16 files to the server. This is achieved by setting the `ScreepsEncoding` property in your csproj to `b32768` (the default is `b64`), for example:
  ```
  <PropertyGroup>
	<ScreepsEncoding>b32768</ScreepsEncoding>
  </PropertyGroup>
  ```
- You can also try the binary encoding that will put the actual data in a fake wasm file and have the bundle js file just reference it, which should turn out to be smaller than both of the text encodings. This is not supported on Arena. This is achieved by setting the `ScreepsEncoding` property in your csproj to `bin`, for example:
  ```
  <PropertyGroup>
	<ScreepsEncoding>bin</ScreepsEncoding>
  </PropertyGroup>
  ```

### CPU time
- Screeps Arena gives you 1000ms of cpu during the first tick and 50ms for every subsequent tick. Screeps World gives you 500ms hard limit every tick and a bucket system to limit average cpu usage based on GCL. If the script takes too long, it will be forcefully terminated, which could have disastrous consequences for the .Net runtime, especially if it happens during GC.
- Code runs slower the first time as the Mono IL interpreter is still figuring out the best way to execute your code. Over time your code will actually gain performance as all the common pathways are hit and optimised internally.
- AOT will compile parts of your code directly to wasm which will run much faster than the Mono IL interpreter can execute it, but also greatly increases the bundle size and the wasm module compile time, which will usually make using AOT on larger codebases impossible.
- There is also an up-front cost to using any methods that involve JS interop, similar to a JIT, as it has to import and bind code to the JS api. This cannot be avoided, even with AOT.
- This means you need to be very careful about calling a bunch of code for the first time after the first tick as this might incur alot of startup overhead and run over the 50ms/500ms limit.
- It is recommended to track CPU usage (via Arena's `IGame.Utils.GetCpuTime()` or World's `IGame.Cpu.GetUsed()`) throughout your main loop and early-out if it's getting too close to the 50ms/500ms limit.
- For Screeps World, this is less of a problem as the bucket should hopefully absorb any CPU spikes caused by JIT or JS interop, but remember you still have a 500ms tick limit and this is still quite easy to hit, especially during the runtime startup phase.

## Troubleshooting

Screeps DotNet is still very young and you're likely to run into all sorts of problems, including ones nobody has ever had before. Unfortunately this means this section is very small and not likely to be too useful. Still, if you're having trouble, here are some things you can try.

- For issues with runtime startup, try enabling verbose logging (`dotNet.setVerboseLogging(true);` in `main.[m]js`). This will hopefully allow you to narrow down where it's getting stuck.
- For Screeps Arena, any calls to `console.log` are ignored during the startup phase. The bootloader deals with this by storing all logs in a buffer and printing them all during the next loop instead. If you need to log something during startup, you'll need to implement something similar.
- For script execution timeouts, these are quite common during the initial startup phase when the wasm module is being compiled but should be recoverable. If not, ensure AOT is turned off and try to avoid running too much code during `Init` or the first `Loop`.

## Local Development

If you wish to use latest changes that have not yet been released to NuGet, you will need to build the project locally.

- Whenever you make or pull a change to either the API or the Bundler (including bundled assets like the bootloader js), you will need to manually increment the version number in the csproj.
- Once the version number is changed, right click on the project in Visual Studio and click Pack. This will generate a NuGet package to the output folder.
- You can copy the generated NuGet package to your bot's local folder and have your bot's csproj reference that instead of the one from the NuGet repository. This allows your bot to compile against latest changes.
- Currently this is necessary for Screeps World support as this has not yet been published to NuGet.

## License

Licensed under the [MIT](LICENSE) license.
