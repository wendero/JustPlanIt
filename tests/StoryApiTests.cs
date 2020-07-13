using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using JustPlanIt.Classes;
using JustPlanIt.Models;

namespace JustPlanIt.Tests
{
    public class StoryApiTests
    {
        private WebApplicationFactory<JustPlanIt.Startup> _factory;
        private HttpClient _client;
        private const string API_URL_ROOM = "/api/room/";
        private const string API_URL_STORY = "/api/room/{0}/story/{1}";

        [OneTimeSetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<JustPlanIt.Startup>();
            _client = _factory.CreateClient();
        }
        [Test]
        public void CreateStory_ValidValues_Created()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            Assert.AreEqual("My story", story.Title);
            Assert.Greater(story.Identifier, 0);
        }
        [TestCase(null, null, HttpStatusCode.BadRequest)]
        [TestCase("My story", "12346", HttpStatusCode.NotFound)]
        public async Task CreateStory_InvalidValues_FailWithObject(string title, int? roomValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var roomId = roomValue ?? room.Identifier;

            var bodyCreate = new
            {
                Title = title
            };

            var responseCreate = await _client.PostJsonAsync(string.Format(API_URL_STORY, roomId, null), bodyCreate);

            Assert.That(responseCreate.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsCreate = responseCreate.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsCreate.ContainsKey("message"));
        }
        [Test]
        public async Task ChangeStory_ChangingTitle_Accepted()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            Assert.AreEqual("My story", story.Title);

            var bodyChange = new
            {
                Title = "My changed story"
            };

            var responseChange = await _client.PatchJsonAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier), bodyChange);

            Assert.That(responseChange.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

            var resultsChange = responseChange.Content.ReadJson<Models.Story>();

            Assert.AreEqual(bodyChange.Title, resultsChange.Title);
            Assert.AreEqual(0, resultsChange.Points);
            Assert.AreEqual(StoryStatus.Ready, resultsChange.Status);
        }
        [Test]
        public async Task ChangeStory_ChangingStatus_Accepted()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            Assert.AreEqual(StoryStatus.Ready, story.Status);

            var bodyChange = new
            {
                Status = StoryStatus.Done
            };

            var responseChange = await _client.PatchJsonAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier), bodyChange);

            Assert.That(responseChange.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

            var resultsChange = responseChange.Content.ReadJson<Models.Story>();

            Assert.AreEqual(story.Title, resultsChange.Title);
            Assert.AreEqual(0, resultsChange.Points);
            Assert.AreEqual(StoryStatus.Done, resultsChange.Status);
        }
        [Test]
        public async Task ChangeStory_ChangingPointsAndStatus_Accepted()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            Assert.AreEqual(StoryStatus.Ready, story.Status);

            var bodyChange = new
            {
                Status = StoryStatus.Voted,
                Points = 50
            };

            var responseChange = await _client.PatchJsonAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier), bodyChange);

            Assert.That(responseChange.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

            var resultsChange = responseChange.Content.ReadJson<Models.Story>();

            Assert.AreEqual(story.Title, resultsChange.Title);
            Assert.AreEqual(50, resultsChange.Points);
            Assert.AreEqual(StoryStatus.Voted, resultsChange.Status);

        }
        [TestCase(12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, HttpStatusCode.NotFound)]
        public async Task ChangeStory_InvalidValues_FailWithObject(int? roomValue, int? storyValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var roomId = roomValue ?? room.Identifier;
            var story = CreateStory(room.Identifier).Result;
            var storyId = storyValue ?? story.Identifier;

            var bodyChange = new
            {
            };

            var responseChange = await _client.PatchJsonAsync(string.Format(API_URL_STORY, roomId, storyId), bodyChange);

            Assert.That(responseChange.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsChange = responseChange.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsChange.ContainsKey("message"));
        }
        [Test]
        public async Task GetStory_ValidValues_Ok()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            var responseGet = await _client.GetAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.AreEqual("My story", story.Title);
            Assert.Greater(story.Identifier, 0);
        }
        [TestCase(12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, HttpStatusCode.NotFound)]
        public async Task GetStory_InvalidValues_FailWithObject(int? roomValue, int? storyValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var roomId = roomValue ?? room.Identifier;
            var story = CreateStory(room.Identifier).Result;
            var storyId = storyValue ?? story.Identifier;
            var responseGet = await _client.GetAsync(string.Format(API_URL_STORY, roomId, storyId));

            Assert.That(responseGet.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsGet = responseGet.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsGet.ContainsKey("message"));
        }
        [Test]
        public async Task DeleteStory_ValidValues_True()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var responseGet = await _client.GetAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseDelete = await _client.DeleteAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier));
            
            Assert.That(responseDelete.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsDelete = responseDelete.Content.ReadAsStringAsync().Result;

            Assert.AreEqual("true", resultsDelete);

            responseGet = await _client.GetAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier));

            Assert.That(responseGet.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        [TestCase(12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, HttpStatusCode.NotFound)]
        public async Task DeleteStory_InvalidValues_FailWithObject(int? roomValue, int? storyValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var roomId = roomValue ?? room.Identifier;
            var story = CreateStory(room.Identifier).Result;
            var storyId = storyValue ?? story.Identifier;
            var responseGet = await _client.DeleteAsync(string.Format(API_URL_STORY, roomId, storyId));

            Assert.That(responseGet.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsGet = responseGet.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsGet.ContainsKey("message"));
        }

        private async Task<JustPlanIt.Models.Room> CreateRoom()
        {
            var bodyCreate = new
            {
                Room = "My room",
                Name = "My name"
            };
            var responseCreate = await _client.PostJsonAsync(API_URL_ROOM, bodyCreate);

            Assert.That(responseCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var resultsCreate = responseCreate.Content.ReadJson<JustPlanIt.Models.Room>();

            return resultsCreate;
        }
        private async Task<JustPlanIt.Models.Story> CreateStory(int roomId)
        {
            var bodyCreate = new
            {
                Title = "My story"
            };
            var responseCreate = await _client.PostJsonAsync(string.Format(API_URL_STORY, roomId, null), bodyCreate);

            Assert.That(responseCreate.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var resultsCreate = responseCreate.Content.ReadJson<JustPlanIt.Models.Story>();

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