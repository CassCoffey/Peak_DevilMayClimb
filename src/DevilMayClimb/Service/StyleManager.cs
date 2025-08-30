using DevilMayClimb.Monobehavior;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace DevilMayClimb.Service
{
    public struct StyleMod
    {
        public string Descriptor;
        public float Modifier;

        public StyleMod(string descriptor, float modifier)
        {
            Descriptor = descriptor;
            Modifier = modifier;
        }
    }

    public static class StyleManager
    {
        private static Character? LocalCharacter;

        private static GameObject StyleUI;
        private static Transform TrickHistory;
        private static AudioSource StyleAudio;

        private static Image rankImage;
        private static Image rankFillImage;
        private static Image titleImage;

        private static Transform ComboPanel;
        private static TextMeshProUGUI ComboNumber;
        private static TextMeshProUGUI ComboPoints;

        private static int comboCounterTotalPoints = 0;

        private static int previousStyleRank = 0;

        private static float lastTrickTime;
        private static int trickChain = 0;

        private static Color32 SUCCESS_COLOR = new Color32(86, 197, 0, 255);
        private static Color32 FAILURE_COLOR = new Color32(195, 12, 21, 255);

        private static string[] FAIL_LINES = ["Oof", "Ouch", "Owie"];

        private static bool init = false;

        private static Item lastThrownItem;
        private static Vector3 lastThrownItemPos;

        private static void Init()
        {
            GlobalEvents.OnItemConsumed = (Action<Item, Character>)Delegate.Combine(GlobalEvents.OnItemConsumed, new Action<Item, Character>(CheckItemEaten));
            GlobalEvents.OnLuggageOpened = (Action<Luggage, Character>)Delegate.Combine(GlobalEvents.OnLuggageOpened, new Action<Luggage, Character>(CheckLuggageOpened));
            GlobalEvents.OnItemThrown = (Action<Item>)Delegate.Combine(GlobalEvents.OnItemThrown, new Action<Item>(CheckItemThrown));
            GlobalEvents.OnItemRequested = (Action<Item, Character>)Delegate.Combine(GlobalEvents.OnItemRequested, new Action<Item, Character>(CheckItemCaught));

            init = true;
        }

        public static void RegisterPlayer(Player localPlayer)
        {
            if (!init) Init();

            AttachHUD();
        }

        public static void RegisterCharacter(Character localCharacter)
        {
            // cleanup old char
            if (LocalCharacter != null)
            {
                GameObject.Destroy(LocalCharacter.gameObject.GetComponent<StyleTracker>());
            }

            LocalCharacter = localCharacter;
            LocalCharacter.gameObject.AddComponent<StyleTracker>();
        }

        public static void AttachHUD()
        {
            StyleUI = GameObject.Instantiate(DMCAssetManager.styleUIPrefab, GUIManager.instance.hudCanvas.transform);
            TrickHistory = StyleUI.transform.Find("StylePanel/TrickHistory");
            StyleAudio = StyleUI.GetComponent<AudioSource>();
            StyleAudio.volume = Config.styleVolume.Value;

            rankImage = StyleUI.transform.Find("StylePanel/Rank").GetComponent<Image>();
            rankFillImage = StyleUI.transform.Find("StylePanel/Rank/RankFill").GetComponent<Image>();
            titleImage = StyleUI.transform.Find("StylePanel/Title").GetComponent<Image>();

            rankFillImage.gameObject.AddComponent<FillBounce>();

            ComboPanel = StyleUI.transform.Find("StylePanel/ComboPanel");
            ComboNumber = ComboPanel.Find("ComboNumber").GetComponent<TextMeshProUGUI>();
            ComboPoints = ComboPanel.Find("ComboPoints").GetComponent<TextMeshProUGUI>();
        }

        public static void ApplyStyleAction(string action, int points, float time, int comboCount)
        {
            // Handle trick
            if (points < 0)
            {
                StyleAudio.PlayOneShot(DMCAssetManager.fail);
            }
            else if (points < 50)
            {
                StyleAudio.PlayOneShot(DMCAssetManager.minorTrick_0);
            } 
            else
            {
                float timeSinceLastTrick = time - lastTrickTime;
                lastTrickTime = time;
                // We're chaining
                if (timeSinceLastTrick < 5f)
                {
                    trickChain++;
                    if (trickChain > 4) trickChain = 4;
                    StyleAudio.PlayOneShot(DMCAssetManager.trickChain[trickChain]);
                }
                else
                {
                    trickChain = 0;
                    StyleAudio.PlayOneShot(DMCAssetManager.trick_0);
                }
            }

            if (points < 0)
            {
                AddTrickHistory(action + "! " + points, FAILURE_COLOR);
            } 
            else
            {
                AddTrickHistory(action + "! +" + points, SUCCESS_COLOR);
            }

            UpdateComboCounter(points, comboCount);
        }

        public static void ApplyFailure()
        {
            StyleAudio.PlayOneShot(DMCAssetManager.fail);

            AddTrickHistory(FAIL_LINES[UnityEngine.Random.Range(0, 3)] + "! -" + (100f * Config.rankMult.Value), FAILURE_COLOR);
        }

        public static void ApplyWipeout(int points)
        {
            StyleAudio.PlayOneShot(DMCAssetManager.wipeout);

            AddTrickHistory("Wipeout! -" + points, FAILURE_COLOR);
        }

        public static void UpdateComboCounter(int points, int comboCount)
        {
            if (comboCount == 2)
            {
                ComboPanel.GetComponent<Animator>().Play("ComboAppear");
            }

            ComboNumber.text = comboCount.ToString();
            comboCounterTotalPoints += points;
            ComboPoints.text = comboCounterTotalPoints + " pts";
        }

        public static void DropCombo()
        {
            ComboPanel.GetComponent<Animator>().Play("ComboDisappear");

            comboCounterTotalPoints = 0;
        }

        public static void UpdateStyleRank(int rank)
        {
            rankImage.sprite = DMCAssetManager.ranks[rank];
            rankFillImage.sprite = DMCAssetManager.fills[rank];
            titleImage.sprite = DMCAssetManager.titles[rank];

            if (previousStyleRank < rank) rankFillImage.fillAmount = 0f;
            if (previousStyleRank > rank) rankFillImage.fillAmount = 1f;

            rankImage.GetComponent<Animator>().Play("RankAnimation");
            titleImage.GetComponent<Animator>().Play("TitleAnimation");

            previousStyleRank = rank;
        }

        public static void UpdateStyleFill(float percent)
        {
            if (percent != rankFillImage.fillAmount && percent - rankFillImage.fillAmount > 0.1f)
            {
                rankFillImage.GetComponent<FillBounce>().SetGoal(percent);
            }
            else
            {
                rankFillImage.fillAmount = percent;
            }
        }

        private static void AddTrickHistory(string text, Color32 color)
        {
            GameObject trick = GameObject.Instantiate(DMCAssetManager.trickUIPanelPrefab, TrickHistory);
            trick.transform.Find("TrickText").GetComponent<TextMeshProUGUI>().text = text;
            trick.transform.Find("TrickText").GetComponent<TextMeshProUGUI>().color = color;
            trick.AddComponent<TrickFadout>();
        }

        private static void CheckItemEaten(Item item, Character character)
        {
            if (!character.IsLocal || !StyleTracker.localStyleTracker) return;
            
            StyleTracker.localStyleTracker.ItemEaten(item);
        }

        private static void CheckLuggageOpened(Luggage luggage, Character character)
        {
            if (!character.IsLocal || !StyleTracker.localStyleTracker) return;
            
            StyleTracker.localStyleTracker.LuggageOpened(luggage);
        }

        private static void CheckItemThrown(Item item)
        {
            lastThrownItem = item;
            lastThrownItemPos = item.transform.position;
        }

        public static void CheckItemCaught(Item item, Character character)
        {
            if (!StyleTracker.localStyleTracker || lastThrownItem != item) return;

            StyleTracker.localStyleTracker.ItemCaught(item, character, lastThrownItemPos);
        }
    }
}
