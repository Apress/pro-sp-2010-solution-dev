using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using Word = Microsoft.Office.Word.Server;

namespace PublishToPDF.Workflow1
{
    public sealed partial class Workflow1 : SequentialWorkflowActivity
    {
        public Workflow1()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        private void codeActivity1_ExecuteCode(object sender, EventArgs e)
        {
            string file = workflowProperties.WebUrl + "/" + workflowProperties.ItemUrl;

            //schedule the conversion
            Word.Conversions.ConversionJob conversionJob = new Word.Conversions.ConversionJob("Word Automation Services");
            conversionJob.Name = "Proposal Conversion";
            //run under the user that ran the workflow 
            conversionJob.UserToken = workflowProperties.OriginatorUser.UserToken;
            conversionJob.AddFile(file, file.Replace(".docx", ".pdf"));
            SPSecurity.RunWithElevatedPrivileges(delegate { conversionJob.Start(); });

        }


    }
}
