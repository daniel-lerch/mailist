namespace Mailist.SpamFilter;

public enum SpamCategory
{
    None = 0,
    Legitimate = 1,
    Irrelevant = 2,
    Dangerous = 3,
    NoTextContent = 4,
    ClassificationFailed = 5,
}
