using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public class FFMessageLoadConfig
    {
        /// <summary>
        /// `814.ffmpeg.js` URL.<br />
        /// This is the script used for the primary ffmpeg worker
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkerLoadURL { get; set; } = "_content/SpawnDev.BlazorJS.FFmpegWasm/814.ffmpeg.js";

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
        /// This file is only needed if using multithreading
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkerURL { get; set; }
    }
}
