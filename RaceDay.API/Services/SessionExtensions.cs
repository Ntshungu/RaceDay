using System.Text.Json;
using RaceDay.API.DTOs;

namespace RaceDay.API.Services;

public static class SessionExtensions
{
    public const string UserKey = "RaceDayUser";

    public static void SetUser(this ISession session, UserResponse user)
        => session.SetString(UserKey, JsonSerializer.Serialize(user));

    public static UserResponse? GetUser(this ISession session)
    {
        var json = session.GetString(UserKey);
        return string.IsNullOrEmpty(json)
            ? null
            : JsonSerializer.Deserialize<UserResponse>(json);
    }

    public static bool IsOrganiser(this ISession session)
        => session.GetUser()?.Role == "Organiser";

    public static bool IsParticipant(this ISession session)
        => session.GetUser()?.Role == "Participant";
}