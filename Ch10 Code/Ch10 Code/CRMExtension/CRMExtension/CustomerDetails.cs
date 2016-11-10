using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.SharePoint.Client;
using System.Windows.Forms;



namespace CRMExtension
{
    partial class CustomerDetails
    {
        #region Form Region Factory

        [Microsoft.Office.Tools.Outlook.FormRegionMessageClass(Microsoft.Office.Tools.Outlook.FormRegionMessageClassAttribute.Contact)]
        [Microsoft.Office.Tools.Outlook.FormRegionName("CRMExtension.CustomerDetails")]
        public partial class CustomerDetailsFactory
        {
            // Occurs before the form region is initialized.
            // To prevent the form region from appearing, set e.Cancel to true.
            // Use e.OutlookItem to get a reference to the current Outlook item.
            private void CustomerDetailsFactory_FormRegionInitializing(object sender, Microsoft.Office.Tools.Outlook.FormRegionInitializingEventArgs e)
            {
            }
        }

        #endregion

        // Occurs before the form region is displayed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void CustomerDetails_FormRegionShowing(object sender, System.EventArgs e)
        {
            Outlook.ContactItem item = (Outlook.ContactItem)this.OutlookItem;
            if (item.UserProperties["CustomerID::5"] != null)
            {
                lstYear.Items.Clear();
                int currentYear = DateTime.Today.Year;
                for (int i = 0; i < 5; i++)
                {
                    lstYear.Items.Add(currentYear - i);
                }

                lstYear.SelectedIndex = 0;
                lstPriority.SelectedIndex = 0;
            }
            else
            {
                lblMessage.Text = "This contact is not a CRM customer.";
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
            }
        }

        // Occurs when the form region is closed.
        // Use this.OutlookItem to get a reference to the current Outlook item.
        // Use this.OutlookFormRegion to get a reference to the form region.
        private void CustomerDetails_FormRegionClosed(object sender, System.EventArgs e)
        {
        }

        private void btnDisplay_Click(object sender, EventArgs e)
        {
            btnDisplay.Enabled = false;
            Outlook.ContactItem item = (Outlook.ContactItem)this.OutlookItem;
            int customerID = (int)item.UserProperties["CustomerID::5"].Value;
            int year = (int)lstYear.SelectedItem;
            string url = string.Format(@"http://edhild3/sites/crm/_vti_bin/ExcelRest.aspx/Shared%20Documents/CRMSales.xlsx/model/charts('Chart%203')?Ranges('CalYear')={0}&Ranges('CustomerID')={1}", year, customerID);
            chart1.SetImage(url);
            btnDisplay.Enabled = true;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            btnSubmit.Enabled = false;
            using (ClientContext context = new ClientContext("http://edhild3/sites/crm"))
            {
                List oList = context.Web.Lists.GetByTitle("FollowUps");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);
                oListItem["Title"] = txtTitle.Text;
                oListItem["DueDate"] = dpDueDate.Value.ToShortDateString();
                Outlook.ContactItem item = (Outlook.ContactItem)this.OutlookItem;
                oListItem["Customer"] = item.CompanyName;

                oListItem["Priority"] = lstPriority.SelectedItem.ToString();
                oListItem["Body"] = txtNote.Text;


                oListItem.Update();
                context.ExecuteQuery();

                MessageBox.Show("Follow up submitted");
                btnSubmit.Enabled = true;

            }
        }
    }
}
