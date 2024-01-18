//using CounterStrikeSharp.API.Core;
//using CounterStrikeSharp.API.Core.Attributes.Registration;
//using WpCShpRpg;

//namespace SkillDamage
//{
//    public class skill_damage : BasePlugin
//    {
//        public override string ModuleName => "WpCssRpg Damage";
//        public override string ModuleVersion => "v1.0";

//        string UPGRADE_SHORTNAME = "damage";

//        public override void Load(bool hotReload)
//        {
//            OnLibraryAdded("smrpg");
//        }

//        public void OnLibraryAdded(string name)
//        {
//            // Register this upgrade in SM:RPG
//            if (string.Equals(name, "wpcshprpg"))
//            {
//                WpCShpRpg.Upgrades.RegisterUpgradeType("Damage+", UPGRADE_SHORTNAME, "Deal additional damage on enemies.", 10, true, 5, 5, 10, 0, null, null);
//            }
//        }

//        [GameEventHandler]
//        public HookResult PlayerHurt(EventPlayerHurt @event, GameEventInfo info)
//        {
//            CCSPlayerController? died = @event?.Userid;
//            CCSPlayerController? killer = @event?.Attacker;
//            int health = @event?.Health ?? 0;
//            int damage = @event?.DmgHealth ?? 0;

//            int Client = (int)killer.UserId;

//            uint iLevel = PlayerData.GetClientUpgradeLevel(Client, UPGRADE_SHORTNAME);
//            if (iLevel <= 0)
//                return HookResult.Continue;

//            if (died != null && killer != null && killer != died && died.IsBot != true && killer.IsBot != true && killer.TeamNum != died.TeamNum)
//            {
//                @event.DmgHealth = 200;
//            }

//            return HookResult.Changed;
//        }
//    }
//}
