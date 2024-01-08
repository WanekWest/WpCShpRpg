using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using System.Collections;

namespace WpCShpRpg
{
    internal class PlayerData
    {
        struct PlayerUpgradeInfo
        {
            int purchasedlevel;
            int selectedlevel;
            bool enabled;
            bool visuals;
            bool sounds;
        }

        struct PlayerInfo
        {
            int level;
            int experience;
            int credits;
            int dbId;
            bool showMenuOnLevelup;
            bool fadeOnLevelup;
            bool dataLoadedFromDB;
            ArrayList upgrades;
            int lastReset;
            int lastSeen;
        }

        PlayerInfo[] g_iPlayerInfo = new PlayerInfo[Server.MaxPlayers + 1];
        bool[] g_bFirstLoaded = new bool[Server.MaxPlayers + 1];
        char[,] g_sOriginalBotName = new char[Server.MaxPlayers + 1, Player.MaxNameLength];

        void RemovePlayer(int client, bool bKeepBotName = false, bool g_hCVShowMenuOnLevelDefault, bool g_hCVFadeOnLevelDefault)
        {
            ResetStats(client);
            delete g_iPlayerInfo[client].upgrades;
            g_iPlayerInfo[client].dbId = -1;
            g_iPlayerInfo[client].dataLoadedFromDB = false;
            g_iPlayerInfo[client].showMenuOnLevelup = g_hCVShowMenuOnLevelDefault;
            g_iPlayerInfo[client].fadeOnLevelup = g_hCVFadeOnLevelDefault;
            g_iPlayerInfo[client].lastReset = 0;
            g_iPlayerInfo[client].lastSeen = 0;

            if (!bKeepBotName)
                g_sOriginalBotName[client][0] = '\0';
        }

        bool IsPlayerDataLoaded(int client)
        {
            return g_iPlayerInfo[client].dataLoadedFromDB;
        }

        int GetPlayerLastReset(int client)
        {
            return g_iPlayerInfo[client].lastReset;
        }

        void SetPlayerLastReset(int client, int time)
        {
            g_iPlayerInfo[client].lastReset = time;
        }

        int GetPlayerLastSeen(int client)
        {
            return g_iPlayerInfo[client].lastSeen;
        }

        void GetPlayerUpgradeInfoByIndex(int client, int index, PlayerUpgradeInfo playerupgrade)
        {
            g_iPlayerInfo[client].upgrades.GetArray(index, playerupgrade, sizeof(PlayerUpgradeInfo));
        }

        void SavePlayerUpgradeInfo(int client, int index, PlayerUpgradeInfo playerupgrade)
        {
            g_iPlayerInfo[client].upgrades.SetArray(index, playerupgrade, sizeof(PlayerUpgradeInfo));
        }

        void ResetStats(int client)
        {
            Console.WriteLine("Stats have been reset for player: %N", client);

            int iSize = GetUpgradeCount();
            Upgrades.InternalUpgradeInfo upgrade;
            PlayerUpgradeInfo playerupgrade;
            bool bWasEnabled;
            for (int i = 0; i < iSize; i++)
            {
                GetPlayerUpgradeInfoByIndex(client, i, playerupgrade);
                // See if this upgrade has been enabled and should be notified to stop the effect.
                bWasEnabled = playerupgrade.enabled && playerupgrade.selectedlevel > 0;

                // Reset upgrade to level 0
                playerupgrade.purchasedlevel = 0;
                playerupgrade.selectedlevel = 0;
                SavePlayerUpgradeInfo(client, i, playerupgrade);

                // No need to inform the upgrade plugin, that this player was reset,
                // if it wasn't active before at all.
                if (!bWasEnabled)
                    continue;

                GetUpgradeByIndex(i, upgrade);

                if (!IsValidUpgrade(upgrade))
                    continue;

                // Plugin doesn't care? OK :(
                if (upgrade.queryCallback == INVALID_FUNCTION)
                    continue;

                Call_StartFunction(upgrade.plugin, upgrade.queryCallback);
                Call_PushCell(client);
                Call_PushCell(UpgradeQueryType_Sell);
                Call_Finish();
            }

            g_iPlayerInfo[client].level = 1;
            g_iPlayerInfo[client].experience = 0;
            g_iPlayerInfo[client].credits = g_hCVCreditsStart.IntValue;
        }
    }
}
