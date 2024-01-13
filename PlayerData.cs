using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;
using System.Collections;
using static WpCShpRpg.Upgrades;

namespace WpCShpRpg
{
    public class PlayerData
    {
        Config config;
        Upgrades upgradesClass;

        public PlayerData(Config config, Upgrades upgradesClass)
        {
            this.config = config;
            this.upgradesClass = upgradesClass;
        }

        public struct PlayerUpgradeInfo
        {
            public int purchasedlevel;
            public int selectedlevel;
            public bool enabled;
            public bool visuals;
            public bool sounds;

            public PlayerUpgradeInfo(int purchasedlevel, int selectedlevel, bool enabled, bool visuals, bool sounds)
            {
                this.purchasedlevel = purchasedlevel;
                this.selectedlevel = selectedlevel;
                this.enabled = enabled;
                this.visuals = visuals;
                this.sounds = sounds;
            }
        }

        public struct PlayerInfo
        {
            public uint level;
            public uint experience;
            public uint credits;
            public int dbId;
            public bool showMenuOnLevelup;
            public bool fadeOnLevelup;
            public bool dataLoadedFromDB;
            public ArrayList upgrades;
            public float lastReset;
            public float lastSeen;
            public PlayerInfo(uint level, uint experience, uint credits, int dbId, bool showMenuOnLevelup, bool fadeOnLevelup,
            bool dataLoadedFromDB, ArrayList upgrades, float lastReset, float lastSeen)
            {
                this.level = level;
                this.experience = experience;
                this.credits = credits;
                this.dbId = dbId;
                this.showMenuOnLevelup = showMenuOnLevelup;
                this.fadeOnLevelup = fadeOnLevelup;
                this.dataLoadedFromDB = dataLoadedFromDB;
                this.upgrades = upgrades;
                this.lastReset = lastReset;
                this.lastSeen = lastSeen;
            }

            PlayerInfo[] g_iPlayerInfo = new PlayerInfo[Server.MaxPlayers + 1];
            bool[] g_bFirstLoaded = new bool[Server.MaxPlayers + 1];
            char[,] g_sOriginalBotName = new char[Server.MaxPlayers + 1, Player.MaxNameLength];

            public void RemovePlayer(int client, bool bKeepBotName = false, bool g_hCVShowMenuOnLevelDefault, bool g_hCVFadeOnLevelDefault)
            {
                ResetStats(client);
                g_iPlayerInfo[client].upgrades.Clear();
                g_iPlayerInfo[client].dbId = -1;
                g_iPlayerInfo[client].dataLoadedFromDB = false;
                g_iPlayerInfo[client].showMenuOnLevelup = g_hCVShowMenuOnLevelDefault;
                g_iPlayerInfo[client].fadeOnLevelup = g_hCVFadeOnLevelDefault;
                g_iPlayerInfo[client].lastReset = 0;
                g_iPlayerInfo[client].lastSeen = 0;

                if (!bKeepBotName)
                    g_sOriginalBotName[client, 0] = '\0';
            }

            public bool IsPlayerDataLoaded(int client)
            {
                return g_iPlayerInfo[client].dataLoadedFromDB;
            }

            public float GetPlayerLastReset(int client)
            {
                return g_iPlayerInfo[client].lastReset;
            }

            public void SetPlayerLastReset(int client, float time)
            {
                g_iPlayerInfo[client].lastReset = time;
            }

            public float GetPlayerLastSeen(int client)
            {
                return g_iPlayerInfo[client].lastSeen;
            }

            public PlayerUpgradeInfo GetPlayerUpgradeInfoByIndex(int client, int index)
            {
                return (PlayerUpgradeInfo)g_iPlayerInfo[client].upgrades[index];
            }

            public void SavePlayerUpgradeInfo(int client, int index, PlayerUpgradeInfo playerupgrade)
            {
                g_iPlayerInfo[client].upgrades[index] = playerupgrade;
            }

            public void ResetStats(int client)
            {
                Console.WriteLine("Stats have been reset for player: %N", client);

                int iSize = upgradesClass.GetUpgradeCount();
                Upgrades.InternalUpgradeInfo upgrade;
                PlayerUpgradeInfo playerupgrade;
                bool bWasEnabled;
                for (int i = 0; i < iSize; i++)
                {
                    playerupgrade = GetPlayerUpgradeInfoByIndex(client, i);
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

                    upgrade = upgradesClass.GetUpgradeByIndex(i);

                    if (upgradesClass.IsValidUpgrade(upgrade) == false)
                        continue;

                    // Plugin doesn't care? OK :(
                    if (upgrade.queryCallback == INVALID_FUNCTION)
                        continue;
                }

                g_iPlayerInfo[client].level = 1;
                g_iPlayerInfo[client].experience = 0;
                g_iPlayerInfo[client].credits = config.g_hCVCreditsStart;
            }

