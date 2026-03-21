using CustomRolesCrimsonBreach.API.CustomRole;
using CustomRolesCrimsonBreach.API.Extension;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CustomRolesCrimsonBreach.Events;

public class CustomPlayerHandler
{
    private readonly Dictionary<uint, CustomRole> _assignedRoles = new();
    private readonly Dictionary<Type, int> _roleCounts = new();

    public void OnRoundEnd(RoundEndedEventArgs ev) 
    {
        RoleManager.Reset();
    }

    public void OnRoundStart()
    {
        RoleManager.Reset();
    }


    public void OnKillDeath(PlayerDeathEventArgs ev)
    {
        if (!ev.Player.IsCustomRole()) return;

        if (!_assignedRoles.TryGetValue(ev.Player.NetworkId, out var role))
            return;

        if (_roleCounts.TryGetValue(role.GetType(), out int count))
        {
            _roleCounts[role.GetType()] = Math.Max(0, count - 1);
        }

        RoleManager.RemoveRole(ev.Player);
    }

    public void OnSpawned(PlayerSpawnedEventArgs ev)
    {
        var role = RoleManager.GetRandomRole(ev.Role.RoleTypeId);

        if (role != null)
        {
            RoleManager.AssignRole(ev.Player, role, out _);
        }
    }

    public void OnChangedRole(PlayerChangedRoleEventArgs ev)
    {
        if (ev.Player == null) return;

        if (!_assignedRoles.TryGetValue(ev.Player.NetworkId, out var role))
            return;

        if (!role.KeepRoleOnEscape)
        {
            RoleManager.RemoveRole(ev.Player);
        }
    }
}
