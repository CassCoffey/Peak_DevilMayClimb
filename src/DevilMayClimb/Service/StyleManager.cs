using DevilMayClimb.Monobehavior;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
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
        private static Player? LocalPlayer;
        private static Character? LocalCharacter;

        private static GameObject StyleUI;
        private static Transform TrickHistory;
        private static AudioSource StyleAudio;

        private static float lastTrickTime;
        private static int trickChain = 0;

        public static void RegisterPlayer(Player localPlayer)
        {
            LocalPlayer = localPlayer;

            Plugin.Log.LogInfo("Registering player");

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

            Plugin.Log.LogInfo("Registering char");
        }

        public static void AttachHUD()
        {
            StyleUI = GameObject.Instantiate(DMCAssetManager.styleUIPrefab, GUIManager.instance.hudCanvas.transform);
            TrickHistory = StyleUI.transform.Find("StylePanel/TrickHistory");
            StyleAudio = StyleUI.GetComponent<AudioSource>();
        }

        public static void ApplyStyleAction(string action, int points, float time)
        {
            // Handle trick
            if (points < 50)
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

            AddTrickHistory(action + "! +" + points);
        }

        public static void UpdateStyleRank(int rank)
        {
            Image rankImage = StyleUI.transform.Find("StylePanel/Rank").GetComponent<Image>();
            Image rankFillImage = StyleUI.transform.Find("StylePanel/Rank/RankFill").GetComponent<Image>();
            Image titleImage = StyleUI.transform.Find("StylePanel/Title").GetComponent<Image>();

            rankImage.sprite = DMCAssetManager.ranks[rank];
            rankFillImage.sprite = DMCAssetManager.fills[rank];
            titleImage.sprite = DMCAssetManager.titles[rank];
        }

        public static void UpdateStyleFill(float percent)
        {
            Image rankFillImage = StyleUI.transform.Find("StylePanel/Rank/RankFill").GetComponent<Image>();

            rankFillImage.fillAmount = percent;
        }

        private static void AddTrickHistory(string text)
        {
            GameObject trick = GameObject.Instantiate(DMCAssetManager.trickUIPanelPrefab, TrickHistory);
            trick.transform.Find("TrickText").GetComponent<TextMeshProUGUI>().text = text;
            trick.AddComponent<TrickFadout>();
        }
    }
}
