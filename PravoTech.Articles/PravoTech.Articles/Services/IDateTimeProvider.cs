namespace PravoTech.Articles.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
} 