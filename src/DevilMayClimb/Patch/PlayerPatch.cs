using DevilMayClimb.Monobehavior;
using DevilMayClimb.Service;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
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

            if (__instance.FallTime() <= __instance.fallDamageTime && __instance.FallTime() > __instance.fallDamageTime - 0.4f)
            {
                StyleTracker.localStyleTracker.CloseFall();
            }
        }

        [HarmonyPatch(typeof(GUIManager), "Grasp")]
        [HarmonyPostfix]
        public static void GUIGrasp(ref GUIManager __instance)
        {
            if (!StyleTracker.localStyleTracker) return;

            StyleTracker.localStyleTracker.Grasp();
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

            if (__instance.item.holderCharacter && __instance.item.holderCharacter == Character.localCharacter)
            {
                StyleTracker.localStyleTracker.Cooked(__instance.timesCookedLocal);
            }
        }

        [HarmonyPatch(typeof(RespawnChest), "SpawnItems")]
        [HarmonyPrefix]
        public static void RespawnChestSpawnItems(ref RespawnChest __instance, List<Transform> spawnSpots)
        {
            if (!StyleTracker.localStyleTracker) return;

            StyleTracker.localStyleTracker.RespawnChestActivated();
        }

        [HarmonyPatch(typeof(ScoutEffigy), "FinishConstruction")]
        [HarmonyPrefix]
        public static void EffigyFinish(ref ScoutEffigy __instance)
        {
            if (!StyleTracker.localStyleTracker || __instance.item.holderCharacter != Character.localCharacter) return;

            StyleTracker.localStyleTracker.EffigyActivated();
        }

        [HarmonyPatch(typeof(Character), "FeedItem")]
        [HarmonyPrefix]
        public static void CharacterFeed(ref Character __instance, Item item)
        {
            if (!StyleTracker.localStyleTracker || __instance != Character.localCharacter) return;

            StyleTracker.localStyleTracker.FedItem(item);
        }

        [HarmonyPatch(typeof(ThornOnMe), "Interact_CastFinished")]
        [HarmonyPostfix]
        public static void ThornInteract(ref ThornOnMe __instance, Character interactor)
        {
            if (!StyleTracker.localStyleTracker || interactor != Character.localCharacter) return;

            StyleTracker.localStyleTracker.RemovedThorn();
        }

        [HarmonyPatch(typeof(Scorpion), "InflictAttack")]
        [HarmonyPostfix]
        public static void ScorpionAttack(ref Scorpion __instance, Character character)
        {
            if (!StyleTracker.localStyleTracker || character != Character.localCharacter) return;

            StyleTracker.localStyleTracker.ScorpionSting();
        }

        [HarmonyPatch(typeof(SlipperyJellyfish), "Trigger")]
        [HarmonyPostfix]
        public static void JellyfishSlip(ref SlipperyJellyfish __instance, int targetID)
        {
            if (!StyleTracker.localStyleTracker) return;

            Character component = PhotonView.Find(targetID).GetComponent<Character>();
            if (component == null || component != Character.localCharacter)
            {
                return;
            }

            StyleTracker.localStyleTracker.JellyfishSlip();
        }

        [HarmonyPatch(typeof(TumbleWeed), "OnCollisionEnter")]
        [HarmonyPostfix]
        public static void TumbleWeedCollide(ref TumbleWeed __instance, Collision collision)
        {
            //Don't like this, will need to redo to avoid rewriting full tumbleweed collision function
            if (!StyleTracker.localStyleTracker) return;

            Character componentInParent = collision.gameObject.GetComponentInParent<Character>();
            if (!componentInParent || !componentInParent.IsLocal) return;
            if (__instance.ignored.Contains(componentInParent)) return;

            float num = __instance.transform.localScale.x / __instance.originalScale;
            if (__instance.originalScale == 0f)
            {
                num = 1f;
            }
            num = Mathf.Clamp01(num);
            float num2 = Mathf.Clamp01(__instance.rig.linearVelocity.magnitude * num * __instance.powerMultiplier);
            if (__instance.testFullPower)
            {
                num2 = 1f;
            }
            if (num2 < 0.2f)
            {
                return;
            }

            StyleTracker.localStyleTracker.Tumbleweed();
        }

        [HarmonyPatch(typeof(Bonkable), "Bonk")]
        [HarmonyPrefix]
        public static void BonkableBonk(ref Bonkable __instance, Collision coll)
        {
            if (!StyleTracker.localStyleTracker) return;

            Character componentInParent = coll.gameObject.GetComponentInParent<Character>();
            if (componentInParent && Time.time > __instance.lastBonkedTime + __instance.bonkCooldown)
            {
                StyleTracker.localStyleTracker.Bonk(__instance.item, componentInParent);
            }
        }
    }
}
