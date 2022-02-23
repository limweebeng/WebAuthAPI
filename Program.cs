using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OA.AuthLibrary;
using OA.WebAuthLibrary;
using System.IO;
using System.Text;

namespace WebAuthAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            WebAuthHelper.UpdateErrorMessage(WebAuthHelper.AuthWebFilePath);
            AuthEngine.UpdateLocation(WebAuthHelper.AuthWebFilePath, DataShared.Properties.Resources.licenser);
            WebAuthHelper.UpdateLanguage(WebAuthHelper.AuthWebFilePath);
            WebAuthHelper.SetLanguage(DataShared.LanguageEnum.English);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                   webBuilder.UseIISIntegration();
                   webBuilder.ConfigureKestrel(options =>
                   {
                       options.Limits.MaxRequestBodySize = long.MaxValue;
                   })
                   .UseStartup<Startup>();
               });
    }
}
