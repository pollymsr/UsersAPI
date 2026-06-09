namespace UsersAPI.Integration.Events;

public record UserCreatedEvent(Guid UserId, string Name, string Email);
