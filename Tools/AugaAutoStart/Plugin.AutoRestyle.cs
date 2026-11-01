using System;
using System.Collections;
using System.IO;
using System.Linq;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using ValheimAutoTest;
using Object = UnityEngine.Object;

namespace AugaAutoStart
{
    // AUGA_TEST_AUTORESTYLE: the Auga API stub this helper compiles in (API 2.0, which finds Auga at runtime and
    // forwards to it), and Auga restyling the UI of a mod that depends on it, which this helper is, along all three
    // paths: a UI prefab from the mod's bundle (Assets\augatest, built by AugaTestBundle in AugaUnity), widgets the mod
    // builds with Jotunn's GUIManager, and an object the mod adds to the inventory. "1" expects each restyled;
    // "excluded" has this helper opted out (Awake) and expects each left as it was built. The stub checks run either way.
    public partial class Plugin
    {
        private static IEnumerator AutoRestyleCheck()
        {
            var expectRestyled = Environment.GetEnvironmentVariable("AUGA_TEST_AUTORESTYLE") == "1";
            var mode = expectRestyled ? "restyled" : "excluded";
            void Check(bool ok, string what)
            {
                if (ok) Debug.Log($"[AugaAutoStart] autorestyle ({mode}) ok: {what}");
                else AutoTest.Fail($"AUGA_TEST_AUTORESTYLE ({mode}): {what}");
            }
            var front = GUIManager.CustomGUIFront.transform;

            // the stub: a call with a plain result, Auga's own widgets, a tuple, a data class copied field by field
            // and an Action run by Auga
            Check(Auga.API.IsLoaded(), "API.IsLoaded() through the stub");
            Check(Auga.API.GetBoldFontAsset() != null, "API.GetBoldFontAsset() returns Auga's font");
            Sprite augaButtonSprite = null;
            Try(() =>
            {
                var sample = Auga.API.MediumButton_Create(front, "AutoRestyleSample", "sample");
                augaButtonSprite = sample != null && sample.targetGraphic is Image image ? image.sprite : null;
                if (sample != null) Object.Destroy(sample.gameObject);
            });
            Check(augaButtonSprite != null, "API.MediumButton_Create returns an Auga button");
            Try(() =>
            {
                var divider = Auga.API.Divider_CreateMedium(front, "AutoRestyleDivider", 200f);
                Check(divider != null && divider.Item1 != null && divider.Item2 != null, "API.Divider_CreateMedium returns its tuple");
                if (divider?.Item1 != null) Object.Destroy(divider.Item1);
            });
            Try(() =>
            {
                var tab = Auga.API.PlayerPanel_AddTab("AutoRestyleTest_Tab", TestSprite(), "TEST", _ => { });
                Check(tab != null && tab.ContentGO != null && tab.TabButtonGO != null, "API.PlayerPanel_AddTab returns the stub's PlayerPanelTabData with its fields");
            });
            var linkClicked = false;
            Try(() =>
            {
                Check(Auga.API.PlayerPanel_AddLink("AutoRestyleTest_Link", TestSprite(), "test link", () => linkClicked = true), "API.PlayerPanel_AddLink");
                var link = Auga.API.PlayerPanel_GetLink("AutoRestyleTest_Link");
                var linkButton = link != null ? link.GetComponent<Button>() : null;
                if (linkButton != null) linkButton.onClick.Invoke();
            });
            Check(linkClicked, "the link's Action, passed through the stub, runs on click");

            // Jotunn GUIManager widgets; the click listener is added after the button is built, as mods do
            GameObject panel = null;
            Text text = null;
            Button button = null;
            var clicked = false;
            Try(() =>
            {
                var gui = GUIManager.Instance;
                var middle = new Vector2(0.5f, 0.5f);
                panel = gui.CreateWoodpanel(front, middle, middle, new Vector2(-260f, 60f), 380f, 420f, false);
                panel.name = "AutoRestyleJotunnPanel";
                text = gui.CreateText("Jotunn widgets", panel.transform, middle, middle, new Vector2(0f, 170f), gui.AveriaSerifBold, 24, new Color(1f, 0.63f, 0.24f), true, Color.black, 300f, 40f, false).GetComponent<Text>();
                button = gui.CreateButton("Button", panel.transform, middle, middle, new Vector2(0f, 110f), 160f, 40f).GetComponent<Button>();
                button.onClick.AddListener(() => clicked = true);
                ((RectTransform)gui.CreateToggle(panel.transform, 28f, 28f).transform).anchoredPosition = new Vector2(0f, 60f);
                gui.CreateInputField(panel.transform, middle, middle, new Vector2(0f, 10f), InputField.ContentType.Standard, "input field", 16, 220f, 30f);
                gui.CreateDropDown(panel.transform, middle, middle, new Vector2(0f, -40f), 16, 220f, 30f);
                ((RectTransform)gui.CreateScrollView(panel.transform, false, true, 8f, 4f, ColorBlock.defaultColorBlock, new Color(0f, 0f, 0f, 0.5f), 300f, 100f).transform).anchoredPosition = new Vector2(0f, -130f);
            });
            Check(panel != null && text != null && button != null, "the GUIManager widgets were built");
            if (panel != null && text != null && button != null)
            {
                var backdrop = panel.GetComponent<Image>().sprite;
                Check((backdrop == null) == expectRestyled, $"the woodpanel's backdrop sprite is {NameOf(backdrop)}");
                Check(IsAugaFont(text.font) == expectRestyled, $"the GUIManager text's font is {NameOf(text.font)}");
                var buttonSprite = button.targetGraphic is Image image ? image.sprite : null;
                Check((buttonSprite == augaButtonSprite) == expectRestyled, $"the GUIManager button's sprite is {NameOf(buttonSprite)}");
                Try(() => button.onClick.Invoke());
                Check(clicked, "a listener added after the button was built runs (the button is the same object)");
            }

            // a UI prefab from this helper's bundle, restyled on the asset when it is loaded
            Stream stream = null;
            AssetBundle bundle = null;
            GameObject prefab = null;
            GameObject copy = null;
            Try(() =>
            {
                stream = typeof(Plugin).Assembly.GetManifestResourceStream("AugaAutoStart.augatest");
                bundle = stream != null ? AssetBundle.LoadFromStream(stream) : null;
                prefab = bundle != null ? bundle.LoadAsset<GameObject>("AutoRestyleTestPanel") : null;
            });
            Check(prefab != null, "the test bundle's AutoRestyleTestPanel loaded");
            if (prefab != null)
            {
                var backdrop = prefab.GetComponent<Image>().sprite;
                Check((backdrop == null) == expectRestyled, $"the bundle prefab's backdrop sprite is {NameOf(backdrop)}");
                var title = prefab.transform.Find("Title").GetComponent<Text>();
                Check(IsAugaFont(title.font) == expectRestyled, $"the bundle prefab's title font is {NameOf(title.font)}");
                var prefabButton = prefab.transform.Find("Button").GetComponent<Button>();
                var prefabButtonSprite = prefabButton.targetGraphic is Image image ? image.sprite : null;
                Check((prefabButtonSprite == augaButtonSprite) == expectRestyled, $"the bundle prefab's button sprite is {NameOf(prefabButtonSprite)}");
                Try(() =>
                {
                    copy = Object.Instantiate(prefab, front, false);
                    ((RectTransform)copy.transform).anchoredPosition = new Vector2(240f, 160f);
                });
            }

            // objects added to the inventory: one carrying a component of this helper (a dependent), one carrying only
            // Unity's, which Auga cannot tie to a mod and must leave alone either way
            GameObject owned = null;
            GameObject unowned = null;
            Try(() =>
            {
                var root = InventoryGui.instance.m_inventoryRoot;
                owned = MakeBox(root, "AutoRestyleOwned", new Vector2(160f, -60f), true);
                unowned = MakeBox(root, "AutoRestyleUnowned", new Vector2(160f, -140f), false);
                InventoryGui.instance.Show(null);
            });
            yield return new WaitForSecondsRealtime(1f);
            Check(owned != null && unowned != null, "the inventory test objects were added");
            if (owned != null && unowned != null)
            {
                Check((owned.GetComponent<Image>().sprite == null) == expectRestyled, $"the dependent's object in the inventory has backdrop {NameOf(owned.GetComponent<Image>().sprite)}");
                Check(IsAugaFont(owned.GetComponentInChildren<Text>().font) == expectRestyled, $"the dependent's object in the inventory has font {NameOf(owned.GetComponentInChildren<Text>().font)}");
                Check(unowned.GetComponent<Image>().sprite != null && !IsAugaFont(unowned.GetComponentInChildren<Text>().font), "the object no mod can be tied to is left alone");
            }
            Shot("20_autorestyle_" + mode);
            // a screenshot is written a while after it is asked for
            yield return new WaitForSecondsRealtime(2.5f);

            Try(() => InventoryGui.instance.Hide());
            foreach (var go in new[] { panel, copy, owned, unowned })
                if (go != null) Object.Destroy(go);
            Try(() => bundle?.Unload(true));
            stream?.Dispose();
        }

