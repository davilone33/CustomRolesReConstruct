using LabApi.Features.Wrappers;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CustomRolesCrimsonBreach.API.CustomRole;

public abstract class CustomAbility
{
    public static HashSet<CustomAbility> Registered { get; } = new();
    private static readonly Dictionary<uint, CustomAbility?> idLookupTable = new();
    private static Dictionary<string, CustomAbility?> stringLookupTable = new();
    public static event EventHandler<PlayerUseHability>? PlayerUsedAbility;
    Dictionary<uint, float> lastUseTime;

    public abstract uint ID { get; }
    public abstract string Name { get; }
    public abstract float Cooldown { get; }
    public abstract string Description { get; }

    public abstract bool NeedCooldown { get; }

    public Dictionary<uint, float> RealCooldown = new Dictionary<uint, float>();
    public virtual bool TryRegister()
    {
        if (Registered.Any(c => c.ID == ID)) return false;

        lock (Registered)
        {
            Registered.Add(this);
        }

        idLookupTable.Add(ID, this);

        if (!stringLookupTable.ContainsKey(Name))
            stringLookupTable.Add(Name, this);

        return true;
    }

    public virtual bool TryUnregister()
    {
        Registered.Remove(this);
        idLookupTable.Remove(ID);
        stringLookupTable.Remove(Name);
        return Registered.Remove(this);
    }

    public static IEnumerable<CustomAbility> RegisterSkills()
    {
        List<CustomAbility> items = new();
        Assembly assembly = Assembly.GetExecutingAssembly();

        foreach (Type type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(CustomAbility)) || type.IsAbstract)
                continue;

            if (Registered.Any(r => r.GetType() == type))
                continue;

            try
            {
                var instance = (CustomAbility)Activator.CreateInstance(type)!;
                if (instance.TryRegister())
                    items.Add(instance);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating ability {type.Name}: {e}");
            }
        }

        return items;
    }

    public static IEnumerable<CustomAbility> UnRegisterSkills()
    {
        var items = Registered.ToList();
        foreach (var item in items)
            item.TryUnregister();
        return items;
    }

    public static CustomAbility? Get(string name) => stringLookupTable.TryGetValue(name, out var ability) ? ability : null;
    public static CustomAbility? Get(uint ID) => idLookupTable.TryGetValue(ID, out var ability) ? ability : null;
    public static CustomAbility? Get(CustomAbility ability) => Registered.FirstOrDefault(a => a.Equals(ability));


    public virtual void OnUseWithCooldown(Player player)
    {
        uint id = player.NetworkId;

        if (!NeedCooldown)
        {
            OnUse(player);
            return;
        }

        if (!lastUseTime.TryGetValue(id, out float lastTime))
        {
            lastUseTime[id] = Time.time;
            OnUse(player);
            return;
        }

        float timePassed = Time.time - lastTime;
        float remaining = Cooldown - timePassed;

        if (remaining <= 0)
        {
            lastUseTime[id] = Time.time;
            OnUse(player);
        }
        else
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.CeilToInt(remaining % 60f);

            player.SendHint(
                Main.Instance.Config.HabilityCooldownMessage
                    .Replace("MINUTES", minutes.ToString())
                    .Replace("SECONDS", seconds.ToString()),
                5f
            );
        }
    }

    /*private IEnumerator<float> CooldownCoroutine(Player player)
    {
        while (RealCooldown.ContainsKey(player.NetworkId) && RealCooldown[player.NetworkId] > 0)
        {
            RealCooldown[player.NetworkId] -= 1f;

            yield return Timing.WaitForSeconds(1f);
        }

        if (RealCooldown.ContainsKey(player.NetworkId))
            RealCooldown.Remove(player.NetworkId);

        player.SendHint(Main.Instance.Config.HabilityCooldownSuccesfull, 5f);
    }*/

    public virtual void OnUse(Player player)
    {
        var args = new PlayerUseHability(player, this);
        PlayerUsedAbility?.Invoke(this, args);

        if (!args.IsAllowed)
            return;
    }
}