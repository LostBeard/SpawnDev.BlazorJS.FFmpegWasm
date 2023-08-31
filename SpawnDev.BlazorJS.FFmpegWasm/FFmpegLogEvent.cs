using Microsoft.JSInterop;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public class FFmpegLogEvent : JSObject
    {
        public FFmpegLogEvent(IJSInProcessObjectReference _ref) : base(_ref) { }
        public string Message => JSRef.Get<string>("message");
    }
}
