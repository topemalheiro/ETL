using NUnit.Framework;
using TestFramework;
using Serilog;
using System.Net;

namespace API.AutomationTests.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class JsonPlaceholderApiTests
    {
        private ApiClient? _apiClient;
        private const string BaseUrl = "https://jsonplaceholder.typicode.com";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Configure Serilog for API test logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/api-test-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("=== API Tests Suite Started ===");
        }

        [SetUp]
        public void SetUp()
        {
            _apiClient = new ApiClient(BaseUrl, TimeSpan.FromSeconds(30));
            Log.Information("API Client initialized for {BaseUrl}", BaseUrl);
        }

        [TearDown]
        public void TearDown()
        {
            _apiClient?.Dispose();
            Log.Information("API Client disposed");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Log.Information("=== API Tests Suite Completed ===");
            Log.CloseAndFlush();
        }

        [Test]
        [Category("Smoke")]
        [Description("Verify API endpoint is accessible and returns expected status")]
        public async Task GetPosts_ShouldReturnSuccessStatus()
        {
            // Act
            var response = await _apiClient!.GetAsync<List<Post>>("/posts");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, "API should return success status");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Should return 200 OK");
                Assert.That(response.Data, Is.Not.Null, "Response data should not be null");
                Assert.That(response.Data!.Count, Is.GreaterThan(0), "Should return posts data");
            });

            Log.Information("✓ API endpoint accessible - returned {PostCount} posts", response.Data!.Count);
        }

        [Test]
        [Category("Functional")]
        [Description("Verify getting a specific post by ID returns correct data")]
        [TestCase(1)]
        [TestCase(50)]
        [TestCase(100)]
        public async Task GetPostById_ShouldReturnCorrectPost(int postId)
        {
            // Act
            var response = await _apiClient!.GetAsync<Post>($"/posts/{postId}");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, $"Should successfully get post {postId}");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Should return 200 OK");
                Assert.That(response.Data, Is.Not.Null, "Post data should not be null");
                Assert.That(response.Data!.Id, Is.EqualTo(postId), "Post ID should match requested ID");
                Assert.That(response.Data.Title, Is.Not.Empty, "Post should have a title");
                Assert.That(response.Data.Body, Is.Not.Empty, "Post should have a body");
                Assert.That(response.Data.UserId, Is.GreaterThan(0), "Post should have a valid user ID");
            });

            Log.Information("✓ Successfully retrieved post {PostId}: '{Title}'", postId, response.Data!.Title);
        }

        [Test]
        [Category("Functional")]
        [Description("Verify creating a new post returns expected response")]
        public async Task CreatePost_ShouldReturnNewPost()
        {
            // Arrange
            var newPost = new Post
            {
                Title = "Test Automation Post",
                Body = "This post was created by automated testing framework",
                UserId = 1
            };

            // Act
            var response = await _apiClient!.PostAsync<Post>("/posts", newPost);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, "Post creation should succeed");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Should return 201 Created");
                Assert.That(response.Data, Is.Not.Null, "Created post data should not be null");
                Assert.That(response.Data!.Title, Is.EqualTo(newPost.Title), "Title should match input");
                Assert.That(response.Data.Body, Is.EqualTo(newPost.Body), "Body should match input");
                Assert.That(response.Data.UserId, Is.EqualTo(newPost.UserId), "UserId should match input");
                Assert.That(response.Data.Id, Is.GreaterThan(0), "Should assign an ID to new post");
            });

            Log.Information("✓ Successfully created post with ID {PostId}", response.Data!.Id);
        }

        [Test]
        [Category("Functional")]
        [Description("Verify updating an existing post works correctly")]
        public async Task UpdatePost_ShouldReturnUpdatedPost()
        {
            // Arrange
            var postId = 1;
            var updatedPost = new Post
            {
                Id = postId,
                Title = "Updated Test Post",
                Body = "This post has been updated by automation tests",
                UserId = 1
            };

            // Act
            var response = await _apiClient!.PutAsync<Post>($"/posts/{postId}", updatedPost);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, "Post update should succeed");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Should return 200 OK");
                Assert.That(response.Data, Is.Not.Null, "Updated post data should not be null");
                Assert.That(response.Data!.Id, Is.EqualTo(postId), "Post ID should remain the same");
                Assert.That(response.Data.Title, Is.EqualTo(updatedPost.Title), "Title should be updated");
                Assert.That(response.Data.Body, Is.EqualTo(updatedPost.Body), "Body should be updated");
            });

            Log.Information("✓ Successfully updated post {PostId}", postId);
        }

        [Test]
        [Category("Functional")]
        [Description("Verify deleting a post returns expected response")]
        public async Task DeletePost_ShouldReturnSuccessStatus()
        {
            // Arrange
            var postId = 1;

            // Act
            var response = await _apiClient!.DeleteAsync<object>($"/posts/{postId}");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, "Post deletion should succeed");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Should return 200 OK");
            });

            Log.Information("✓ Successfully deleted post {PostId}", postId);
        }

        [Test]
        [Category("Negative")]
        [Description("Verify API handles non-existent resources appropriately")]
        public async Task GetNonExistentPost_ShouldReturn404()
        {
            // Arrange
            var nonExistentPostId = 99999;

            // Act
            var response = await _apiClient!.GetAsync<Post>($"/posts/{nonExistentPostId}");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.False, "Should not succeed for non-existent post");
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Should return 404 Not Found");
                Assert.That(response.Data, Is.Null, "Data should be null for non-existent resource");
                Assert.That(response.ErrorMessage, Is.Not.Null, "Should have error message");
            });

            Log.Information("✓ API correctly returned 404 for non-existent post {PostId}", nonExistentPostId);
        }

        [Test]
        [Category("Performance")]
        [Description("Verify API response time is within acceptable limits")]
        public async Task GetPosts_ShouldRespondWithinTimeLimit()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var response = await _apiClient!.GetAsync<List<Post>>("/posts");
            stopwatch.Stop();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccess, Is.True, "API call should succeed");
                Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(3000), 
                    "API should respond within 3 seconds");
            });

            Log.Information("✓ API responded in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }

        [Test]
        [Category("Data Validation")]
        [Description("Verify API returns data in expected format and structure")]
        public async Task GetPosts_ShouldReturnValidDataStructure()
        {
            // Act
            var response = await _apiClient!.GetAsync<List<Post>>("/posts");

            // Assert
            Assert.That(response.IsSuccess, Is.True, "API call should succeed");
            Assert.That(response.Data, Is.Not.Null, "Response data should not be null");

            var posts = response.Data!;
            Assert.That(posts.Count, Is.GreaterThan(50), "Should return substantial number of posts");

            // Validate first few posts have required fields
            foreach (var post in posts.Take(5))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(post.Id, Is.GreaterThan(0), $"Post {post.Id} should have valid ID");
                    Assert.That(post.UserId, Is.GreaterThan(0), $"Post {post.Id} should have valid UserId");
                    Assert.That(post.Title, Is.Not.Empty, $"Post {post.Id} should have title");
                    Assert.That(post.Body, Is.Not.Empty, $"Post {post.Id} should have body");
                });
            }

            Log.Information("✓ All posts have valid data structure");
        }

        [Test]
        [Category("Integration")]
        [Description("Verify full CRUD operations work together")]
        public async Task FullCrudOperations_ShouldWorkTogether()
        {
            // Create
            var newPost = new Post
            {
                Title = "Integration Test Post",
                Body = "Testing full CRUD operations",
                UserId = 1
            };

            var createResponse = await _apiClient!.PostAsync<Post>("/posts", newPost);
            Assert.That(createResponse.IsSuccess, Is.True, "Create operation should succeed");
            var createdPostId = createResponse.Data!.Id;

            // Read
            var readResponse = await _apiClient.GetAsync<Post>($"/posts/{createdPostId}");
            Assert.That(readResponse.IsSuccess, Is.True, "Read operation should succeed");
            Assert.That(readResponse.Data!.Title, Is.EqualTo(newPost.Title), "Read should return created data");

            // Update
            var updatedPost = new Post
            {
                Id = createdPostId,
                Title = "Updated Integration Test Post",
                Body = "Updated body content",
                UserId = 1
            };

            var updateResponse = await _apiClient.PutAsync<Post>($"/posts/{createdPostId}", updatedPost);
            Assert.That(updateResponse.IsSuccess, Is.True, "Update operation should succeed");
            Assert.That(updateResponse.Data!.Title, Is.EqualTo(updatedPost.Title), "Update should modify data");

            // Delete
            var deleteResponse = await _apiClient.DeleteAsync<object>($"/posts/{createdPostId}");
            Assert.That(deleteResponse.IsSuccess, Is.True, "Delete operation should succeed");

            Log.Information("✓ Full CRUD operations completed successfully for post {PostId}", createdPostId);
        }
    }

    // Data models for API testing
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }
} 