using Microsoft.JSInterop;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public static partial class FFmpegFactoryExtensions
    {
        public const string RootPath = "_content/SpawnDev.BlazorJS.FFmpegWasm.Core";
        public static FFMessageLoadConfig CreateLoadCoreConfig(this FFmpegFactory _this)
        {
            return new FFMessageLoadConfig {
                CoreURL = new Uri(_this.BaseAddress, $"{RootPath}/ffmpeg-core.js").ToString(),
                WasmURL = new Uri(_this.BaseAddress, $"{RootPath}/ffmpeg-core.wasm").ToString(),
                WorkerLoadURL = _this.FFmpegWasmConfig.WorkerLoadURL,
            };
        }
    }
}
