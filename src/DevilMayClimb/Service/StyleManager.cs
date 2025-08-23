using System;
using System.Collections.Generic;
using System.Text;

namespace DevilMayClimb.Service
{
    public static class StyleManager
    {
        private static Player? LocalPlayer;
        private static Character? LocalCharacter;

        public static void RegisterPlayer(Player localPlayer)
        {
            LocalPlayer = localPlayer;

            Plugin.Log.LogInfo("Registering player");
        }

        public static void RegisterCharacter(Character localCharacter)
        {
            LocalCharacter = localCharacter;

            Plugin.Log.LogInfo("Registering char");

            LocalCharacter.jumpAction += () => { ApplyStyleAction("jump"); };
        }

        public static void ApplyStyleAction(string action)
        {
            Plugin.Log.LogInfo("Jumped!");
        }
    }
}
