using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using KSP.Game;
using KSP.IO;
using KuriosityScience.Models;
using Newtonsoft.Json;
using Redux.ExtraModTypes;
using ReduxLib.Configuration;
using RTG;
using SpaceWarp2.API.Mods;
using UnityEngine;
using ILogger = ReduxLib.Logging.ILogger;

namespace KuriosityScience
{
    /* Extend KerbalMod instead if you need the MonoBehaviour update loop/references to game stuff like SW 1.x mods */
    public class KuriositySciencePlugin : KerbalMod
    {
        [PublicAPI] public static KuriositySciencePlugin Instance { get; set; }

        public static Dictionary<string, KuriosityExperiment> KuriosityExperiments;

        private const bool debugMode = false;

        // Logger
        public static ILogger Logger;

        // UI window state
        private bool _isWindowOpen;

        // AppBar button IDs
        private const string ToolbarFlightButtonID = "BTN-KuriosityScienceFlight";

        // Base Kuriosity Factor configuration
        internal ConfigValue<double> baseKuriosityFactor;

        public override void OnPreInitialized()
        {
            SetupConfiguration();
        }

        /// <summary>
        /// Runs when the mod is first initialized.
        /// </summary>
        public override void OnInitialized()
        {
            Logger = SWLogger;
            Instance = this;

            // Register Flight AppBar button
            // Register all Harmony patches in the project
            CreateHarmonyAndPatchAll();

            KuriosityExperiments = new();

            GameManager.Instance.Assets.LoadByLabel<TextAsset>("kuriosity_experiment", RegisterKuriosityExperiment,
                assets => { GameManager.Instance.Assets.ReleaseAsset(assets); });
        }

        private static void RegisterKuriosityExperiment(TextAsset asset)
        {
            var experiment = JsonConvert.DeserializeObject<KuriosityExperiment>(asset.text);

            Logger.LogDebug($"Experiment loaded: {experiment.ExperimentId}");

            KuriosityExperiments.Add(experiment.ExperimentId, experiment);
        }

        public override void OnPostInitialized()
        {
        }

        private void SetupConfiguration()
        {
            baseKuriosityFactor = new(SWConfiguration.Bind("Kuriosity Science", $"Base Kuriosity Factor", 1.0,
                "Base Kuriosity Factor.",
                new ListConstraint<double>(0.01, 0.1, 0.5, 1.0, 5.0, 10.0, 50.0)));
        }
    }
}