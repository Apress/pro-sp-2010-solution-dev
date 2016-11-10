using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Administration;
using System.Collections.ObjectModel;

namespace SiteProvisionFeature.Features.SiteProvisionFeature
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("878e93bf-320f-46dd-9aaa-720f9601bed1")]
    public class SiteProvisionFeatureEventReceiver : SPFeatureReceiver
    {
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWebApplication currentWebApp = (SPWebApplication)properties.Feature.Parent;
            SPWebConfigModification configMod = new SPWebConfigModification();
            configMod.Name = "authorizedType[@Assembly='SiteProvisionActivity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5c639ac57ef5dbf'][@Namespace='SiteProvisionActivity'][@TypeName='*'][@Authorized='True']";
            configMod.Owner = "SiteProvisionFeature";
            configMod.Path = "configuration/System.Workflow.ComponentModel.WorkflowCompiler/authorizedTypes";
            configMod.Type = SPWebConfigModification.SPWebConfigModificationType.EnsureChildNode;
            configMod.Value = @"<authorizedType Assembly='SiteProvisionActivity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=d5c639ac57ef5dbf' Namespace='SiteProvisionActivity' TypeName='*' Authorized='True' />";
            currentWebApp.WebConfigModifications.Add(configMod);
            currentWebApp.Update();
            currentWebApp.WebService.ApplyWebConfigModifications();
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
             SPWebApplication currentWebApp = (SPWebApplication)properties.Feature.Parent;
            Collection<SPWebConfigModification> modificationCollection = currentWebApp.WebConfigModifications;
            Collection<SPWebConfigModification> removeCollection = new Collection<SPWebConfigModification>();

            int count = modificationCollection.Count;
            for (int i = 0; i < count; i++)
            {
                SPWebConfigModification modification = modificationCollection[i];
                if (modification.Owner == "SiteProvisionFeature")
                {
                    // collect modifications to delete
                    removeCollection.Add(modification);
                }
            }

            // now delete the modifications from the web application
            if (removeCollection.Count > 0)
            {
                foreach (SPWebConfigModification modificationItem in removeCollection)
                {
                    currentWebApp.WebConfigModifications.Remove(modificationItem);
                }

                // Commit modification removals to the specified web application
                currentWebApp.Update();
                // Push modifications through the farm
                currentWebApp.WebService.ApplyWebConfigModifications();
            }
        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
