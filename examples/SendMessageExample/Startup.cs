using AzureServiceBus.Client.Bus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SendMessageExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new TopicClientSendOperations("Endpoint=sb://stylianoslinux.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/CNuBgPssNjxJ2CDnZFYrEy+cN6zekrwzSzs4aNRkjk=", "testtopic22", 1, 10, 5, "1a98abbf-23f4-4593-9e87-0cc577b3374c", "e5a7d2ab-9d55-4996-9480-0e23248d23ec", "9Ue1y~_H1~.Rzeog8pk1sUrb_lqc~LI4l1", "af7b2dac-d8ce-4995-a2e2-e45935ef2b51", "LinuxStelios", "stylianoslinux", false, true));
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
