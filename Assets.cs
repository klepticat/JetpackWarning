using System.Reflection;
using UnityEngine;

// credit to upsidedowncatfish for the asset loader class

namespace JetpackWarning {
    public static class Assets {
        public static string mainAssetBundleName = "jetpackAssets";
        public static AssetBundle MainAssetBundle = null;

        private static string GetAssemblyName() => Assembly.GetExecutingAssembly().FullName.Split(',')[0];
        public static void PopulateAssets() {
            if(MainAssetBundle == null) {
                using(var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GetAssemblyName() + "." + mainAssetBundleName)) {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }
        }
    }
}