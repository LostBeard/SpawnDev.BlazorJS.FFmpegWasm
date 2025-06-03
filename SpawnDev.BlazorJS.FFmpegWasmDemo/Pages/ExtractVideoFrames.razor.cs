using Microsoft.AspNetCore.Components;
using SpawnDev.BlazorJS.FFmpegWasm;
using SpawnDev.BlazorJS.JSObjects;
using SpawnDev.BlazorJS.Toolbox;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace SpawnDev.BlazorJS.FFmpegWasmDemo.Pages
{
    public partial class ExtractVideoFrames
    {
        [Inject]
        BlazorJSRuntime JS { get; set; }

        [Inject]
        FFmpegFactory FFmpegFactory { get; set; }

        bool busy = false;
        string logMessage = "";
        string progressMessage = "";
        FFmpeg? ffmpeg = null;
        ElementReference fileInputRef;
        HTMLInputElement? fileInput;
        bool beenInit = false;
        double percentComplete = 0;
        string outputURL = "";

        protected override void OnAfterRender(bool firstRender)
        {
            if (!beenInit)
            {
                beenInit = true;
                fileInput = new HTMLInputElement(JS.ToJSRef(fileInputRef));
                fileInput.OnChange += FileInput_OnChange;
            }
        }

        public void Dispose()
        {
            CancelRun();
            _abortController?.Dispose();
            if (beenInit)
            {
                beenInit = false;
                fileInput.OnChange -= FileInput_OnChange;
                fileInput.Dispose();
            }
            if (ffmpeg != null)
            {
                ffmpeg.Terminate();
                ffmpeg.Dispose();
                ffmpeg = null;
            }
        }

        File? file = null;

        void FileInput_OnChange(Event ev)
        {
            using var files = fileInput!.Files;
            file = files?.FirstOrDefault();
            if (file == null) return;

        }

        async Task Run()
        {
            await TranscodeLocalFile();
        }

        AbortController? _abortController = null;
        void CancelRun()
        {
            _abortController?.Abort();
            Unload();
        }
        void Unload()
        {
            if (ffmpeg == null) return;
            ffmpeg.OnLog -= FFmpeg_OnLog;
            ffmpeg.OnProgress -= FFmpeg_OnProgress;
            ffmpeg.Terminate();
            ffmpeg = null;
        }

        async Task Load()
        {
            if (ffmpeg != null) return;
            await FFmpegFactory.Init();
            ffmpeg = new FFmpeg();
            ffmpeg.OnLog += FFmpeg_OnLog;
            ffmpeg.OnProgress += FFmpeg_OnProgress;
            // Use FFmpegFactory extension methods supplied by the Nuget packages
            // SpawnDev.BlazorJS.FFmpegWasm.Core
            // SpawnDev.BlazorJS.FFmpegWasm.CoreMT
            //
            // From SpawnDev.BlazorJS.FFmpegWasm.Core
            // - Contains the ffmpeg.wasm core for single thread files
            // - Adds CreateLoadCoreConfig to FFmpegFactory
            // From SpawnDev.BlazorJS.FFmpegWasm.CoreMT
            // - Contains the ffmpeg.wasm core for multi thread files
            // - Adds CreateLoadCoreMTConfig to FFmpegFactory
            // Single thread and multi thread versions acn be used independently of each other to lower resource packaging.
            var loadConfig = FFmpegFactory.MultiThreadSupported ? FFmpegFactory.CreateLoadCoreMTConfig() : FFmpegFactory.CreateLoadCoreConfig();
            await ffmpeg.Load(loadConfig);
            StateHasChanged();
        }

        // -ss 61.0 -t 2.5 -i StickAround.mp4 -filter_complex "[0:v] fps=12,scale=480:-1,split [a][b];[a] palettegen [p];[b][p] paletteuse" SmallerStickAround.gif
        string[] CreateExtractFramesCommand(string inputFile, string outputFile)
        {
            var ret = new string[] {
                "-i", inputFile,
                "-vsync", "0",
                outputFile,
            };
            return ret;
        }

        // https://engineering.giphy.com/how-to-make-gifs-with-ffmpeg
        string outputFileName = "";
        async Task TranscodeLocalFile()
        {
            if (file == null) return;
            busy = true;
            StateHasChanged();
            await Load();
            if (ffmpeg == null) return;
            var outputDir = "/output";
            await ffmpeg.CreateDir(outputDir);
            var inputDir = "/input";
            var inputFile = $"{inputDir}/{file.Name}";
            var outputFile = $"{outputDir}/frame_%08d.png";
            await ffmpeg.CreateDir(inputDir);
            await ffmpeg.MountWorkerFS(inputDir, new FSMountWorkerFSOptions { Files = new[] { file } });
            var ls = await ffmpeg.ListDir(inputDir);
            logMessage = "Transcoding source video";
            StateHasChanged();
            _abortController?.Dispose();
            using var abortController = new AbortController();
            _abortController = abortController;
            using var signal = abortController.Signal;
            var cmd = CreateExtractFramesCommand(inputFile, outputFile);
            int result = -2;
            var cancelled = false;
            try
            {
                result = await ffmpeg.Exec(cmd, signal: signal);
                JS.Log("execTask", result);
            }
            catch (Exception ex)
            {
                // operation was cancelled
                var nmt = true;
                cancelled = true;
                logMessage = "Done";
                busy = false;
                StateHasChanged();
                return;
            }
            logMessage = "Done";
            StateHasChanged();
            await ffmpeg.Unmount(inputDir);
            await ffmpeg.DeleteDir(inputDir);
            var outputs = await ffmpeg.ListDir(outputDir);
            FileSystemDirectoryHandle? outputFolder = null;
            if (outputFolder != null)
            {
                foreach (var output in outputs)
                {
                    if (new[] { ".", ".." }.Contains(output.Name)) continue;
                    if (output.IsDir) continue;
                    var fname = output.Name;
                    using var fdata = await ffmpeg.ReadFileUint8Array($"{outputDir}/{fname}");
                    await outputFolder.Write(fname, fdata);
                }
            }
            await ffmpeg.DeleteDir(outputDir);
            //using var data = await ffmpeg.ReadFileUint8Array(outputFile);
            //using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = "video/gif" });
            //outputFileName = outputFile;
            //var objSrc = URL.CreateObjectURL(blob);
            //outputURL = objSrc;
            busy = false;
            _abortController = null;
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
            percentComplete = ev.Progress * 100d;
            StateHasChanged();
        }

    }
}
