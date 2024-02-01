namespace scan_pulumi;

public record StackListResponseData
(
    List<StackData> Stacks,
    string ContinuationToken
);