            public void InitPlayer(int client, bool bGetBotName = true)
            {
                g_bFirstLoaded[client] = true;

                // See if the player should start at a higher level than 1?
                uint[] StartLevelCredits = GetStartLevelAndExperience();

                g_iPlayerInfo[client].level = StartLevelCredits[0];
                g_iPlayerInfo[client].experience = 0;
                g_iPlayerInfo[client].credits = StartLevelCredits[1];
                g_iPlayerInfo[client].dbId = -1;
                g_iPlayerInfo[client].dataLoadedFromDB = false;
                g_iPlayerInfo[client].showMenuOnLevelup = config.g_hCVShowMenuOnLevelDefault;
                g_iPlayerInfo[client].fadeOnLevelup = config.g_hCVFadeOnLevelDefault;
                g_iPlayerInfo[client].lastReset = Server.CurrentTime;
                g_iPlayerInfo[client].lastSeen = Server.CurrentTime;

                g_iPlayerInfo[client].upgrades = new ArrayList();
                int iNumUpgrades = upgradesClass.GetUpgradeCount();

                for (int i = 0; i < iNumUpgrades; i++)
                {
                    // start level (default 0) for all upgrades
                    InitPlayerNewUpgrade(client);
                }

                // Save the name the bot joined with, so we fetch the right info, even if some plugin changes the name of the bot afterwards.
                if (bGetBotName && IsFakeClient(client))
                {
                    GetClientName(client, g_sOriginalBotName[client], sizeof(g_sOriginalBotName[]));
                }
            }

            public void InsertPlayer(int client, bool g_hCVEnable, bool g_hCVSaveData, bool g_hCVBotSaveStats)
            {
                if (!g_hCVEnable || !g_hCVSaveData || !g_hCVBotSaveStats)
                    return;

                char[] sQuery = new char[512];
                char[] sName = new char[256], sNameEscaped = new char[512 + 1];

                GetClientName(client, sName, sizeof(sName));

                // Make sure to keep the original bot name.
                if (IsFakeClient(client))
                {
                    sName = g_sOriginalBotName[client, 0];
                }

                // Store the steamid of the player
                if (!IsFakeClient(client))
                {
                    Format(sQuery, sizeof(sQuery), "INSERT INTO %s (name, steamid, level, experience, credits, showmenu, fadescreen, lastseen, lastreset) VALUES ('%s', %d, %d, %d, %d, %d, %d, %d, %d)",
                        "players", sNameEscaped, GetSteamAccountID(client), GetClientLevel(client), GetClientExperience(client), GetClientCredits(client), ShowMenuOnLevelUp(client), FadeScreenOnLevelUp(client), GetTime(), GetTime());
                }
                // Bots are identified by their name!
                else
                {
                    Format(sQuery, sizeof(sQuery), "INSERT INTO %s (name, steamid, level, experience, credits, showmenu, fadescreen, lastseen, lastreset) VALUES ('%s', NULL, %d, %d, %d, %d, %d, %d, %d)",
                        "players", sNameEscaped, GetClientLevel(client), GetClientExperience(client), GetClientCredits(client), ShowMenuOnLevelUp(client), FadeScreenOnLevelUp(client), GetTime(), GetTime());
                }
            }

            void InitPlayerNewUpgrade(int client)
            {
                // Let the player start this upgrade on its set start level by default.
                ArrayList clienUpgrades = GetClientUpgrades(client);
                int iIndex = clienUpgrades.Count;
                InternalUpgradeInfo upgrade;
                upgrade = upgradesClass.GetUpgradeByIndex(iIndex);

                PlayerUpgradeInfo playerupgrade;
                playerupgrade.purchasedlevel = 0;
                playerupgrade.selectedlevel = 0;
                playerupgrade.enabled = true;
                playerupgrade.visuals = true;
                playerupgrade.sounds = true;
                clienUpgrades.PushArray(playerupgrade, sizeof(PlayerUpgradeInfo));

                // Get the money for the start level?
                // TODO: Make sure to document the OnBuyUpgrade forward being called on clients not ingame yet + test.
                // (This is can be called OnClientConnected.)
                bool bFree = config.g_hCVUpgradeStartLevelsFree;
                for (int i = 0; i < upgrade.startLevel; i++)
                {
                    if (bFree)
                    {
                        if (!GiveClientUpgrade(client, iIndex))
                            break;
                    }
                    else
                    {
                        if (!BuyClientUpgrade(client, iIndex))
                            break;
                    }
                }
            }

