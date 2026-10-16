namespace RaceDay.API.DTOs;

public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string Role,
    string? PhoneNumber);

public record LoginDto(string Email, string Password);

public record ProfileUpdateDto(
    string FullName,
    string? PhoneNumber,
    string? ProfilePictureUrl);

public record EventDto(
    string Name,
    string? Description,
    DateTime EventDate,
    string Location,
    decimal Distance,
    string EventType,
    string? BannerImageUrl);

public record CategoryDto(string CategoryName, string? Description);

public record EnrolmentDto(int EventId, int CategoryId);

public record ResultDto(int EnrolmentId, TimeSpan FinishTime, int FinishingPosition);

public record UserResponse(
    int UserId,
    string FullName,
    string Email,
    string Role,
    string? PhoneNumber,
    string? ProfilePictureUrl);