using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Modularity;
using WpCShpRpgCoreApi;
using static WpCShpRpgCoreApi.IWpCShpRpgCoreApi;

namespace Skill_ArmorRegen
{
    public class ArmorRegen : BasePlugin, IModulePlugin
    {
        public override string ModuleAuthor => "WanekWest";
        public override string ModuleName => "WpCssRpg | ArmorRegen";
        public override string ModuleVersion => "v1.0";

        private static string UPGRADE_SHORTNAME = "armorregen";

        private IWpCShpRpgCoreApi _api = null!;

        private double[]? _regenInterval, _intervalOnClient;
        private int[]? _regenValue;

        private bool IsSkillLoaded = false;

        private double Interval, IntervalDecrease, RegenAmount, AmountIncrease;
        private int MaxArmor;

        private CounterStrikeSharp.API.Modules.Timers.Timer RoundTimer = null;

        public void LoadModule(IApiProvider provider)
        {
            _api = provider.Get<IWpCShpRpgCoreApi>();

            Server.PrintToConsole($"api LoadModule {UPGRADE_SHORTNAME}");
            Server.PrintToConsole($"api LoadModule {UPGRADE_SHORTNAME}");
            Server.PrintToConsole($"api LoadModule {UPGRADE_SHORTNAME}");
            Server.PrintToConsole($"api LoadModule {UPGRADE_SHORTNAME}");

            if (_api == null)
            {
                Server.PrintToConsole($"api is null {UPGRADE_SHORTNAME}");
                Server.PrintToConsole($"api is null {UPGRADE_SHORTNAME}");
                Server.PrintToConsole($"api is null {UPGRADE_SHORTNAME}");
                Server.PrintToConsole($"api is null {UPGRADE_SHORTNAME}");
                return;
            }

            _api.CssRpg_UpgradeBuySell += CssRpg_BuySell;
            _api.CssRpg_OnCoreLoaded += RpgCoreLoaded;
        }