        private static GameObject MakeBox(Transform parent, string name, Vector2 position, bool owned)
        {
            var box = new GameObject(name, typeof(RectTransform), typeof(Image));
            box.transform.SetParent(parent, false);
            var rect = (RectTransform)box.transform;
            rect.sizeDelta = new Vector2(200f, 60f);
            rect.anchoredPosition = position;
            box.GetComponent<Image>().sprite = TestSprite();
            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(box.transform, false);
            ((RectTransform)label.transform).sizeDelta = new Vector2(190f, 50f);
            var text = label.GetComponent<Text>();
            text.text = name;
            text.font = GUIManager.Instance.AveriaSerifBold;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            if (owned) box.AddComponent<AutoRestyleProbe>();
            return box;
        }

        private static Sprite _testSprite;

        private static Sprite TestSprite()
        {
            if (_testSprite == null)
            {
                var texture = new Texture2D(4, 4);
                texture.SetPixels(Enumerable.Repeat(new Color(0.55f, 0.25f, 0.25f), 16).ToArray());
                texture.Apply();
                _testSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f));
                _testSprite.name = "AutoRestyleTestSprite";
            }
            return _testSprite;
        }

        private static bool IsAugaFont(Font font) => font != null && font.name.Contains("SourceSans");

        private static string NameOf(Object o) => o == null ? "none" : o.name;
    }

    /// <summary>Marks an object as this helper's, so Auga's screens pass can tie it to a dependent mod.</summary>
    public class AutoRestyleProbe : MonoBehaviour
    {
    }
}
