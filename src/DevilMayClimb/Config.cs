using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevilMayClimb
{
    public class Config
    {
        public static ConfigEntry<float> styleVolume;
        public static ConfigEntry<float> decayMult;
        public static ConfigEntry<float> rankMult;

        public Config(ConfigFile cfg)
        {
            // General
            styleVolume = cfg.Bind(
                    "General",
                    "styleVolume",
                    0.5f,
                    "The volume of the Style audio (0-1)."
            );

            decayMult = cfg.Bind(
                    "General",
                    "decayMult",
                    1f,
                    "The multplier for Style Decay. Higher numbers = Faster decay."
            );

            rankMult = cfg.Bind(
                    "General",
                    "rankMult",
                    1f,
                    "The multplier for Style Rank size. Higher numbers means more points needed to hit the next Style Rank. 1 = 100pts to rank up."
            );
        }
    }
}
