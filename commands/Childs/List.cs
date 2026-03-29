using CommandSystem;
using CustomRolesReConstruct.Events;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using System;
using System.Linq;
using System.Text;

namespace CustomRolesReConstruct.commands.Childs;


[CommandHandler(typeof(Parent))]
public class List : ICommand
{
    public string Command => "list";
    public string[] Aliases => new[] { "cilist", "cil" };
    public string Description => "Lists all registered CustomRoles.";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player executor = Player.Get(sender);

        if (executor != null && !executor.HasPermissions("customroles.list"))
        {
            response = Main.Instance.Config.DontHaveAccess;
            return false;
        }

        var items = CustomRoleHandler.Registered;

        if (!items.Any())
        {
            response = "There are no CustomRoles registered.";
            return false;
        }

        StringBuilder sb = new();
        sb.AppendLine("Registered CustomRoles:");

        foreach (var item in items)
        {
            sb.AppendLine($"- {item.Name} (ID: {item.Id}, Role: {item.BaseRole})");
        }

        response = sb.ToString();
        return true;
    }
}
