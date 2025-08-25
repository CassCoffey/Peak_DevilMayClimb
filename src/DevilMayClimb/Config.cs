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
        }
    }
}
