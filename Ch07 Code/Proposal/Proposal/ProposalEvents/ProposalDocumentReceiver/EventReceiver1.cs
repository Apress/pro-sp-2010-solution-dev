using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
//added
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProposalEvents.EventReceiver1
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class EventReceiver1 : SPItemEventReceiver
    {
       /// <summary>
       /// An item was added.
       /// </summary>
       public override void ItemAdded(SPItemEventProperties properties)
       {
           if (properties.List.Title != "Proposals") return;
           SPFile file = properties.ListItem.File;
           SPWeb web = properties.ListItem.Web;

           ProcessFile(file, web);
           base.ItemAdded(properties);
       }

       /// <summary>
       /// An item was updated.
       /// </summary>
       public override void ItemUpdated(SPItemEventProperties properties)
       {
           if (properties.List.Title != "Proposals") return;
           SPFile file = properties.ListItem.File;
           SPWeb web = properties.ListItem.Web;

           ProcessFile(file, web);
           base.ItemUpdated(properties);
       }

       private void ProcessFile(SPFile file, SPWeb web)
       {
           if (!file.Name.EndsWith(".docx")) return;

           this.EventFiringEnabled = false;
           file.Item.Properties["ProposalID"] = file.Properties["_dlc_DocId"].ToString();
           file.Item.SystemUpdate(false);
           this.EventFiringEnabled = true;

           //process for tasks
           using (Stream stream = file.OpenBinaryStream())
           {
               using (WordprocessingDocument wpDoc = WordprocessingDocument.Open(stream, true))
               {
                   MainDocumentPart docPart = wpDoc.MainDocumentPart;
                   DocumentFormat.OpenXml.Wordprocessing.Document doc = docPart.Document;
                   //find all resume requests
                   string alias = "Resume Request";
                   List<SdtBlock> requests = new List<SdtBlock>();
                   requests = (from w in doc.Descendants<SdtBlock>()
                               where w.Descendants<SdtAlias>().FirstOrDefault() != null &&
                               w.Descendants<SdtAlias>().FirstOrDefault().Val.Value == alias
                               select w).ToList();

                   //for each resume request see if there is a corresponding task
                   foreach (SdtBlock request in requests)
                   {
                       //get the tag for this request
                       string tag = request.GetFirstChild<SdtProperties>().GetFirstChild<Tag>().Val.Value;

                       //is there a task list item with that GUID as a RequestID field
                       using (ProposalEntitiesDataContext dc = new ProposalEntitiesDataContext(web.Url))
                       {
                           var resumeTasks = dc.GetList<Item>("ResumeTasks").Cast<ResumeTasksTask>();
                           var foundTasks = from task in resumeTasks
                                            where task.RequestID == tag
                                            select task;
                           if (foundTasks == null || foundTasks.Count<ResumeTasksTask>() == 0)
                           {
                               //if no then create one
                               SPList resumeTaskList = web.Lists["ResumeTasks"];
                               SPListItem newTask = resumeTaskList.Items.Add();
                               newTask["Title"] = "Your resume is requested";
                               newTask["Body"] = "Please attach your latest resume for inclusion in a proposal";
                               //parse current content for assigned person and date
                               string instruction = request.GetFirstChild<SdtContentBlock>().GetFirstChild<Paragraph>().GetFirstChild<Run>().GetFirstChild<Text>().Text;
                               // Resume request for sample\administrator due by 12/12/2009
                               string account = instruction.Substring(19, instruction.IndexOf(" ", 19) - 19);
                               string dateDue = instruction.Substring(instruction.LastIndexOf(" ") + 1, instruction.Length - instruction.LastIndexOf(" ") - 1);
                               SPUser person = web.AllUsers[account];
                               newTask["AssignedTo"] = person;
                               newTask["DueDate"] = dateDue;
                               newTask["ProposalID"] = file.Properties["_dlc_DocId"].ToString();
                               newTask["RequestID"] = tag;
                               //save the task
                               newTask.Update();
                           }
                       }
                       //else skip to next

                   }

               }
           }
       }



    }
}
