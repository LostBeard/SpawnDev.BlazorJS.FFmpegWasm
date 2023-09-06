using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using SpawnDev.BlazorJS.JSObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    // latest UMD versions
    // ffmpeg/ffmpeg
    // https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/ffmpeg.js
    // https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/814.ffmpeg.js
    // ffmpeg/core
    // https://unpkg.com/@ffmpeg/core/dist/umd/ffmpeg-core.js
    // https://unpkg.com/@ffmpeg/core/dist/umd/ffmpeg-core.wasm
    // ffmpeg/core-mt
    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.js
    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.wasm
    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.worker.js
    public class FFmpegFactory : IDisposable
    {
        string FFmpegJSURLDefault = "https://unpkg.com/@ffmpeg/ffmpeg@0.12.6/dist/umd/ffmpeg.js";
        string baseCoreMTURLDefault = $"https://unpkg.com/@ffmpeg/core-mt@0.12.3/dist/umd/";
        string baseCoreURLDefault = $"https://unpkg.com/@ffmpeg/core@0.12.3/dist/umd/";
        BlazorJSRuntime JS;
        public FFmpegWasmConfig FFmpegWasmConfig { get; private set; }
        string FFmpeg814ObjUrl = "";
        public bool MultiThreadSupported { get; } = false;
        public bool BeenInit { get; private set; } = false;
        public FFmpegFactory(BlazorJSRuntime js, NavigationManager navigationManager, FFmpegWasmConfig? ffmpegLoadConfig = null)
        {
            FFmpegWasmConfig = ffmpegLoadConfig ?? new FFmpegWasmConfig();
            JS = js;
            var baseAddress = new Uri(navigationManager.BaseUri);
            MultiThreadSupported = JS.CrossOriginIsolated && !JS.IsUndefined(nameof(SharedArrayBuffer));
            if (string.IsNullOrEmpty(FFmpegWasmConfig.FFmpegURL))
            {
                FFmpegWasmConfig.FFmpegURL = FFmpegJSURLDefault;
            }
            // in case this a relative URL change to absolute URL
            var ffmpegUri = new Uri(baseAddress, FFmpegWasmConfig.FFmpegURL);
            FFmpegWasmConfig.FFmpegURL = ffmpegUri.ToString();
            if (string.IsNullOrEmpty(FFmpegWasmConfig.Worker814URL))
            {
                // if the 814.ffmpeg.js path is not set assume it is in the same folder as ffmpeg.js
                var worker814Path = new Uri(ffmpegUri, "814.ffmpeg.js");
                FFmpegWasmConfig.Worker814URL = worker814Path.ToString();
            }
        }

        Task<bool>? _Ready = null;

        /// <summary>
        /// This method will import ffmpeg.js if it has not already been imported
        /// </summary>
        /// <returns></returns>
        public Task<bool> Init()
        {
            if (_Ready == null) _Ready = _Init();
            return _Ready;
        }

        async Task<bool> _Init()
        {
            var ret = false;
            try
            {
                if (JS.IsUndefined("FFmpegWASM"))
                {
                    var FFmpegObjUrl = await ToBlobURL(FFmpegWasmConfig.FFmpegURL!, "application/javascript", (js) => js.Replace("new URL(e.p+e.u(814),e.b)", "r.worker814URL"));
                    using var FFmpegWASM = await JS.Import(FFmpegObjUrl);
                    URL.RevokeObjectURL(FFmpegObjUrl);
                    if (FFmpegWASM == null) throw new Exception($"FFmpegWasm could not be initialized.");
                    FFmpeg814ObjUrl = await ToBlobURL(FFmpegWasmConfig.Worker814URL!, "application/javascript");
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FFmpegFactory.Init failed: {ex.Message}");
                throw;
            }
            return ret;
        }

        /// <summary>
        /// Loads ffmpeg/core or ffmpeg/core-mt (if SharedArrayBuffer is available) from unpkg.com
        /// from https://unpkg.com/@ffmpeg/core or https://unpkg.com/@ffmpeg/core-mt<br />
        /// </summary>
        /// <param name="allowMultiThreading"></param>
        /// <returns></returns>
        public async Task<FFMessageLoadConfig> CreateDefaultConfig(bool allowMultiThreading = true)
        {
            var config = new FFMessageLoadConfig { Worker814URL = FFmpeg814ObjUrl };
            var useThreading = MultiThreadSupported;
            if (useThreading && allowMultiThreading)
            {
                config.CoreURL = await ToBlobURL($"{baseCoreMTURLDefault}ffmpeg-core.js", "application/javascript");
                config.WasmURL = await ToBlobURL($"{baseCoreMTURLDefault}ffmpeg-core.wasm", "application/wasm");
                config.WorkerURL = await ToBlobURL($"{baseCoreMTURLDefault}ffmpeg-core.worker.js", "application/javascript");
            }
            else
            {
                config.CoreURL = await ToBlobURL($"{baseCoreURLDefault}ffmpeg-core.js", "application/javascript");
                config.WasmURL = await ToBlobURL($"{baseCoreURLDefault}ffmpeg-core.wasm", "application/wasm");
            }
            return config;
        }

        public async Task<Uint8Array> FetchFile(string resource)
        {
            using var resp = await JS.Fetch(resource);
            using var body = await resp.ArrayBuffer();
            return new Uint8Array(body);
        }
        public async Task<string> FetchText(string resource)
        {
            using var resp = await JS.Fetch(resource);
            return await resp.Text();
        }
        public async Task<string> ToBlobURL(string src, string mimeType)
        {
            var data = await FetchFile(src);
            using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = mimeType });
            return URL.CreateObjectURL(blob);
        }
        public async Task<string> ToBlobURL(string src, string mimeType, Func<string, string> patcher)
        {
            var text = await FetchText(src);
            if (patcher != null) text = patcher(text);
            using var blob = new Blob(new string[] { text }, new BlobOptions { Type = mimeType });
            return URL.CreateObjectURL(blob);
        }
        public void Dispose()
        {
            if (!string.IsNullOrEmpty(FFmpeg814ObjUrl))
            {
                URL.RevokeObjectURL(FFmpeg814ObjUrl);
                FFmpeg814ObjUrl = "";
            }
        }
    }
}
