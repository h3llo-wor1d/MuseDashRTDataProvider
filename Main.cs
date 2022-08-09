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

namespace MuseDashRTDataProvider
{

    [HarmonyPatch(typeof(BattleEnemyManager), "SetPlayResult")]
    public class SetPlayPatch
    {
        public void Postfix(int idx, byte result, bool isMulStart = false, bool isMulEnd = false, bool isLeft = false)
        {
            Console.WriteLine($"Switching for result {result}");
            switch (result)
            {

            }
        }
    }

    public class Main : MelonMod
    {

        public static Dictionary<string, object> gameFeed = new Dictionary<string, object>();
        public static websocketServer server = new websocketServer();
        public Task serverInstance = null;

        public static void clientUpdate()
        {
            // Basically just copied and pasted from my Celeste mod lol
            try
            {
                server.sendMessage(JsonConvert.SerializeObject(gameFeed));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MuseDashRTDataProvider: an error has occured with clientUpdate: {ex}");
            }
        }

        public void addListing(int id, string name)
        {
            List<int> list = (List<int>)gameFeed[name];
            list.Add(id);
            gameFeed[name] = list;
        }

        private void resetFeed()
        {
            // Init all variables
            gameFeed["collectableNoteMiss"] = 0;
            gameFeed["ghostMiss"] = 0;
            gameFeed["greatNum"] = 0;
        }

        public void removeListing(int id, string name)
        {
            try
            {
                List<int> list = (List<int>)gameFeed[name];
                list.Remove(id);
                gameFeed[name] = list;
            }
            catch
            {
                LoggerInstance.Msg("err: removeListing failed. Continuing...");
            }

        }

        public override void OnApplicationStart()
        {
            resetFeed();
            serverInstance = Task.Run(() => server.startServer(8080));
            LoggerInstance.Msg("MuseDashRTDataProvider v0.0.1 started...");
        }

    }
}