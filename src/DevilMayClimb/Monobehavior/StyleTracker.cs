using DevilMayClimb.Service;
using Peak.Afflictions;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI.Extensions.Tweens;

namespace DevilMayClimb.Monobehavior
{
    public class StyleTracker : MonoBehaviour
    {
        public static StyleTracker localStyleTracker;

        public const float RANK_POINTS = 100f;
        public const float MAX_STYLE = RANK_POINTS * 7f;

        private const float SLOW_DECAY = 0.02f;
        private const float FAST_DECAY = 0.04f;
        private const float TRICK_GRACE = 5f;

        // Squared to avoid using sqrt
        private const float MIN_CLIMB = 2f * 2f;
        private const float LONG_CLIMB = 10f * 10f;

        private Character? localCharacter;

        private float stylePoints = 0f;
        private int styleRank = 0;

        private float lastTrickTime = 0f;
        private float lastUpsidedown = 0f;

        private Vector3 climbStartPos = Vector3.zero;
        private Vector3 climbLastPos = Vector3.zero;

        private bool wallSliding = false;
        private float wallSlideStart = 0f;
        private bool ropeClimbing = false;
        private bool vineClimbing = false;
        private bool vineGrinding = false;

        private bool cannonLaunched = false;

        private bool passedOut = false;
        private float passedOutTime = 0f;
        private float passedOutPoints = 0f;

        public void Awake()
        {
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
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) >= .3f) mods.Add(new StyleMod("Gluttonous", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Weight) >= .3f) mods.Add(new StyleMod("Loaded", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) >= .3f) mods.Add(new StyleMod("Blazing", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) >= .3f) mods.Add(new StyleMod("Chill", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Thorns) >= .3f) mods.Add(new StyleMod("Spiked", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Drowsy) >= .3f) mods.Add(new StyleMod("Dreamy", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison) >= .3f) mods.Add(new StyleMod("Toxic", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury) >= .3f) mods.Add(new StyleMod("Busted", .25f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Curse) >= .3f) mods.Add(new StyleMod("Wicked", .5f));

            return mods;
        }

        private void SendStyleAction(string action, int points)
        {
            if (localCharacter.data.passedOut || localCharacter.data.fullyPassedOut || localCharacter.data.dead) return;

            int totalPoints = points;
            float totalModifier = 1.0f;
            string fullTrickName = action;

            foreach (StyleMod mod in GetActiveModifiers())
            {
                totalModifier += mod.Modifier;
                fullTrickName = mod.Descriptor + " " + fullTrickName;
            }

            // Special case for combo
            if (Time.time - lastTrickTime <= 5f)
            {
                fullTrickName += " Combo";
                totalModifier += 0.25f;
            }

            totalPoints = Mathf.RoundToInt((float)points * totalModifier);

            stylePoints += totalPoints;
            StyleManager.ApplyStyleAction(fullTrickName, totalPoints, Time.time);

            lastTrickTime = Time.time;
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
                    stylePoints = passedOutPoints + 100f;

                    StyleManager.ApplyStyleAction("Close Call", Mathf.RoundToInt(passedOutPoints + 100f), Time.time);
                }

                passedOut = false;
            }
        }

        private void CheckClimbing()
        {
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

        public void LuggageOpened()
        {
            SendStyleAction("Plunder", 10);
        }

        private void SetClimbStart()
        {
            climbStartPos = localCharacter.TorsoPos();
            climbLastPos = climbStartPos;
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
            }
        }

        public void Fail()
        {
            stylePoints -= 100f;
            StyleManager.ApplyFailure();
        }

        public void CloseFall()
        {
            SendStyleAction("Risky Landing", 25);
        }

        public void Grasp()
        {
            SendStyleAction("Helping Hand", 20);
        }

        public void Campfire()
        {
            SendStyleAction("Campfire", 50);
        }

        public void Cooked()
        {
            SendStyleAction("Chef", 10);
        }

        public void FriendBoosted()
        {
            SendStyleAction("Boost Jump", 40);
        }

        public void FriendBooster()
        {
            SendStyleAction("Friend Boost", 30);
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
                if (styleRank > 3) decay *= 2f;

                stylePoints -= decay;
            }

            if (stylePoints < 0) stylePoints = 0;

            // We need to update the style ranking
            int newRank = Mathf.FloorToInt(stylePoints / 100f);
            if (newRank < 0) newRank = 0;
            if (newRank > 6) newRank = 6;
            if (newRank != styleRank)
            {
                StyleManager.UpdateStyleRank(newRank);
                styleRank = newRank;
            }

            StyleManager.UpdateStyleFill((stylePoints / 100f) - (float)styleRank);
        }
    }
}
