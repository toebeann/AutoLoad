using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using QModManager.API;
using UnityEngine;
using Logger = BepInEx.Subnautica.Logger;
using HarmonyLib;
using Straitjacket.Subnautica.Mods.AutoLoad.Patches;
using System.Diagnostics;

namespace Straitjacket.Subnautica.Mods.AutoLoad
{
    internal enum AutoLoadMode { MostRecentlySaved, MostRecentlyLoaded }
    internal enum AutoLoadPause { Off, AutoLoadOnly, All }

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
            if (Startup && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey) && !VirtualKey.GetKey(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
            {
                yield return new WaitUntil(() => modCheckComplete);

#pragma warning disable CS0436 // Type conflicts with imported type
                if (!FailedMods.Any() && !KeyCodeUtils.GetKeyHeld(Config.OverrideKey) && !VirtualKey.GetKey(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
                {
                    yield return new WaitWhile(() => SaveLoadManager.main == null);
                    yield return SaveLoadManager.main.LoadSlotsAsync();

                    string[] activeSlotNames = SaveLoadManager.main.GetActiveSlotNames();
#pragma warning disable CS0436 // Type conflicts with imported type
                    if (KeyCodeUtils.GetKeyHeld(Config.OverrideKey) || VirtualKey.GetKey(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
                    {
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else if (!activeSlotNames.Any())
                    {
                        Logger.LogInfo("No active save slots found, initialising StartScreen GUI.");
                        yield return RunCoroutine(startScreen.Load());
                    }
                    else if (!string.IsNullOrEmpty(Config.SpecificSaveSlot))
                    {
                        if ((activeSlotNames as IEnumerable<string>).Contains(Config.SpecificSaveSlot))
                        {
                            Logger.LogInfo($"Beginning load of specific save [{Config.SpecificSaveSlot}]...");
                            if (!LoadSpecificSaveGame(Config.SpecificSaveSlot))
                            {
                                yield return RunCoroutine(startScreen.Load());
                            }
                        }
                        else
                        {
                            Logger.LogWarning($"Specific save [{Config.SpecificSaveSlot}] not found, " +
                                $"initialising StartScreen GUI.");
                            yield return RunCoroutine(startScreen.Load());
                        }
                    }
                    else
                    {
#pragma warning disable CS0436 // Type conflicts with imported type
                        var toggleAutoLoadMode = KeyCodeUtils.GetKeyHeld(Config.ToggleAutoLoadModeKey) ||
                            VirtualKey.GetKey(Config.ToggleAutoLoadModeKey);
#pragma warning restore CS0436 // Type conflicts with imported type
                        if ((!toggleAutoLoadMode && Config.AutoLoadMode == AutoLoadMode.MostRecentlySaved)
                            || (toggleAutoLoadMode && Config.AutoLoadMode == AutoLoadMode.MostRecentlyLoaded))
                        {
                            Logger.LogInfo("Beginning load of most recent save...");
                            if (!LoadMostRecentSavedGame(activeSlotNames))
                            {
                                yield return RunCoroutine(startScreen.Load());
                            }
                        }
                        else if (MostRecentlyLoadedSlot != null)
                        {
                            Logger.LogInfo("Beginning load of most recently loaded game...");
                            if (!LoadMostRecentLoadedGame())
                            {
                                if (Config.StartNewGame)
                                {
                                    Logger.LogInfo($"Starting new game in {MostRecentlyLoadedSlot.GameMode} mode...");
                                    yield return RunCoroutine(StartNewGame(MostRecentlyLoadedSlot.GameMode));
                                }
                                else
                                {
                                    Logger.LogInfo("Initialising StartScreen GUI.");
                                    yield return RunCoroutine(startScreen.Load());
                                }
                            }
                        }
                        else
                        {
                            Logger.LogInfo("Information about most recently loaded save not available, " +
                                "initialising StartScreen GUI.");
                            yield return RunCoroutine(startScreen.Load());
                        }
                    }
                }
#pragma warning disable CS0436 // Type conflicts with imported type
                else if (!KeyCodeUtils.GetKeyHeld(Config.OverrideKey) && !VirtualKey.GetKey(Config.OverrideKey))
#pragma warning restore CS0436 // Type conflicts with imported type
                {
                    Logger.LogWarning("Detected the following mods were not loaded:");
                    foreach (var mod in FailedMods)
                    {
                        Logger.LogMessage($"    {mod.DisplayName}");
                    }
                    Logger.LogWarning("Skipping AutoLoad.");
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

        public static IEnumerator StartNewGame(GameMode gameMode)
        {
            if (isStartingNewGame)
            {
                yield break;
            }

            yield return LoadUserSettings();

            isStartingNewGame = true;
            isAutoLoad = true;
            Guid.NewGuid().ToString();
            PlatformUtils.main.GetServices().ShowUGCRestrictionMessageIfNecessary();
            global::Utils.SetContinueMode(false);
            global::Utils.SetLegacyGameMode(gameMode);
            CoroutineTask<SaveLoadManager.CreateResult> createSlotTask = null;
            if (PlatformUtils.isPS4Platform)
            {
                createSlotTask = SaveLoadManager.main.SetupSlotPS4Async();
            }
            else
            {
                createSlotTask = SaveLoadManager.main.CreateSlotAsync();
            }
            yield return createSlotTask;
            SaveLoadManager.CreateResult result = createSlotTask.GetResult();
            if (!result.success)
            {
                yield return RunCoroutine(StartScreen.Load());
                if (result.slotName == SaveLoadManager.Error.OutOfSpace.ToString())
                {
                    string descriptionText = Language.main.Get("SaveFailedSpace");
                    uGUI.main.confirmation.Show(descriptionText, null);
                }
                else if (result.slotName == SaveLoadManager.Error.OutOfSlots.ToString())
                {
                    string descriptionText2 = Language.main.Get("SaveFailedSlot");
                    uGUI.main.confirmation.Show(descriptionText2, null);
                }
                isStartingNewGame = false;
                isAutoLoad = false;
                yield break;
            }
            SaveLoadManager.main.SetCurrentSlot(result.slotName);
            VRLoadingOverlay.Show();
            if (!PlatformUtils.isPS4Platform)
            {
                UserStorageUtils.AsyncOperation clearSlotTask = SaveLoadManager.main.ClearSlotAsync(result.slotName);
                yield return clearSlotTask;
                if (!clearSlotTask.GetSuccessful())
                {
                    Logger.LogError("Clearing save data failed. But we ignore it.");
                }
                clearSlotTask = null;
            }
            GamepadInputModule.current.SetCurrentGrid(null);
            uGUI.main.loading.BeginAsyncSceneLoad("Main");
        }

        /// <summary>
        /// Copied from <see cref="uGUI_MainMenu"/>, altered to work statically
        /// </summary>
        public static bool LoadMostRecentSavedGame(string[] activeSlotNames)
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
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adapted from <see cref="LoadMostRecentSavedGame(string[])"/>.
        /// </summary>
        public static bool LoadSpecificSaveGame(string saveGame, string sessionId = null)
        {
            Logger.LogInfo($"Attempting to load save slot {saveGame}...");
            var gameInfo = SaveLoadManager.main.GetGameInfo(saveGame);
            if (gameInfo != null && SlotIsValid(saveGame, sessionId))
            {
                RunCoroutine(LoadGameAsync(saveGame, gameInfo.changeSet, gameInfo.gameMode));
                return true;
            }
            else
            {
                Logger.LogError("Specified save slot does not exist or is not valid.");
                return false;
            }
        }

        public static bool LoadMostRecentLoadedGame() => LoadSpecificSaveGame(MostRecentlyLoadedSlot.SaveGame, MostRecentlyLoadedSlot.Session);

        public static IEnumerator LoadUserSettings()
        {
            var userStorage = PlatformUtils.main.GetUserStorage();
            var initTask = userStorage.InitializeAsync();
            yield return initTask;

            if (!initTask.GetSuccessful())
            {
                Logger.LogError($"Save data init failed ({initTask.result})");
                yield return RunCoroutine(StartScreen.Load());
                yield break;
            }

            var loadOptionsTask = GameSettings.LoadAsync();
            yield return loadOptionsTask;

            if (!loadOptionsTask.GetResult())
            {
                Logger.LogError(Language.main.Get("LoadOptionsFailed"));
                yield return RunCoroutine(StartScreen.Load());
                yield break;
            }
        }

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

            yield return LoadUserSettings();

            isStartingNewGame = true;
            isAutoLoad = true;
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
                isAutoLoad = false;
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
                Logger.LogInfo("Loading complete.");
            }
            isStartingNewGame = false;
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

        private static bool SlotIsValid(string saveSlot, string sessionId = null)
        {
            if ((SaveLoadManager.main.GetActiveSlotNames() as IEnumerable<string>).Contains(saveSlot))
            {
                var gameInfo = SaveLoadManager.main.GetGameInfo(saveSlot);
                if (gameInfo != null)
                {
                    if (sessionId != null)
                    {
                        var result = gameInfo.session == sessionId;
                        if (!result)
                        {
                            Logger.LogError("Session ID mismatch.");
                        }
                        return result;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool isAutoLoad = false;
        public static IEnumerator PauseOnLoad()
        {
            if (Config.PauseOnLoad != AutoLoadPause.Off)
            {
                if (Config.PauseOnLoad == AutoLoadPause.All || isAutoLoad)
                {
                    yield return new WaitUntil(() => IngameMenu.main != null);
                    yield return new WaitUntil(() => Time.timeSinceLevelLoad >= 1f);

                    if (Utils.GetLegacyGameMode() != GameMode.Hardcore)
                    {
                        IngameMenu.main.Open();
                    }
                    isAutoLoad = false;
                }
                else
                {
                    isAutoLoad = false;
                }
            }
        }

        public static SaveSlotInfo MostRecentlyLoadedSlot
        {
            get
            {
                var saveSlotInfoString = PlayerPrefs.GetString("MostRecentlyLoadedSaveSlot", null);
                if (saveSlotInfoString != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<SaveSlotInfo>(saveSlotInfoString, new JsonConverter[] { new StringEnumConverter() });
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            set => PlayerPrefs.SetString("MostRecentlyLoadedSaveSlot", JsonConvert.SerializeObject(value, new JsonConverter[] { new StringEnumConverter() }));
        }
        public static Config Config = new Config();
        public static void Initialise()
        {
            Logger.LogInfo("Initialising...");
            var stopwatch = Stopwatch.StartNew();

            ApplyHarmonyPatches();
            Config.Load();
            OptionsPanelHandler.RegisterModOptions(new Options());

            stopwatch.Stop();
            Logger.LogInfo($"Initialised in {stopwatch.ElapsedMilliseconds}ms.");
        }

        public static void ApplyHarmonyPatches()
        {
            var stopwatch = Stopwatch.StartNew();

            var harmony = new Harmony("com.tobeyblaber.straitjacket.subnautica.autoload.mod");
            harmony.PatchAll(typeof(PlayerPatch));
            harmony.PatchAll(typeof(SaveLoadManagerPatch));
            harmony.PatchAll(typeof(StartScreenPatch));

            stopwatch.Stop();
            Logger.LogInfo($"Harmony patches applied in {stopwatch.ElapsedMilliseconds}ms.");
        }
    }
}
