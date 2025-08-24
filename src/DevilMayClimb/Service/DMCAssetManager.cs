using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevilMayClimb.Service
{
    public class DMCAssetManager
    {
        public static AudioClip wipeout;
        public static AudioClip minorTrick_0;
        public static AudioClip trick_0;
        public static AudioClip trick_1;
        public static AudioClip trick_2;
        public static AudioClip trick_3;
        public static AudioClip trick_4;

        public static Sprite dRank;
        public static Sprite cRank;
        public static Sprite bRank;
        public static Sprite aRank;
        public static Sprite sRank;
        public static Sprite ssRank;
        public static Sprite sssRank;

        public static Sprite dRankFill;
        public static Sprite cRankFill;
        public static Sprite bRankFill;
        public static Sprite aRankFill;
        public static Sprite sRankFill;
        public static Sprite ssRankFill;
        public static Sprite sssRankFill;

        public static Sprite dTitle;
        public static Sprite cTitle;
        public static Sprite bTitle;
        public static Sprite aTitle;
        public static Sprite sTitle;
        public static Sprite ssTitle;
        public static Sprite sssTitle;

        public static GameObject styleUIPrefab;
        public static GameObject trickUIPanelPrefab;

        public static AudioClip[] trickChain;

        public static Sprite[] ranks;
        public static Sprite[] fills;
        public static Sprite[] titles;

        public static void Init()
        {
            Plugin.Log.LogInfo($"Loading Assets...");

            styleUIPrefab = (GameObject)Plugin.DMCAssets.LoadAsset("StyleGroup");
            trickUIPanelPrefab = (GameObject)Plugin.DMCAssets.LoadAsset("TrickUIPrefab");

            dRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("DRank"));
            cRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("CRank"));
            bRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("BRank"));
            aRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("ARank"));
            sRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SRank"));
            ssRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSRank"));
            sssRank = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSSRank"));

            dRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("DRankFill"));
            cRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("CRankFill"));
            bRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("BRankFill"));
            aRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("ARankFill"));
            sRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SRankFill"));
            ssRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSRankFill"));
            sssRankFill = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSSRankFill"));

            dTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("DTitle"));
            cTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("CTitle"));
            bTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("BTitle"));
            aTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("ATitle"));
            sTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("STitle"));
            ssTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSTitle"));
            sssTitle = ConvertToSprite((Texture2D)Plugin.DMCAssets.LoadAsset("SSSTitle"));

            wipeout = (AudioClip)Plugin.DMCAssets.LoadAsset("Wipeout");
            minorTrick_0 = (AudioClip)Plugin.DMCAssets.LoadAsset("MinorTrick_0");
            trick_0 = (AudioClip)Plugin.DMCAssets.LoadAsset("Trick_0");
            trick_1 = (AudioClip)Plugin.DMCAssets.LoadAsset("Trick_1");
            trick_2 = (AudioClip)Plugin.DMCAssets.LoadAsset("Trick_2");
            trick_3 = (AudioClip)Plugin.DMCAssets.LoadAsset("Trick_3");
            trick_4 = (AudioClip)Plugin.DMCAssets.LoadAsset("Trick_4");

            trickChain = [trick_0, trick_1, trick_2, trick_3, trick_4];
            ranks = [dRank, cRank, bRank, aRank, sRank, ssRank, sssRank];
            fills = [dRankFill, cRankFill, bRankFill, aRankFill, sRankFill, ssRankFill, sssRankFill];
            titles = [dTitle, cTitle, bTitle, aTitle, sTitle, ssTitle, sssTitle];
        }

        private static Sprite ConvertToSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
