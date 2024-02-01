namespace pulumi_yoyo.api;

public record StackOperation(
    string Kind,
    string Author,
    long Started
)
{
    public DateTime StartedDateTime => new DateTime(1970, 1, 1).AddSeconds(Started);

}