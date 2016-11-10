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

namespace BuildBriefing.wizardSteps
{
    public partial class step1 : UserControl, IStep 
    {
        private ucTaskPane taskpane = null;
        


        public step1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
           
           
        }
       

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.btnNext.Enabled = false;
            this.ParentPane.Message = "Examining Site";
            this.UseWaitCursor = true;
           
            
            this.ParentPane.SiteUrl = this.txtSiteUrl.Text;
            //establish client context for accessing SharePoint content
            using (SPClient.ClientContext ctx = new SPClient.ClientContext(this.ParentPane.SiteUrl))
            {
                SPClient.Web web = ctx.Web;
                SPClient.ListCollection coll = web.Lists;
                ctx.Load(coll);
                //gather default properties about all lists in the site
                ctx.ExecuteQuery();

                var q1 = from list in coll
                         where list.Title == "Agenda"
                         select list;
                if (q1 != null) this.ParentPane.HasAgendaList = true;


                var q2 = from list in coll
                         where list.Title == "Objectives"
                         select list;
                if (q2 != null) this.ParentPane.HasObjectivesList = true;


                var q3 = from list in coll
                         where list.BaseTemplate == 2100
                         select list;

                if (q3 != null)
                {
                    this.ParentPane.HasSlideLibrary = true;
                    Uri link = new Uri(this.ParentPane.SiteUrl);
                    foreach (SPClient.List list in coll)
                    {
                        if (list.BaseTemplate == 2100)
                        {
                            LibraryItem slideLibrary = new LibraryItem();
                            slideLibrary.Name = list.Title;
                            if (link.Port == 80 || link.Port == 443)
                            {
                                slideLibrary.Url = link.Scheme + "://" + link.Host + list.DefaultViewUrl;
                            }
                            else
                            {
                                slideLibrary.Url = link.Scheme + "://" + link.Host + ":" + link.Port + list.DefaultViewUrl;
                            }
                            this.ParentPane.SlideLibraries.Add(slideLibrary);
                        }
                        
                    }
                }
            }
                this.UseWaitCursor = false;
                this.ParentPane.Message = "Done";
               
                this.WorkComplete();


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

            this.Visible = true;
            //get reference to the custom task pane
            taskpane = (ucTaskPane)this.Parent;
        }



        public ucTaskPane ParentPane
        {
            get { return taskpane; }
        }
    }
}
