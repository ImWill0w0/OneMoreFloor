using Sandbox;
using Sandbox.UI;
using OneMoreFloor.Player;

namespace OneMoreFloor.Hud
{
    class Crosshair : Panel
    {
        private Panel CrosshairPanel;

        public Crosshair()
        {
            Log.Info("crosshair!");
            StyleSheet.Load("/Hud/Crosshair.scss");

            CrosshairPanel = Add.Panel("wrapper").Add.Panel("crosshair");
        }

        public override void Tick()
        {
            CrosshairPanel.SetClass("show", OMFPlayer.LocalPlayer.GetUsableTrace() != null);
        }
    }
}