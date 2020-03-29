using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using QModManager.API;
using UnityEngine;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal enum AutoLoadMode { MostRecentlySaved, MostRecentlyLoaded }

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

        private static StartScreen StartScreen;
        public static IEnumerator OnGuiInitialized(StartScreen startScreen)
        {
            StartScreen = startScreen;
#pragma warning disable CS0436 // Type conflicts with imported type
            if (Startup && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
            {
                yield return new WaitUntil(() => modCheckComplete);

#pragma warning disable CS0436 // Type conflicts with imported type
                if (!FailedMods.Any() && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
                {
                    yield return new WaitWhile(() => SaveLoadManager.main == null);
                    yield return SaveLoadManager.main.LoadSlotsAsync();

                    string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
#pragma warning disable CS0436 // Type conflicts with imported type
                    if (KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
                    {
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else if (!activeSlotNames.Any())
                    {
                        Console.WriteLine("[AutoLoad] No active save slots found, initialising StartScreen GUI.");
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else if (!string.IsNullOrEmpty(Config.SpecificSaveSlot))
                    {
                        if ((activeSlotNames as IEnumerable<string>).Contains(Config.SpecificSaveSlot))
                        {
                            Console.WriteLine($"[AutoLoad] Beginning load of specific save [{Config.SpecificSaveSlot}]...");
                            LoadSpecificSaveGame(Config.SpecificSaveSlot);
                        }
                        else
                        {
                            Console.WriteLine($"[AutoLoad] Specific save [{Config.SpecificSaveSlot}] not found, " +
                                $"initialising StartScreen GUI.");
                            yield return RunCoroutine(startScreen.Load());
                        }
                    }
                    else
                    {
#pragma warning disable CS0436 // Type conflicts with imported type
                        var toggleAutoLoadMode = KeyCodeUtils.GetKeyHeld(Config.ToggleAutoLoadModeKey);
#pragma warning restore CS0436 // Type conflicts with imported type
                        if ((!toggleAutoLoadMode && Config.AutoLoadMode == AutoLoadMode.MostRecentlySaved)
                            || (toggleAutoLoadMode && Config.AutoLoadMode == AutoLoadMode.MostRecentlyLoaded))
                        {
                            Console.WriteLine("[AutoLoad] Beginning load of most recent save...");
                            LoadMostRecentSavedGame(activeSlotNames);
                        }
                        else if (MostRecentlyLoadedSlot != null)
                        {
                            Console.WriteLine("[AutoLoad] Beginning load of most recent load...");
                            LoadMostRecentLoadedGame();
                        }
                        else
                        {
                            Console.WriteLine("[AutoLoad] Information about most recently loaded save not available, " +
                                "initialising StartScreen GUI.");
                            yield return RunCoroutine(startScreen.Load());
                        }
                    }
                }
#pragma warning disable CS0436 // Type conflicts with imported type
                else if (!KeyCodeUtils.GetKeyHeld(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
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
        /// Copied from <see cref="uGUI_MainMenu"/>, altered to work statically
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

        /// <summary>
        /// Adapted from <see cref="LoadMostRecentSavedGame(string[])"/>.
        /// </summary>
        public static void LoadSpecificSaveGame(string saveGame)
        {
            var gameInfo = SaveLoadManager.main.GetGameInfo(saveGame);
            if (gameInfo != null)
            {
                RunCoroutine(LoadGameAsync(saveGame, gameInfo.changeSet, gameInfo.gameMode));
            }
        }

        public static void LoadMostRecentLoadedGame() => LoadSpecificSaveGame(MostRecentlyLoadedSlot);

        private static bool isStartingNewGame = false;
        /// <summary>
        /// Copied from <see cref="uGUI_MainMenu"/>, altered to work statically, with portions copied from
        /// <see cref="StartScreen.Load"/> to handle loading user preferences.
        /// </summary>
        /// <param name="saveGame"></param>
        /// <param name="changeSet"></param>
        /// <param name="gameMode"></param>
        /// <returns></returns>
#pragma warning disable CS0618 // Type or member is obsolete
        public static IEnumerator LoadGameAsync(string saveGame, int changeSet, GameMode gameMode)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if (isStartingNewGame)
            {
                yield break;
            }

            var userStorage = PlatformUtils.main.GetUserStorage();
            var initTask = userStorage.InitializeAsync();
            yield return initTask;

            if (!initTask.GetSuccessful())
            {
                Console.WriteLine("[AutoLoad] Save data init failed ({0})", new object[]
                {
                    initTask.result
                });
                yield return RunCoroutine(StartScreen.Load());
                yield break;
            }

            var loadOptionsTask = GameSettings.LoadAsync();
            yield return loadOptionsTask;

            if (!loadOptionsTask.GetResult())
            {
                string descriptionText = Language.main.Get("LoadOptionsFailed");
                Console.WriteLine("[AutoLoad] " + descriptionText);
                yield return RunCoroutine(StartScreen.Load());
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
        /// Copied from <see cref="uGUI_MainMenu"/>, altered to work statically
        /// </summary>
        /// <param name="confirmed"></param>
        /// <param name="saveGame"></param>
        /// <param name="changeSet"></param>
        /// <param name="gameMode"></param>
#pragma warning disable CS0618 // Type or member is obsolete
        private static void OnErrorConfirmed(bool confirmed, string saveGame, int changeSet, GameMode gameMode)
#pragma warning restore CS0618 // Type or member is obsolete
        {
            if (confirmed)
            {
                RunCoroutine(LoadGameAsync(saveGame, changeSet, gameMode));
                return;
            }
            FPSInputModule.SelectGroup(null, false);
        }

        internal static string MostRecentlyLoadedSlot
        {
            get => PlayerPrefs.GetString("MostRecentlyLoaded", null);
            set => PlayerPrefs.SetString("MostRecentlyLoaded", value);
        }
        public static Config Config = new Config();
        public static void Initialise()
        {
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());
        }
    }
}
