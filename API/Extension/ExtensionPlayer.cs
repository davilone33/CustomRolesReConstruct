using LabApi.Features.Wrappers;

namespace CustomRolesReConstruct.API.Extension;

public static class ExtensionPlayer
{
    public static bool IsCustomRole(this Player player)
    {
        var role = CustomRole.CustomRole.GetRole(player);
        return role != null;
    }
}