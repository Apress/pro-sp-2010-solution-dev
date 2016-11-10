using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace DynamicPowerPoint.DynPPTWP
{
    [ToolboxItemAttribute(false)]
    public class DynPPTWP : WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/DynamicPowerPoint/DynPPTWP/DynPPTWPUserControl.ascx";
        public string m_errorMessage = string.Empty;

        [WebBrowsable(true), Personalizable(PersonalizationScope.Shared)]
        public string LibraryName { get; set; }

        [WebBrowsable(true), Personalizable(PersonalizationScope.Shared)]
        public string FileName { get; set; }

        [WebBrowsable(true), Personalizable(PersonalizationScope.Shared)]
        public string TemplateName { get; set; }

        protected override void CreateChildControls()
        {
            Control control = Page.LoadControl(_ascxPath);
            Controls.Add(control);
        }
    }
}
