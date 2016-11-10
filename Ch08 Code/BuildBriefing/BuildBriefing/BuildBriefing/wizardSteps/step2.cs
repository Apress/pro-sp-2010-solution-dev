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
    public partial class step2 : UserControl, IStep
    {
        private ucTaskPane taskpane = null;
       
        public step2()
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
           if (ParentPane.HasObjectivesList)
           {
                //show this step
                this.Visible = true;
                
           }
            else
           {
                //skip over this step
               this.WorkComplete();
           }
        
        }



        public ucTaskPane ParentPane
        {
            get { return taskpane; }
        }
  

        private void btnBuild_Click(object sender, EventArgs e)
        {
            this.btnNext.Enabled = false;
            this.ParentPane.Message = "Building Objectives";
                        
            //establish client context for accessing SharePoint content
            using (SPClient.ClientContext ctx = new SPClient.ClientContext(this.ParentPane.SiteUrl))
            {
                //query to get the list items
                SPClient.Web web = ctx.Web;
                SPClient.ListCollection coll = web.Lists;
                SPClient.List objectivesList = coll.GetByTitle("Objectives");
                SPClient.CamlQuery query = SPClient.CamlQuery.CreateAllItemsQuery();
                SPClient.ListItemCollection items = objectivesList.GetItems(query);

                ctx.Load(items);
                ctx.ExecuteQuery();

                //add a slide to the current presentation
                PowerPoint.Slide slide;
                PowerPoint.Presentation presentation;
                presentation = Globals.ThisAddIn.Application.ActivePresentation;
                PowerPoint.CustomLayout layout = presentation.SlideMaster.CustomLayouts[PowerPoint.PpSlideLayout.ppLayoutText];
                slide = presentation.Slides.AddSlide(presentation.Slides.Count+1,layout);
                slide.Shapes.Title.TextFrame.TextRange.Text = "Objectives";

                StringBuilder sBuilder = new StringBuilder();
           
                foreach (SPClient.ListItem item in items)
                {
                    
                    sBuilder.Append((string)item.FieldValues["Objective"]);
                    sBuilder.Append("\n");
                }
                slide.Shapes[2].TextFrame.TextRange.Text = sBuilder.ToString().TrimEnd('\n');
                Globals.ThisAddIn.Application.ActiveWindow.View.GotoSlide(slide.SlideIndex);

            }
            this.ParentPane.Message = "Done";
            this.WorkComplete();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            this.WorkComplete();
        }
    }
}
