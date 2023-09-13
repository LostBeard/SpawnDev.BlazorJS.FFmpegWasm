using Microsoft.JSInterop;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public static partial class FFmpegFactoryExtensions
    {
        public const string RootPath = "_content/SpawnDev.BlazorJS.FFmpegWasm.CoreMT";
        public static FFMessageLoadConfig CreateLoadCoreMTConfig(this FFmpegFactory _this)
        {
            return new FFMessageLoadConfig
            {
                CoreURL = new Uri(_this.BaseAddress, $"{RootPath}/ffmpeg-core.js").ToString(),
                WasmURL = new Uri(_this.BaseAddress, $"{RootPath}/ffmpeg-core.wasm").ToString(),
                WorkerURL = new Uri(_this.BaseAddress, $"{RootPath}/ffmpeg-core.worker.js").ToString(),
                WorkerLoadURL = _this.FFmpegWasmConfig.WorkerLoadURL,
            };
        }
    }
}
