namespace config;

public record EnvironmentConfig(
    string SubscriptionName,
    string? DefaultDirectoryForEnvironment
);
