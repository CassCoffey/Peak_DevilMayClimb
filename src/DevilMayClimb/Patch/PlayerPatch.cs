using DevilMayClimb.Monobehavior;
using DevilMayClimb.Service;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using static CharacterAfflictions;

namespace DevilMayClimb.Patch
{
    public class PlayerPatch
    {
        [HarmonyPatch(typeof(Player), "Awake")]
        [HarmonyPostfix]
        public static void PlayerAwake(ref Player __instance)
        {
            // Only care about the local player
            if (__instance.view == null || !__instance.view.IsMine) return;

            StyleManager.RegisterPlayer(__instance);
        }

        [HarmonyPatch(typeof(Character), "Awake")]
        [HarmonyPostfix]
        public static void CharacterAwake(ref Character __instance)
        {
            // Only care about the local character
            if (!__instance.IsLocal) return;

            StyleManager.RegisterCharacter(__instance);
        }

        [HarmonyPatch(typeof(CharacterAfflictions), "AddStatus")]
        [HarmonyPostfix]
        public static void CharacterAddStatus(ref CharacterAfflictions __instance, CharacterAfflictions.STATUSTYPE statusType, float amount, bool fromRPC)
        {
            // Only care about the local character
            if (!__instance.character.IsLocal || !StyleTracker.localStyleTracker) return;

            if (amount > 0f && statusType == STATUSTYPE.Injury)
            {
                StyleTracker.localStyleTracker.Fail();
            }
        }

        [HarmonyPatch(typeof(CharacterMovement), "CheckFallDamage")]
        [HarmonyPostfix]
        public static void CharacterCheckFallDamage(ref CharacterMovement __instance)
        {
            // Only care about the local character
            if (!__instance.character.IsLocal || !StyleTracker.localStyleTracker) return;

            if (__instance.FallTime() <= __instance.fallDamageTime && __instance.FallTime() > __instance.fallDamageTime - 0.5f)
            {
                StyleTracker.localStyleTracker.CloseFall();
            }
        }

        [HarmonyPatch(typeof(GUIManager), "Grasp")]
        [HarmonyPostfix]
        public static void GUIGrasp(ref GUIManager __instance)
        {
            if (StyleTracker.localStyleTracker)
            {
                StyleTracker.localStyleTracker.Grasp();
            }
        }

        [HarmonyPatch(typeof(CharacterMovement), "CheckForPalJump")]
        [HarmonyPrefix]
        public static void CharacterCheckFallDamage(ref CharacterMovement __instance, Character c)
        {
            // Only care about the local character
            if (!__instance.character.IsLocal && !c.IsLocal) return;
            if (!StyleTracker.localStyleTracker) return;

            if (__instance.character.data.sinceStandOnPlayer < 0.3f && c.data.sinceJump < 0.3f)
            {
                if (__instance.character.IsLocal)
                {
                    StyleTracker.localStyleTracker.FriendBoosted();
                } 
                else if (c.IsLocal)
                {
                    StyleTracker.localStyleTracker.FriendBooster();
                }
            }
        }

        [HarmonyPatch(typeof(Campfire), "Light_Rpc")]
        [HarmonyPostfix]
        public static void CampfireLight(ref Campfire __instance)
        {
            if (!StyleTracker.localStyleTracker) return;

            StyleTracker.localStyleTracker.Campfire();
        }

        [HarmonyPatch(typeof(ItemCooking), "FinishCooking")]
        [HarmonyPostfix]
        public static void ItemFinishCooking(ref ItemCooking __instance)
        {
            if (!StyleTracker.localStyleTracker) return;

            Plugin.Log.LogInfo("Cooked - " + __instance.timesCookedLocal);

            if (__instance.timesCookedLocal == 1)
            {
                StyleTracker.localStyleTracker.Cooked();
            }
        }
    }
}
