using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using System.Data;

namespace templateProject
{
    partial class ProposalPane : UserControl
    {
        public ProposalPane()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (PersonResultItem person in lstResults.SelectedItems)
            {
                Microsoft.Office.Interop.Word.Paragraph last = Globals.ThisDocument.Paragraphs[Globals.ThisDocument.Paragraphs.Count];
                last.Range.InsertParagraphAfter();
                Globals.ThisDocument.Paragraphs[Globals.ThisDocument.Paragraphs.Count].Range.Select();
                Microsoft.Office.Tools.Word.RichTextContentControl ctl;
                Guid id = Guid.NewGuid();
                ctl = Globals.ThisDocument.Controls.AddRichTextContentControl("Resume " + id.ToString());
                ctl.Title = "Resume Request";
                ctl.Tag = id.ToString();
                ctl.PlaceholderText = string.Format("Resume request for {0} due by {1}", person.AccountName, dtDueDate.Value.ToShortDateString());
            }
            
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SPQueryService.QueryService service = new SPQueryService.QueryService();
            service.Url = "http://edhild3/Search/_vti_bin/search.asmx";
            service.PreAuthenticate = true;
            service.Credentials = System.Net.CredentialCache.DefaultCredentials;
            service.Timeout = 20000; //wait 20 secs

            string queryPacketTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>"
                + "<QueryPacket xmlns=\"urn:Microsoft.Search.Query\">"
                + "<Query domain=\"QDomain\"><SupportedFormats>"
                + "<Format>urn:Microsoft.Search.Response.Document.Document</Format>"
                + "</SupportedFormats>"
                + "<Context><QueryText language=\"en-US\" type=\"MSSQLFT\">"
                + "{0}</QueryText></Context>"
                + "</Query></QueryPacket>";

            string sqlQueryTemplate = "SELECT PreferredName, AccountName FROM SCOPE() "
                + "WHERE \"Scope\" = 'People' AND CONTAINS(*,'{0}') ";

            string enteredText = txtKeywords.Text.Replace(' ', '+');
            string sqlQuery = string.Format(sqlQueryTemplate, enteredText);
            string queryXml = string.Format(queryPacketTemplate, sqlQuery);

            this.UseWaitCursor = true;
            DataSet ds = service.QueryEx(queryXml);
            DataTable tbl = ds.Tables["RelevantResults"];
            lstResults.Items.Clear();
            foreach (DataRow r in tbl.Rows)
            {
                lstResults.Items.Add(new PersonResultItem(r["PreferredName"].ToString(), r["AccountName"].ToString()));
            }
            this.UseWaitCursor = false;
                       
        }

        

        
    }

    internal class PersonResultItem
    {
        public string PreferredName { get; set; }
        public string AccountName { get; set; }
        public PersonResultItem(string preferredName, string accountName)
        {
            PreferredName = preferredName;
            AccountName = accountName;
        }
        public override string ToString()
        {
            return this.PreferredName;
        }

    }
}
