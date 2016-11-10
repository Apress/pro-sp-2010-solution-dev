using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
//added
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.IO;
using DocumentFormat.OpenXml;
using System.IO.Packaging;
using DocumentFormat.OpenXml.Packaging;
using System.Xml;



namespace CustomerDocumentsFeature.Layouts.CustomerDocumentsFeature
{
    public partial class BuildCustomerDoc : LayoutsPageBase
    {
        SPSite siteCollection = null;
        SPWeb webObj = null;
        Guid listId = Guid.Empty;
        int itemId = 0;
        SPFolder customerDocLib = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                siteCollection = SPContext.Current.Site;
                webObj = SPContext.Current.Web;
                listId = new Guid(Server.UrlDecode(Request.QueryString["List"]));
                itemId = int.Parse(Request.QueryString["ID"]);
                customerDocLib = webObj.GetFolder("Lists/CustomerDocuments");
                lblMessage.Visible = false;

                //build button event handler     
                btnOK.Click += new EventHandler(btnOK_Click);


                if (!this.IsPostBack)
                {
                    string returnUrl = "javascript:GoToPage('{0}');return false;";
                    this.btnCancel.OnClientClick = string.Format(returnUrl, Request.QueryString["Source"]);

                    IList<SPContentType> contentTypes = customerDocLib.ContentTypeOrder;
                    //populate the drop down
                    int i = 0;
                    foreach (SPContentType contType in contentTypes)
                    {
                        if (contType.Name != "Document")
                        {
                            ListItem item = new ListItem(contType.Name, i.ToString());
                            lstContentTypes.Items.Add(item);
                        }
                        i++;
                    }
                }

            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }

        }
        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                Stream docStream = new MemoryStream();
                SPContentType contentType = customerDocLib.ContentTypeOrder[int.Parse(lstContentTypes.SelectedValue)];

                SPFile templateFile = contentType.ResourceFolder.Files[contentType.DocumentTemplate];
                Stream templateStream = templateFile.OpenBinaryStream();
                BinaryReader reader = new BinaryReader(templateStream);
                BinaryWriter writer = new BinaryWriter(docStream);
                writer.Write(reader.ReadBytes((int)templateStream.Length));
                writer.Flush();
                reader.Close();
                templateStream.Dispose();

                //insert custom xml part into the document stream
                SPListItem contactItem = null;
                contactItem = webObj.Lists[this.listId].GetItemById(this.itemId);
                //open .docx file in memory stream as package file
                docStream.Position = 0;

                //Begin edits here for Open XML SDK
                WordprocessingDocument wordDoc = WordprocessingDocument.Open(docStream, true);
                MainDocumentPart mainPart = wordDoc.MainDocumentPart;

                //retrieve package part with XML data
                XmlDocument xDoc = null;
                CustomXmlPart customPart = null;

                foreach (CustomXmlPart cp in mainPart.CustomXmlParts)
                {
                    xDoc = new XmlDocument();
                    xDoc.Load(cp.GetStream());
                    if (xDoc.DocumentElement.NamespaceURI == "http://www.sample.com/2006/schemas/contact/")
                    {
                        customPart = cp;
                        break;
                    }
                }

                //serialize the contact item into this customXml part  
                XmlNode rootNode = xDoc.DocumentElement;
                rootNode.RemoveAll();
                foreach (SPField field in contactItem.Fields)
                {
                    XmlNode fieldNode = xDoc.CreateElement("sc", XmlConvert.EncodeName(field.Title), "http://www.sample.com/2006/schemas/contact/");
                    if (contactItem[field.Id] != null)
                    {
                        XmlNode fieldVal = xDoc.CreateTextNode(contactItem[field.Id].ToString());
                        fieldNode.AppendChild(fieldVal);
                    }
                    rootNode.AppendChild(fieldNode);
                }

                xDoc.Save(customPart.GetStream(FileMode.Create, FileAccess.Write));
                //deliver file to library
                customerDocLib.Files.Add(txtFileName.Text, docStream);
                lblMessage.Visible = true;

            }
            catch (Exception ex)
            {
                Response.Write(ex.ToString());
            }

        }
    }
}
