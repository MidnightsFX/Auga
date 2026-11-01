using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// The test bundle of the AugaAutoStart smoke test (Tools\AugaAutoStart\Assets\augatest): one panel in a plain,
// non-Auga look (a sprite backdrop, a button, texts in the Norse font), which the AUGA_TEST_AUTORESTYLE step loads to
// check that Auga restyles the UI prefabs in the bundles of mods that depend on it. The prefab is made here and
// deleted again, so the project keeps no test asset, and only this bundle is built (not augaassets).
// Batch mode: Unity.exe -batchmode -quit -projectPath AugaUnity -executeMethod AugaTestBundle.Build
public static class AugaTestBundle
{
    private const string PrefabFolder = "Assets/AutoRestyleTest";
    private const string PrefabPath = PrefabFolder + "/AutoRestyleTestPanel.prefab";

    [MenuItem("Auga/Build AugaAutoStart Test Bundle")]
    public static void Build()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var staging = Path.Combine(projectRoot, "Temp", "AugaTestBundle");
        var output = Path.GetFullPath(Path.Combine(projectRoot, "..", "Tools", "AugaAutoStart", "Assets"));
        Directory.CreateDirectory(staging);
        Directory.CreateDirectory(output);
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets", "AutoRestyleTest");

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ActiveEffectBG.png");
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Norse.otf");

        var panel = new GameObject("AutoRestyleTestPanel", typeof(RectTransform), typeof(Image));
        ((RectTransform)panel.transform).sizeDelta = new Vector2(320f, 220f);
        panel.GetComponent<Image>().sprite = sprite;
        AddText(panel.transform, "Title", "Auto restyle test", font, new Vector2(0f, 60f), new Vector2(300f, 40f));

        var button = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        button.transform.SetParent(panel.transform, false);
        var buttonRect = (RectTransform)button.transform;
        buttonRect.sizeDelta = new Vector2(160f, 40f);
        buttonRect.anchoredPosition = new Vector2(0f, -50f);
        var buttonImage = button.GetComponent<Image>();
        buttonImage.sprite = sprite;
        button.GetComponent<Button>().targetGraphic = buttonImage;
        AddText(button.transform, "Text", "Button", font, Vector2.zero, new Vector2(150f, 36f));

        PrefabUtility.SaveAsPrefabAsset(panel, PrefabPath);
        Object.DestroyImmediate(panel);
        try
        {
            var build = new AssetBundleBuild { assetBundleName = "augatest", assetNames = new[] { PrefabPath } };
            BuildPipeline.BuildAssetBundles(staging, new[] { build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            File.Copy(Path.Combine(staging, "augatest"), Path.Combine(output, "augatest"), true);
        }
        finally
        {
            AssetDatabase.DeleteAsset(PrefabFolder);
        }
        Debug.Log("Finished building the AugaAutoStart test bundle: " + Path.Combine(output, "augatest"));
    }

    private static void AddText(Transform parent, string name, string value, Font font, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        var text = go.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }
}
