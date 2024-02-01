namespace scan_pulumi;

public record StackUpdateResponseData
(
    List<StackUpdateData> Updates,
    string ContinuationToken
);