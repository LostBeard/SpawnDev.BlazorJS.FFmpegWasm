# SpawnDev.BlazorJS.FFmpegWasm
[![NuGet](https://img.shields.io/nuget/dt/SpawnDev.BlazorJS.FFmpegWasm.svg?label=SpawnDev.BlazorJS.FFmpegWasm)](https://www.nuget.org/packages/SpawnDev.BlazorJS.FFmpegWasm) 

The SpawnDev.BlazorJS.FFmpegWasm Nuget wraps [ffmpeg.wasm](https://github.com/ffmpegwasm/ffmpeg.wasm) with a [JSObject](https://github.com/LostBeard/SpawnDev.BlazorJS#jsobject-base-class) allowing it to be used in Blazor WASM. 

## With FFmpegFactory
The below code is taken from [BasicFactoryExample.razor](https://github.com/LostBeard/SpawnDev.BlazorJS.FFmpegWasm/blob/main/SpawnDev.BlazorJS.FFmpegWasmDemo/Pages/BasicFactoryExample.razor) and demonstrates using the FFmpegFactory service from SpawnDev.BlazorJS.FFmpegWasm. FFmpegFactory can handle the importing FFmpegWasm and includes a few helper methods.
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
        // FFmpegFactory handles importing umd version of FFmpegWasm and patching ffmpeg.js
        await FFmpegFactory.Init();
        ffmpeg = new FFmpeg();
        ffmpeg.OnLog += FFmpeg_OnLog;
        ffmpeg.OnProgress += FFmpeg_OnProgress;
        // FFmpegFactory.CreateDefaultConfig creates a config by loading ffmpeg/core or ffmpeg/core-mt (if allowMultiThreading && SharedArrayBuffer is available) from unpkg.com
        // latest version (at time of SpawnDev.BlazorJS.FFmpegWasm build) are used but other versions can be set
        var defaultConfig = await FFmpegFactory.CreateDefaultConfig(allowMultiThreading: true);
        await ffmpeg.Load(defaultConfig);
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

## Without FFmpegFactory
Or you can use FFmpegWasm without the FFmpegFactory.
The below code is taken from [BasicExample.razor](https://github.com/LostBeard/SpawnDev.BlazorJS.FFmpegWasm/blob/main/SpawnDev.BlazorJS.FFmpegWasmDemo/Pages/BasicExample.razor)
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