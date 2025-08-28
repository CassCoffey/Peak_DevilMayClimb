using DevilMayClimb.Service;
using Peak.Afflictions;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI.Extensions.Tweens;
using static Zorro.ControllerSupport.Rumble.RumbleClip;

namespace DevilMayClimb.Monobehavior
{
    public class StyleTracker : MonoBehaviour
    {
        public static StyleTracker localStyleTracker;

        public static float RANK_POINTS = (100f * Config.rankMult.Value);
        public static float MAX_STYLE = RANK_POINTS * 7f;

        private const float SLOW_DECAY = 0.025f;
        private const float FAST_DECAY = 0.05f;
        private const float TRICK_GRACE = 5f;
        private const float INTERNAL_TRICK_COOLDOWN = 0.1f;

        // Squared to avoid using sqrt
        private const float MIN_CLIMB = 2f * 2f;
        private const float LONG_CLIMB = 10f * 10f;

        private Character? localCharacter;

        private Dictionary<string, float> trickRecordDict;

        private float stylePoints = 0f;
        private int styleRank = 0;

        private float lastTrickTime = 0f;
        private float lastUpsidedown = 0f;

        private int comboCounter = 1;

        private Vector3 climbStartPos = Vector3.zero;
        private Vector3 climbLastPos = Vector3.zero;
        private Vector3 climbLastNormal = Vector3.zero;

        private bool canTransfer = false;

        private bool wallSliding = false;
        private float wallSlideStart = 0f;
        private bool ropeClimbing = false;
        private bool vineClimbing = false;
        private bool vineGrinding = false;

        private bool cannonLaunched = false;

        private float lastGrasp = 0f;

        private float lavaHeatCounter = 0f;

        private bool passedOut = false;
        private float passedOutTime = 0f;
        private float passedOutPoints = 0f;

        public void Awake()
        {
            trickRecordDict = new Dictionary<string, float>();

            localStyleTracker = this;

            localCharacter = GetComponent<Character>();

            AttachEvents();
        }

        private void AttachEvents()
        {
            if (!localCharacter) return;

            localCharacter.startClimbAction += () => { SetClimbStart(); };
        }

