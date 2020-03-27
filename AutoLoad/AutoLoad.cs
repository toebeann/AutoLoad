using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using Straitjacket.Subnautica.Mods.AutoLoad.SMLHelper.V2.Options;
using Straitjacket.Subnautica.Mods.AutoLoad.SMLHelper.V2.Utility;
using QModManager.API;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal class AutoLoad : MonoBehaviour
    {
        private static AutoLoad main;
        public static AutoLoad Singleton()
        {
            if (!main)
            {
                main = new GameObject("AutoLoad").AddComponent<AutoLoad>();
            }
            return main;
        }
        public static Coroutine RunCoroutine(IEnumerator coroutine) => Singleton().StartCoroutine(coroutine);
        public static bool Startup { get; private set; } = true;

        public static IEnumerable<IQMod> FailedMods { get; private set; }
        private static bool modCheckComplete = false;
        public static void CheckLoadedMods()
        {
            FailedMods = QModServices.Main.GetAllMods().Where(mod => mod.Enable && !mod.IsLoaded);
            modCheckComplete = true;
        }

        public static IEnumerator OnGuiInitialized(StartScreen startScreen)
        {
            if (Startup && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
            {
                yield return new WaitUntil(() => modCheckComplete);

                if (!FailedMods.Any() && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
                {
                    yield return new WaitWhile(() => SaveLoadManager.main == null);
                    yield return SaveLoadManager.main.LoadSlotsAsync();

                    string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
                    if (KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
                    {
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else if (!activeSlotNames.Any())
                    {
                        Console.WriteLine("[AutoLoad] No active save slots found, initialising StartScreen GUI.");
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else
                    {
                        Console.WriteLine("[AutoLoad] Beginning load...");
                        LoadMostRecentSavedGame(activeSlotNames);
                    }
                }
                else if (!KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
                {
                    Console.WriteLine("[AutoLoad] Detected the following mods were not loaded:");
                    foreach (var mod in FailedMods)
                    {
                        Console.WriteLine($"[AutoLoad]     {mod.DisplayName}");
                    }
                    Console.WriteLine("[AutoLoad] Skipping AutoLoad.");
                    yield return RunCoroutine(startScreen.Load());
                }
                else
                {
                    yield return RunCoroutine(startScreen.Load());
                }
            }
            else
            {
                yield return RunCoroutine(startScreen.Load());
            }
            Startup = false;
        }

        /// <summary>
        /// Copied from uGUI_MainMenu, altered to work statically
        /// </summary>
        public static void LoadMostRecentSavedGame(string[] activeSlotNames)
        {
            long num = 0L;
            SaveLoadManager.GameInfo gameInfo = null;
            string saveGame = string.Empty;
            int i = 0;
            int num2 = activeSlotNames.Length;
            while (i < num2)
            {
                SaveLoadManager.GameInfo gameInfo2 = SaveLoadManager.main.GetGameInfo(activeSlotNames[i]);
                if (gameInfo2.dateTicks > num)
                {
                    gameInfo = gameInfo2;
                    num = gameInfo2.dateTicks;
                    saveGame = activeSlotNames[i];
                }
                i++;
            }
            if (gameInfo != null)
            {
                RunCoroutine(LoadGameAsync(saveGame, gameInfo.changeSet, gameInfo.gameMode));
            }
        }

        private static bool isStartingNewGame = false;
        /// <summary>
        /// Copied from uGUI_MainMenu, altered to work statically
        /// </summary>
        /// <param name="saveGame"></param>
        /// <param name="changeSet"></param>
        /// <param name="gameMode"></param>
        /// <returns></returns>
        public static IEnumerator LoadGameAsync(string saveGame, int changeSet, GameMode gameMode)
        {
            if (isStartingNewGame)
            {
                yield break;
            }
            isStartingNewGame = true;
            FPSInputModule.SelectGroup(null, false);
            uGUI.main.loading.ShowLoadingScreen();
            yield return BatchUpgrade.UpgradeBatches(saveGame, changeSet);
            global::Utils.SetContinueMode(true);
            global::Utils.SetLegacyGameMode(gameMode);
            SaveLoadManager.main.SetCurrentSlot(Path.GetFileName(saveGame));
            VRLoadingOverlay.Show();
            CoroutineTask<SaveLoadManager.LoadResult> task = SaveLoadManager.main.LoadAsync();
            yield return task;
            SaveLoadManager.LoadResult result = task.GetResult();
            if (!result.success)
            {
                yield return new WaitForSecondsRealtime(1f);
                isStartingNewGame = false;
                uGUI.main.loading.End(false);
                string descriptionText = Language.main.GetFormat<string>("LoadFailed", result.errorMessage);
                if (result.error == SaveLoadManager.Error.OutOfSpace)
                {
                    descriptionText = Language.main.Get("LoadFailedSpace");
                }
                uGUI.main.confirmation.Show(descriptionText, delegate (bool confirmed)
                {
                    OnErrorConfirmed(confirmed, saveGame, changeSet, gameMode);
                });
            }
            else
            {
                FPSInputModule.SelectGroup(null, false);
                uGUI.main.loading.BeginAsyncSceneLoad("Main");
            }
            isStartingNewGame = false;
            Console.WriteLine("[AutoLoad] Loading complete.");
            yield break;
        }

        /// <summary>
        /// Copied from uGUI_MainMenu, altered to work statically
        /// </summary>
        /// <param name="confirmed"></param>
        /// <param name="saveGame"></param>
        /// <param name="changeSet"></param>
        /// <param name="gameMode"></param>
        private static void OnErrorConfirmed(bool confirmed, string saveGame, int changeSet, GameMode gameMode)
        {
            if (confirmed)
            {
                RunCoroutine(LoadGameAsync(saveGame, changeSet, gameMode));
                return;
            }
            FPSInputModule.SelectGroup(null, false);
        }

        public static Config Config = ModConfig.Load<Config>();
        public static void Initialise()
        {
            OptionsPanelHandler.RegisterModOptions(new Options());
        }
    }
}
