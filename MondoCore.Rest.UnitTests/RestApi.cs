using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MondoCore.Rest.UnitTests
{
    [TestClass]
    public class RestApiTests
    {
        private Mock<IHttpClientFactory>  _httpClientFactory = new ();
        private WireMockServer?           _server;
        private readonly HttpClient       _httpClient;
        private readonly IRestApi<string> _api;

        public RestApiTests()
        {
            _httpClient = new HttpClient();

            _httpClientFactory.Setup(f => f.CreateClient("test")).Returns(_httpClient);

            _httpClient.BaseAddress = new Uri("http://localhost:9876");

            _api = new RestApi<string>(_httpClientFactory.Object, "test");
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
        public async Task RestApi_Get()
        {
            var factory = new Mock<IHttpClientFactory>();
            using var client = new HttpClient();

            factory.Setup(f => f.CreateClient("test1")).Returns(client);

            client.BaseAddress = new Uri("https://datos.comunidad.madrid");

            IRestApi<string> api = new RestApi<string>(factory.Object, "test1");
            var result = await api.Get<MunicipioResponse>("catalogo/dataset/032474a0-bf11-4465-bb92-392052962866/resource/301aed82-339b-4005-ab20-06db41ee7017/download/municipio_comunidad_madrid.json");
 
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.data);
            Assert.AreNotEqual(0, result!.data!.Length);
            Assert.AreEqual("Sierra Norte", result.data?[0].nuts4_nombre);
        }

        [TestMethod]
        [DataRow("Bob's your uncle")]
        [DataRow("Fred's your aunt")]
        public async Task RestApi_Get_text(string response)
        {
            SetUpGet(response);

            var result = await _api.Get<string>("/test/1");

            Assert.AreEqual(response, result);
        }

        [TestMethod]
        [DataRow("Not found", HttpStatusCode.NotFound)]
        [DataRow("Gateway error", HttpStatusCode.GatewayTimeout)]
        public async Task RestApi_Get_error(string msg, HttpStatusCode statusCode)
        {
            SetUpGet("{ type: 'blah', title: '" + msg + "', detail: 'Error'}", (int)statusCode, "application/problem+json");

            var ex = await Assert.ThrowsExceptionAsync<RestException>( async ()=> await _api.Get<string>("/test/1"));

            Assert.IsNotNull(ex);
            Assert.AreEqual(msg, ex.Message);
            Assert.AreEqual(statusCode, ex.StatusCode);
        }

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Get_json(string response)
        {
            SetUpGet(response, contentType: "application/json");

            var result = await _api.Get<Automobile>("/test/1");

            Assert.AreEqual("Chevy", result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        [TestMethod]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 } ]")]
        public async Task RestApi_Get_json_array(string response)
        {
            SetUpGet(response, contentType: "application/json");

            var result = await _api.Get<List<Automobile>>("/test/1");

            Assert.AreEqual("Chevy",    result[0].Make);
            Assert.AreEqual("Corvette", result[0].Model);
        }

        [TestMethod]
        [DataRow("[ { Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 },  { Make: 'Pontiac', Model: 'Firebird', Color: 'Green', Year: 1969 } ]")]
        public async Task RestApi_Get_json_array2(string response)
        {
            SetUpGet(response, contentType: "application/json");

            var result = await _api.Get<List<Automobile>>("/test/1");

            Assert.AreEqual("Chevy",    result[0].Make);
            Assert.AreEqual("Corvette", result[0].Model);

            Assert.AreEqual("Pontiac",  result[1].Make);
            Assert.AreEqual("Firebird", result[1].Model);
        }

        #endregion

        #region Post

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Post_json(string response)
        {
            SetUpPost(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await _api.Post<Automobile, Automobile>("/test", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Post_no_response(string response)
        {
            SetUpPost(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            await _api.Post<Automobile>("/test", auto);
        }

        #endregion

        #region Put

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Put_json(string response)
        {
            SetUpPut(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await _api.Put<Automobile, Automobile>("/test/1", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }

        #endregion

        #region Patch

        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Patch_json(string response)
        {
            SetUpPatch(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            var result = await _api.Patch<Automobile, Automobile>("/test/1", auto);

            Assert.AreEqual("Chevy",    result.Make);
            Assert.AreEqual("Corvette", result.Model);
        }


        [TestMethod]
        [DataRow("{ Make: 'Chevy', Model: 'Corvette', Color: 'Blue', Year: 1956 }")]
        public async Task RestApi_Patch_no_response(string response)
        {
            SetUpPatch(response, contentType: "application/json");

            var auto = new Automobile { Make = "Chevy", Model = "Corvette" };

            await _api.Patch<Automobile>("/test/1", auto);
        }

        #endregion
        
        #region Delete

        [TestMethod]
        public async Task RestApi_Delete()
        {
            SetUpDelete();

            await _api.Delete("/test/1");
        }

        [TestMethod]
        public async Task RestApi_Delete_404()
        {
            SetUpDelete();

            var ex = await Assert.ThrowsExceptionAsync<RestException>( async ()=> await _api.Delete("/test/2"));

            Assert.IsNotNull(ex);
            Assert.AreEqual("Rest Api Exception, Status Code = NotFound", ex.Message);
            Assert.AreEqual(HttpStatusCode.NotFound, ex.StatusCode);
        }

        #endregion

        #region Private

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

        private HttpClient SetUpClient(string name, string url)
        {
            var client = new HttpClient();

            _httpClientFactory.Setup(f => f.CreateClient(name)).Returns(client);

            client.BaseAddress = new Uri(url);

            return client;
        }       
        
        internal class MunicipioResponse
        {
            public Municipio[]? data { get; set; }
        }

        internal class Municipio
        {
            public string? municipio_codigo      { get; set; }
            public string? densidad_por_km2      { get; set; }
            public string? municipio_codigo_ine  { get; set; }
            public string? nuts4_nombre          { get; set; }
            public string? municipio_nombre      { get; set; }
            public string? nuts4_codigo          { get; set; }
            public string? superficie_km2        { get; set; }
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