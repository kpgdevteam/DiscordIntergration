using System.Reflection;
using DiscordIntegration.API.Commands;
using DiscordIntegration.Enums;
using InventorySystem.Items;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using PlayerStatsSystem;
using PluginAPI.Core;
using Scp914;
using UnityEngine;

namespace DiscordIntegration;

public static class Handlers
{
    private const int RecontainmentDamageTypeID = 27;
    private const int WarheadDamageTypeID = 2;
    private const int MicroHidTypeID = 18;
    private const int GrenadeTypeID = 19;
    private const int Scp018TypeID = 38;
    private const int DisruptorTypeID = 46;

    private static readonly Dictionary<string, int> FirearmDamageTypeIDs = new()
    {
        { "GunCOM15", 12 },
        { "GunE11SR", 14 },
        { "GunLogicer", 16 },
        { "GunCOM18", 32 },
        { "GunAK", 33 },
        { "GunShotgun", 34 },
        { "GunCrossvec", 35 },
        { "GunFSP9", 36 },
        { "MicroHID", 18 },
        { "GunRevolver", 31 },
        { "MolecularDisruptor", 45 },
        { "GunCom45", 50 }
    };

    public static readonly Dictionary<DeathTranslation, int> UniversalDamageTypeIDs = new();

    private static readonly Dictionary<string, int> RoleDamageTypeIDs = new()
    {
        { "Scp173", 24 },
        { "Scp106", 23 },
        { "Scp049", 20 },
        { "Scp096", 22 },
        { "Scp0492", 21 },
        { "Scp939", 25 },
    };

    private static readonly Dictionary<string, int> Scp096DamageTypeIDs = new()
    {
        { "GateKill", 40 },
        { "Slap", 22 },
        { "Charge", 39 }
    };

    private static readonly Dictionary<string, int> Scp939DamageTypeIDs = new()
    {
        { "None", 46 },
        { "Claw", 47 },
        { "LungeTarget", 48 },
        { "LungeSecondary", 49 }
    };

    public static bool IsWeapon(this ItemBase item) =>
        item.ItemTypeId switch
        {
            ItemType.GunCrossvec or ItemType.GunLogicer or ItemType.GunRevolver or ItemType.GunShotgun or ItemType.GunAK or ItemType.GunCOM15 or ItemType.GunCOM18 or ItemType.GunE11SR 
                or ItemType.GunFSP9 or ItemType.ParticleDisruptor or ItemType.MicroHID => true,
            _ => false
        };

    public static int ToId(this DamageHandlerBase damageHandler)
    {
        switch (damageHandler)
        {
            case RecontainmentDamageHandler _:
                return RecontainmentDamageTypeID;
            case MicroHidDamageHandler _:
                return MicroHidTypeID;
            case ExplosionDamageHandler _:
                return GrenadeTypeID;
            case WarheadDamageHandler _:
                return WarheadDamageTypeID;
            case Scp018DamageHandler _:
                return Scp018TypeID;
            case DisruptorDamageHandler _:
                return DisruptorTypeID;
            case FirearmDamageHandler firearmDamageHandler:
            {
                var id = firearmDamageHandler.WeaponType.ToString();

                return FirearmDamageTypeIDs.TryGetValue(id, out var output) ? output : -1;
            }
            case UniversalDamageHandler universalDamageHandler:
            {
                var id = universalDamageHandler.TranslationId;
                if (!DeathTranslations.TranslationsById.TryGetValue(id, out var translation)) return -1;

                return UniversalDamageTypeIDs.TryGetValue(translation, out var output) ? output : -1;
            }
            case ScpDamageHandler scpDamageHandler:
            {
                var id = scpDamageHandler.Attacker.Role.ToString();

                return RoleDamageTypeIDs.TryGetValue(id, out var output) ? output : -1;
            }
            default:
                return -1;
        }
    }

