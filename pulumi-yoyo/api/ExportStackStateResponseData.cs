namespace scan_pulumi;

public record ExportStackStateResponseData
(
    int Version,
    DeploymentData Deployment
);
