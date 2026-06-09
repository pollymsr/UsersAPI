namespace FiapCloudGames.Events;

public record UserCreatedEvent(Guid UserId, string Name, string Email);
