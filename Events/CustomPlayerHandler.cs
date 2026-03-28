using CustomRolesCrimsonBreach.API.CustomRole;
using CustomRolesCrimsonBreach.API.Extension;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CustomRolesCrimsonBreach.Events;

public class CustomPlayerHandler
{
    private readonly Dictionary<string, CustomRole> _assignedRoles = new();
    private readonly Dictionary<Type, int> _roleCounts = new();

    public void OnKillDeath(PlayerDeathEventArgs ev)
    {
        if (!ev.Player.IsCustomRole()) return;

        if (!_assignedRoles.TryGetValue(ev.Player.UserId, out var role))
            return;

        role.RemoveRole(ev.Player);
        _assignedRoles.Remove(ev.Player.UserId);
    }

    public void OnSpawned(PlayerSpawnedEventArgs ev)
    {
        if (ev.Player == null || ev.Role == null) return;

        if (_assignedRoles.ContainsKey(ev.Player.UserId)) return;

        var candidates = CustomRoleHandler.Registered
            .Where(role => role.BaseRole == ev.Role.RoleTypeId &&
                           (role.SpawnNumber == 0 || !_roleCounts.TryGetValue(role.GetType(), out var count) || count < role.SpawnNumber))
            .ToList();

        if (!candidates.Any()) return;

        float totalCustomWeight = candidates.Sum(r => r.SpawnPercentage);
        float defaultWeight = Main.Instance.Config.DefaultHumanSpawnChance;
        float totalWeight = totalCustomWeight + defaultWeight;

        float roll = (float)new Random().NextDouble() * totalWeight;

        if (roll > totalCustomWeight)
        {
            Logger.Debug($"Roll ({roll}) fell into the weight of 'Human by Default' ({defaultWeight}).", Main.Instance.Config.debug);
            return;
        }

        float cumulative = 0f;
        foreach (var role in candidates)
        {
            cumulative += role.SpawnPercentage;
            if (roll <= cumulative)
            {
                AssignCustomRole(ev.Player, role);
                break;
            }
        }
    }

    public void OnChangedRole(PlayerChangedRoleEventArgs ev)
    {
        if (ev.Player == null) return;

        if (!_assignedRoles.TryGetValue(ev.Player.UserId, out var role))
            return;

        if (ev.NewRole.RoleTypeId != role.BaseRole && !role.KeepRoleOnEscape)
        {
            role.RemoveRole(ev.Player);
            _assignedRoles.Remove(ev.Player.UserId);

            if (_roleCounts.TryGetValue(role.GetType(), out int count))
            {
                _roleCounts[role.GetType()] = Math.Max(0, count - 1);
            }
        }
    }

    private void AssignCustomRole(Player player, CustomRole role)
    {
        _assignedRoles[player.UserId] = role;

        var type = role.GetType();
        if (!_roleCounts.ContainsKey(type)) _roleCounts[type] = 0;
        _roleCounts[type]++;

        Logger.Info($"Assigning {role.Name} to {player.Nickname}");

        role.AddRole(player);
    }
}
