using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;
using log4net;
using Loki.Bot;
using Loki.Common;
using Loki.Game;
using Loki.Game.Objects;
using EXtensions;
using Buddy.Coroutines;
using Loki.Game.GameData;

namespace VollFarmer
{
    internal class VollFarmer : IPlugin
    {
        private static readonly ILog Log = Logger.GetLoggerInstanceForType();
        private readonly Stopwatch _itemStopwatch = new Stopwatch();
        private EXtensions.Positions.WalkablePosition _VollPos;
        private int _FoundVoll = 0;
        private int _CombatRangeStore = 0;
        private int _itemPickup = 0;
        private int _sleepTime = 1000;
        private int _killedVoll = 0;

        private VollFarmerSettingsGUI _instance;

        #region Implementation of IAuthored

        /// <summary> The name of the plugin. </summary>
        public string Name
        {
            get
            {
                return "VollFarmer";
            }
        }

        /// <summary> The description of the plugin. </summary>
        public string Description
        {
            get
            {
                return
                    "VollFarmer";
            }
        }

        /// <summary>The author of the plugin.</summary>
        public string Author
        {
            get
            {
                return "Clandestine";
            }
        }

        /// <summary>The version of the plugin.</summary>
        public string Version
        {
            get
            {
                return "0.0.1.1";
            }
        }

        #endregion

        #region Implementation of IBase

        /// <summary>Initializes this plugin.</summary>
        public void Initialize()
        {
            _CombatRangeStore = OldRoutine.OldRoutineSettings.Instance.CombatRange;
            Log.DebugFormat("[VollFarmer] Storing CombatRange original value");
            Log.DebugFormat("[VollFarmer] Initialize");
        }

        /// <summary>Deinitializes this object. This is called when the object is being unloaded from the bot.</summary>
        public void Deinitialize()
        {
            OldRoutine.OldRoutineSettings.Instance.CombatRange = _CombatRangeStore;
            Log.DebugFormat("[VollFarmer] Resetting CombatRange to original Value");
            Log.DebugFormat("[VollFarmer] Deinitialize");
        }

        #endregion

        #region Implementation of IRunnable

        /// <summary> The plugin start callback. Do any initialization here. </summary>
        public void Start()
        {
            Log.DebugFormat("[VollFarmer] Start");
        }

        /// <summary> The plugin tick callback. Do any update logic here. </summary>
        public void Tick()
        {
        }

        /// <summary> The plugin stop callback. Do any pre-dispose cleanup here. </summary>
        public void Stop()
        {
            Log.DebugFormat("[VollFarmer] Resetting CombatRange to original Value");
            OldRoutine.OldRoutineSettings.Instance.CombatRange = _CombatRangeStore;
            Log.DebugFormat("[VollFarmer] Stop");
        }

        #endregion

        #region Implementation of ILogic

