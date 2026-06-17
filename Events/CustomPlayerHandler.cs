using CustomRolesReConstruct.API.CustomRole;
using CustomRolesReConstruct.API.Extension;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CustomRolesReConstruct.Events;

public class CustomPlayerHandler
{
    public readonly Dictionary<string, CustomRole> _assignedRoles = new();
    private readonly Dictionary<Type, int> _roleCounts = new();
    private static readonly Random _rng = new Random();

    public void OnKillDeath(PlayerDeathEventArgs ev)
    {
        if (!ev.Player.IsCustomRole()) return;

        CustomRole.GetRole(ev.Player)?.RemoveRole(ev.Player);
    }

    public void OnSpawned(PlayerSpawnedEventArgs ev)
    {
        if (ev.Player == null || ev.Role == null) return;
        if (CustomRole.GetRole(ev.Player) != null) return;

        var candidates = CustomRoleHandler.Registered
            .Where(role => role.BaseRole == ev.Role.RoleTypeId &&
                           (role.SpawnNumber == 0 || CustomRole.CountAssigned(role.GetType()) < role.SpawnNumber))
            .ToList();

        if (!candidates.Any()) return;

        float totalCustomWeight = candidates.Sum(r => r.SpawnPercentage);
        float defaultWeight = Main.Instance.Config.DefaultHumanSpawnChance;
        float totalWeight = totalCustomWeight + defaultWeight;

        float roll = (float)_rng.NextDouble() * totalWeight;

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
                Logger.Info($"Assigning {role.Name} to {ev.Player.Nickname}");
                role.AddRole(ev.Player);
                break;
            }
        }
    }
    public void OnChangedRole(PlayerChangedRoleEventArgs ev)
    {
        if (ev.Player == null) return;

        var role = CustomRole.GetRole(ev.Player);
        if (role == null) return;

        if (ev.NewRole.RoleTypeId != role.BaseRole && !role.KeepRoleOnEscape)
        {
            role.RemoveRole(ev.Player);
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

    public void OnLeft(PlayerLeftEventArgs ev)
    {
        if (ev.Player == null) return;

        CustomRole.GetRole(ev.Player)?.RemoveRole(ev.Player);
    }
}
