using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpawnDev.BlazorJS.FFmpegWasm;
using SpawnDev.BlazorJS.FFmpegWasmDemo;

namespace SpawnDev.BlazorJS.FFmpegWasmDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddBlazorJSRuntime();
            builder.Services.AddSingleton<FFmpegFactory>();
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
#if DEBUG
            var host = builder.Build();
            var JS = BlazorJSRuntime.JS;
            JS.Log(nameof(JS.CrossOriginIsolated), JS.CrossOriginIsolated);


            //var ffprobe = new FFProbe(JS);
            //try
            //{
            //    JS.Log("FetchFile");
            //    var file = await ffprobe.FetchFile("https://video-examples-public.s3.us-west-2.amazonaws.com/tears-of-steel-360p.mp4");
            //    JS.Log("GetFileInfo");
            //    var info = await ffprobe.GetMediaInfo(file);
            //    JS.Log("info", info);
            //}
            //catch (Exception ex)
            //{
            //    JS.Log("error", ex.Message);
            //}

            //using var http = new HttpClient();
            //try
            //{
            //    var response = await http.GetAsync("https://registry.npmjs.org/@ffmpeg/ffmpeg/-/ffmpeg-0.12.3.tgz");
            //    var respp = true;
            //}
            //catch (Exception ex)
            //{
            //    var nmt = true;
            //}
            await host.BlazorJSRunAsync();
#else
            await builder.Build().BlazorJSRunAsync();
#endif
        }
    }
}
