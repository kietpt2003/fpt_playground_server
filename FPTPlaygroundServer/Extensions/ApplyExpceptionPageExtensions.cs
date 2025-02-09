namespace FPTPlaygroundServer.Extensions;

public static class ApplyExpceptionPageExtensions
{
    public static void UseExceptionPageInLocal(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

        if (env.EnvironmentName == "Local")
        {
            app.UseDeveloperExceptionPage();
        }
    }
}
