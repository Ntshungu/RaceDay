using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.DTOs;
using RaceDay.API.Services;

namespace RaceDay.Tests;

public static class TestHelpers
{
    public static RaceDayDbContext NewDb(string name)
    {
        var opts = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new RaceDayDbContext(opts);
    }

    public static UserResponse JarredOrganiser()
        => new(1, "Jarred Naidoo", "jarred@raceday.co.za", "Organiser", "0821234567", null);

    public static UserResponse JarredParticipant()
        => new(2, "Jarred Naidoo", "jarred.runner@raceday.co.za", "Participant", "0837654321", null);

    /// <summary>Wires a fake session into a controller's ControllerContext.</summary>
    public static T WithSession<T>(this T controller, UserResponse? user) where T : ControllerBase
    {
        var http = new DefaultHttpContext();
        var session = new FakeSession();
        if (user is not null) session.SetUser(user);

        http.Features.Set<ISessionFeature>(new FakeSessionFeature { Session = session });
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        return controller;
    }

    private class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken ct = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value)
        {
            var ok = _store.TryGetValue(key, out var v);
            value = v!;
            return ok;
        }
    }

    private class FakeSessionFeature : ISessionFeature
    {
        public ISession Session { get; set; } = null!;
    }
}