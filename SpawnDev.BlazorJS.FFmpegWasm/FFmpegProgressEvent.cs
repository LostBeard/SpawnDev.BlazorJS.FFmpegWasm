using Microsoft.JSInterop;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public class FFmpegProgressEvent : JSObject
    {
        public FFmpegProgressEvent(IJSInProcessObjectReference _ref) : base(_ref) { }
        public double Progress => JSRef.Get<double>("progress");
        public double Time => JSRef.Get<double>("time");
    }
}
