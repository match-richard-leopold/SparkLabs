using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SparkLabs.Common.Clients;
using SparkLabs.ProfileApi.Tests.Fakes;

namespace SparkLabs.ProfileApi.Tests;

public class ProfileApiFactory : WebApplicationFactory<Program>
{
    public FakeModerationClient ModerationClient { get; private set; } = null!;
    public FakePhotoApiClient PhotoApiClient { get; private set; } = null!;

    public TimeSpan ModerationDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public TimeSpan PhotoApiDelay { get; set; } = TimeSpan.Zero;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real implementations
            var moderationDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IModerationClient));
            if (moderationDescriptor != null)
                services.Remove(moderationDescriptor);

            var photoApiDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IPhotoApiClient));
            if (photoApiDescriptor != null)
                services.Remove(photoApiDescriptor);

            // Also remove the HttpClient factory registration for IPhotoApiClient
            var httpClientDescriptors = services
                .Where(d => d.ServiceType.FullName?.Contains("IPhotoApiClient") == true)
                .ToList();
            foreach (var descriptor in httpClientDescriptors)
                services.Remove(descriptor);

            // Create and register fakes
            ModerationClient = new FakeModerationClient(ModerationDelay);
            PhotoApiClient = new FakePhotoApiClient(PhotoApiDelay);

            services.AddSingleton<IModerationClient>(ModerationClient);
            services.AddSingleton<IPhotoApiClient>(PhotoApiClient);
        });
    }
}