    public static float Amount(this DamageHandlerBase damageHandler) =>
        damageHandler switch
        {
            RecontainmentDamageHandler handler => handler.Damage,
            MicroHidDamageHandler handler => handler.Damage,
            ExplosionDamageHandler handler => handler.Damage,
            WarheadDamageHandler handler => handler.Damage,
            Scp018DamageHandler handler => handler.Damage,
            DisruptorDamageHandler handler => handler.Damage,
            FirearmDamageHandler firearmDamageHandler => firearmDamageHandler.Damage,
            UniversalDamageHandler universalDamageHandler => universalDamageHandler.Damage,
            ScpDamageHandler scpDamageHandler => scpDamageHandler.Damage,
            _ => -1
        };

    public static string DamageType(this int id)
    {
        switch (id)
        {
            case RecontainmentDamageTypeID:
                return "Recontainment";
            case WarheadDamageTypeID:
                return "Warhead";
            case MicroHidTypeID:
                return "Micro";
            case GrenadeTypeID:
                return "Grenade";
            case Scp018TypeID:
                return "Balls";
            case DisruptorTypeID:
                return "Disruptor";
            default:
                if (FirearmDamageTypeIDs.ContainsValue(id))
                    return FirearmDamageTypeIDs.First(p => p.Value == id).Key;
                if (UniversalDamageTypeIDs.ContainsValue(id))
                    return UniversalDamageTypeIDs.First(p => p.Value == id).Key.ToString();
                if (RoleDamageTypeIDs.ContainsValue(id))
                    return RoleDamageTypeIDs.First(p => p.Value == id).Key;
                if (Scp096DamageTypeIDs.ContainsValue(id))
                    return Scp096DamageTypeIDs.First(p => p.Value == id).Key;
                return Scp939DamageTypeIDs.ContainsValue(id) ? Scp939DamageTypeIDs.First(p => p.Value == id).Key : "Unknown";
        }
    }
    
    public static Team Team(this Player player) => player.ReferenceHub.GetTeam();
        
    public static Side Side(this Player player) => player.Team() switch
    {
        PlayerRoles.Team.SCPs => Enums.Side.Scp,
        PlayerRoles.Team.FoundationForces or PlayerRoles.Team.Scientists => Enums.Side.Mtf,
        PlayerRoles.Team.ChaosInsurgency or PlayerRoles.Team.ClassD => Enums.Side.ChaosInsurgency,
        PlayerRoles.Team.OtherAlive => Enums.Side.Tutorial,
        _ => Enums.Side.None,
    };
    
    public static void RemoteAdminMessage(this Player player, string message, bool success = true, string pluginName = null) => player.ReferenceHub.queryProcessor._sender.RaReply((pluginName ?? Assembly.GetCallingAssembly().GetName().Name) + "#" + message, success, true, string.Empty);

    public static UserGroup GetGroup(this Player player) => player.ReferenceHub.serverRoles.Group;

    public static void SetGroup(this Player player, UserGroup group) => player.ReferenceHub.serverRoles.SetGroup(group, false);
    
    public static string GetRankName(this Player player) => player.ReferenceHub.serverRoles.Network_myText;
    public static void SetRankName(this Player player, string name) => player.ReferenceHub.serverRoles.SetText(name);
    
    public static string GetRankColor(this Player player) => player.ReferenceHub.serverRoles.Network_myColor;
    public static void SetRankColor(this Player player, string color) => player.ReferenceHub.serverRoles.SetColor(color);
    
    public static bool IsBadgeHidden(this Player player) => !string.IsNullOrEmpty(player.ReferenceHub.serverRoles.HiddenBadge);

    public static void SetBadgeHidden(this Player player, bool state)
    {
        if (state)
            player.ReferenceHub.characterClassManager.UserCode_CmdRequestHideTag();
    }
    
    public static Scp914KnobSetting GetNobSetting() => Scp914Controller.Singleton.Network_knobSetting;
    public static Scp914KnobSetting SetNobSetting(Scp914KnobSetting state) => Scp914Controller.Singleton.Network_knobSetting = state;
    
    public static RemoteAdmin.PlayerCommandSender GetSender(this Player player) => player.ReferenceHub.queryProcessor._sender;
    
    public static double GetServerTps() => Math.Round(1f / Time.smoothDeltaTime);
}