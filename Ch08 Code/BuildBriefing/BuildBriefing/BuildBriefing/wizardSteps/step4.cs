using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//added
using SPClient = Microsoft.SharePoint.Client;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace BuildBriefing.wizardSteps
{
    public partial class step4 : UserControl, IStep
    {
       private ucTaskPane taskpane = null;
       
        public step4()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            

        }

        public event EventHandler Completed;

        public void WorkComplete()
        {
            if (Completed != null)
            {
                Completed(this, new EventArgs());
            }
        }

        public void Start()
        {
            //get reference to the custom task pane
            taskpane = (ucTaskPane)this.Parent;
           if (ParentPane.HasSlideLibrary)
           {
               ListSlideLibraryLinks();
                //show this step
                this.Visible = true;
               
                
           }
            else
           {
                //skip over this step
               this.WorkComplete();
           }
        
        }
        private void ListSlideLibraryLinks()
        {
            pnlLinks.Controls.Clear();
            LinkLabel[] linkControls = new LinkLabel[ParentPane.SlideLibraries.Count];
            for (int i=0; i<this.ParentPane.SlideLibraries.Count; i++)
            {
                linkControls[i] = new LinkLabel();
                linkControls[i].Location = new Point(5, 25 * i);
                linkControls[i].LinkClicked +=new LinkLabelLinkClickedEventHandler(step4_LinkClicked);
                linkControls[i].LinkBehavior = LinkBehavior.AlwaysUnderline;
                LibraryItem item = (LibraryItem) ParentPane.SlideLibraries[i];
                linkControls[i].Text = item.Name;
                LinkLabel.Link link = linkControls[i].Links.Add(0, item.Name.Length);
                link.LinkData = item.Url;
            }
            pnlLinks.Controls.AddRange(linkControls);
        }

        void step4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }
   



        public ucTaskPane ParentPane
        {
            get { return taskpane; }
        }
  

      

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.WorkComplete();
        }

       
    }
}
