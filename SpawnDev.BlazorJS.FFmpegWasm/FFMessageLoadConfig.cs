using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    /// <summary>
    /// Config argument when creating a new FFmpegFactory
    /// </summary>
    public class FFmpegWasmConfig
    {
        /// <summary>
        /// `ffmpeg.js` URL.<br />
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FFmpegURL { get; set; }

        /// <summary>
        /// `814.ffmpeg.js` URL (UMD version).<br />
        /// This is the script used for the primary ffmpeg worker. The UMD ffmpeg.wasm release has it named '814.ffmpeg.js' and<br /?
        /// it is usually stored in the same folder as ffmpeg.js
        /// Passed to ffmpeg.load call
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Worker814URL { get; set; }
    }

    public class FFMessageLoadConfig
    {
        /// <summary>
        /// `814.ffmpeg.js` URL.<br />
        /// This is the script used for the primary ffmpeg worker
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
        /// This file is only needed if using multithreading
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkerURL { get; set; }
    }
}
