namespace pulumi_yoyo.api;

public record StackListResponseData
(
    List<StackData> Stacks,
    string ContinuationToken
);