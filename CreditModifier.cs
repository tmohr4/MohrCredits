extern alias Legacy;
global using HarmonyLib;
global using RoR2;
global using System;
using BepInEx;
using BepInEx.Configuration;
using MiscFixes.Modules;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine.Networking;

[assembly: AssemblyVersion(CreditModifier.version)]
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618


[BepInPlugin(identifier, "MohrCredits", version)]
public class CreditModifier : BaseUnityPlugin
{
	public const string identifier = "MohrCredits";
	public const string version = "1.0.0";
    private static Harmony instance = null;

    static double m = 10;
    static double b = 0;

    public void Awake()
	{
        Load(Config);

		Harmony.CreateAndPatchAll(typeof(CreditModifier));

		Run.onRunStartGlobal += Begin;
		Run.onRunDestroyGlobal += End;
	}

    public static void Load(ConfigFile cfg)
    {

        try { cfg.Reload(); }
        catch (FileNotFoundException) { cfg.Clear(); }

        string title = "Credit Modifier Controls";

        CreditModifier.b = cfg.BindOptionSlider(title, "Flat", "Flat credit scalar", 0.5f).Value;
        CreditModifier.m = cfg.BindOptionSlider(title, "Scalar", "Per-player credit scalar", 0.5f).Value;
    }

    public static void Begin(Run thisRun)
    {
        if (instance is null && NetworkServer.active)
        {
            instance = Harmony.CreateAndPatchAll(typeof(CreditModifier));

            SceneDirector.onPrePopulateSceneServer += AdjustInteractableCredits;
        }
    }

    public static void End(object _)
    {
        SceneDirector.onPrePopulateSceneServer -= AdjustInteractableCredits;

        instance?.UnpatchSelf();
        instance = null;
    }

    //	[HarmonyPatch(typeof(SceneDirector), nameof(SceneDirector.PopulateScene))]
    //	[HarmonyPrefix]
    private static void AdjustInteractableCredits(SceneDirector __instance)
    {
        SceneDef currentScene = SceneInfo.instance?.sceneDef;
        string sceneName = currentScene?.baseSceneName;
        bool hiddenRealms = sceneName == "arena" || sceneName == "voidstage" || currentScene?.sceneType == SceneType.Intermission;
        if (hiddenRealms)
        {
            System.Console.WriteLine($"Prevent extra items in hidden realm \"{sceneName}\".");
            return;
        }

        double playerCount = Run.instance.participatingPlayerCount;
        double initialCredits = __instance.interactableCredit;
        double baseCredits = initialCredits / (0.5 * playerCount + 0.5);
        double extraCredits = baseCredits * (m * playerCount + b) - initialCredits;
        extraCredits = Math.Round(extraCredits, MidpointRounding.AwayFromZero);

        __instance.interactableCredit += (int)extraCredits;

        System.Console.WriteLine($"adjusted initial {initialCredits} credits by {extraCredits} to {__instance.interactableCredit}.");
        Run.instance.RecalculateDifficultyCoefficent();
    }
}
