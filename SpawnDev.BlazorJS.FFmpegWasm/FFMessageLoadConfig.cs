using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public class FFMessageLoadConfig
    {
        /// <summary>
        /// `814.ffmpeg.js` URL.<br />
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Worker814URL { get; set; }

        /// <summary>
        /// `ffmpeg-core.js` URL.<br />
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CoreURL { get; set; }

        /// <summary>
        /// `ffmpeg-core.wasm` URL.<br />
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WasmURL { get; set; }

        /// <summary>
        /// `ffmpeg-core.worker.js` URL.<br />
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkerURL { get; set; }
    }
}
