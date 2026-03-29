using LabApi.Features.Wrappers;
using System;

namespace CustomRolesReConstruct.API.CustomRole;

public class PlayerUseHability : EventArgs
{
    public Player Player { get; }
    public bool IsAllowed { get; set; } = true;
    public CustomAbility customAbility { get; set; }

    public PlayerUseHability(Player player, CustomAbility customAbility)
    {
        Player = player;
        this.customAbility = customAbility;
    }
}
