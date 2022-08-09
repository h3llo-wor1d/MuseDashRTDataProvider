using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Assets.Scripts.GameCore.HostComponent;
using Assets.Scripts.PeroTools.Commons;
using GameLogic;
using HarmonyLib;
using UnityEngine.AddressableAssets;

namespace MuseDashRTDataProvider
{

    [HarmonyPatch(typeof(BattleEnemyManager), "SetPlayResult")]
    internal class SetPlayPatch
    {
        private static void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
        {
            string key = "";
            switch ((int)result)
            {
                case 1:
                    key = "hitMiss";
                    break;
                case 3:
                    key = "hitGreat";
                    break;
                case 4:
                    key = "hitPerfect";
                    break;
            }
            if (key != "")
            {
                Main.handleNewScore(key);
            }     
        }
    }

    public class Main : MelonMod
    {

        public static Dictionary<string, object> gameFeed = new Dictionary<string, object>();
        public static websocketServer server = new websocketServer();
        public Task serverInstance = null;

        public static void addToKey(string key)
        {
            int cur = (int)gameFeed[key];
            gameFeed[key] = cur + 1;
        }

        public static void handleNewScore(string key)
        {
            if (key == "hitMiss")
            {
                gameFeed["comboCount"] = 0;
            } else
            {
                addToKey("comboCount");
            }
            Console.WriteLine($"Debug: handling new score for key {key}");
            addToKey(key);
            clientUpdate();
        }

        public static void clientUpdate()
        {
            // Basically just copied and pasted from my Celeste mod lol
            try
            {
                Console.WriteLine($"Debug: sending message with payload {JsonConvert.SerializeObject(gameFeed)}");
                server.sendMessage(JsonConvert.SerializeObject(gameFeed));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MuseDashRTDataProvider: an error has occured with clientUpdate: {ex}");
            }
        }

        private void resetFeed()
        {
            // Init all variables
            gameFeed["hitMiss"] = 0;
            gameFeed["hitGreat"] = 0;
            gameFeed["hitPerfect"] = 0;
            gameFeed["comboCount"] = 0;
        }

        public override void OnApplicationStart()
        {
            resetFeed();
            serverInstance = Task.Run(() => server.startServer(8080));
            LoggerInstance.Msg("MuseDashRTDataProvider v0.0.1 started...");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "GameMain") {}
            else
            {
                resetFeed();
                clientUpdate();
            }
        }

    }
}