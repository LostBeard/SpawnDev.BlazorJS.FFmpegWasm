using Microsoft.JSInterop;
using SpawnDev.BlazorJS.JSObjects;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    /// <summary>
    /// FFmpeg<br/>
    /// https://ffmpegwasm.netlify.app/docs/api/ffmpeg/classes/FFmpeg
    /// </summary>
    public class FFmpeg : JSObject
    {
        /// <summary>
        /// Returns true if loaded
        /// </summary>
        public bool Loaded => JSRef!.Get<bool>("loaded");
        ///<inheritdoc/>
        public FFmpeg(IJSInProcessObjectReference _ref) : base(_ref) { }
        /// <summary>
        /// Constructs a new FFmpeg instance (umd version)
        /// </summary>
        public FFmpeg() : base(JS.New("FFmpegWASM.FFmpeg")) { }
        /// <summary>
        /// Loads ffmpeg-core inside web worker. It is required to call this method first as it initializes WebAssembly and other essential variables.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public Task<bool> Load(FFMessageLoadConfig config) => JSRef!.CallAsync<bool>("load", config);
        /// <summary>
        /// Loads ffmpeg-core inside web worker. It is required to call this method first as it initializes WebAssembly and other essential variables.
        /// </summary>
        /// <returns></returns>
        public Task<bool> Load() => JSRef!.CallAsync<bool>("load");
        /// <summary>
        /// Add an event listener
        /// </summary>
        /// <param name="eventNam"></param>
        /// <param name="callback"></param>
        public void On(string eventNam, Callback callback) => JSRef!.CallVoid("on", eventNam , callback);
        /// <summary>
        /// Remove an event listener
        /// </summary>
        /// <param name="eventNam"></param>
        /// <param name="callback"></param>
        public void Off(string eventNam, Callback callback) => JSRef!.CallVoid("off", eventNam, callback);
        /// <summary>
        /// Log event handler
        /// </summary>
        public ActionEvent<FFmpegLogEvent> OnLog { get => new ActionEvent<FFmpegLogEvent>(o => On("log", o), o => Off("log", o)); set { /** set MUST BE HERE TO ENABLE += -= operands **/ } }
        /// <summary>
        /// Progress event handler
        /// </summary>
        public ActionEvent<FFmpegProgressEvent> OnProgress { get => new ActionEvent<FFmpegProgressEvent>(o => On("progress", o), o => Off("progress", o)); set { /** set MUST BE HERE TO ENABLE += -= operands **/ } }
        /// <summary>
        /// Terminate all ongoing API calls and terminate web worker. FFmpeg.load() must be called again before calling any other APIs
        /// </summary>
        public void Terminate() => JSRef!.CallVoid("terminate");
        /// <summary>
        /// Execute ffmpeg command.
        /// </summary>
        /// <param name="args">ffmpeg command line args</param>
        /// <param name="timeout">milliseconds to wait before stopping the command execution. Default Value -1</param>
        /// <param name="options"></param>
        /// <returns>0 if no error, != 0 if timeout (1) or error.</returns>
        public Task<int> Exec(IEnumerable<string> args, long timeout = -1, FFMessageOptions? options = null) => options == null ? JSRef!.CallAsync<int>("exec", args, timeout) : JSRef!.CallAsync<int>("exec", args, timeout, options);
        /// <summary>
        /// Execute ffmpeg command.
        /// </summary>
        /// <param name="args">ffmpeg command line args</param>
        /// <param name="timeout">milliseconds to wait before stopping the command execution. Default Value -1</param>
        /// <param name="signal">Abort signal for the command</param>
        /// <returns>0 if no error, != 0 if timeout (1) or error.</returns>
        public Task<int> Exec(IEnumerable<string> args, long timeout = -1, AbortSignal? signal = null) => signal == null ? JSRef!.CallAsync<int>("exec", args, timeout) : JSRef!.CallAsync<int>("exec", args, timeout, new { Signal = signal });
        /// <summary>
        /// Execute ffmpeg command.
        /// </summary>
        /// <param name="args">ffmpeg command line args</param>
        /// <returns>0 if no error, != 0 if timeout (1) or error.</returns>
        public Task<int> Exec(IEnumerable<string> args) => JSRef!.CallAsync<int>("exec", args);
        /// <summary>
        /// Execute ffprobe command.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>0 if no error, != 0 if timeout (1) or error.</returns>
        public Task<int> FFprobe(IEnumerable<string> args) => JSRef!.CallAsync<int>("ffprobe", args);
        /// <summary>
        /// Execute ffprobe command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="timeout"></param>
        /// <returns>0 if no error, != 0 if timeout (1) or error.</returns>
        public Task<int> FFprobe(IEnumerable<string> args, long timeout) => JSRef!.CallAsync<int>("ffprobe", args, timeout);
        #region FileSystemMethods
        /// <summary>
        /// Create a directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> CreateDir(string path) => JSRef!.CallAsync<bool>("createDir", path);
        /// <summary>
        /// Delete an empty directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> DeleteDir(string path) => JSRef!.CallAsync<bool>("deleteDir", path);
        /// <summary>
        /// List directory contents.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<FSNode[]> ListDir(string path) => JSRef!.CallAsync<FSNode[]>("listDir", path);
        /// <summary>
        /// Rename a file or directory.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public Task<bool> Rename(string oldPath, string newPath) => JSRef!.CallAsync<bool>("rename", oldPath, newPath);
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> DeleteFile(string path) => JSRef!.CallAsync<bool>("deleteFile", path);
        // Read
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="encoding">File content encoding, supports two encodings: - utf8: read file as text file, return data in string type. - binary: read file as binary file, return data in Uint8Array type. Default Value binary</param>
        /// <returns></returns>
        public Task<T> ReadFile<T>(string path, string encoding) => JSRef!.CallAsync<T>("readFile", path, encoding);
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<string> ReadFileUTF8(string path) => ReadFile<string>(path, "utf8"); 
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<Uint8Array> ReadFile(string path) => ReadFile<Uint8Array>(path, "binary");
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<Uint8Array> ReadFileUint8Array(string path) => ReadFile<Uint8Array>(path, "binary");
        /// <summary>
        /// Read data from ffmpeg.wasm.
        /// </summary>
        public Task<byte[]> ReadFileBytes(string path) => ReadFile<byte[]>(path, "binary");
        // Write
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, string data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, Uint8Array data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        /// <summary>
        /// Write data to ffmpeg.wasm.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Task<bool> WriteFile(string path, byte[] data) => JSRef!.CallAsync<bool>("writeFile", path, data);
        /// <summary>
        /// Allows mounting of WORKERFS in supported builds of ffmpeg.wasm
        /// </summary>
        /// <param name="fsType"></param>
        /// <param name="options"></param>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public Task<bool> Mount(EnumString<FFFSType> fsType, FSMountOptions options, string mountPoint) => JSRef!.CallAsync<bool>("mount", fsType, options, mountPoint);
        /// <summary>
        /// Use to unmount a mounted filesystem
        /// </summary>
        /// <param name="mountPoint"></param>
        /// <returns></returns>
        public Task<bool> Unmount(string mountPoint) => JSRef!.CallAsync<bool>("unmount", mountPoint);
        /// <summary>
        /// Convenience function to mount WORKERFS in supported builds of ffmpeg.wasm
        /// </summary>
        /// <param name="mountPoint"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<bool> MountWorkerFS(string mountPoint, FSMountWorkerFSOptions options) => Mount(FFFSType.WORKERFS, options, mountPoint);
        #endregion
    }
}
