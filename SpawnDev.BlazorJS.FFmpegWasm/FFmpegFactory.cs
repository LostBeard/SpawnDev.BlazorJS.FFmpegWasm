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
    //
    // https://github.com/emscripten-core/emscripten/blob/1.29.12/src/library_idbfs.js
    public class FFmpegFactory : IDisposable
    {
        const string RootPath = "_content/SpawnDev.BlazorJS.FFmpegWasm";
        BlazorJSRuntime JS;
        public FFmpegWasmConfig FFmpegWasmConfig { get; private set; }
        public bool MultiThreadSupported { get; } = false;
        public bool BeenInit => _Ready != null;
        public Uri BaseAddress { get; }

        private Task<bool>? _Ready = null;
        public FFmpegFactory(BlazorJSRuntime js, NavigationManager navigationManager, FFmpegWasmConfig? ffmpegLoadConfig = null)
        {
            FFmpegWasmConfig = ffmpegLoadConfig ?? new FFmpegWasmConfig();
            JS = js;
            BaseAddress = new Uri(navigationManager.BaseUri);
            MultiThreadSupported = JS.CrossOriginIsolated && !JS.IsUndefined(nameof(SharedArrayBuffer));
        }

        /// <summary>
        /// This method will import ffmpeg.js if it has not already been imported
        /// </summary>
        /// <returns></returns>
        public Task<bool> Init(FFmpegWasmConfig? ffmpegWasmConfig = null)
        {
            if (_Ready == null)
            {
                if (ffmpegWasmConfig != null) FFmpegWasmConfig = ffmpegWasmConfig;
                if (string.IsNullOrEmpty(FFmpegWasmConfig.FFmpegURL))
                {
                    FFmpegWasmConfig.FFmpegURL = $"{RootPath}/ffmpeg.js"; 
                }
                // in case this a relative URL change to absolute URL
                var ffmpegUri = new Uri(BaseAddress, FFmpegWasmConfig.FFmpegURL);
                FFmpegWasmConfig.FFmpegURL = ffmpegUri.ToString();
                if (string.IsNullOrEmpty(FFmpegWasmConfig.WorkerLoadURL))
                {
                    // if the 814.ffmpeg.js path is not set assume it is in the same folder as ffmpeg.js
                    var worker814Path = new Uri(ffmpegUri, "814.ffmpeg.js");
                    FFmpegWasmConfig.WorkerLoadURL = worker814Path.ToString();
                }
                _Ready = _Init();
            }
            return _Ready;
        }

        private async Task<bool> _Init()
        {
            var ret = false;
            try
            {
                if (JS.IsUndefined("FFmpegWASM"))
                {
                    using var FFmpegWASM = await JS.Import(FFmpegWasmConfig.FFmpegURL!);
                    if (FFmpegWASM == null) throw new Exception($"FFmpegWasm could not be initialized.");
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

        public async Task<Uint8Array> FetchFile(Blob resource)
        {
            using var body = await resource.ArrayBuffer();
            return new Uint8Array(body);
        }
        public async Task<Uint8Array> FetchFile(string resource)
        {
            using var resp = await JS.Fetch(resource);
            using var body = await resp.ArrayBuffer();
            return new Uint8Array(body);
        }
        public async Task<string> ToBlobURL(string src, string mimeType)
        {
            var data = await FetchFile(src);
            using var blob = new Blob(new Uint8Array[] { data }, new BlobOptions { Type = mimeType });
            return URL.CreateObjectURL(blob);
        }

        public void Dispose()
        {

        }
    }
}
