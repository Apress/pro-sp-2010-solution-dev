using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BuildBriefing.wizardSteps
{
    public partial class step5 : UserControl, IStep
    {
         private ucTaskPane taskpane = null;
       
        public step5()
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
           this.Visible = true;
           
           
        
        }

        public ucTaskPane ParentPane
        {
            get { return taskpane; }
        }

    }
}