            bool GiveClientUpgrade(int client, int iUpgradeIndex)
            {
                InternalUpgradeInfo upgrade;
                upgrade = upgradesClass.GetUpgradeByIndex(iUpgradeIndex);

                if (upgradesClass.IsValidUpgrade(upgrade)==false)
                    return false;

                int iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

                if (iCurrentLevel >= upgrade.maxLevel)
                    return false;

                // Upgrade level +1!
                iCurrentLevel++;

                // See if some plugin doesn't want this player to level up this upgrade
                Action result;
                Call_StartForward(g_hfwdOnBuyUpgrade);
                Call_PushCell(client);
                Call_PushString(upgrade.shortName);
                Call_PushCell(iCurrentLevel);
                Call_Finish(result);

                // Some plugin doesn't want this to happen :(
                if (result > Plugin_Continue)
                    return false;

                // Actually update the upgrade level.
                SetClientPurchasedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);
                // Also have it select the new higher upgrade level.
                SetClientSelectedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);

                Call_StartForward(g_hfwdOnBuyUpgradePost);
                Call_PushCell(client);
                Call_PushString(upgrade.shortName);
                Call_PushCell(iCurrentLevel);
                Call_Finish();

                return true;
            }

            bool BuyClientUpgrade(int client, int iUpgradeIndex)
            {
                InternalUpgradeInfo upgrade;
                upgradesClass.GetUpgradeByIndex(iUpgradeIndex, upgrade);

                int iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

                // can't get higher than this.
                if (iCurrentLevel >= upgrade.maxLevel)
                    return false;

                uint iCost = upgradesClass.GetUpgradeCost(iUpgradeIndex, iCurrentLevel + 1);

                // Not enough credits?
                if (iCost > g_iPlayerInfo[client].credits)
                    return false;

                if (!GiveClientUpgrade(client, iUpgradeIndex))
                    return false;

                g_iPlayerInfo[client].credits -= iCost;

                return true;
            }

            bool TakeClientUpgrade(int client, int iUpgradeIndex)
            {
                InternalUpgradeInfo upgrade;
                upgradesClass.GetUpgradeByIndex(iUpgradeIndex, upgrade);

                if (!IsValidUpgrade(upgrade))
                    return false;

                int iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

                // Can't get negative levels
                if (iCurrentLevel <= 0)
                    return false;

                // Upgrade level -1!
                iCurrentLevel--;

                // See if some plugin doesn't want this player to level down this upgrade
                Action result;
                Call_StartForward(g_hfwdOnSellUpgrade);
                Call_PushCell(client);
                Call_PushString(upgrade.shortName);
                Call_PushCell(iCurrentLevel);
                Call_Finish(result);

                // Some plugin doesn't want this to happen :(
                if (result > Plugin_Continue)
                    return false;

                // Actually update the upgrade level.
                SetClientPurchasedUpgradeLevel(client, iUpgradeIndex, iCurrentLevel);

                Call_StartForward(g_hfwdOnSellUpgradePost);
                Call_PushCell(client);
                Call_PushString(upgrade.shortName);
                Call_PushCell(iCurrentLevel);
                Call_Finish();

                return true;
            }

            bool SellClientUpgrade(int client, int iUpgradeIndex)
            {
                InternalUpgradeInfo upgrade;
                upgradesClass.GetUpgradeByIndex(iUpgradeIndex, upgrade);

                int iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, iUpgradeIndex);

                // can't get negative
                if (iCurrentLevel <= 0)
                    return false;

                if (!TakeClientUpgrade(client, iUpgradeIndex))
                    return false;

                g_iPlayerInfo[client].credits += upgradesClass.GetUpgradeSale(iUpgradeIndex, iCurrentLevel);