        /// <summary>
        /// Coroutine logic to execute.
        /// </summary>
        /// <param name="type">The requested type of logic to execute.</param>
        /// <param name="param">Data sent to the object from the caller.</param>
        /// <returns>true if logic was executed to handle this type and false otherwise.</returns>
        public async Task<bool> Logic(string type, params dynamic[] param)
        {
            if (!LokiPoe.IsInGame)
            {
                return false;
            }
                

            if (LokiPoe.Me.IsInTown)
            {
                await NewGrindZone();
                return false;
            }

            var Voll = FindUniqueMonster("Voll, Emperor of Purity");

            if (_FoundVoll == 0 && Voll == null)
            {
                GlobalLog.Warn($"[VollFarmer] Can't see Voll, setting range to 0");
                OldRoutine.OldRoutineSettings.Instance.CombatRange = 0;
                return false;
            }

            if (Voll == null && _killedVoll == 1)
            {
                GlobalLog.Warn($"[VollFarmer] Looks like we moved out of range of Voll. Presume looting is done and start new run.");
                OldRoutine.OldRoutineSettings.Instance.CombatRange = 0;
                _FoundVoll = 0;
                _killedVoll = 0;
                _itemStopwatch.Stop();
                _sleepTime = 1000;
                await Coroutines.FinishCurrentAction();
                await CommunityLib.LibCoroutines.CreateAndTakePortalToTown();
                await NewGrindZone();
                return true;
            }

            if (Voll != null && _killedVoll == 1)
            {
                GlobalLog.Warn($"[VollFarmer] Sleeptime: {_itemStopwatch.ElapsedMilliseconds} of {_sleepTime}");
                if (_itemStopwatch.IsRunning && _itemStopwatch.ElapsedMilliseconds > _sleepTime)
                {
                    _itemStopwatch.Reset();
                    _itemStopwatch.Start();
                    GlobalLog.Warn($"[VollFarmer] increasing looting timeout by 500ms to {_sleepTime}");
                    _sleepTime += 500;

                    await CheckItems();
                    await Coroutines.LatencyWait();
                    if (_itemPickup == 1)
                    {
                        GlobalLog.Warn($"[VollFarmer] We have more items to pick up");
                        return false;
                    }
                    else
                    {
                        GlobalLog.Warn($"[VollFarmer] Looks like looting is done. Starting new run");
                        OldRoutine.OldRoutineSettings.Instance.CombatRange = 0;
                        await Coroutines.FinishCurrentAction();
                        await CommunityLib.LibCoroutines.CreateAndTakePortalToTown();
                        await NewGrindZone();
                        _FoundVoll = 0;
                        _killedVoll = 0;
                        _sleepTime = 1000;
                        _itemStopwatch.Stop();
                        return true;
                    }
                }
                else
                {
                    _itemStopwatch.Start();
                    return false;
                }
            }

            if (Voll != null)
            {
                _FoundVoll = 1;
                if (Voll.IsDead)
                {
                    GlobalLog.Warn($"[VollFarmer] Voll is dead, setting flag.");
                    _killedVoll = 1;
                    return false;
                }
                _VollPos = FindUniqueMonster("Voll, Emperor of Purity")?.WalkablePosition();
                if (_VollPos.IsFar)
                {
                    _VollPos.Come();
                    return false;
                }
                GlobalLog.Warn($"[VollFarmer] Looks like we found Voll, increasing Combat Range to 50");
                OldRoutine.OldRoutineSettings.Instance.CombatRange = 50;
                return false;
            }

            return false;
        }


        /// <summary>
        /// Non-coroutine logic to execute.
        /// </summary>
        /// <param name="name">The name of the logic to invoke.</param>
        /// <param name="param">The data passed to the logic.</param>
        /// <returns>Data from the executed logic.</returns>
        public object Execute(string name, params dynamic[] param)
        {
            return true;
        }

        #endregion

        #region Implementation of IConfigurable

        /// <summary>The settings object. This will be registered in the current configuration.</summary>
        public JsonSettings Settings
        {
            get
            {
                return VollFarmerSettings.Instance;
            }
        }

        /// <summary> The plugin's settings control. This will be added to the Exilebuddy Settings tab.</summary>
        public UserControl Control
        {
            get
            {
                return (_instance ?? (_instance = new VollFarmerSettingsGUI()));
            }
        }

        #endregion

        #region Implementation of IEnableable

        /// <summary> The plugin is being enabled.</summary>
        public void Enable()
        {
            Log.DebugFormat("[VollFarmer] Enable");
        }

        /// <summary> The plugin is being disabled.</summary>
        public void Disable()
        {
            Log.DebugFormat("[VollFarmer] Disable");
        }

        #endregion

        #region Override of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name + ": " + Description;
        }


        #endregion

        private async Task NewGrindZone()
        {
            string name = "";
            string difficulty = "";

            name = VollFarmerSettings.Instance.VollFarmer1Name;
            difficulty = VollFarmerSettings.Instance.VollFarmerDifficulty;

            Log.InfoFormat("[VollFarmer] Resetting Zone {0}:{1}.", name, difficulty);

            BotManager.CurrentBot.Execute("oldgrindbot_change_grind_zone", name, difficulty);
            return;
        }
        private async Task CheckItems()
        {
            GlobalLog.Warn($"[VollFarmer] Voll is dead, now looting");
            IItemFilter ItemOut;
            Func<Item, bool> itemGonnaBePickedUp = i => Loki.Bot.ItemEvaluator.Match(i, EvaluationType.PickUp, out ItemOut);
            var WorldItems = LokiPoe.ObjectManager.GetObjectsByType<WorldItem>();
            _itemPickup = 0;
            foreach (var wi in WorldItems)
            {
                var item = wi.Item;
                if (itemGonnaBePickedUp(item))
                {
                    _itemPickup = 1;
                }
            }
            await Coroutines.LatencyWait();
            return;
        }
        public static Monster FindUniqueMonster(string name)
        {
            return LokiPoe.ObjectManager.Objects
                .OfType<Monster>()
                .FirstOrDefault(m => m.Rarity == Rarity.Unique && m.Name == name);
        }
    }
}