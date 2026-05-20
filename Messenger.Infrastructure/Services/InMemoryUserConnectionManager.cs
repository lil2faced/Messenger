using System.Collections.Concurrent;
using Messenger.Application.Interfaces;

namespace Messenger.Infrastructure.Services;

/// <summary>
/// Хранит сопоставление UserId -> ConnectionId в памяти.
/// </summary>
public class InMemoryUserConnectionManager : IUserConnectionManager
{
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        _userConnections.AddOrUpdate(userId,
            _ => new HashSet<string> { connectionId },
            (_, set) => { set.Add(connectionId); return set; });
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (_userConnections.TryGetValue(userId, out var set))
        {
            set.Remove(connectionId);
            if (set.Count == 0)
                _userConnections.TryRemove(userId, out _);
        }
    }

    public IEnumerable<string> GetConnections(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var set))
            return set;
        return Enumerable.Empty<string>();
    }

    public bool IsOnline(Guid userId) => _userConnections.ContainsKey(userId);
}