                return true;
            }

            // Have bots buy upgrades too :)
            void BotPickUpgrade(int client)
            {
                bool bUpgradeBought;
                int iCurrentIndex;

                int iSize = upgradesClass.GetUpgradeCount();
                InternalUpgradeInfo upgrade;

                ArrayList hRandomBuying = new ArrayList();
                for (int i = 0; i < iSize; i++)
                    hRandomBuying.Add(i);

                while (GetClientCredits(client) > 0)
                {
                    // Shuffle the order of upgrades randomly. That way the bot won't upgrade one upgrade as much as he can before trying another one.
                    Array_Shuffle(hRandomBuying);

                    bUpgradeBought = false;
                    for (int i = 0; i < iSize; i++)
                    {
                        iCurrentIndex = hRandomBuying[i];
                        upgradesClass.GetUpgradeByIndex(iCurrentIndex, upgrade);

                        // Valid upgrade the bot can use?
                        if (!IsValidUpgrade(upgrade) || !upgrade.enabled)
                            continue;

                        // Don't buy it, if bots aren't allowed to use it at all..
                        if (!upgrade.allowBots)
                            continue;

                        // Don't let him buy upgrades, which are restricted to the other team.
                        if (!IsClientInLockedTeam(client, upgrade))
                            continue;

                        if (BuyClientUpgrade(client, iCurrentIndex))
                            bUpgradeBought = true;
                    }
                    if (!bUpgradeBought)
                        break; /* Couldn't afford anything */
                }

                delete hRandomBuying;
            }

            /**
             * Player info accessing functions (getter/setter)
             */
            void CheckItemMaxLevels(int client)
            {
                int iSize = upgradesClass.GetUpgradeCount();
                InternalUpgradeInfo upgrade;
                int iMaxLevel, iCurrentLevel;
                for (int i = 0; i < iSize; i++)
                {
                    upgradesClass.GetUpgradeByIndex(i, upgrade);
                    iMaxLevel = upgrade.maxLevel;
                    iCurrentLevel = upgradesClass.GetClientPurchasedUpgradeLevel(client, i);
                    while (iCurrentLevel > iMaxLevel)
                    {
                        /* Give player their credits back */
                        SetClientCredits(client, GetClientCredits(client) + GetUpgradeCost(i, iCurrentLevel--));
                    }
                    if (upgradesClass.GetClientPurchasedUpgradeLevel(client, i) != iCurrentLevel)
                        upgradesClass.SetClientPurchasedUpgradeLevel(client, i, iCurrentLevel);
                }
            }

            int GetClientCredits(int client)
            {
                return g_iPlayerInfo[client].credits;
            }

            bool SetClientCredits(int client, int iCredits)
            {
                if (iCredits < 0)
                    iCredits = 0;

                // See if some plugin doesn't want this player to get some credits
                Action result;
                Call_StartForward(g_hfwdOnClientCredits);
                Call_PushCell(client);
                Call_PushCell(g_iPlayerInfo[client].credits);
                Call_PushCell(iCredits);
                Call_Finish(result);

                // Some plugin doesn't want this to happen :(
                if (result > Plugin_Continue)
                    return false;

                int iOldCredits = g_iPlayerInfo[client].credits;
                g_iPlayerInfo[client].credits = iCredits;

                Call_StartForward(g_hfwdOnClientCreditsPost);
                Call_PushCell(client);
                Call_PushCell(iOldCredits);
                Call_PushCell(iCredits);
                Call_Finish();

                return true;
            }

            int GetClientLevel(int client)
            {
                return g_iPlayerInfo[client].level;
            }

            bool SetClientLevel(int client, int iLevel)
            {
                if (iLevel < 1)
                    iLevel = 1;

                // See if some plugin doesn't want this player to get some credits
                Action result;
                Call_StartForward(g_hfwdOnClientLevel);
                Call_PushCell(client);
                Call_PushCell(g_iPlayerInfo[client].level);
                Call_PushCell(iLevel);
                Call_Finish(result);

                // Some plugin doesn't want this to happen :(
                if (result > Plugin_Continue)
                    return false;

                int iOldLevel = g_iPlayerInfo[client].level;
                g_iPlayerInfo[client].level = iLevel;

                Call_StartForward(g_hfwdOnClientLevelPost);
                Call_PushCell(client);
                Call_PushCell(iOldLevel);
                Call_PushCell(iLevel);
                Call_Finish();

                return true;
            }

            int GetClientExperience(int client)
            {
                return g_iPlayerInfo[client].experience;
            }

            bool SetClientExperience(int client, int iExperience)
            {
                if (iExperience < 0)
                    iExperience = 0;

                // See if some plugin doesn't want this player to get some credits
                Action result;
                Call_StartForward(g_hfwdOnClientExperience);
                Call_PushCell(client);
                Call_PushCell(g_iPlayerInfo[client].experience);
                Call_PushCell(iExperience);
                Call_Finish(result);

                // Some plugin doesn't want this to happen :(
                if (result > Plugin_Continue)
                    return false;

                int iOldExperience = g_iPlayerInfo[client].experience;
                g_iPlayerInfo[client].experience = iExperience;

                Call_StartForward(g_hfwdOnClientExperiencePost);
                Call_PushCell(client);
                Call_PushCell(iOldExperience);
                Call_PushCell(iExperience);
                Call_Finish();

                return true;
            }

            public uint[] GetStartLevelAndExperience()
            {
                // See if the player should start at a higher level than 1?
                uint[] StartLevelCredits = { config.g_hCVLevelStart, config.g_hCVCreditsStart };
                if (StartLevelCredits[0] < 1)
                    StartLevelCredits[0] = 1;

                // If the start level is at a higher level than 1, he might get more credits for his level.
                if (config.g_hCVLevelStartGiveCredits)
                    StartLevelCredits[1] += config.g_hCVCreditsInc * (StartLevelCredits[0] - 1);

                return StartLevelCredits;
            }

            public ArrayList GetClientUpgrades(int client)
            {
                return g_iPlayerInfo[client].upgrades;
            }
        }
    }
}
