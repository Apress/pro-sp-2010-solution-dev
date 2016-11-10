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

namespace ProposalEvents.ProposalTaskReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class ProposalTaskReceiver : SPItemEventReceiver
    {
       /// <summary>
       /// An item was updated.
       /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {

            //see if the task is completed
            if (properties.ListItem["PercentComplete"] != null && float.Parse(properties.ListItem["PercentComplete"].ToString()) == 1)
            {
                //make sure there is a proposal id
                if (!String.IsNullOrEmpty(properties.ListItem["ProposalID"].ToString()))
                {
                    //make sure there is an attached resume
                    if (properties.ListItem.Attachments.Count == 1)
                    {
                        //retrieve attachment
                        string resumeAttachment = String.Empty;
                        resumeAttachment = properties.ListItem.Attachments.UrlPrefix + properties.ListItem.Attachments[0];
                        SPFile resumeFile = properties.Web.GetFile(resumeAttachment);
                        //make sure it is a word document
                        if (resumeAttachment.EndsWith(".docx"))
                        {
                            //get the proposal document
                            SPFile proposalFile = null;
                            using (ProposalEntitiesDataContext dc = new ProposalEntitiesDataContext(properties.WebUrl))
                            {
                                Microsoft.SharePoint.Linq.EntityList<ProposalsDocument> proposals = dc.GetList<ProposalsDocument>("Proposals");
                                var found = from doc in proposals.ToList()
                                            where doc.DocumentIDValue == properties.ListItem["ProposalID"].ToString()
                                            select doc.Name;
                                string name = found.First<String>();
                                proposalFile = properties.Web.Folders["Proposals"].Files[name];

                            }

                            Stream stream = null;
                            using (stream = proposalFile.OpenBinaryStream())
                            {
                                using (WordprocessingDocument wpDoc = WordprocessingDocument.Open(stream, true))
                                {
                                    MainDocumentPart docPart = wpDoc.MainDocumentPart;
                                    DocumentFormat.OpenXml.Wordprocessing.Document doc = docPart.Document;
                                    //find the work item
                                    string alias = "Resume Request";
                                    List<SdtBlock> requests = new List<SdtBlock>();
                                    requests = (from w in doc.Descendants<SdtBlock>()
                                                where w.Descendants<SdtAlias>().FirstOrDefault() != null &&
                                                w.Descendants<SdtAlias>().FirstOrDefault().Val.Value == alias &&
                                                w.Descendants<Tag>().FirstOrDefault() != null &&
                                                w.Descendants<Tag>().FirstOrDefault().Val.Value == properties.ListItem["RequestID"].ToString()
                                                select w).ToList();

                                    //build the addition
                                    string chunkId = String.Format("AltChunkId{0}", properties.ListItemId.ToString());
                                    AlternativeFormatImportPart chunk = docPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.WordprocessingML, chunkId);
                                    chunk.FeedData(resumeFile.OpenBinaryStream());
                                    AltChunk altChunk = new AltChunk();
                                    altChunk.Id = chunkId;
                                    SdtBlock newBlock = new SdtBlock();
                                    newBlock.AppendChild(altChunk);
                                    requests[0].InsertBeforeSelf(newBlock);
                                    //remove the request content control
                                    requests[0].Remove();
                                    //save the result
                                    doc.Save();
                                    wpDoc.Close();
                                    if (proposalFile.LockType != SPFile.SPLockType.Exclusive)
                                    {
                                        proposalFile.CreateSharedAccessRequest();
                                        proposalFile.SaveBinary(stream);
                                        proposalFile.RemoveSharedAccessRequest();
                                    }

                                }
                            }


                        }
                    }
                }
            }
        }


    }
}
