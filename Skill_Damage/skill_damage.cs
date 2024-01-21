using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Modularity;
using WpCShpRpg.Core.Additions;
using WpCShpRpgCoreApi;
using static WpCShpRpg.Core.Additions.Upgrades;

namespace WpCShpRpgSkills
{
    public class skill_damage : BasePlugin, IModulePlugin
    {
        public override string ModuleName => "WpCssRpg | Damage+";
        public override string ModuleVersion => "v1.0";

        string UPGRADE_SHORTNAME = "damage";

        private IWpCShpRpgCoreApi _api = null!;

        public override void Load(bool hotReload)
        {
            if (!hotReload)
            {
                CheckRpgCore();
            }
            else
            {
                Server.PrintToConsole($"Данный скилл не поддерживает перезагрузку: wpcssrpg_upgrade_{UPGRADE_SHORTNAME}");
            }
        }

        public void LoadModule(IApiProvider provider)
        {
            _api = provider.Get<IWpCShpRpgCoreApi>();
        }

        private void CheckRpgCore()
        {
            if (_api != null)
            {
                try
                {
                    Server.PrintToConsole($"Проверка статуса загрузки ядра RPG wanek {_api.IsRpgCoreLoaded()}");

                    if (!_api.IsRpgCoreLoaded())
                    {
                        Server.PrintToConsole("Ядро еще не загружено, повторная проверка через 2 секунды.");
                        AddTimer(2.0f, CheckRpgCore);
                    }
                    else
                    {
                        Server.PrintToConsole("Ядро загружено, регистрация навыка Damage+");

                        if (!_api.UpgradeExists(UPGRADE_SHORTNAME))
                            _api.RegisterUpgradeType("Damage+", UPGRADE_SHORTNAME, "Deal additional damage on enemies.", 10, true, 5, 5, 10, 0, null, null);
                    }
                }
                catch (Exception ex)
                {
                    Server.PrintToConsole(ex.Message);
                    AddTimer(2.0f, CheckRpgCore);
                }
            }
            else
            {
                Server.PrintToConsole("Ожидаю загрузки модуля WpCssRpg | Damage+");
                AddTimer(2.0f, CheckRpgCore);
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
            if (@event == null)
                return HookResult.Continue;

            CCSPlayerController? died = @event?.Userid;
            CCSPlayerController? killer = @event?.Attacker;
            //int health = @event?.Health ?? 0;
            //int damage = @event?.DmgHealth ?? 0;

            if (died == null || killer == null || killer.UserId == null)
                return HookResult.Continue;

            int Client = (int)killer.UserId;

            uint iLevel = PlayerData.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
            if (iLevel <= 0)
                return HookResult.Continue;

            if (killer != died && !died.IsBot && !killer.IsBot && killer.TeamNum != died.TeamNum)
            {
                if (_api == null)
                    return HookResult.Continue;

                InternalUpgradeInfo internalUpgradeInfo = new InternalUpgradeInfo();
                if (!GetUpgradeByShortname(UPGRADE_SHORTNAME, ref internalUpgradeInfo))
                {
                    Server.PrintToConsole($"Не удалось найти скилл: {UPGRADE_SHORTNAME}");
                    return HookResult.Continue;
                }

                var ConfigData = _api.GetParamsFromConfig(_api.GetModuleDirectoryImproved(), UPGRADE_SHORTNAME);

                // Additional
                double DamagePercent = 0.0;
                int DamageMax = 0;
                if (ConfigData.TryGetValue("csshprpg_damage_percent", out string? csshprpg_damage_percent))
                {
                    DamagePercent = Convert.ToDouble(csshprpg_damage_percent);
                    if (DamagePercent <= 0.0)
                        return HookResult.Continue;
                }

                if (ConfigData.TryGetValue("csshprpg_damage_max", out string? csshprpg_damage_max))
                {
                    DamageMax = Convert.ToInt32(csshprpg_damage_max);
                }

                int fDmgInc = (int)(@event.DmgHealth * DamagePercent * iLevel);

                if (DamageMax > 0 && fDmgInc > DamageMax)
                    fDmgInc = DamageMax;

                @event.DmgHealth = fDmgInc;
            }

            return HookResult.Changed;
        }
    }
}