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
        /// This is the script used for the primary ffmpeg worker. The UMD ffmpeg.wasm release has it named '814.ffmpeg.js' and<br/>
        /// it is usually stored in the same folder as ffmpeg.js
        /// Passed to ffmpeg.load call
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WorkerLoadURL { get; set; }
    }
}
