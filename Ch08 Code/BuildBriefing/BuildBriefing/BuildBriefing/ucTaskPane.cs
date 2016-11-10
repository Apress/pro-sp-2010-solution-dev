using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//added
using System.Collections;
using BuildBriefing.wizardSteps;



namespace BuildBriefing
{
    public partial class ucTaskPane : UserControl
    {
        private IStep[] steps = new wizardSteps.IStep[] {new step1(), new step2(), new step3(), new step4(), new step5() };


        public bool HasAgendaList =     false;
        public bool HasObjectivesList = false;
        public bool HasSlideLibrary =   false;
        public ArrayList SlideLibraries = new ArrayList();

        public string SiteUrl = String.Empty;
        private int currentStep = 1;

        public string Message
        {
            get
            {
                return toolStripStatusLabel1.Text;
            }
            set
            {
                toolStripStatusLabel1.Text = value;
                this.Update();
            }
        }
   
        public ucTaskPane()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            this.Message = String.Empty;

            foreach (IStep step in steps)
            {
                step.Completed += new EventHandler(wizard_Completed);
                UserControl stepCtl = step as UserControl;
                stepCtl.Visible = false;
                this.Controls.Add(stepCtl);
            }

            steps[0].Start();
        }

        void wizard_Completed(object sender, EventArgs e)
        {
            if (currentStep < steps.Length) this.MoveToNextStep();
        }

        private void MoveToNextStep()
        {
            this.Controls[currentStep].Visible = false;
            currentStep++;
            IStep step = (IStep) this.Controls[currentStep];
            step.Start();

        }
    }

    public struct LibraryItem
    {
        public string Url;
        public string Name;
    }
}
