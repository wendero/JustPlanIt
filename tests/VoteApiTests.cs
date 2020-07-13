using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using JustPlanIt.Classes;
using JustPlanIt.Models;
using static JustPlanIt.Controllers.VoteController;

namespace JustPlanIt.Tests
{
    public class VoteApiTests
    {
        private WebApplicationFactory<JustPlanIt.Startup> _factory;
        private HttpClient _client;
        private const string API_URL_ROOM = "/api/room/";
        private const string API_URL_STORY = "/api/room/{0}/story/{1}";
        private const string API_URL_VOTE = "/api/room/{0}/vote";

        [OneTimeSetUp]
        public void SetUp()
        {
            _factory = new WebApplicationFactory<JustPlanIt.Startup>();
            _client = _factory.CreateClient();
        }
        [Test]
        public void StartVoting_ValidValues_Ok()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            Assert.AreEqual(Models.StoryStatus.Voting, votingStory.Status);
        }
        [TestCase(12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, HttpStatusCode.NotFound)]
        public async Task Start_InvalidValues_FailWithObject(int? roomValue, int? storyValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            var roomId = roomValue ?? room.Identifier;
            var storyId = storyValue ?? story.Identifier;

            var responseStart = await _client.PostAsync(string.Format($"{API_URL_VOTE}/{storyId}/start", roomId), null);

            Assert.That(responseStart.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsStart = responseStart.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsStart.ContainsKey("message"));
        }
        [Test]
        public async Task Vote_ValidValues_Ok()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            var bodyVote = new
            {
                MemberId = room.Members.First().Identifier,
                Points = 13
            };

            var responseVote = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/{story.Identifier}", room.Identifier), bodyVote);

            Assert.That(responseVote.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsVote = responseVote.Content.ReadJson<Member>();
            Assert.AreEqual(bodyVote.MemberId, resultsVote.Identifier);

            var responseGet = await _client.GetAsync(string.Format(API_URL_STORY, room.Identifier, story.Identifier));
            var resultsGet = responseGet.Content.ReadJson<Story>();

            Assert.IsNotEmpty(resultsGet.Votes);
            Assert.AreEqual(resultsGet.Votes.First().Key, bodyVote.MemberId.ToString());
            Assert.AreEqual(resultsGet.Votes.First().Value, bodyVote.Points);
        }
        [TestCase(12346, null, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, null, 12346, HttpStatusCode.NotFound)]
        public async Task Vote_InvalidValues_FailWithObject(int? roomValue, int? storyValue, int? memberValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            var roomId = roomValue ?? room.Identifier;
            var storyId = storyValue ?? story.Identifier;
            var memberId = memberValue ?? room.Members.First().Identifier;

            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            var bodyVote = new
            {
                MemberId = memberId,
                Points = 13
            };

            var responseVote = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/{storyId}", roomId), bodyVote);

            Assert.That(responseVote.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsVote = responseVote.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsVote.ContainsKey("message"));
        }

        [Test]
        public async Task Stop_ValidValues_Accepted()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            var bodyVote = new
            {
                MemberId = room.Members.First().Identifier,
                Points = 13
            };

            var responseVote = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/{story.Identifier}", room.Identifier), bodyVote);

            Assert.That(responseVote.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseStop = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/stop", room.Identifier), null);

            Assert.That(responseStop.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsStop = responseStop.Content.ReadJson<ResultsApiModel>();

            Assert.AreEqual(story.Identifier, resultsStop.Story.Identifier);
            Assert.AreEqual(StoryStatus.ShowingResults, resultsStop.Story.Status);
            Assert.AreEqual(13, resultsStop.Average);
            Assert.AreEqual(13, resultsStop.Maximum);
            Assert.AreEqual(13, resultsStop.Minimum);
            Assert.AreEqual(bodyVote.MemberId, resultsStop.MaximumMembers.First().Identifier);
        }
        [Test]
        public async Task Stop_InvalidRoom_FailWithObject()
        {
            var responseStop = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/stop", "12346"), null);

            Assert.That(responseStop.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var resultsVote = responseStop.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsVote.ContainsKey("message"));
            Assert.True(resultsVote["message"].Contains("session"));
        }
        [Test]
        public async Task Stop_VotingNotStarted_FailWithObject()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            var responseStop = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/stop", room.Identifier), null);

            Assert.That(responseStop.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var resultsVote = responseStop.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsVote.ContainsKey("message"));
            Assert.True(resultsVote["message"].Contains("running voting"));
        }
        [Test]
        public async Task Stop_NoVotes_FailWithObject()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            var responseStop = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/stop", room.Identifier), null);

            Assert.That(responseStop.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var resultsVote = responseStop.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsVote.ContainsKey("message"));
            Assert.True(resultsVote["message"].Contains("voted yet"));
        }
        [Test]
        public async Task ShowResults_ValidValues_Ok()
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;
            var votingStory = StartVoting(room.Identifier, story.Identifier).Result;

            var bodyVote = new
            {
                MemberId = room.Members.First().Identifier,
                Points = 13
            };

            var responseVote = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/{story.Identifier}", room.Identifier), bodyVote);

            Assert.That(responseVote.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseStop = await _client.PostJsonAsync(string.Format($"{API_URL_VOTE}/stop", room.Identifier), null);

            Assert.That(responseStop.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseResults = await _client.GetAsync(string.Format($"{API_URL_VOTE}/{story.Identifier}/results", room.Identifier));

            var resultsResults = responseResults.Content.ReadJson<ResultsApiModel>();

            Assert.AreEqual(story.Identifier, resultsResults.Story.Identifier);
            Assert.AreEqual(StoryStatus.ShowingResults, resultsResults.Story.Status);
            Assert.AreEqual(13, resultsResults.Average);
            Assert.AreEqual(13, resultsResults.Maximum);
            Assert.AreEqual(13, resultsResults.Minimum);
            Assert.AreEqual(bodyVote.MemberId, resultsResults.MaximumMembers.First().Identifier);
        }
        [TestCase(12346, null, HttpStatusCode.NotFound)]
        [TestCase(null, 12346, HttpStatusCode.NotFound)]
        public async Task ShowResults_InvalidValues_Ok(int? roomValue, int? storyValue, HttpStatusCode expectedHttpCode)
        {
            var room = CreateRoom().Result;
            var story = CreateStory(room.Identifier).Result;

            var roomId = roomValue ?? room.Identifier;
            var storyId = storyValue ?? story.Identifier;

            var responseResults = await _client.GetAsync(string.Format($"{API_URL_VOTE}/{storyId}/results", roomId));

            Assert.That(responseResults.StatusCode, Is.EqualTo(expectedHttpCode));

            var resultsResults = responseResults.Content.ReadJson<Dictionary<string, string>>();

            Assert.True(resultsResults.ContainsKey("message"));
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
        private async Task<JustPlanIt.Models.Story> StartVoting(int roomId, int storyId)
        {
            var responseStart = await _client.PostAsync(string.Format($"{API_URL_VOTE}/{storyId}/start", roomId), null);

            Assert.That(responseStart.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var resultsStart = responseStart.Content.ReadJson<Models.Story>();

            return resultsStart;
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }
    }
}