using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Modularity;
using WpCShpRpgCoreApi;
using static WpCShpRpgCoreApi.IWpCShpRpgCoreApi;

namespace WpCShpRpgSkills
{
    public class skill_damage : BasePlugin, IModulePlugin
    {
        public override string ModuleAuthor => "WanekWest";
        public override string ModuleName => "WpCssRpg | Damage+";
        public override string ModuleVersion => "v1.0";


        string UPGRADE_SHORTNAME = "damage";

        private IWpCShpRpgCoreApi _api = null!;

        private double DamagePercent = 0.0;
        private int DamageMax = 0;

        public override void Load(bool hotReload)
        {
            if (hotReload)
                Server.PrintToConsole($"Данный скилл не поддерживает перезагрузку: wpcssrpg_upgrade_{UPGRADE_SHORTNAME}");
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

            try
            {
                InternalUpgradeInfo upgrade = new InternalUpgradeInfo();
                if (_api.GetUpgradeByShortname(UPGRADE_SHORTNAME, ref upgrade))
                {
                    if (upgrade.parameters.TryGetValue("csshprpg_damage_percent", out string? csshprpg_damage_percent))
                    {
                        DamagePercent = Convert.ToDouble(csshprpg_damage_percent);
                    }

                    if (upgrade.parameters.TryGetValue("csshprpg_damage_max", out string? csshprpg_damage_max))
                    {
                        DamageMax = Convert.ToInt32(csshprpg_damage_max);
                    }
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Скилл {UPGRADE_SHORTNAME} не загружен с ошибкой: {ex.Message}!");
                return;
            }
        }

        private void RpgCoreLoaded()
        {
            if (!_api.UpgradeExists(UPGRADE_SHORTNAME))
            {
                _api.RegisterUpgradeType("Damage+", UPGRADE_SHORTNAME, "Deal additional damage on enemies.", 10, true, 5, 5, 10, 0, null, null);
            }
        }

        public override void Unload(bool hotReload)
        {
            if (_api != null && _api.UpgradeExists(UPGRADE_SHORTNAME))
                _api.UnregisterUpgradeType(UPGRADE_SHORTNAME);
        }

        [GameEventHandler]
        public HookResult PlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (@event == null || _api == null)
                return HookResult.Continue;

            CCSPlayerController? died = @event?.Userid;
            CCSPlayerController? killer = @event?.Attacker;
            //int health = @event?.Health ?? 0;
            //int damage = @event?.DmgHealth ?? 0;

            if (died == null || killer == null || killer.UserId == null)
                return HookResult.Continue;

            int Client = (int)killer.UserId;

            uint iLevel = _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
            if (iLevel <= 0)
                return HookResult.Continue;

            if (killer != died && !died.IsBot && !killer.IsBot && killer.TeamNum != died.TeamNum)
            {
                if (_api == null)
                    return HookResult.Continue;

                int fDmgInc = (int)(@event.DmgHealth * DamagePercent * iLevel);

                if (DamageMax > 0 && fDmgInc > DamageMax)
                    fDmgInc = DamageMax;

                @event.DmgHealth = fDmgInc;
            }

            return HookResult.Changed;
        }
    }
}