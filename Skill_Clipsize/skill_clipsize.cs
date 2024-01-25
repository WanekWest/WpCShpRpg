using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Modularity;
using WpCShpRpgCoreApi;

namespace Skill_Clipsize
{
    public class skill_clipsize : BasePlugin, IModulePlugin
    {
        public override string ModuleAuthor => "WanekWest";
        public override string ModuleName => "WpCssRpg | Clipsize+";
        public override string ModuleVersion => "v1.0";


        string UPGRADE_SHORTNAME = "clipsize";

        private IWpCShpRpgCoreApi _api = null!;

        Dictionary<string, string> WeaponList; // TODO: Получать из: addons/counterstrikesharp/configs/wpcshrpg/skill_configs/clipsize_weapons.cfg
        Dictionary<string, string> WeaponMaxClip; // TODO: Получать из: addons/counterstrikesharp/configs/wpcshrpg/skill_configs/max_clipsize_weapons.cfg

        public override void Load(bool hotReload)
        {
            if (hotReload)
                Server.PrintToConsole($"Данный скилл не поддерживает перезагрузку: wpcssrpg_upgrade_{UPGRADE_SHORTNAME}");
        }

        public override void Unload(bool hotReload)
        {
            if (_api != null && _api.UpgradeExists(UPGRADE_SHORTNAME))
            {
                _api.UnregisterUpgradeType(UPGRADE_SHORTNAME);
            }
        }

        public void LoadModule(IApiProvider provider)
        {
            _api = provider.Get<IWpCShpRpgCoreApi>();

            if (_api == null)
            {
                Server.PrintToConsole($"Ошибка загрузки модуля, данный навык не будет работать: {UPGRADE_SHORTNAME}");
                return;
            }

            _api.CssRpg_OnCoreLoaded += RpgCoreLoaded;
        }

        private void RpgCoreLoaded()
        {
            if (!_api.UpgradeExists(UPGRADE_SHORTNAME))
            {
                _api.RegisterUpgradeType("Armor regeneration", UPGRADE_SHORTNAME, "Deal additional damage on enemies.", 10, true, 5, 5, 10, 0, null, null);
            }
        }

        [GameEventHandler]
        public HookResult EventWeaponReload(EventWeaponReload @event, GameEventInfo info)
        {
            if (@event == null || _api == null)
                return HookResult.Continue;

            CCSPlayerController? player = @event?.Userid;
            if (player != null && player.IsValid && !player.IsBot && player.UserId != null && player.UserId >= 0)
            {
                var PlayerPawn = player.Pawn;
                if (PlayerPawn.IsValid && PlayerPawn.Value!.IsValid && PlayerPawn.Value!.ItemServices != null && PlayerPawn.Value!.WeaponServices != null && PlayerPawn.Value!.WeaponServices!.ActiveWeapon.IsValid)
                {
                    CCSPlayer_ItemServices cCSPlayer_ItemServices = new CCSPlayer_ItemServices(PlayerPawn.Value!.ItemServices!.Handle);
                    CCSPlayer_WeaponServices cCSPlayer_WeaponServices = new CCSPlayer_WeaponServices(PlayerPawn.Value!.WeaponServices!.Handle);

                    var ActiveWeapon = cCSPlayer_WeaponServices.ActiveWeapon.Value;
                    if (ActiveWeapon != null)
                        return HookResult.Continue;

                    int Client = (int)player.UserId;

                    string Weapon = ActiveWeapon.Globalname;
                    int fClipIncrease = 0;
                    if (WeaponList.TryGetValue(Weapon, out string? weapon))
                    {
                        fClipIncrease = Convert.ToInt32(weapon);
                    }

                    if (fClipIncrease == 0)
                        return HookResult.Continue;

                    if (!_api.CssRpg_IsEnabled())
                        return HookResult.Continue;

                    //if (!SMRPG_IsUpgradeEnabled(UPGRADE_SHORTNAME))
                    //    return HookResult.Continue;

                    // Are bots allowed to use this upgrade?
                    //if (player.IsBot && SMRPG_IgnoreBots())
                    //    return HookResult.Continue;

                    uint iLevel = _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
                    if (iLevel <= 0)
                        return HookResult.Continue;

                    fClipIncrease *= (int)iLevel;

                    int iNewMaxClip;
                    if (WeaponMaxClip.TryGetValue(Weapon, out string? weaponMaxClip))
                    {
                        iNewMaxClip = Convert.ToInt32(weaponMaxClip) + fClipIncrease;

                        if (ActiveWeapon.Clip1 == iNewMaxClip)
                            return HookResult.Handled;

                        ActiveWeapon.Clip1 = iNewMaxClip;
                    }
                }
            }

            return HookResult.Continue;
        }
    }
}