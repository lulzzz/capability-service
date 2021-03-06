﻿using System;
using System.Net;
using System.Threading.Tasks;
using DFDS.CapabilityService.Tests.Builders;
using DFDS.CapabilityService.Tests.TestDoubles;
using DFDS.CapabilityService.WebApi.Application;
using DFDS.CapabilityService.WebApi.Domain.Exceptions;
using DFDS.CapabilityService.WebApi.Domain.Models;
using Xunit;

namespace DFDS.CapabilityService.Tests.Infrastructure.Api
{
    public class TestCapabilitiesRoute
    {
        [Fact]
        public async Task get_all_capabilities_returns_expected_status_code()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService())
                    .Build();

                var response = await client.GetAsync("api/v1/capabilities");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task get_all_capabilities_returns_expected_body_when_no_capabilities_available()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService())
                    .Build();

                var response = await client.GetAsync("api/v1/capabilities");

                Assert.Equal(
                    expected: "{\"items\":[]}",
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }

        [Fact]
        public async Task get_single_capability_returns_expected_status_code()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(subCapability))
                    .Build();

                var response = await client.GetAsync($"api/v1/capabilities/{subCapability.Id}");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task get_single_capability_without_any_members_returns_expected_body()
        {
            using (var builder = new HttpClientBuilder())
            {
                var stubCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(stubCapability))
                    .Build();

                var response = await client.GetAsync($"api/v1/capabilities/{stubCapability.Id}");

                Assert.Equal(
                    expected: $"{{\"id\":\"{stubCapability.Id}\",\"name\":\"{stubCapability.Name}\",\"description\":\"{stubCapability.Description}\",\"members\":[],\"contexts\":[]}}",
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }

        [Fact]
        public async Task get_single_capability_with_a_single_member_returns_expected_body()
        {
            using (var builder = new HttpClientBuilder())
            {
                var memberEmail = "foo@bar.com";

                var stubCapability = new CapabilityBuilder()
                    .WithMembers(memberEmail)
                    .Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(stubCapability))
                    .Build();

                var response = await client.GetAsync($"api/v1/capabilities/{stubCapability.Id}");

                Assert.Equal(
                    expected: $"{{\"id\":\"{stubCapability.Id}\",\"name\":\"{stubCapability.Name}\",\"description\":\"{stubCapability.Description}\",\"members\":[{{\"email\":\"{memberEmail}\"}}],\"contexts\":[]}}",
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }

        [Fact]
        public async Task get_single_capability_with_a_single_context_returns_expected_body()
        {
            using (var builder = new HttpClientBuilder())
            {
                var contextName = "foo@bar.com";
                var contextDescription = "wowsuchdescription";
                var contextGuid = Guid.NewGuid();
                var context = new Context(contextGuid, contextName);
                var stubCapability = new CapabilityBuilder()
                    .WithContexts(context)
                    .Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(stubCapability))
                    .Build();

                var response = await client.GetAsync($"api/v1/capabilities/{stubCapability.Id}");

                Assert.Equal(
                    expected: $"{{\"id\":\"{stubCapability.Id}\",\"name\":\"{stubCapability.Name}\",\"description\":\"{stubCapability.Description}\",\"members\":[],\"contexts\":[{{\"id\":\"{contextGuid}\",\"name\":\"{contextName}\"}}]}}",
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }
        
        [Fact]
        public async Task get_single_capability_returns_expected_status_code_when_capability_was_not_found()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService())
                    .Build();

                var unknownCapabilityId = Guid.Empty;
                var response = await client.GetAsync($"api/v1/capabilities/{unknownCapabilityId}");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task get_single_capability_returns_expected_status_code_when_input_capabilityid_is_invalid()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService())
                    .Build();

                var invalidCapabilityId = "im-not-a-valid-capability-id";
                var response = await client.GetAsync($"api/v1/capabilities/{invalidCapabilityId}");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task post_returns_expected_status_code_when_creating_a_new_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var dummyCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(dummyCapability))
                    .Build();

                var stubInput = "{\"name\":\"foo\"}";
                var response = await client.PostAsync("api/v1/capabilities", new JsonContent(stubInput));

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            }
        }

        [Fact]
        public async Task post_returns_expected_location_header_when_creating_a_new_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(subCapability))
                    .Build();

                var stubInput = "{\"name\":\"foo\"}";
                var response = await client.PostAsync("api/v1/capabilities", new JsonContent(stubInput));

                Assert.EndsWith(
                    expectedEndString: $"/api/v1/capabilities/{subCapability.Id}",
                    actualString: response.Headers.Location.ToString()
                );
            }
        }

        [Fact]
        public async Task post_returns_expected_body_when_creating_a_new_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(subCapability))
                    .Build();

                var stubInput = $"{{\"name\":\"{subCapability.Name}\"}}";
                var response = await client.PostAsync("api/v1/capabilities", new JsonContent(stubInput));

                Assert.Equal(
                    expected: $"{{\"id\":\"{subCapability.Id}\",\"name\":\"{subCapability.Name}\",\"description\":\"{subCapability.Description}\",\"members\":[],\"contexts\":[]}}",
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }

        [Fact]
        public async Task post_returns_badrequest_when_creating_a_new_capability_with_invalid_name()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder().Build();

                var errorBody = "message regarding bad name";
                var client = builder
                    .WithService<ICapabilityApplicationService>(new ErroneousCapabilityApplicationService(new CapabilityValidationException(errorBody)))
                    .Build();

                var stubInput = $"{{\"name\":\"{subCapability.Name}\"}}";
                var response = await client.PostAsync("api/v1/capabilities", new JsonContent(stubInput));

                var erroredMessageJSON = $"{{\"message\":\"{errorBody}\"}}";

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Equal(
                    expected: erroredMessageJSON,
                    actual: await response.Content.ReadAsStringAsync()
                );
            }
        }

        [Fact]
        public async Task post_returns_expected_status_code_when_adding_a_member_to_an_existing_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder().Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(subCapability))
                    .Build();

                var stubInput = "{\"email\":\"foo\"}";
                var response = await client.PostAsync($"api/v1/capabilities/{subCapability.Id}/members", new JsonContent(stubInput));

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task delete_returns_expected_status_code_when_deleting_an_existing_member_on_a_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var subCapability = new CapabilityBuilder()
                    .WithMembers("foo")
                    .Build();

                var client = builder
                    .WithService<ICapabilityApplicationService>(new StubCapabilityApplicationService(subCapability))
                    .Build();

                var response = await client.DeleteAsync($"api/v1/capabilities/{subCapability.Id}/members/foo");

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task delete_returns_expected_status_code_when_deleting_a_non_existing_member_on_a_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new ErroneousCapabilityApplicationService(new NotMemberOfCapabilityException()))
                    .Build();

                var dummyCapabilityId = "foo";
                var dummyMemberEmail = "bar";

                var response = await client.DeleteAsync($@"api/v1/capabilities/{dummyCapabilityId}/members/{dummyMemberEmail}");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task delete_returns_expected_status_code_when_deleting_a_member_on_a_non_existing_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new ErroneousCapabilityApplicationService(new CapabilityDoesNotExistException()))
                    .Build();

                var dummyCapabilityId = "foo";
                var dummyMemberEmail = "bar";

                var response = await client.DeleteAsync($@"api/v1/capabilities/{dummyCapabilityId}/members/{dummyMemberEmail}");

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Fact]
        public async Task post_returns_expected_status_code_when_adding_a_member_to_a_non_existing_capability()
        {
            using (var builder = new HttpClientBuilder())
            {
                var client = builder
                    .WithService<ICapabilityApplicationService>(new ErroneousCapabilityApplicationService(new CapabilityDoesNotExistException()))
                    .Build();

                var nonExistingCapabilityId = "foo";
                var dummyInput = "{}";
                var response = await client.PostAsync($"api/v1/capabilities/{nonExistingCapabilityId}/members", new JsonContent(dummyInput));

                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
        }
    }
}