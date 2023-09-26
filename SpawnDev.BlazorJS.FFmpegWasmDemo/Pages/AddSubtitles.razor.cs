using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SpawnDev.BlazorJS.FFmpegWasm;
using SpawnDev.BlazorJS.JSObjects;
using static System.Formats.Asn1.AsnWriter;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace SpawnDev.BlazorJS.FFmpegWasmDemo.Pages
{
    public partial class AddSubtitles
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
        ElementReference srtInputRef;
        HTMLInputElement? fileInput;
        HTMLInputElement? srtInput;
        ElementReference videoResult;
        HTMLVideoElement videoEl;
        bool beenInit = false;
        double percentComplete = 0;
        string outputURL = "";
        File? inputFileObj = null;
        File? srtFileObj = null;

        protected override void OnAfterRender(bool firstRender)
        {
            if (!beenInit)
            {
                beenInit = true;
                videoEl = new HTMLVideoElement(videoResult);
                fileInput = new HTMLInputElement(JS.ToJSRef(fileInputRef));
                fileInput.OnChange += FileInput_OnChange;
                srtInput = new HTMLInputElement(JS.ToJSRef(srtInputRef));
                srtInput.OnChange += SrtInput_OnChange;
            }
        }

        async Task Transcode()
        {
            if (inputFileObj == null || srtFileObj == null) return;
            await TranscodeLocalFile(inputFileObj, srtFileObj);
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
            inputFileObj = files != null && files.Length > 0 ? files[0] : null;
            StateHasChanged();
        }
        void SrtInput_OnChange(Event ev)
        {
            using var files = srtInput.Files;
            srtFileObj = files != null && files.Length > 0 ? files[0] : null;
            StateHasChanged();
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

        // https://engineering.giphy.com/how-to-make-gifs-with-ffmpeg
        string outputFileName = "";
        string outputMimeType = "";
        async Task TranscodeLocalFile(File videoFile, File srtFile)
        {
            busy = true;
            StateHasChanged();
            // load input videoFile and srtFile
            var inputDir = "/input";
            await ffmpeg.CreateDir(inputDir);
            await ffmpeg.MountWorkerFS(inputDir, new FSMountWorkerFSOptions { Files = new[] { videoFile, srtFile } });
            var inputFile = $"{inputDir}/{videoFile.Name}";
            var srtPath = $"/{inputDir}/{srtFile.Name}";
            var pos = videoFile.Name.LastIndexOf(".");
            var inputFilenameBase = pos > -1 ? videoFile.Name.Substring(0, pos) : videoFile.Name;
            var outputFile = inputFilenameBase + ".mp4";
            // load font
            var fontFile = "/tmp/calibri-regular.ttf";
            var fontURL = "./fonts/calibri-regular.ttf";
            await ffmpeg.WriteFile(fontFile, await FFmpegFactory.FetchFile(fontURL));
            // transcode
            logMessage = "Transcoding source video";
            StateHasChanged();
            string font_name = "Calibri";
            string primary_colour = "&H8ffffff";
            string outline_colour = "&H00000000";
            string back_colour = "";
            string border_style = "0";
            string outline = "1";
            string shadow = "0";
            string marginv = "20";
            string font_size = "32";
            await ffmpeg.Exec(new string[] {
                "-i",
                inputFile,
                "-vf",
                $"subtitles={srtPath}:fontsdir=/tmp:force_style='Fontname='{font_name}',Fontsize={font_size},PrimaryColour={primary_colour},OutlineColour={outline_colour},BorderStyle={border_style},Outline={outline},Shadow={shadow},MarginV={marginv},BackColour={back_colour}',scale=1280:720",
                "-c:v",
                "libx264",
                "-preset",
                "ultrafast",
                "-c:a",
                "copy",
                "-y",
                outputFile
            });
            logMessage = "Source video transcoded";
            StateHasChanged();
            await ffmpeg.Unmount(inputDir);
            await ffmpeg.DeleteDir(inputDir);
            using var data = await ffmpeg.ReadFileUint8Array(outputFile);
            using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = "video/mp4" });
            outputFileName = outputFile;
            var objSrc = URL.CreateObjectURL(blob);
            videoEl.Src = objSrc;
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
