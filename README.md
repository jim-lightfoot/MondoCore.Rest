# MondoCore.Rest
  Class/interface for calling REST apis
<br>

## Dependency Injection

 - In your dependency injection code create an instance of an api. 
     - Each unique api should be a different instance of RestApi. 
     - Differentiate each api by using a different class.
     
  <br/>


 
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

## Calling an API with IRestApi

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

## Reference

#### IRestAPI<T>
Interface used to call REST API. Note that generic type isn't actually used in the interface itself, it's just used to differentiate apis in dependency injection.

###### Task SendRequest<TRequest>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null)
No need to call this directly. See the default interface methods below.

###### Task<TResponse> SendRequest<TRequest, TResponse>(HttpMethod method, string url, TRequest? content = default(TRequest?), object? headers = null);
No need to call this directly. See the default interface methods below.

##### Default Methods

###### Task<T> Get<T>(string url, object? headers = null)
Does a GET request to the api. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task<TResponse> Post<TRequest, TResponse>(string url, TRequest content, object? headers = null)
Does a POST request to the api that returns a response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task Post<TRequest>(string url, TRequest content, object? headers = null)
Does a POST request to the api thats returns no response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task<TResponse> Put<TRequest, TResponse>(string url, TRequest content, object? headers = null)
Does a PUT request to the api that returns a response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task Put<TRequest>(string url, TRequest content, object? headers = null)
Does a PUT request to the api thats returns no response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task<TResponse> Patch<TRequest, TResponse>(string url, TRequest content, object? headers = null)
Does a PATCH request to the api that returns a response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task Patch<TRequest>(string url, TRequest content, object? headers = null)
Does a PATCH request to the api thats returns no response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

###### Task Delete<TRequest>(string url, object? headers = null)
Does a DELETE request to the api thats returns no response. Note that url is appended to the url defined in dependency injection. "headers" can be a POCO, anonymous object or a dictionary

License
----

MIT
