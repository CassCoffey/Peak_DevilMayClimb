using DevilMayClimb.Service;
using Peak.Afflictions;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevilMayClimb.Monobehavior
{
    public class StyleTracker : MonoBehaviour
    {
        public const float RANK_POINTS = 100f;
        public const float MAX_STYLE = RANK_POINTS * 7f;

        private const float SLOW_DECAY = 0.1f;
        private const float FAST_DECAY = 0.5f;
        private const float TRICK_GRACE = 5f;

        private Character? localCharacter;
        private Player? localPlayer;

        private float stylePoints = 0f;
        private int styleRank = 0;

        private float lastTrickTime = 0f;

        public void Awake()
        {
            localCharacter = Character.localCharacter;
            localPlayer = Player.localPlayer;

            AttachEvents();
        }

        private void AttachEvents()
        {
            if (!localCharacter) return;

            localCharacter.jumpAction += () => { SendStyleAction("Jump", 40); };
        }

        private List<StyleMod> GetActiveModifiers()
        {
            List<StyleMod> mods = new List<StyleMod>();

            if (!localCharacter) return mods;

            // Status Mods
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger) >= .3f) mods.Add(new StyleMod("Gluttonous", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Weight) >= .3f) mods.Add(new StyleMod("Loaded", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hot) >= .3f) mods.Add(new StyleMod("Blazing", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Cold) >= .3f) mods.Add(new StyleMod("Chill", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Thorns) >= .3f) mods.Add(new StyleMod("Spiked", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Drowsy) >= .3f) mods.Add(new StyleMod("Dreamy", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison) >= .3f) mods.Add(new StyleMod("Toxic", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Injury) >= .3f) mods.Add(new StyleMod("Busted", .5f));
            if (localCharacter.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Curse) >= .3f) mods.Add(new StyleMod("Wicked", .5f));

            // General Stamina
            if (localCharacter.GetMaxStamina() <= 0.1f) mods.Add(new StyleMod("Death's Door", 1f));
            if (localCharacter.data.extraStamina >= 1f) mods.Add(new StyleMod("Happy", 0.5f));

            // Items
            if (localCharacter.refs.afflictions.HasAfflictionType(Affliction.AfflictionType.InfiniteStamina, out _)) mods.Add(new StyleMod("Sugar Rush", .5f));
            if (localCharacter.refs.afflictions.HasAfflictionType(Affliction.AfflictionType.FasterBoi, out _)) mods.Add(new StyleMod("Energized", .5f));
            if (localCharacter.refs.balloons.currentBalloonCount > 0) mods.Add(new StyleMod("Air Head", -0.75f));
            if (localCharacter.data.carriedPlayer) mods.Add(new StyleMod("Supportive", 1f));
            if (localCharacter.data.currentItem)
            {
                if (localCharacter.data.currentItem.name.Contains("BingBong", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Bing Bong", .5f));
                if (localCharacter.data.currentItem.name.Contains("Bugle", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Melodic", .5f));
                if (localCharacter.data.currentItem.name.Contains("Parasol", StringComparison.CurrentCultureIgnoreCase)) mods.Add(new StyleMod("Lightweight", -0.75f));
            }

            return mods;
        }

        private void SendStyleAction(string action, int points)
        {
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
                totalModifier += 0.5f;
            }

            totalPoints = Mathf.RoundToInt((float)points * totalModifier);

            stylePoints += totalPoints;
            StyleManager.ApplyStyleAction(fullTrickName, totalPoints, Time.time);

            lastTrickTime = Time.time;
        }

        public void FixedUpdate()
        {
            if (!localCharacter) return;

            if (stylePoints > MAX_STYLE) stylePoints = MAX_STYLE;

            float timeSinceLastTrick = Time.time - lastTrickTime;

            // Calculate Style decay
            if (timeSinceLastTrick > TRICK_GRACE) 
            {
                float decay = FAST_DECAY;
                if (localCharacter.data.isSprinting || localCharacter.data.isJumping) decay = SLOW_DECAY;

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
