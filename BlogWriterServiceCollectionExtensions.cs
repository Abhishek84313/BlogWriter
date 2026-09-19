using Azure.AI.Projects;
using Azure.Core;
using Microsoft.Agents.AI;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlogWriter;

public static class BlogWriterServiceCollectionExtensions
{
    public static IServiceCollection AddBlogWriterApplication(
        this IServiceCollection services,
        IConfiguration configuration,
        TokenCredential credential)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(credential);

        Uri foundryEndpoint = GetRequiredUri(configuration, "Foundry:ProjectEndpoint", "FOUNDRY_PROJECT_ENDPOINT");
        Uri cosmosEndpoint = GetRequiredUri(configuration, "Cosmos:Endpoint", "COSMOS_ENDPOINT");
        string cosmosDatabase = GetRequired(configuration, "Cosmos:DatabaseName", "COSMOS_DATABASE_NAME");
        string cosmosContainer = GetRequired(configuration, "Cosmos:ContainerName", "COSMOS_CONTAINER_NAME");
        long maxTokens = long.TryParse(
            configuration["Foundry:MaxTotalTokens"] ?? configuration["MAX_TOTAL_TOKENS"],
            out long configuredMaxTokens)
            ? configuredMaxTokens
            : 40000;

        services.AddSingleton(credential);
        services.AddSingleton(new CosmosClient(cosmosEndpoint.ToString(), credential));
        services.AddSingleton(sp => sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabase, cosmosContainer));
        services.AddSingleton(new AIProjectClient(foundryEndpoint, credential));
        services.AddSingleton(TokenCapChatClient.CreateSharedFactory(maxTokens));

        services.AddSingleton<IBloggerAgent>(sp => new BloggerAgent(
            BuildAgent(sp, foundryEndpoint, GetName(configuration, "Blogger", "BLOGGER_AGENT_NAME")),
            sp.GetRequiredService<ILogger<BloggerAgent>>()));
        services.AddSingleton<IResearcherAgent>(sp => new ResearcherAgent(
            BuildAgent(sp, foundryEndpoint, GetName(configuration, "Researcher", "RESEARCHER_AGENT_NAME")),
            sp.GetRequiredService<ILogger<ResearcherAgent>>()));
        services.AddSingleton<IAuthorAgent>(sp => new AuthorAgent(
            BuildAgent(sp, foundryEndpoint, GetName(configuration, "Author", "AUTHOR_AGENT_NAME")),
            sp.GetRequiredService<ILogger<AuthorAgent>>()));
        services.AddSingleton<IReviewerAgent>(sp => new ReviewerAgent(
            BuildAgent(sp, foundryEndpoint, GetName(configuration, "Reviewer", "REVIEWER_AGENT_NAME")),
            sp.GetRequiredService<ILogger<ReviewerAgent>>()));
        services.AddSingleton<IBlogWorkflow, BlogWorkflow>();
        services.AddScoped<IBlogSessionStore, CosmosBlogSessionStore>();
        services.AddScoped<IBlogWriterSessionService, BlogWriterSessionService>();

        return services;
    }

    private static AIAgent BuildAgent(IServiceProvider services, Uri projectEndpoint, string agentName)
    {
        Uri agentEndpoint = new($"{projectEndpoint.AbsoluteUri.TrimEnd('/')}/agents/{agentName}/endpoint/protocols/openai");
        return services.GetRequiredService<AIProjectClient>().AsAIAgent(
            agentEndpoint,
            tools: null,
            clientFactory: services.GetRequiredService<Func<IChatClient, IChatClient>>(),
            services: null);
    }

    private static string GetName(IConfiguration configuration, string defaultName, string environmentKey) =>
        configuration[$"Foundry:{defaultName}AgentName"] ?? configuration[environmentKey] ?? defaultName;

    private static Uri GetRequiredUri(IConfiguration configuration, string primaryKey, string fallbackKey) =>
        new(GetRequired(configuration, primaryKey, fallbackKey));

    private static string GetRequired(IConfiguration configuration, string primaryKey, string fallbackKey) =>
        configuration[primaryKey] ?? configuration[fallbackKey] ??
        throw new InvalidOperationException($"Missing configuration value '{primaryKey}'.");
}
