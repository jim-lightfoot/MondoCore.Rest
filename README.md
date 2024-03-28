# MondoCore.Rest
  Class/interface for calling REST apis

<br>

### Dependency Injection

 - In your dependency injection code create an instance of an api. 
     - Each unique api should be a different instance of RestApi. 
     - Differentiate each api by using a different class.

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ...

            builder.Services.AddHttpClient();

            // Register the apis. Automobile and Truck are just used to differentiate apis and can be blank classes
            builder.Services.AddRestApi<Automobile>("cars", "https://www.notrealcars.com)
                            .AddRestApi<Truck>("trucks", "https://www.notrealtrucks.com);

            // ...
        }
    }

    public static class Extensions
    {
        public static IServiceCollection AddRestApi<T>(this IServiceCollection services, string name, string url)
        {
            // Register the api and url with the HttpClientFactory
            services.AddHttpClient(name: name, configureClient: (p, client) =>
                    {
                        client.BaseAddress = new Uri(url);
                        client.Timeout = TimeSpan.FromSeconds(60);
                    });

            // Now inject the RestApi class
            services.AddScoped<IRestApi<T>>( p=> new RestApi<T>(p.GetRequiredService<IHttpClientFactory>(), name));

            return services;
        }
    }

<br>

### Calling an API with IRestApi

Inject the IRestApi interface into your class
 
    using MondoCore.Rest;

    public class CarService
    {
        private readonly IRestApi<Automoble> _api;

        public CoolClass(IRestApi<Automoble> api)
        {
            _api = api;
        }

        public async Task<List<Automobile>> GetGreenCars()
        {
            return _api.Get<List<Automobile>>("all/green"); // Appends "/all/green" onto the url given at DI time
        }

        public async Task<Automobile> GetCar(string id)
        {
            return _api.Get<Automobile>("car/" + id); // Appends "car/1" (for instance) onto the url given at DI time
        }
    }


<br>

License
----

MIT
