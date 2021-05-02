﻿using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CSLShowCommuterDestination {
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private GameObject stopDestinationInfoPanel;

        public string Name => "CSL Show Commuter Destination Mod";
        public string Description => "When viewing a public transport stop, see the target buildings for all commuters currently at that stop";

        public void OnEnabled() {
            UnityEngine.Debug.Log("OnEnabled hit it yooo");
            HarmonyHelper.DoOnHarmonyReady(Patcher.Patch);
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.Unpatch();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            UIView uiView = UnityEngine.Object.FindObjectOfType<UIView>();
            if ((UnityEngine.Object)uiView != (UnityEngine.Object)null)
            {
                this.stopDestinationInfoPanel = new GameObject("StopDestinationInfoPanel");
                this.stopDestinationInfoPanel.transform.parent = uiView.transform;
                this.stopDestinationInfoPanel.AddComponent<StopDestinationInfoPanel>();
            }
        }
    }
}
