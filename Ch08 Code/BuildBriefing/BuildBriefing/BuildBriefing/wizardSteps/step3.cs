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
    public partial class step3 : UserControl, IStep
    {
        private ucTaskPane taskpane = null;
       
        public step3()
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
           if (ParentPane.HasAgendaList)
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
            this.ParentPane.Message = "Building Agenda";
            
            //establish client context for accessing SharePoint content
            using (SPClient.ClientContext ctx = new SPClient.ClientContext(this.ParentPane.SiteUrl))
            {
                //query to get the list items
                SPClient.Web web = ctx.Web;
                SPClient.ListCollection coll = web.Lists;
                SPClient.List agendaList = coll.GetByTitle("Agenda");
                SPClient.CamlQuery query = SPClient.CamlQuery.CreateAllItemsQuery();
                SPClient.ListItemCollection items = agendaList.GetItems(query);

                ctx.Load(items);
                ctx.ExecuteQuery();

                //add a slide to the current presentation
                PowerPoint.Slide slide;
                PowerPoint.Presentation presentation;
                presentation = Globals.ThisAddIn.Application.ActivePresentation;
                PowerPoint.CustomLayout layout = presentation.SlideMaster.CustomLayouts[PowerPoint.PpSlideLayout.ppLayoutText];
                slide = presentation.Slides.AddSlide(presentation.Slides.Count+1,layout);
                slide.Shapes.Title.TextFrame.TextRange.Text = "Agenda";

                PowerPoint.Shape placeholderTable = slide.Shapes.Placeholders[2];
                PowerPoint.Shape tblAgenda = slide.Shapes.AddTable(items.Count, 2, placeholderTable.Left, placeholderTable.Top, placeholderTable.Width, placeholderTable.Height);
                

                tblAgenda.Table.Columns[1].Width = 200;
                tblAgenda.Table.Columns[2].Width = 400;
                tblAgenda.Table.FirstRow = false;

                StringBuilder notesText = new StringBuilder();
           
                for (int i = 1; i<=items.Count; i++)
                {
                    string time = (string) items[i-1].FieldValues["Time"];
                    string title = (string) items[i-1].FieldValues["Title"];
                    string owner = (string) items[i-1].FieldValues["Owner"];
                    string notes = string.Empty;
                    
                    //we need to make another request to get the notes field stripped of HTML markup
                    SPClient.FieldStringValues values = items[i-1].FieldValuesAsText;
                    ctx.Load(values);
                    ctx.ExecuteQuery();
                    if (values["Notes"] != null && values["Notes"] != string.Empty)
                    {
                        notesText.Append(string.Format("({0}): {1}", owner, values["Notes"]));
                        notesText.Append("\n");
                    }
                   
                    tblAgenda.Table.Cell(i,1).Shape.TextFrame2.TextRange.Text = time;
                    tblAgenda.Table.Cell(i, 2).Shape.TextFrame2.TextRange.Text = title;
                    
                }
                slide.NotesPage.Shapes.Placeholders[2].TextFrame2.TextRange.Text = notesText.ToString();
            
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
