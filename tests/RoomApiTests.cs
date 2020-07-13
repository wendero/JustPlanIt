using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using JustPlanIt.Classes;

namespace JustPlanIt.Tests
{
    public class RoomApiTests
    {
        private WebApplicationFactory<JustPlanIt.Startup> _factory;
        private HttpClient _client;
        private const string API_URL = "/api/room";

        [OneTimeSetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<JustPlanIt.Startup>();
            _client = _factory.CreateClient();
        }
        [Test]
        public void CreateRoom_ValidValues_Created()
        {
            var room = CreateRoom().Result;

            Assert.AreEqual("My room", room.Name);
            Assert.Greater(room.Identifier, 0);
            Assert.IsNotEmpty(room.Members);
            Assert.AreEqual("My name", room.Members[0].Name);
        }
        [TestCase(null, "My Name")]
        [TestCase("My Room", null)]
        public async Task CreateRoom_InvalidValues_BadRequest(string roomValue, string memberValue)
        {
            var bodyCreate = new
            {
                Room = roomValue,
                Name = memberValue
            };
            var responseCreate = await _client.PostJsonAsync(API_URL, bodyCreate);

            Assert.That(responseCreate.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var resultsCreate = responseCreate.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsCreate.ContainsKey("message"));
        }
        [Test]
        public async Task CheckRoom_ExistingRoom_True()
        {
            var room = CreateRoom().Result;
            var responseCheck = await _client.GetAsync(string.Format($"{API_URL}/{room.Identifier}/check"));

            Assert.That(responseCheck.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsCheck = responseCheck.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("true", resultsCheck);
        }
        [Test]
        public async Task CheckRoom_NotExistingRoom_False()
        {
            var room = CreateRoom().Result;
            var responseCheck = await _client.GetAsync(string.Format($"{API_URL}/12346/check"));

            Assert.That(responseCheck.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsCheck = responseCheck.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("false", resultsCheck);
        }
        [Test]
        public async Task GetRoom_ExistingRoom_Ok()
        {
            var room = CreateRoom().Result;
            var responseGet = await _client.GetAsync(string.Format($"{API_URL}/{room.Identifier}"));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsGet = responseGet.Content.ReadJson<JustPlanIt.Models.Room>();

            Assert.AreEqual("My room", resultsGet.Name);
            Assert.IsNotEmpty(resultsGet.Members);
            Assert.AreEqual("My name", resultsGet.Members[0].Name);
        }
        [TestCase("12346", HttpStatusCode.NotFound)]
        public async Task GetRoom_InvalidRoom_FailWithMessage(string invalidValue, HttpStatusCode expectedHttpCode)
        {
            var responseGet = await _client.GetAsync(string.Format($"{API_URL}/{invalidValue}"));

            Assert.That(responseGet.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsGet = responseGet.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsGet.ContainsKey("message"));
        }
        [Test]
        public async Task JoinRoom_ValidValues_AcceptedWithObject()
        {
            var room = CreateRoom().Result;
            var bodyJoin = new
            {
                Name = "My Member name"
            };

            var responseJoin = await _client.PostJsonAsync(string.Format($"{API_URL}/{room.Identifier}/join"), bodyJoin);

            Assert.That(responseJoin.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

            var resultsJoin = responseJoin.Content.ReadJson<JustPlanIt.Models.Member>();

            Assert.AreEqual(bodyJoin.Name, resultsJoin.Name);
            Assert.Greater(resultsJoin.Identifier, 0);
        }
        [TestCase("12346", null, HttpStatusCode.BadRequest)]
        [TestCase("123456", "My name", HttpStatusCode.NotFound)]
        public async Task JoinRoom_InvalidValues_FailWithObject(string roomValue, string memberName, HttpStatusCode expectedHttpCode)
        {
            var bodyJoin = new
            {
                Name = memberName
            };
            var responseJoin = await _client.PostJsonAsync(string.Format($"{API_URL}/{roomValue}/join"), bodyJoin);

            Assert.That(responseJoin.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsJoin = responseJoin.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsJoin.ContainsKey("message"));
        }
        [Test]
        public async Task CloseRoom_ExistingRoom_True()
        {
            var room = CreateRoom().Result;
            var responseGet = await _client.GetAsync(string.Format($"{API_URL}/{room.Identifier}/check"));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsGet = responseGet.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("true", resultsGet);

            var responseClose = await _client.DeleteAsync(string.Format($"{API_URL}/{room.Identifier}"));

            Assert.That(responseClose.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsClose = responseClose.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("true", resultsClose);

            responseGet = await _client.GetAsync(string.Format($"{API_URL}/{room.Identifier}/check"));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            resultsGet = responseGet.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("false", resultsGet);
        }
        [TestCase("12346", HttpStatusCode.NotFound)]
        public async Task CloseRoom_InvalidRoom_FalseWithObject(string roomValue, HttpStatusCode expectedHttpCode)
        {
            var responseClose = await _client.DeleteAsync(string.Format($"{API_URL}/{roomValue}"));

            Assert.That(responseClose.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsClose = responseClose.Content.ReadJson<Dictionary<string, dynamic>>();

            Assert.True(resultsClose.ContainsKey("message"));
        }

        private async Task<JustPlanIt.Models.Room> CreateRoom()
        {
            var bodyCreate = new
            {
                Room = "My room",
                Name = "My name"
            };
            var responseCreate = await _client.PostJsonAsync(API_URL, bodyCreate);

            Assert.That(responseCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var resultsCreate = responseCreate.Content.ReadJson<JustPlanIt.Models.Room>();

            return resultsCreate;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}