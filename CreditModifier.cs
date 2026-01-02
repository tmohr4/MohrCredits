extern alias Legacy;
global using HarmonyLib;
global using RoR2;
global using System;
using BepInEx;
using BepInEx.Configuration;
using MiscFixes.Modules;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine.Networking;
using static Legacy::R2API.DirectorAPI;

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

    static double scalar = 1;
    static double flat = 0;
    static bool scaleVault = false;

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

        CreditModifier.flat = cfg.BindOptionSlider(title, "Flat", "Flat credit boost", 0.5f).Value;
        CreditModifier.scalar = cfg.BindOptionSlider(title, "Scalar", "Per-player credit scalar", 0.5f).Value;
        CreditModifier.scaleVault = cfg.BindOption(title, "ScaleVault", "Whether the \"vault\" credits should be scaled", false).Value;
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
        double vaultCreditsToRemove = scaleVault ? 0 : ClassicStageInfo.instance?.bonusInteractibleCreditObjects?.Where(
                obj => obj.objectThatGrantsPointsIfEnabled?.activeSelf is true
            ).Sum(obj => obj.points) ?? 0;
        double initialCredits = __instance.interactableCredit - vaultCreditsToRemove;
        double baseCredits = initialCredits / (0.5 * playerCount + 0.5);
        double extraCreditsToAdd = baseCredits * (scalar * playerCount + flat) - initialCredits;
        extraCreditsToAdd = Math.Round(extraCreditsToAdd, MidpointRounding.AwayFromZero);

        __instance.interactableCredit += (int)extraCreditsToAdd;

        System.Console.WriteLine($"adjusted initial {initialCredits} credits by {extraCreditsToAdd} to {__instance.interactableCredit}.");
        Run.instance.RecalculateDifficultyCoefficent();
    }
}
