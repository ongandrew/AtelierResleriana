namespace AtelierResleriana.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder(args);

            webApplicationBuilder
                .Services
                .AddMvc()
                .AddNewtonsoftJson();

            webApplicationBuilder
                .Services
                .AddRazorPages();

            WebApplication webApplication = webApplicationBuilder.Build();

            webApplication
                .UseStaticFiles();

            webApplication
                .MapControllers();

            webApplication
                .MapRazorPages();

            webApplication.Run();
        }
    }
}
