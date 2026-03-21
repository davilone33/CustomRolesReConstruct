using CustomRolesCrimsonBreach;
using CustomRolesCrimsonBreach.API.CustomRole;
using CustomRolesCrimsonBreach.Events;
using LabApi.Features.Wrappers;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

public static class RoleManager
{
    private static readonly Dictionary<uint, CustomRole> _playerRoles = new();
    private static readonly Dictionary<Type, int> _roleCounts = new();

    public static bool AssignRole(Player player, CustomRole role, out string reason)
    {
        reason = string.Empty;

        if (player == null)
        {
            reason = "Player is null";
            return false;
        }

        uint id = player.NetworkId;

        if (_playerRoles.ContainsKey(id))
        {
            reason = "Player already has a role";
            return false;
        }

        if (role.SpawnNumber > 0 &&
            _roleCounts.TryGetValue(role.GetType(), out int count) &&
            count >= role.SpawnNumber)
        {
            reason = "Spawn limit reached";
            return false;
        }

        _playerRoles[id] = role;

        if (_roleCounts.ContainsKey(role.GetType()))
            _roleCounts[role.GetType()]++;
        else
            _roleCounts[role.GetType()] = 1;

        role.AddRole(player);

        return true;
    }

    public static bool RemoveRole(Player player)
    {
        if (player == null) return false;

        uint id = player.NetworkId;

        if (!_playerRoles.TryGetValue(id, out var role))
            return false;

        role.RemoveRole(player);

        _playerRoles.Remove(id);

        if (_roleCounts.TryGetValue(role.GetType(), out int count))
        {
            _roleCounts[role.GetType()] = Math.Max(0, count - 1);
        }

        return true;
    }

    public static bool HasRole(Player player)
    {
        return player != null && _playerRoles.ContainsKey(player.NetworkId);
    }

    public static CustomRole? GetRole(Player player)
    {
        if (player == null) return null;
        return _playerRoles.TryGetValue(player.NetworkId, out var role) ? role : null;
    }

    public static void Reset()
    {
        _playerRoles.Clear();
        _roleCounts.Clear();
    }

    public static CustomRole? GetRandomRole(RoleTypeId baseRole)
    {
        var candidates = CustomRoleHandler.Registered
            .Where(role =>
                role.BaseRole == baseRole &&
                (role.SpawnNumber <= 0 ||
                 !_roleCounts.TryGetValue(role.GetType(), out int count) ||
                 count < role.SpawnNumber)
            )
            .ToList();

        if (candidates.Count == 0)
            return null;

        float totalCustomWeight = candidates.Sum(r => r.SpawnPercentage);
        float defaultWeight = Main.Instance.Config.DefaultHumanSpawnChance;
        float totalWeight = totalCustomWeight + defaultWeight;

        if (totalWeight <= 0f)
            return null;

        float roll = UnityEngine.Random.value * totalWeight;

        if (roll > totalCustomWeight)
            return null;

        float cumulative = 0f;

        foreach (var role in candidates)
        {
            cumulative += role.SpawnPercentage;

            if (roll <= cumulative)
                return role;
        }

        return null;
    }
}