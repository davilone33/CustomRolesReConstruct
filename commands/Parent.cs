using CommandSystem;
using CustomRolesReConstruct.commands.Child2;
using CustomRolesReConstruct.commands.Childs;
using System;

namespace CustomRolesReConstruct.commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Parent : ParentCommand
{
    public override string Command => "CustomRoles";

    public override string[] Aliases => [ "CR" ];

    public override string Description => "parent command of CustomRoles";

    public override void LoadGeneratedCommands()
    {
        RegisterCommand(new Spawn());
        RegisterCommand(new List());
        RegisterCommand(new ListAbility());
    }

    protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        response = "Error, you need to put a subcommand: <list>, <spawn> or <abilityList>";
        return true;
    }
}
