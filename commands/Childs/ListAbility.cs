using CommandSystem;
using CustomRolesReConstruct.API.CustomRole;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using System;
using System.Linq;
using System.Text;

namespace CustomRolesReConstruct.commands.Child2;

[CommandHandler(typeof(Parent))]
public class ListAbility : ICommand
{
    public string Command => "abilityList";
    public string[] Aliases => new[] { "abl" };
    public string Description => "Lists all registered CustomAbilities.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {

        Player executor = Player.Get(sender);

        if (executor != null && !executor.HasPermissions("customroles.listability"))
        {
            response = Main.Instance.Config.DontHaveAccess;
            return false;
        }

        var items = CustomAbility.Registered;

        if (!items.Any())
        {
            response = "There are no custom items registered.";
            return false;
        }

        StringBuilder sb = new();
        sb.AppendLine("Registered Custom Ability:");

        foreach (var item in items)
        {
            sb.AppendLine($"- {item.Name} (ID: {item.ID})");
        }

        response = sb.ToString();
        return true;
    }
}
