namespace pulumi_yoyo.api;

public record ExportStackStateResponseData
(
    int Version,
    DeploymentData Deployment
);
