using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Net;
using System.Threading;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MondoCore.Rest.UnitTests
{
    [TestClass]
    public class RestApiTests
    {
        private WireMockServer? _server;

        public RestApiTests()
        {
        }      

        [TestInitialize]
        public void Initialize() 
        {
            _server = WireMockServer.Start(9876);
        }

        [TestCleanup]
        public void Cleanup() 
        {
            _server!.Stop();
        }

        #region Get 

        [TestMethod]
        [DataRow("Bob's your uncle", false)]
        [DataRow("Fred's your aunt", false)]
        [DataRow("Bob's your uncle", true)]
        [DataRow("Fred's your aunt", true)]
        public async Task RestApi_Get_wHeaders(string response, bool typed)
        {
            var headerFactory = new Mock<IHeaderFactory>();

            headerFactory.Setup(f => f.GetHeaders("test1")).ReturnsAsync(new Dictionary<string, string> { { "bobs", "youruncle" } });

            using var api = CreateFactory(typed);

            SetUpGet(response);

            var result = await api.Get<string>("/test/1");

            Assert.AreEqual(response, result);
        }

        [TestMethod]
        [DataRow("Bob's your uncle", false)]
        [DataRow("Fred's your aunt", false)]
        [DataRow("Bob's your uncle", true)]
        [DataRow("Fred's your aunt", true)]
        public async Task RestApi_Get_text(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGet(response);

            var result = await api.Get<string>("/test/1");

            Assert.AreEqual(response, result);
        }

        [TestMethod]
        [DataRow("Bob's your uncle", false)]
        public async Task RestApi_Get_text_wdelay(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGetWithDelay(response, delay: 8000);

            var result = await api.Get<string>("/test/1");
        }

        [TestMethod]
        [DataRow("Not found", HttpStatusCode.NotFound, false)]
        [DataRow("Gateway error", HttpStatusCode.GatewayTimeout, false)]
        [DataRow("Not found", HttpStatusCode.NotFound, true)]
        [DataRow("Gateway error", HttpStatusCode.GatewayTimeout, true)]
        public async Task RestApi_Get_error(string msg, HttpStatusCode statusCode, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGet("{ type: 'blah', title: '" + msg + "', detail: 'Error'}", (int)statusCode, "application/problem+json");

            var ex = await Assert.ThrowsExceptionAsync<RestException>( async ()=> await api.Get<string>("/test/1"));

            Assert.IsNotNull(ex);
            Assert.AreEqual(msg, ex.Message);
            Assert.AreEqual(statusCode, ex.StatusCode);
        }

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Get_json(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGet(response, contentType: "application/json");

            var result = await api.Get<Automobile>("/test/1");

            Assert.AreEqual("Chevy", result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        [TestMethod]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 } ]", false)]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 } ]", true)]
        public async Task RestApi_Get_json_array(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGet(response, contentType: "application/json");

            var result = await api.Get<List<Automobile>>("/test/1");

            Assert.AreEqual("Chevy",    result[0].Make);
            Assert.AreEqual("Corvette", result[0].Model);
        }

        [TestMethod]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 },  { Make: 'Pontiac', Model: 'Firebird', Color: 'Green', Year: 1969 } ]", false)]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 },  { Make: 'Pontiac', Model: 'Firebird', Color: 'Green', Year: 1969 } ]", true)]
        public async Task RestApi_Get_json_array2(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpGet(response, contentType: "application/json");

            var result = await api.Get<List<Automobile>>("/test/1");

            Assert.AreEqual("Chevy",    result[0].Make);
            Assert.AreEqual("Corvette", result[0].Model);

            Assert.AreEqual("Pontiac",  result[1].Make);
            Assert.AreEqual("Firebird", result[1].Model);
        }

        #endregion

        #region Post

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Post_json(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpPost(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await api.Post<Automobile, Automobile>("/test", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Post_no_response(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpPost(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            await api.Post<Automobile>("/test", auto);
        }

        #endregion

        #region Put

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Put_json(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpPut(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await api.Put<Automobile, Automobile>("/test/1", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        #endregion

        #region Patch

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Patch_json(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpPatch(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await api.Patch<Automobile, Automobile>("/test/1", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }


        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", false)]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }", true)]
        public async Task RestApi_Patch_no_response(string response, bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpPatch(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            await api.Patch<Automobile>("/test/1", auto);
        }

        #endregion
        
        #region Delete

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task RestApi_Delete(bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpDelete();

            await api.Delete("/test/1");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task RestApi_Delete_404(bool typed)
        {
            using var api = CreateFactory(typed);

            SetUpDelete();

            var ex = await Assert.ThrowsExceptionAsync<RestException>( async ()=> await api.Delete("/test/2"));

            Assert.IsNotNull(ex);
            Assert.AreEqual("Rest Api Exception, Status Code = NotFound", ex.Message);
            Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
        }

        [TestMethod]
        public async Task RestApi_timeout()
        {
            var client = new HttpClient();

            // Requires the TestApi to be running
            client.BaseAddress = new Uri("https://localhost:7010/Test/");
            client.Timeout = TimeSpan.FromSeconds(.5);

            using var api = new RestApi<string>(client, "test");

            IRestApi<string> iapi = api;

            var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>( async ()=> await iapi.Get<string>("name_timesout"));

            Assert.IsNotNull(ex.InnerException);
        }

        [TestMethod]
        public async Task RestApi_timeout3()
        {
            var client = new HttpClient();

            // Requires the TestApi to be running
            client.BaseAddress = new Uri("https://localhost:7010/Test/");

            using var cancelTokenSrc = new CancellationTokenSource();

            using IRestApi<string> api = new RestApi<string>(client, "test", timeout: 200);

            var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>( async ()=> await api.Get<string>("name_timesout", cancelTokenSrc.Token));

            Assert.IsNotNull(ex.InnerException);
        }

        [TestMethod]
        public async Task RestApi_testapi_success()
        {
            var client = new HttpClient();

            // Requires the TestApi to be running
            client.BaseAddress = new Uri("https://localhost:7010/Test/");

            using var api = new RestApi<string>(client, "test");

            IRestApi<string> iapi = api;

            var result = await iapi.Get<string>("name");

            Assert.AreEqual("bob", result);
        }

        #endregion

        #region Private

        private IRestApi<string> CreateFactory(bool typedClient, IHeaderFactory? headers = null)
        {
            if(typedClient)
                return CreateTypedApi(headers);

            return CreateFactoryApi(headers);
        }

        private IRestApi<string> CreateFactoryApi(IHeaderFactory? headers)
        {
            var httpClient = new HttpClient();
            Mock<IHttpClientFactory> httpClientFactory = new();

            httpClientFactory.Setup(f => f.CreateClient("test")).Returns(httpClient);

            httpClient.BaseAddress = new Uri("http://localhost:9876");

            return new RestApi<string>(httpClientFactory.Object, "test", headers);
        }

        private IRestApi<string> CreateTypedApi(IHeaderFactory? headers)
        {
            var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("http://localhost:9876");

            return new RestApi<string>(httpClient, "test", headerFactory: headers);
        }

        private void SetUpGet(string body, int statusCode = 200, string contentType = "text/plain")
        {
            _server!.Given
            (
                Request.Create().WithPath("/test/1").UsingGet()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );
        }     

        private void SetUpGetWithDelay(string body, int statusCode = 200, string contentType = "text/plain", int delay = 0)
        {
            _server!.Given
            (
                Request.Create().WithPath("/test/1").UsingGet()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
                        .WithRandomDelay(2000, 4000)
            );
        }     

        private void SetUpPost(string body, int statusCode = 200, string contentType = "text/plain")
        {
            _server!.Given
            (
                Request.Create().WithPath("/test").UsingPost()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );
        }     

        private void SetUpPut(string body, int statusCode = 200, string contentType = "text/plain")
        {
            _server!.Given
            (
                Request.Create().WithPath("/test/1").UsingPut()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );
        }     

        private void SetUpPatch(string body, int statusCode = 200, string contentType = "text/plain")
        {
            _server!.Given
            (
                Request.Create().WithPath("/test/1").UsingPatch()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader("Content-Type", contentType)
                        .WithBody(body)
            );
        }     

        private void SetUpDelete(int statusCode = 200)
        {
            _server!.Given
            (
                Request.Create().WithPath("/test/1").UsingDelete()
            )
            .RespondWith
            (
                Response.Create()
                        .WithStatusCode(statusCode)
            );
        }      

        internal class Automobile
        {
            public string? Make   { get; set; }
            public string? Model  { get; set; }
            public string? Color  { get; set; }
            public int?    Year   { get; set; }
        }

        #endregion
    }
}