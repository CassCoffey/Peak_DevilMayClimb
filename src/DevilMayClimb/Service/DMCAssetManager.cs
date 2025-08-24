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

            dRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("DRank")[1]);
            cRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("CRank")[1]);
            bRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("BRank")[1]);
            aRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("ARank")[1]);
            sRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SRank")[1]);
            ssRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSRank")[1]);
            sssRank = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSSRank")[1]);

            dRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("DRankFill")[1]);
            cRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("CRankFill")[1]);
            bRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("BRankFill")[1]);
            aRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("ARankFill")[1]);
            sRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SRankFill")[1]);
            ssRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSRankFill")[1]);
            sssRankFill = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSSRankFill")[1]);

            dTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("DTitle")[1]);
            cTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("CTitle")[1]);
            bTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("BTitle")[1]);
            aTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("ATitle")[1]);
            sTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("STitle")[1]);
            ssTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSTitle")[1]);
            sssTitle = (Sprite)(Plugin.DMCAssets.LoadAssetWithSubAssets("SSSTitle")[1]);

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
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100, 1, SpriteMeshType.Tight);
        }
    }
}
