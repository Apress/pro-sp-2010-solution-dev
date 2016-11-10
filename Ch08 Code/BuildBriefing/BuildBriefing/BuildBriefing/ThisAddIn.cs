using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;
//added
using Microsoft.Office.Tools;

namespace BuildBriefing
{
    public partial class ThisAddIn
    {
        public CustomTaskPane ctp;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            ctp = Globals.ThisAddIn.CustomTaskPanes.Add(new ucTaskPane(), "Custom Briefing");
            ctp.DockPosition = Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            ctp.Width = 250;

        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
