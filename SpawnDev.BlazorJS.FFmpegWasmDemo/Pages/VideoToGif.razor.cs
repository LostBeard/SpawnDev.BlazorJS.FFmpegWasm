using Microsoft.AspNetCore.Components;
using SpawnDev.BlazorJS.FFmpegWasm;
using SpawnDev.BlazorJS.JSObjects;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace SpawnDev.BlazorJS.FFmpegWasmDemo.Pages
{
    public partial class VideoToGif
    {
        [Inject]
        BlazorJSRuntime JS { get; set; }

        [Inject]
        FFmpegFactory FFmpegFactory { get; set; }

        bool loaded = false;
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

        void FileInput_OnChange(Event ev)
        {
            using var files = fileInput.Files;
            var file = files != null && files.Length > 0 ? files[0] : null;
            if (file == null) return;
            _ = TranscodeLocalFile(file);
        }

        async Task Load()
        {
            busy = true;
            StateHasChanged();
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
            busy = false;
            loaded = true;
            StateHasChanged();
        }

        // -ss 61.0 -t 2.5 -i StickAround.mp4 -filter_complex "[0:v] fps=12,scale=480:-1,split [a][b];[a] palettegen [p];[b][p] paletteuse" SmallerStickAround.gif
        string[] VideoToGifCommand(string inputFile, string outputFile, double start, double duration, bool usePaletteGen)
        {
            if (usePaletteGen)
            {
                var ret = new string[] {
                    "-i", inputFile,
                    "-ss", start.ToString(),
                    "-t", duration.ToString(),
                    //"-filter_complex", "[0:v] fps=12,scale=w=480:h=-1,split [a][b];[a] palettegen=stats_mode=single [p];[b][p] paletteuse=new=1",
                    "-filter_complex", "[0:v] fps=12,scale=480:-1,split [a][b];[a] palettegen [p];[b][p] paletteuse",
                    outputFile,
                };
                return ret;
            }
            else
            {
                var ret = new string[] {
                    "-i", inputFile,
                    "-ss", start.ToString(),
                    "-t", duration.ToString(),
                    "-f","gif",
                    outputFile,
                };
                return ret;
            }
        }

        // https://engineering.giphy.com/how-to-make-gifs-with-ffmpeg
        string outputFileName = "";
        async Task TranscodeLocalFile(File file)
        {
            busy = true;
            StateHasChanged();
            var inputDir = "/input";
            var inputFile = $"/input/{file.Name}";
            var outputFile = file.Name.Substring(0, file.Name.LastIndexOf(".")) + ".gif";
            await ffmpeg.CreateDir(inputDir);
            await ffmpeg.MountWorkerFS(inputDir, new FSMountWorkerFSOptions { Files = new[] { file } });
            var ls = await ffmpeg.ListDir(inputDir);
            logMessage = "Transcoding source video";
            StateHasChanged();
            var cmd = VideoToGifCommand(inputFile, outputFile, 1, 5, false);
            await ffmpeg.Exec(cmd);
            // -ss 61.0 -t 2.5 -i StickAround.mp4 -filter_complex "[0:v] fps=12,scale=w=480:h=-1,split [a][b];[a] palettegen=stats_mode=single [p];[b][p] paletteuse=new=1" StickAroundPerFrame.gif
            logMessage = "Source video transcoded";
            StateHasChanged();
            await ffmpeg.Unmount(inputDir);
            await ffmpeg.DeleteDir(inputDir);
            using var data = await ffmpeg.ReadFileUint8Array(outputFile);
            using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = "video/gif" });
            outputFileName = outputFile;
            var objSrc = URL.CreateObjectURL(blob);
            outputURL = objSrc;
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
            percentComplete = ev.Progress * 100d;
            StateHasChanged();
        }

    }
}