        private List<StyleMod> GetActiveModifiers()
        {
            List<StyleMod> mods = new List<StyleMod>();

            if (!localCharacter) return mods;

            // General Stamina
            if (localCharacter.GetMaxStamina() <= 0.1f) mods.Add(new StyleMod("Death's Door", 1f));
            if (localCharacter.data.extraStamina >= 1f) mods.Add(new StyleMod("Happy", 0.25f));

            // Items
            if (localCharacter.refs.afflictions.HasAfflictionType(Affliction.AfflictionType.InfiniteStamina, out _)) mods.Add(new StyleMod("Sugar Rush", .25f));
            if (localCharacter.refs.afflictions.HasAfflictionType(Affliction.AfflictionType.FasterBoi, out _)) mods.Add(new StyleMod("Energized", .25f));
            if (localCharacter.refs.balloons.currentBalloonCount > 0) mods.Add(new StyleMod("Air Head", -0.75f));
            if (localCharacter.data.carriedPlayer) mods.Add(new StyleMod("Supportive", 1f));
            if (localCharacter.data.currentItem)
            {
                if (localCharacter.data.currentItem.name.Contains("BingBong", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Bing Bong", .5f));
                if (localCharacter.data.currentItem.name.Contains("Bugle", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Melodic", .5f));
                if (localCharacter.data.currentItem.name.Contains("Parasol", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Lightweight", -0.75f));
            }

            // Status Mods
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) >= .3f) mods.Add(new StyleMod("Ravenous", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Weight) >= .3f) mods.Add(new StyleMod("Loaded", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) >= .3f) mods.Add(new StyleMod("Blazing", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) >= .3f) mods.Add(new StyleMod("Chill", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Thorns) >= .3f) mods.Add(new StyleMod("Spiked", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Drowsy) >= .3f) mods.Add(new StyleMod("Dreamy", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison) >= .3f) mods.Add(new StyleMod("Toxic", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury) >= .3f) mods.Add(new StyleMod("Busted", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Curse) >= .3f) mods.Add(new StyleMod("Wicked", .5f));

            // Weather
            if (localCharacter.data.slippy > 0f) mods.Add(new StyleMod("Slippery", .25f));

            return mods;
        }

        private void SendStyleAction(string action, int points)
        {
            if (localCharacter.data.passedOut || localCharacter.data.fullyPassedOut || localCharacter.data.dead) return;

            // Cannot repeat a trick too quickly
            if (trickRecordDict.TryGetValue(action, out float prevTime) && Time.time - prevTime < INTERNAL_TRICK_COOLDOWN) return;

            int totalPoints = points;
            float totalModifier = 1.0f;
            string fullTrickName = action;

            foreach (StyleMod mod in GetActiveModifiers())
            {
                totalModifier += mod.Modifier;
                fullTrickName = mod.Descriptor + " " + fullTrickName;
            }

            // Don't modify negative points or pause style decay
            if (points > 0)
            {
                // Special case for combo
                if (Time.time - lastTrickTime <= 5f)
                {
                    if (Config.comboScaling.Value)
                    {
                        comboCounter++;
                        fullTrickName += " X" + comboCounter + " Combo";
                        totalModifier += 0.1f * (float)comboCounter;
                    }
                    else
                    {
                        fullTrickName += " Combo";
                        totalModifier += 0.25f;
                    }
                }
                else if (comboCounter != 1)
                {
                    comboCounter = 1;
                }

                totalPoints = Mathf.RoundToInt((float)points * totalModifier);
                lastTrickTime = Time.time;
            }

            stylePoints += totalPoints;
            StyleManager.ApplyStyleAction(fullTrickName, totalPoints, Time.time);

            trickRecordDict[action] = Time.time;
        }

        public void FixedUpdate()
        {
            if (!localCharacter) return;

            CheckCannon();
            CheckPassedOut();
            CheckClimbing();
            CheckForFlips();
            UpdateStyle();
        }

        private void CheckCannon()
        {
            if (localCharacter.data.launchedByCannon && !cannonLaunched)
            {
                cannonLaunched = true;
                SendStyleAction("Cannonball", 70);
            }
            else if (!localCharacter.data.launchedByCannon && cannonLaunched)
            {
                cannonLaunched = false;
            }
        }

        private void CheckPassedOut()
        {
            if ((localCharacter.refs.afflictions.statusSum >= 1f || localCharacter.data.passedOut) && !passedOut)
            {
                passedOutTime = Time.time;
                StyleManager.ApplyWipeout(Mathf.RoundToInt(stylePoints));
                passedOutPoints = stylePoints;
                stylePoints = 0;
                passedOut = true;
            }
            else if ((localCharacter.refs.afflictions.statusSum < 1f && !localCharacter.data.passedOut) && passedOut)
            {
                // Quick Revival!
                if (Time.time - passedOutTime < 5f)
                {
                    stylePoints = passedOutPoints + (100f * Config.rankMult.Value);

                    StyleManager.ApplyStyleAction("Close Call", Mathf.RoundToInt(passedOutPoints + (100f * Config.rankMult.Value)), Time.time);
                }

                passedOut = false;
            }
        }

        private void CheckClimbing()
        {
            if (localCharacter.data.isGrounded && canTransfer)
            {
                canTransfer = false;
            }

            // Wall
            if (localCharacter.data.isClimbing && !localCharacter.IsSliding())
            {
                // Check slide
                if (localCharacter.refs.climbing.playerSlide.y < 0f)
                {
                    if (!wallSliding)
                    {
                        wallSliding = true;
                        wallSlideStart = Time.time;
                    }
                }
                else
                {
                    if (wallSliding)
                    {
                        wallSliding = false;
                        if (Time.time - wallSlideStart >= 0.4f) SendStyleAction("Slide", 40);
                        SetClimbStart();
                    }
                    else
                    {
                        EvalClimbDistance(1f);
                    }
                }
            }
            else if (wallSliding)
            {
                wallSliding = false;
            }

            // Rope
            if (localCharacter.data.isRopeClimbing)
            {
                // We just started climbing
                if (!ropeClimbing)
                {
                    ropeClimbing = true;
                    SetClimbStart();
                }
                else
                {
                    EvalClimbDistance(4f);
                }
            }
            else if (ropeClimbing)
            {
                ropeClimbing = false;
            }

            // Vine
            if (localCharacter.data.isVineClimbing)
            {
                // We just started climbing
                if (!vineClimbing)
                {
                    vineClimbing = true;
                    SetClimbStart();
                }
                else
                {
                    // Special case for grinding
                    if (localCharacter.refs.vineClimbing.Sliding())
                    {
                        // We just started grinding
                        if (!vineGrinding)
                        {
                            vineGrinding = true;
                        }

                        if (Vector3.SqrMagnitude(localCharacter.TorsoPos() - climbLastPos) >= MIN_CLIMB * 4f)
                        {
                            SendStyleAction("Grind", 40);
                            climbLastPos = localCharacter.TorsoPos();
                        }
                    }
                    else if (vineGrinding && !localCharacter.refs.vineClimbing.Sliding())
                    {
                        // The grind has ended
                        vineGrinding = false;
                        SetClimbStart();
                    }
                    // Otherwise, it's just normal climbing
                    else 
                    {
                        EvalClimbDistance(4f);
                    }
                }
            }
            else if (vineClimbing)
            {
                vineClimbing = false;
                if (vineGrinding) vineGrinding = false;
            }
        }

        private void CheckForFlips()
        {
            if (Vector3.Angle(localCharacter.GetBodypart(BodypartType.Hip).transform.up, Vector3.down) < 20f)
            {
                lastUpsidedown = Time.time;
            }
            else if (Time.time - lastUpsidedown <= 5f && Vector3.Angle(localCharacter.GetBodypart(BodypartType.Hip).transform.up, Vector3.up) < 20f)
            {
                SendStyleAction("Flip", 40);
                lastUpsidedown = 0f;
            }
        }

        public void ItemEaten(Item item)
        {
            if (item.GetComponent<Action_RestoreHunger>() || item.GetComponent<Action_GiveExtraStamina>())
            {
                if (item.itemTags.HasFlag(Item.ItemTags.GourmandRequirement) && item.GetData<IntItemData>(DataEntryKey.CookedAmount).Value >= 1)
                {
                    SendStyleAction("Gourmand", 15);
                }
                else if (item.GetData<IntItemData>(DataEntryKey.CookedAmount).Value == 1)
                {
                    SendStyleAction("Meal", 10);
                }
                else
                {
                    SendStyleAction("Quaff", 5);
                }
            }
        }

        public void FedItem(Item item)
        {
            if (item.GetComponent<Action_RestoreHunger>() || item.GetComponent<Action_GiveExtraStamina>())
            {
                SendStyleAction("Waiter", 15);
            }
            else if (item.GetComponent<Action_ModifyStatus>())
            {
                Action_ModifyStatus status = item.GetComponent<Action_ModifyStatus>();
                if (status.changeAmount < 0f && (status.statusType == CharacterAfflictions.STATUSTYPE.Poison || status.statusType == CharacterAfflictions.STATUSTYPE.Injury))
                {
                    SendStyleAction("Medic", 25);
                }
            }
        }

        public void LuggageOpened(Luggage luggage)
        {
            // Respawn chest is a special case
            if (luggage is RespawnChest) return;

            if (luggage is LuggageCursed)
            {
                SendStyleAction("Risky Plunder", Mathf.RoundToInt(100f * Config.rankMult.Value));
            }
            else
            {
                SendStyleAction("Plunder", 10);
            }
        }

        private void SetClimbStart()
        {
            climbStartPos = localCharacter.TorsoPos();
            climbLastPos = climbStartPos;

            if (localCharacter.data.isClimbing)
            {
                if (canTransfer && Vector3.Angle(localCharacter.data.climbNormal, climbLastNormal) >= 80f)
                {
                    canTransfer = false;
                    SendStyleAction("Wall Transfer", 25);
                }

                canTransfer = true;
                climbLastNormal = localCharacter.data.climbNormal;
            }
            else if (canTransfer)
            {
                canTransfer = false;
            }
        }

        private void EvalClimbDistance(float distMultSq)
        {
            if (Vector3.SqrMagnitude(localCharacter.TorsoPos() - climbLastPos) >= MIN_CLIMB * distMultSq)
            {
                if (Vector3.SqrMagnitude(localCharacter.TorsoPos() - climbStartPos) < LONG_CLIMB * distMultSq)
                {
                    SendStyleAction("Climb", 10);
                }
                else
                {
                    SendStyleAction("Long Climb", 20);
                }

                climbLastPos = localCharacter.TorsoPos();

                if (localCharacter.data.isClimbing)
                {
                    climbLastNormal = localCharacter.data.climbNormal;
                }
            }
        }

        public void Fail()
        {
            if (localCharacter.refs.afflictions.HasAfflictionType(Affliction.AfflictionType.BingBongShield, out _)) return;

            stylePoints -= (100f * Config.rankMult.Value);
            StyleManager.ApplyFailure();
        }

        public void CloseFall()
        {
            SendStyleAction("Risky Landing", 25);
        }

        public void Grasp()
        {
            if (Time.time - lastGrasp >= 2.5f)
            {
                SendStyleAction("Helping Hand", 20);
                lastGrasp = Time.time;
            }
        }

        public void Campfire()
        {
            SendStyleAction("Campfire", 50);
        }

        public void RemovedThorn()
        {
            SendStyleAction("Medic", 10);
        }

        public void Cooked(int timesCooked)
        {
            if (timesCooked == 1)
            {
                SendStyleAction("Chef", 10);
            }
            if (timesCooked == 2)
            {
                SendStyleAction("Overcooked", 5);
            }
            if (timesCooked >= 3)
            {
                SendStyleAction("Fire Hazard", -10);
            }
        }

        public void RespawnChestActivated()
        {
            if (Ascents.canReviveDead && Character.PlayerIsDeadOrDown())
            {
                SendStyleAction("Rule 0", 50);
            }
            else
            {
                SendStyleAction("Perfect Ascent", 70);
            }
        }

        public void EffigyActivated()
        {
            SendStyleAction("Rule 0", 50);
        }

        public void FriendBoosted()
        {
            SendStyleAction("Boost Jump", 40);
        }

        public void FriendBooster()
        {
            SendStyleAction("Friend Boost", 30);
        }

        public void ScorpionSting()
        {
            SendStyleAction("Stinger", -50);
        }

        public void JellyfishSlip()
        {
            SendStyleAction("Slip", -25);
        }

        public void Tumbleweed()
        {
            SendStyleAction("Tumbled", -25);
        }

        public void LavaHeat(float heatAmount)
        {
            lavaHeatCounter += heatAmount;

            if (lavaHeatCounter > 0.12f)
            {
                lavaHeatCounter = 0f;
                SendStyleAction("Hot Feet", 25);
            }
        }

        public void Bonk(Item item, Character character)
        {
            if (item.lastThrownCharacter && item.lastThrownCharacter == localCharacter)
            {
                SendStyleAction("Bonk", 25);
            }
            if (character == localCharacter)
            {
                SendStyleAction("Bonked", -25);
            }
        }

        public void ItemCaught(Item item, Character character, Vector3 thrownPos)
        {
            // Item must be thrown for at least a half second
            if (Time.time - item.lastThrownTime >= 0.5f)
            {
                // If the item is moving
                if (item.GetComponent<Rigidbody>() && item.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > (3f * 3f))
                {
                    float throwDistSqr = (item.transform.position - thrownPos).sqrMagnitude;
                    float longThrowDistSqr = 10f * 10f;

                    // We were the thrower
                    // Can't evaluate this without networking
                    //if (item.lastThrownCharacter == localCharacter)
                    //{
                    //    if (throwDistSqr >= longThrowDistSqr) 
                    //    {
                    //        SendStyleAction("Long Pass", 40);
                    //    } 
                    //    else
                    //    {
                    //        SendStyleAction("Pass", 25);
                    //    }
                    //}

                    // We were the catcher
                    if (character == localCharacter)
                    {
                        if (throwDistSqr >= longThrowDistSqr)
                        {
                            SendStyleAction("Long Catch", 40);
                        }
                        else
                        {
                            SendStyleAction("Catch", 25);
                        }
                    }
                }
            }
        }

        private void UpdateStyle()
        {
            if (stylePoints > MAX_STYLE) stylePoints = MAX_STYLE;

            float timeSinceLastTrick = Time.time - lastTrickTime;

            // Calculate Style decay
            if (timeSinceLastTrick > TRICK_GRACE)
            {
                float decay = FAST_DECAY;
                if (localCharacter.data.isSprinting || localCharacter.data.isJumping || !localCharacter.data.isGrounded) decay = SLOW_DECAY;

                // Decay faster at S ranks
                if (styleRank == 4) decay *= 2f;
                if (styleRank == 5) decay *= 2.5f;
                if (styleRank == 6) decay *= 3f;

                decay *= Config.decayMult.Value;

                stylePoints -= decay;
            }

            if (stylePoints < 0) stylePoints = 0;

            // We need to update the style ranking
            int newRank = Mathf.FloorToInt(stylePoints / (100f * Config.rankMult.Value));
            if (newRank < 0) newRank = 0;
            if (newRank > 6) newRank = 6;
            if (newRank != styleRank)
            {
                StyleManager.UpdateStyleRank(newRank);
                styleRank = newRank;
            }

            StyleManager.UpdateStyleFill((stylePoints / (100f * Config.rankMult.Value)) - (float)styleRank);
        }
    }
}
