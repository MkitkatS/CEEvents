﻿#define V100

using CaptivityEvents.Brothel;
using CaptivityEvents.Config;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using CaptivityEvents.Custom;

namespace CaptivityEvents.Patches
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel), "CalculateClanIncomeInternal")]
    internal class CEPatchDefaultClanFinanceModel
    {
        [HarmonyPrepare]
        private static bool ShouldPatch() => CESettings.Instance?.ProstitutionControl ?? true;

        [HarmonyPostfix]
        private static void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
            if (clan.IsEliminated) return;
            if (Clan.PlayerClan != clan) return;

            int num = 0;
            int num2 = 0;
            try
            {
                foreach (CEBrothel brothel in CEBrothelBehavior.GetPlayerBrothels())
                {
                    if (brothel.IsRunning)
                    {
                        int num3 = (int)(Math.Max(0, brothel.ProfitMade) / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction());
                        num3 *= (brothel.Level + 1);
                        num += num3;

                        if (applyWithdrawals && num3 > 0) brothel.ChangeGold(-num3);

                        if (num3 > 0 && Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) num2++;
                    }
                }

                goldChange.Add(num, new TextObject("{=CEBROTHEL1001}Brothel income."));
                if (Hero.MainHero.Clan.Leader.GetPerkValue(DefaultPerks.Trade.ArtisanCommunity) && applyWithdrawals) Hero.MainHero.Clan.AddRenown(num2 * DefaultPerks.Trade.ArtisanCommunity.PrimaryBonus);
            } 
            catch (Exception e)
            {
                CECustomHandler.ForceLogToFile("CalculateClanIncomeInternal: " + e);
            }
        }
    }
}