        public override void Load(bool hotReload)
        {
            RegisterListener<Listeners.OnMapStart>(name =>
            {
                _regenInterval = new double[Server.MaxPlayers];
                _regenValue = new int[Server.MaxPlayers];
                _intervalOnClient = new double[Server.MaxPlayers];
            });

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

        private void RpgCoreLoaded()
        {
            Server.PrintToConsole($"Ядро загружено, регистрация навыка {UPGRADE_SHORTNAME}");

            if (!_api.UpgradeExists(UPGRADE_SHORTNAME))
            {
                Server.PrintToConsole($"Ядро загружено, регистрация навыка {UPGRADE_SHORTNAME}");
                Server.PrintToConsole($"Ядро загружено, регистрация навыка {UPGRADE_SHORTNAME}");
                Server.PrintToConsole($"Ядро загружено, регистрация навыка {UPGRADE_SHORTNAME}");
                _api.RegisterUpgradeType("Armor regeneration", UPGRADE_SHORTNAME, "Deal additional damage on enemies.", 10, true, 5, 5, 10, 0, null, null);
            }

            try
            {
                InternalUpgradeInfo upgrade = new InternalUpgradeInfo();
                if (_api.GetUpgradeByShortname(UPGRADE_SHORTNAME, ref upgrade))
                {
                    if (upgrade.parameters.TryGetValue("csshprpg_armorregen_interval", out string? csshprpg_armorregen_interval))
                    {
                        Interval = Convert.ToDouble(csshprpg_armorregen_interval);
                    }

                    if (upgrade.parameters.TryGetValue("csshprpg_armorregen_amount", out string? csshprpg_armorregen_amount))
                    {
                        IntervalDecrease = Convert.ToDouble(csshprpg_armorregen_amount);
                    }

                    if (upgrade.parameters.TryGetValue("csshprpg_damage_percent", out string? csshprpg_damage_percent))
                    {
                        RegenAmount = Convert.ToDouble(csshprpg_damage_percent);
                    }

                    if (upgrade.parameters.TryGetValue("csshprpg_armorregen_amount_inc", out string? csshprpg_armorregen_amount_inc))
                    {
                        AmountIncrease = Convert.ToDouble(csshprpg_armorregen_amount_inc);
                    }

                    if (upgrade.parameters.TryGetValue("csshprpg_armorregen_max_value", out string? csshprpg_armorregen_max_value))
                    {
                        MaxArmor = Convert.ToInt32(csshprpg_armorregen_max_value);
                    }
                }
            }
            catch (Exception ex)
            {
                Server.PrintToConsole($"Скилл {UPGRADE_SHORTNAME} не загружен с ошибкой: {ex.Message}!");
                return;
            }

            IsSkillLoaded = true;
        }


        [GameEventHandler]
        public HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            if (RoundTimer == null)
                RoundTimer = AddTimer(1.0f, Timer_IncreaseArmor);

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult EventRoundStart(EventRoundEnd @event, GameEventInfo info)
        {
            if (RoundTimer != null)
            {
                RoundTimer.Kill();
                RoundTimer = null;
            }

            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnClientConnect(EventPlayerConnectFull @event, GameEventInfo info)
        {
            if (@event == null && @event.Userid != null)
                return HookResult.Continue;

            CCSPlayerController playerController = @event.Userid;

            if (playerController != null && playerController.IsValid && !playerController.IsBot && playerController.UserId != null && playerController.UserId >= 0)
            {
                if (IsSkillLoaded)
                {
                    int Client = (int)playerController.UserId;
                    Server.PrintToConsole($"Client is {Client} in skill {UPGRADE_SHORTNAME}");
                    uint CLientUpgradeLevel = _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
                    _regenInterval[Client] = Interval - IntervalDecrease * (CLientUpgradeLevel - 1);
                    _regenValue[Client] = (int)(RegenAmount + AmountIncrease * (CLientUpgradeLevel - 1));
                    _intervalOnClient[Client] = 0.0f;
                }
            }

            return HookResult.Continue;
        }

        private void CssRpg_BuySell(int Client, UpgradeQueryType type, string UpgradeName)
        {
            Server.PrintToChatAll("Работаем, братья!");
            Server.PrintToChatAll("Работаем, братья!");
            Server.PrintToChatAll("Работаем, братья!");

            if (UpgradeName == UPGRADE_SHORTNAME && _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME) <= 0)
                return;

            SetValuesForCLient(Client);
        }

        private void Timer_IncreaseArmor()
        {
            if (_api != null && _api.CssRpg_IsEnabled())
            {
                foreach (var player in Utilities.GetPlayers().Where(u => u != null && u.PlayerPawn != null && u.PlayerPawn.Value != null && u.PlayerPawn.Value.IsValid && u.PawnIsAlive))
                {
                    if (player != null && player.UserId != null && player.UserId > 0 && !player.IsHLTV && player.IsValid && !player.IsBot)
                    {
                        int Client = (int)player.UserId;
                        uint ClientUpgradeLevel = _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);

                        if (ClientUpgradeLevel > 0)
                        {
                            InternalUpgradeInfo upgrade = new InternalUpgradeInfo();
                            if (_api.GetUpgradeByShortname(UPGRADE_SHORTNAME, ref upgrade))
                            {
                                if (upgrade.enabled)
                                {
                                    _intervalOnClient[Client] += 0.1f;

                                    CCSPlayerPawn playerPawn = player.PlayerPawn.Value;
                                    if (_intervalOnClient[Client] == _regenInterval[Client])
                                    {
                                        _intervalOnClient[Client] = 0.0f;

                                        if (playerPawn.ArmorValue < MaxArmor)
                                        {
                                            playerPawn.ArmorValue += _regenValue[Client];

                                            if (playerPawn.ArmorValue > MaxArmor)
                                                playerPawn.ArmorValue = MaxArmor;

                                            Server.PrintToChatAll("Timer_IncreaseArmor kto-to");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetValuesForCLient(int Client)
        {
            uint CLientUpgradeLevel = _api.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
            _regenInterval[Client] = Interval - IntervalDecrease * (CLientUpgradeLevel - 1);
            _regenValue[Client] = (int)(RegenAmount + AmountIncrease * (CLientUpgradeLevel - 1));
        }
    }
}