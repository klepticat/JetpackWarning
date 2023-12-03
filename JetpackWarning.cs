using BepInEx;
using UnityEngine;
using HarmonyLib;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace JetpackWarning {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class JetpackWarningPlugin : BaseUnityPlugin {
        public static Harmony _harmony;

        private void Awake() {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Assets.PopulateAssets();

            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(typeof(Patches));

            SceneManager.sceneLoaded += OnSceneRelayLoaded;
        }

        public static GameObject meterContainer, meter, frame, warning;

        private void OnSceneRelayLoaded(Scene scene, LoadSceneMode loadMode) {
            if(scene.name == "SampleSceneRelay") {
                GameObject HUD = GameObject.Find("IngamePlayerHUD");

                meterContainer = new GameObject("jetpackMeterContainer");

                meterContainer.AddComponent<CanvasGroup>();

                RectTransform containerTransform = meterContainer.AddComponent<RectTransform>();

                containerTransform.parent = HUD.transform;
                containerTransform.localScale = Vector3.one;
                containerTransform.anchoredPosition = Vector2.zero;
                containerTransform.localPosition = new Vector2(50, 0);
                containerTransform.sizeDelta = Vector2.one;

                meter = AddImageToHUD("jetpackMeter", scene);
                frame = AddImageToHUD("jetpackMeterFrame", scene);
                warning = AddImageToHUD("jetpackMeterWarning", scene);

                GameObject[] meterObjects = { meter, frame, warning };
                foreach(GameObject meterObject in meterObjects) {
                    meterObject.transform.parent = meterContainer.transform;
                    meterObject.transform.localPosition = Vector2.zero;
                }

                meter.GetComponent<Image>().type = Image.Type.Filled;
                meter.GetComponent<Image>().fillMethod = Image.FillMethod.Vertical;

                warning.transform.localPosition += new Vector3(30, 0);
            }
        }

        private GameObject AddImageToHUD(string imageName, Scene scene) {
            Sprite imageSprite = Assets.MainAssetBundle.LoadAsset<Sprite>(imageName);

            GameObject imageObject = new GameObject(imageName);

            SceneManager.MoveGameObjectToScene(imageObject, scene);

            GameObject HUD = GameObject.Find("IngamePlayerHUD");

            RectTransform imageTransform = imageObject.AddComponent<RectTransform>();
            imageTransform.parent = HUD.transform;
            imageTransform.localScale = Vector2.one;
            imageTransform.anchoredPosition = Vector2.zero;
            imageTransform.localPosition = Vector2.zero;
            imageTransform.sizeDelta = new Vector2(imageSprite.rect.width/2, imageSprite.rect.height/2);

            Image image = imageObject.AddComponent<Image>();
            image.sprite = imageSprite;

            CanvasRenderer meterImageCanvasRenderer = imageObject.AddComponent<CanvasRenderer>();

            return imageObject;
        }
    }
}