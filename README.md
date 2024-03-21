<p align="center">
  <a href="#">
    <img alt="ffmpeg.wasm" width="128px" height="128px" src="https://github.com/LostBeard/ffmpeg.wasm/blob/main/apps/website/static/img/logo192.png"></img>
  </a>
</p>

# SpawnDev.BlazorJS.FFmpegWasm
| Package | Description | Includes |
|---------|-------------|----------|
|**SpawnDev.BlazorJS.FFmpegWasm** <br /> [![NuGet version](https://badge.fury.io/nu/SpawnDev.BlazorJS.FFmpegWasm.svg)](https://www.nuget.org/packages/SpawnDev.BlazorJS.FFmpegWasm)| ffmpeg.wasm for Blazor WASM | ffmpeg/*<br />ffmpeg.js<br />814.ffmpeg.js
|**SpawnDev.BlazorJS.FFmpegWasm.Core** <br /> [![NuGet version](https://badge.fury.io/nu/SpawnDev.BlazorJS.FFmpegWasm.Core.svg)](https://www.nuget.org/packages/SpawnDev.BlazorJS.FFmpegWasm.Core)| Includes SpawnDev.BlazorJS.FFmpegWasm and ffmpeg.wasm core resources | core/*<br />ffmpeg-core.js<br />ffmpeg-core.wasm
|**SpawnDev.BlazorJS.FFmpegWasm.CoreMT** <br /> [![NuGet version](https://badge.fury.io/nu/SpawnDev.BlazorJS.FFmpegWasm.CoreMT.svg)](https://www.nuget.org/packages/SpawnDev.BlazorJS.FFmpegWasm.CoreMT)| Includes SpawnDev.BlazorJS.FFmpegWasm and ffmpeg.wasm core-mt resources | core-mt/*<br />ffmpeg-core.js<br />ffmpeg-core.wasm<br />ffmpeg-core.worker.js
 
[ffmpeg.wasm](https://github.com/ffmpegwasm/ffmpeg.wasm) is a pure WebAssembly / Javascript port of FFmpeg. It enables video & audio record, convert and stream right inside browsers.

SpawnDev.BlazorJS.FFmpegWasm uses [SpawnDev.BlazorJS](https://github.com/LostBeard/SpawnDev.BlazorJS) [JSObjects](https://github.com/LostBeard/SpawnDev.BlazorJS#jsobject-base-class) to bring [ffmpeg.wasm](https://github.com/ffmpegwasm/ffmpeg.wasm) into Blazor WASM apps. A slightly customized version of ffmpeg.wasm ([repo here](https://github.com/LostBeard/ffmpeg.wasm)) is used to add additional functionality to the base version.

[Live Demo](https://lostbeard.github.io/SpawnDev.BlazorJS.FFmpegWasm/)
- [Transcode](https://lostbeard.github.io/SpawnDev.BlazorJS.FFmpegWasm/)
- [Video to Gif](https://lostbeard.github.io/SpawnDev.BlazorJS.FFmpegWasm/VideoToGif)
- [Add subtitles](https://lostbeard.github.io/SpawnDev.BlazorJS.FFmpegWasm/AddSubtitles)

## FFmpegFactory
FFmpegFactory is an optional service that can handle importing FFmpegWasm and includes helper methods like ToBlobURL, FetchFile, CreateLoadCoreConfig, and CreateLoadCoreMTConfig.

#### With FFmpegFactory
Source [BasicFactoryExample.razor](https://github.com/LostBeard/SpawnDev.BlazorJS.FFmpegWasm/blob/main/SpawnDev.BlazorJS.FFmpegWasmDemo/Pages/BasicFactoryExample.razor) 

<details>
<summary>Example code</summary>

Program.cs
```cs
// ...
// Add SpawnDev.BlazorJS.BlazorJSRuntime service
builder.Services.AddBlazorJSRuntime();
// Add FFmpegFactory service
builder.Services.AddSingleton<FFmpegFactory>();
// ...
```

BasicFactoryExample.razor
```cs
@page "/BasicFactoryExample"
@using System.Text
@using SpawnDev.BlazorJS
@using SpawnDev.BlazorJS.FFmpegWasm

<h3>Basic FFmpegFactory Example of ffmpeg.wasm in Blazor</h3>
<p>A bare bones example of ffmpeg.wasm in Blazor using SpawnDev.BlazorJS and SpawnDev.BlazorJS.FFmpegWASM</p>
<video @ref=videoResult autoplay muted id="video-result" controls></video>
<br />
<button @onclick=Load disabled="@(loaded || busy)" id="load-button">Load ffmpeg-core (~31 MB)</button>
<br />
<button @onclick=Transcode disabled="@(!loaded || busy)" id="transcode-button">Transcode webm to mp4</button>
<br />
<p>@logMessage</p>
<p>@progressMessage</p>
<p>Open Developer Tools (Ctrl+Shift+I) to View Logs</p>

@code {
    [Inject]
    BlazorJSRuntime JS { get; set; }

    [Inject]
    FFmpegFactory FFmpegFactory { get; set; }

    ElementReference videoResult;
    HTMLVideoElement? videoEl;
    bool loaded = false;
    bool busy = false;
    string logMessage = "";
    string progressMessage = "";

    FFmpeg? ffmpeg = null;

    async Task Load()
    {
        busy = true;
        StateHasChanged();
        videoEl = new HTMLVideoElement(videoResult);
        await FFmpegFactory.Init();
        ffmpeg = new FFmpeg();
        ffmpeg.OnLog += FFmpeg_OnLog;
        ffmpeg.OnProgress += FFmpeg_OnProgress;
        // Use FFmpegFactory extension methods supplied by the Nuget packages
        // SpawnDev.BlazorJS.FFmpegWasm.Core
        // SpawnDev.BlazorJS.FFmpegWasm.CoreMT
        //
        // Single thread and multi thread versions acn be used independently of each other to lower resource packaging.
        //
        // From SpawnDev.BlazorJS.FFmpegWasm.Core 
        // - Contains the ffmpeg.wasm core for single thread files
        // - Adds CreateLoadCoreConfig to FFmpegFactory
        // From SpawnDev.BlazorJS.FFmpegWasm.CoreMT 
        // - Contains the ffmpeg.wasm core for multi thread files
        // - Adds CreateLoadCoreMTConfig to FFmpegFactory
        var loadConfig = FFmpegFactory.MultiThreadSupported ? FFmpegFactory.CreateLoadCoreMTConfig() : FFmpegFactory.CreateLoadCoreConfig();
        await ffmpeg.Load(loadConfig);
        busy = false;
        loaded = true;
        StateHasChanged();
    }

    async Task Transcode()
    {
        busy = true;
        StateHasChanged();
        logMessage = "Downloading source video";
        StateHasChanged();
        await ffmpeg.WriteFile("input.webm", await FFmpegFactory.FetchFile("https://raw.githubusercontent.com/ffmpegwasm/testdata/master/Big_Buck_Bunny_180_10s.webm"));
        logMessage = "Transcoding source video";
        StateHasChanged();
        var ret = await ffmpeg.Exec(new string[] { "-i", "input.webm", "output.mp4" });
        logMessage = "Source video transcoded";
        StateHasChanged();
        using var data = await ffmpeg.ReadFileUint8Array("output.mp4");
        using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = "video/mp4" });
        var objSrc = URL.CreateObjectURL(blob);
        videoEl.Src = objSrc;
        busy = false;
        StateHasChanged();
    }

    void FFmpeg_OnLog(FFmpegLogEvent ev)
    {
        logMessage = ev.Message;
        JS.Log("FFmpeg_OnLog", ev.Message);
        StateHasChanged();
    }

    void FFmpeg_OnProgress(FFmpegProgressEvent ev)
    {
        var progress = ev.Progress;
        var time = ev.Time;
        progressMessage = $"{progress * 100} % (transcoded time: {time / 1000000} s)";
        JS.Log("FFmpeg_OnProgress", ev.Time, ev.Progress);
        StateHasChanged();
    }
}
```
</details>

#### Without FFmpegFactory
Source [BasicExample.razor](https://github.com/LostBeard/SpawnDev.BlazorJS.FFmpegWasm/blob/main/SpawnDev.BlazorJS.FFmpegWasmDemo/Pages/BasicExample.razor)

<details>
<summary>Example code</summary>

Program.cs
```cs
// ...
// Add SpawnDev.BlazorJS.BlazorJSRuntime service
builder.Services.AddBlazorJSRuntime();
// ...
```

BasicExample.razor
```cs
@page "/"
@using System.Text
@using SpawnDev.BlazorJS
@using SpawnDev.BlazorJS.FFmpegWasm

<h3>Basic Example of ffmpeg.wasm in Blazor</h3>
<p>A bare bones example of ffmpeg.wasm in Blazor using SpawnDev.BlazorJS and SpawnDev.BlazorJS.FFmpegWASM</p>
<video @ref=videoResult autoplay muted id="video-result" controls></video>
<br />
<button @onclick=Load disabled="@(loaded || busy)" id="load-button">Load ffmpeg-core (~31 MB)</button>
<br />
<button @onclick=Transcode disabled="@(!loaded || busy)" id="transcode-button">Transcode webm to mp4</button>
<br />
<p>@logMessage</p>
<p>@progressMessage</p>
<p>Open Developer Tools (Ctrl+Shift+I) to View Logs</p>

@code {
    [Inject]
    BlazorJSRuntime JS { get; set; }

    ElementReference videoResult;
    HTMLVideoElement? videoEl;
    bool loaded = false;
    bool busy = false;
    string logMessage = "";
    string progressMessage = "";
    string baseURLFFmpeg = "https://unpkg.com/@ffmpeg/ffmpeg@0.12.6/dist/umd";
    string baseURLCore = "https://unpkg.com/@ffmpeg/core@0.12.3/dist/umd";

    FFmpeg? ffmpeg = null;

    async Task Load()
    {
        busy = true;
        StateHasChanged();
        videoEl = new HTMLVideoElement(videoResult);
        if (JS.IsUndefined("FFmpegWASM"))
        {
            // a quick patch to allow us the ability to specify the full path to the primary ffmpeg worker (814.ffmpeg.js in umd version) via our FFMessageLoadConfig
            // the ffmpeg.js script tries to build the path location itself (with the code being replaced) but will fail (in our scenario) so we patch it to allow us to specify the path
            // essentially the same as Pull request #562 (https://github.com/ffmpegwasm/ffmpeg.wasm/pull/562) except this works on the minified UMD version
            var FFmpegObjUrl = await ToBlobURL($"{baseURLFFmpeg}/ffmpeg.js", "application/javascript", (js) => js.Replace("new Worker(new URL(e.p+e.u(814),e.b),{type:void 0})", "new Worker(r.worker814URL,{type:void 0})"));
            await JS.Import(FFmpegObjUrl);
            URL.RevokeObjectURL(FFmpegObjUrl);
        }
        ffmpeg = new FFmpeg();
        ffmpeg.OnLog += FFmpeg_OnLog;
        ffmpeg.OnProgress += FFmpeg_OnProgress;
        await ffmpeg.Load(new FFMessageLoadConfig
            {
                Worker814URL = await ToBlobURL($"{baseURLFFmpeg}/814.ffmpeg.js", "application/javascript"),
                CoreURL = await ToBlobURL($"{baseURLCore}/ffmpeg-core.js", "application/javascript"),
                WasmURL = await ToBlobURL($"{baseURLCore}/ffmpeg-core.wasm", "application/wasm"),
            });
        busy = false;
        loaded = true;
        StateHasChanged();
    }

    async Task Transcode()
    {
        busy = true;
        StateHasChanged();
        logMessage = "Downloading source video";
        StateHasChanged();
        await ffmpeg.WriteFile("input.webm", await FetchFile("https://raw.githubusercontent.com/ffmpegwasm/testdata/master/Big_Buck_Bunny_180_10s.webm"));
        logMessage = "Transcoding source video";
        StateHasChanged();
        var ret = await ffmpeg.Exec(new string[] { "-i", "input.webm", "output.mp4" });
        logMessage = "Source video transcoded";
        StateHasChanged();
        using var data = await ffmpeg.ReadFileUint8Array("output.mp4");
        using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = "video/mp4" });
        var objSrc = URL.CreateObjectURL(blob);
        videoEl.Src = objSrc;
        busy = false;
        StateHasChanged();
    }

    void FFmpeg_OnLog(FFmpegLogEvent ev)
    {
        logMessage = ev.Message;
        JS.Log("FFmpeg_OnLog", ev.Message);
        StateHasChanged();
    }

    void FFmpeg_OnProgress(FFmpegProgressEvent ev)
    {
        var progress = ev.Progress;
        var time = ev.Time;
        progressMessage = $"{progress * 100} % (transcoded time: {time / 1000000} s)";
        JS.Log("FFmpeg_OnProgress", ev.Time, ev.Progress);
        StateHasChanged();
    }

    async Task<Uint8Array> FetchFile(string resource)
    {
        using var resp = await JS.Fetch(resource);
        using var body = await resp.ArrayBuffer();
        return new Uint8Array(body);
    }
    async Task<string> FetchText(string resource)
    {
        using var resp = await JS.Fetch(resource);
        return await resp.Text();
    }
    async Task<string> ToBlobURL(string src, string mimeType)
    {
        using var data = await FetchFile(src);
        using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = mimeType });
        return URL.CreateObjectURL(blob);
    }
    async Task<string> ToBlobURL(string src, string mimeType, Func<string, string> patcher)
    {
        var text = await FetchText(src);
        if (patcher != null) text = patcher(text);
        using var blob = new Blob(new string[] { text }, new BlobOptions { Type = mimeType });
        return URL.CreateObjectURL(blob);
    }
}
```
</details>
