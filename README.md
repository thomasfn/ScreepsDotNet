# Screeps DotNet

A toolset and API to build bots for [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) and [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) (coming soon) using .Net 7.0.

* [What is Screeps DotNet?](#what-is-screeps-dotnet)
* [Quickstart](#quickstart)
* [Project structure](#project-structure)
* [Limitations, Issues and Implications](#limitations-issues-and-implications)
* [Troubleshooting](#troubleshooting)
* [License](#license)

This repo contains the code to build the .NET runtime, libraries and shared host (`dotnet`) installers for
all supported platforms, as well as the sources to .NET runtime and libraries.

## What is Screeps DotNet?

Screeps DotNet allows you to write bots for Screeps in any language that targets .Net 7.0, for example C#, and provides tooling to compile your bot to wasm ready to be deployed to the Screeps environment.

- [Screeps Arena](https://store.steampowered.com/app/1137320/Screeps_Arena/) - :heavy_check_mark: full support
- [Screeps World](https://store.steampowered.com/app/464350/Screeps_World/) - :x: not yet supported (wip)

A managed API is provided that handles the interop with the Screeps javascript API, meaning you only need to write code against a set of generic interfaces. For some examples, please see the [example project](ScreepsDotNet/Arena) which contains example solutions for all 10 tutorials of Screeps Arena.

## Quickstart

To get started making your first bot for Screeps in C#, follow these steps. You'll need a working dotnet environment as we're using terminal commands here. If you're using Visual Studio, you can use the Package Manager Console to run them.

### Workload
Install the wasm workload if you haven't done already.
```
dotnet workload install wasm-tools
dotnet workload install wasm-experimental
```

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
            game = new Native.Arena.NativeGame();
        }

        [JSExport]
        internal static void Loop()
        {
            Console.WriteLine($"Hello world from C#, the current tick is {game.Utils.GetTicks()}");
        }
    }
}
```

### Entrypoint (World)
To be determined. Screeps World support is still in development!

### Building

You can change the namespace if you wish, but make a note that you will also need to change the `main.mjs` file emitted by the bundler, so that the bootloader can find the entrypoint.

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

### CPU time
- Screeps Arena gives you 1000ms of cpu during the first tick and 50ms for every subsequent tick. If the script takes too long, it will be forcefully terminated, which could have disastrous consequences for the .Net runtime, especially if it happens during GC.
- The first time your code is executed it will need to be JIT'd. This comes with a one-off cpu time cost.
- AOT avoids this but inflates the bundle size by quite a bit.
- There is also an up-front cost to using any methods that involve JS interop, similar to the JIT, as it has to import and bind code to the JS api. This cannot be avoided, even with AOT.
- This means you need to be very careful about calling a bunch of code for the first time after the first tick as this might incur alot of JIT time and run over the 50ms limit.
- It is recommended to track CPU usage (via `IGame.Utils.GetCpuTime()`) throughout your main loop and early-out if it's getting too close to the 50ms limit.
- For Screeps World, this is less of a problem as the bucket should hopefully absorb any CPU spikes caused by JIT or JS interop, but remember you still have a 500ms tick limit and this is still quite easy to hit, especially during the runtime startup phase.

## Troubleshooting

Screeps DotNet is still very young and you're likely to run into all sorts of problems, including ones nobody has ever had before. Unfortunately this means this section is very small and not likely to be too useful. Still, if you're having trouble, here are some things you can try.

- For issues with runtime startup, try enabling verbose logging (`dotNet.setVerboseLogging(true);` in `main.[m]js`). This will hopefully allow you to narrow down where it's getting stuck.
- For Screeps Arena, any calls to `console.log` are ignored during the startup phase. The bootloader deals with this by storing all logs in a buffer and printing them all during the next loop instead. If you need to log something during startup, you'll need to implement something similar.

## License

Licensed under the [MIT](LICENSE) license.
