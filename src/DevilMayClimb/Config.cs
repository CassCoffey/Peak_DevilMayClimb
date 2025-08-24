using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevilMayClimb
{
    public class Config
    {
        public static ConfigEntry<float> styleVolume;

        public Config(ConfigFile cfg)
        {
            // General
            styleVolume = cfg.Bind(
                    "General",
                    "styleVolume",
                    0.5f,
                    "The volume of the Style audio (0-1)."
            );
        }
    }
}
