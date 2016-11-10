using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
//added
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Microsoft.SharePoint.Navigation;
using System.Diagnostics;


namespace SiteProvisionActivity
{
    public partial class ProvisionActivity : Activity
    {
        public ProvisionActivity()
        {
            InitializeComponent();
        }

        #region Dependency Fields
        public static DependencyProperty __ContextProperty = DependencyProperty.Register("__Context", typeof(WorkflowContext), typeof(ProvisionActivity));

        [Description("Context")]
        [ValidationOption(ValidationOption.Required)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public WorkflowContext __Context
        {
            get
            {
                return ((WorkflowContext)(base.GetValue(ProvisionActivity.__ContextProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.__ContextProperty, value);
            }
        }

        public static DependencyProperty SiteUrlProperty = DependencyProperty.Register("SiteUrl", typeof(string), typeof(ProvisionActivity));

        [Description("SiteUrl")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SiteUrl
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.SiteUrlProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.SiteUrlProperty, value);
            }
        }

        public static DependencyProperty SiteTitleProperty = DependencyProperty.Register("SiteTitle", typeof(string), typeof(ProvisionActivity));

        [Description("SiteTitle")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SiteTitle
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.SiteTitleProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.SiteTitleProperty, value);
            }
        }

        public static DependencyProperty SiteDescriptionProperty = DependencyProperty.Register("SiteDescription", typeof(string), typeof(ProvisionActivity));

        [Description("SiteDescription")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SiteDescription
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.SiteDescriptionProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.SiteDescriptionProperty, value);
            }
        }

        public static DependencyProperty LocaleIDProperty = DependencyProperty.Register("LocaleID", typeof(UInt32), typeof(ProvisionActivity));

        [Description("LocaleID")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public UInt32 LocaleID
        {
            get
            {
                return ((UInt32)(base.GetValue(ProvisionActivity.LocaleIDProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.LocaleIDProperty, value);
            }
        }

        public static DependencyProperty SiteTemplateProperty = DependencyProperty.Register("SiteTemplate", typeof(string), typeof(ProvisionActivity));

        [Description("SiteTemplate")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string SiteTemplate
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.SiteTemplateProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.SiteTemplateProperty, value);
            }
        }

        public static DependencyProperty UseUniquePermissionsProperty = DependencyProperty.Register("UseUniquePermissions", typeof(string), typeof(ProvisionActivity));

        [Description("UseUniquePermissions")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string UseUniquePermissions
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.UseUniquePermissionsProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.UseUniquePermissionsProperty, value);
            }
        }

        public static DependencyProperty ConvertIfThereProperty = DependencyProperty.Register("ConvertIfThere", typeof(string), typeof(ProvisionActivity));

        [Description("ConvertIfThere")]
        [Category("Site Provision Actions")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string ConvertIfThere
        {
            get
            {
                return ((string)(base.GetValue(ProvisionActivity.ConvertIfThereProperty)));
            }
            set
            {
                base.SetValue(ProvisionActivity.ConvertIfThereProperty, value);
            }
        }


        #endregion

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            try
            {
                using (SPSite sourceSite = new SPSite(this.__Context.Web.Site.ID))
                {
                    using (SPWeb currentWeb = sourceSite.AllWebs[this.__Context.Web.ID])
                    {
                        SPWeb newWeb = currentWeb.Webs.Add(SiteUrl, SiteTitle, SiteDescription, LocaleID, SiteTemplate, bool.Parse(UseUniquePermissions), bool.Parse(ConvertIfThere));

                    }
                }

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("ProvisionActivity", ex.ToString());
            }

            return ActivityExecutionStatus.Closed;
        }
    }
}
