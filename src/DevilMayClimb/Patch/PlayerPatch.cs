using DevilMayClimb.Service;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
