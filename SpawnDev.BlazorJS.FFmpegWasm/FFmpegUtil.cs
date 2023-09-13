//using SpawnDev.BlazorJS.JSObjects;
//using System.Text;

//namespace SpawnDev.BlazorJS.FFmpegWasm
//{
//    // latest versions
//    // ffmpeg/ffmpeg
//    // https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/ffmpeg.js
//    // https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/814.ffmpeg.js
//    // ffmpeg/core
//    // https://unpkg.com/@ffmpeg/core/dist/umd/ffmpeg-core.js
//    // https://unpkg.com/@ffmpeg/core/dist/umd/ffmpeg-core.wasm
//    // ffmpeg/core-mt
//    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.js
//    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.wasm
//    // https://unpkg.com/@ffmpeg/core-mt/dist/umd/ffmpeg-core.worker.js

//    public static class FFmpegUtil
//    {
//        static HttpClient HttpClient = new HttpClient();
//        static BlazorJSRuntime JS => BlazorJSRuntime.JS;
//        public static bool MultiThreadSupported = JS.CrossOriginIsolated && !JS.IsUndefined(nameof(SharedArrayBuffer));

//        public static async Task<FFMessageLoadConfig> CreateDefaultConfig()
//        {
//            var ffmpegBaseURL = $"https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/";
//            if (string.IsNullOrEmpty(FFmpeg814ObjUrl))
//            {
//                FFmpeg814ObjUrl = await ToBlobURL($"{ffmpegBaseURL}814.ffmpeg.js", "application/javascript");
//            }
//            var config = new FFMessageLoadConfig { WorkerLoadURL = FFmpeg814ObjUrl };
//            var useThreading = MultiThreadSupported);
//            if (useThreading)
//            {
//                var baseURL = $"https://unpkg.com/@ffmpeg/core-mt/dist/umd/";
//                config.CoreURL = await ToBlobURL($"{baseURL}ffmpeg-core.js", "application/javascript");
//                config.WasmURL = await ToBlobURL($"{baseURL}ffmpeg-core.wasm", "application/wasm");
//                config.WorkerURL = await ToBlobURL($"{baseURL}ffmpeg-core.worker.js", "application/javascript");
//            }
//            else
//            {
//                var baseURL = $"https://unpkg.com/@ffmpeg/core/dist/umd/";
//                config.CoreURL = await ToBlobURL($"{baseURL}ffmpeg-core.js", "application/javascript");
//                config.WasmURL = await ToBlobURL($"{baseURL}ffmpeg-core.wasm", "application/wasm");
//            }
//            return config;
//        }

//        static  Task? _Init = null;
//        public static Task Init
//        {
//            get
//            {
//                if (_Init == null)
//                {
//                    _Init = InitInternal();
//                }
//                return _Init;
//            }
//        }

//        static async Task InitInternal()
//        {

//        }

//        public static async Task<FFmpeg> CreateFFmpeg()
//        {
//            var ffmpegBaseURL = $"https://unpkg.com/@ffmpeg/ffmpeg/dist/umd/";
//            if (JS.IsUndefined("FFmpegWASM"))
//            {
//                FFmpegObjUrl = await ToBlobURL($"{ffmpegBaseURL}ffmpeg.js", "application/javascript", (js) =>
//                {
//                    // a quick patch to allow us the ability to specify the full path to the primary ffmpeg worker (814.ffmpeg.js) via our FFMessageLoadConfig
//                    // the ffmpeg.js script tries to build the path location itself (with the code being replaced) but will
//                    // fail (in our scenario) so we patch it to allow us to specify the path
//                    return js.Replace("new URL(e.p+e.u(814),e.b)", "r.worker814URL");
//                });
//                using var FFmpegWASM = await JS.Import(FFmpegObjUrl);
//                if (FFmpegWASM == null) throw new Exception($"FFmpegWasm could not be initialized.");
//            }
//            return new FFmpeg();
//        }

//        public static Task<byte[]> FetchFile(string src) => HttpClient.GetByteArrayAsync(src);
//        public static async Task<string> ToBlobURL(string src, string mimeType)
//        {
//            var bytes = await HttpClient.GetByteArrayAsync(src);
//            using var blob = new Blob(new byte[][] { bytes }, new BlobOptions { Type = mimeType });
//            return URL.CreateObjectURL(blob);
//        }

//        public static async Task<string> ToBlobURL(string src, string mimeType, Func<string, string> patcher)
//        {
//            var text = await HttpClient.GetStringAsync(src);
//            text = patcher(text);
//            var bytes = Encoding.UTF8.GetBytes(text);
//            using var blob = new Blob(new byte[][] { bytes }, new BlobOptions { Type = mimeType });
//            return URL.CreateObjectURL(blob);
//        }
//    }
//}
