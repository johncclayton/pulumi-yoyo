namespace pulumi_yoyo.api;

public record StackUpdateResponseData
(
    List<StackUpdateData> Updates,
    string ContinuationToken
);