using SandBox.View.Map;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Library.CommandLineFunctionality;

namespace GameInterface.Services.GameDebug.Commands
{
    internal class ResetCamera
    {
        [CommandLineArgumentFunction("reset_camera", "coop.debug")]
        public static string ResetCameraCommand(List<string> strings)
        {
            var cameraView = MapScreen.Instance?.GetMapView<MapCameraView>();

            if (cameraView == null) return "Unable to find camera";

            cameraView.ResetCamera(true, true);

            return "Camera reset";
        }
    }
    internal class ResetCamera2
    {
        [CommandLineArgumentFunction("get_items", "coop.debug")]
        public static string ResetCameraCommand2(List<string> strings)
        {
            ItemRoster roster = MobileParty.MainParty.ItemRoster;

            return roster.Count.ToString();
        }
    }
    internal class ResetCamera3
    {
        [CommandLineArgumentFunction("give_item", "coop.debug")]
        public static string ResetCameraCommand3(List<string> strings)
        {
            ItemRoster roster = MobileParty.MainParty.ItemRoster;

            roster.AddToCounts(new EquipmentElement(new ItemObject("hunting_bow")), 1);

            return "Item Added";
        }
    }
    internal class ResetCamera4
    {
        [CommandLineArgumentFunction("get_others_items", "coop.debug")]
        public static string ResetCameraCommand4(List<string> strings)
        {
            List<MobileParty> parties = MobileParty.All.FindAll(p => p.Ai.IsDisabled);

            foreach(MobileParty party in parties) 
            {
                InformationManager.DisplayMessage(new InformationMessage(party.Name.ToString() + " : " + party.ItemRoster.Count));
            }

            return "Items printed";
        }
    }
}
