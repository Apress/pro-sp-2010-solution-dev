using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
//added
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.IO;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Packaging;
using Drawing = DocumentFormat.OpenXml.Drawing;
using System.Linq;
using System.Text;


namespace DynamicPowerPoint.DynPPTWP
{
    public partial class DynPPTWPUserControl : UserControl
    {
        DynPPTWP parentWebPart = null;
        SPWeb m_web = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            parentWebPart = (DynPPTWP)this.Parent;
            this.lblErrorMessage.Visible = false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (parentWebPart.m_errorMessage != String.Empty)
            {
                lblErrorMessage.Text = parentWebPart.m_errorMessage;
                lblErrorMessage.Visible = true;
            }
            base.OnPreRender(e);
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            Stream templateStream = null;
            try
            {

                m_web = SPControl.GetContextWeb(this.Context);
                SPFolder sharedDocs = m_web.GetFolder(parentWebPart.LibraryName);
                SPFile templateFile = sharedDocs.Files[parentWebPart.TemplateName];
                templateStream = templateFile.OpenBinaryStream();
                this.ProcessSlides(templateStream);

                sharedDocs.Files.Add(parentWebPart.FileName, templateStream, true);
                this.btnGenerate.Visible = false;
                lblInstructions.Text = String.Format("The presentation has been created as {0} in the {1} library", parentWebPart.FileName, parentWebPart.LibraryName);

            }
            catch (Exception ex)
            {
                parentWebPart.m_errorMessage = ex.Message;
            }

        }
        public void ProcessSlides(Stream fileStream)
        {
            PresentationDocument presDoc = PresentationDocument.Open(fileStream, true);
            PresentationPart presPart = presDoc.PresentationPart;
            Presentation presentation = presPart.Presentation;
            if (presentation.SlideIdList != null)
            {
                //get the title of each slide in the slide order
                foreach (SlideId slideId in presentation.SlideIdList.Elements<SlideId>())
                {
                    SlidePart slidePart = (SlidePart)presPart.GetPartById(slideId.RelationshipId.ToString());
                    //get the slide's title
                    string title = GetSlideTitle(slidePart);
                    switch (title)
                    {
                        case "#Site Title#":
                            BuildTitleSlide(slidePart, m_web.Title);
                            break;
                        case "Hardware Issues":
                            BuildHardwareSlide(slidePart, m_web.Url);
                            break;
                        case "Software Issues":
                            BuildSoftwareSlide(slidePart, m_web.Url);
                            break;
                        case "Other":
                            BuildOtherSlide(slidePart, m_web.Url);
                            break;
                    }
                    slidePart.Slide.Save();
                }
            }

        }
        private void BuildTitleSlide(SlidePart slidePart, string siteTitle)
        {
            var shapes = from shape in slidePart.Slide.Descendants<Shape>()
                         select shape;
            foreach (Shape shape in shapes)
            {
                PlaceholderShape placeholderShape = shape.NonVisualShapeProperties.ApplicationNonVisualDrawingProperties.GetFirstChild<PlaceholderShape>();
                if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
                {
                    Drawing.Paragraph paragraph = null;
                    Drawing.Text text = null;

                    switch (placeholderShape.Type.Value)
                    {
                        case PlaceholderValues.Title:
                            paragraph = shape.TextBody.Descendants<Drawing.Paragraph>().First();
                            text = paragraph.Descendants<Drawing.Text>().First();
                            text.Text = siteTitle;
                            break;
                        case PlaceholderValues.CenteredTitle:
                            paragraph = shape.TextBody.Descendants<Drawing.Paragraph>().First();
                            text = paragraph.Descendants<Drawing.Text>().First();
                            text.Text = siteTitle;
                            break;
                        case PlaceholderValues.SubTitle:
                            paragraph = shape.TextBody.Descendants<Drawing.Paragraph>().First();
                            Drawing.Text authortext = paragraph.Descendants<Drawing.Text>().First();
                            authortext.Text = this.Context.User.Identity.Name;
                            paragraph = shape.TextBody.Descendants<Drawing.Paragraph>().ElementAt(1);
                            Drawing.Text timestamptext = paragraph.Descendants<Drawing.Text>().First();
                            timestamptext.Text = DateTime.Today.ToShortDateString();
                            break;
                    }
                }

            }
        }
        private void BuildHardwareSlide(SlidePart slidePart, string webUrl)
        {
            try
            {
                using (EntitiesDataContext dc = new EntitiesDataContext(webUrl))
                {
                    var texts = from text in slidePart.Slide.Descendants<TextBody>()
                                select text;

                    texts.First().RemoveAllChildren();

                    var q = from issue in dc.Issues
                            where issue.Category == Category.Hardware
                            select issue.Title;

                    texts.First().AppendChild(new Drawing.BodyProperties());
                    texts.First().AppendChild(new Drawing.ListStyle());

                    foreach (string title in q)
                    {
                        texts.First().AppendChild(this.GenerateParagraph(title));
                    }

                    texts.First().LastChild.AppendChild(new Drawing.EndParagraphRunProperties() { Language = "en-US" });


                }
            }
            catch (Exception ex)
            {
                parentWebPart.m_errorMessage = ex.Message;
            }

        }
        private void BuildOtherSlide(SlidePart slidePart, string webUrl)
        {
            try
            {
                using (EntitiesDataContext dc = new EntitiesDataContext(webUrl))
                {
                    var texts = from text in slidePart.Slide.Descendants<TextBody>()
                                select text;

                    texts.First().RemoveAllChildren();

                    var q = from issue in dc.Issues
                            where issue.Category == Category.Other
                            select issue.Title;

                    texts.First().AppendChild(new Drawing.BodyProperties());
                    texts.First().AppendChild(new Drawing.ListStyle());

                    foreach (string title in q)
                    {
                        texts.First().AppendChild(this.GenerateParagraph(title));
                    }

                    texts.First().LastChild.AppendChild(new Drawing.EndParagraphRunProperties() { Language = "en-US" });


                }
            }
            catch (Exception ex)
            {
                parentWebPart.m_errorMessage = ex.Message;
            }

        }
        private void BuildSoftwareSlide(SlidePart slidePart, string webUrl)
        {
            try
            {
                var tables = from table in slidePart.Slide.Descendants<Drawing.Table>()
                             select table;

                using (EntitiesDataContext dc = new EntitiesDataContext(webUrl))
                {

                    var q = from issue in dc.Issues
                            where issue.Category == Category.Software
                            select issue;

                    foreach (IssuesIssue item in q)
                    {
                        //add a row to the table
                        Drawing.TableRow tr = new Drawing.TableRow();
                        tr.Height = 370840;
                        tr.Append(CreateTableCell(item.Title.ToString()));
                        tr.Append(CreateTableCell(item.AssignedTo.ToString()));
                        tr.Append(CreateTableCell(item.Priority.Value.ToString()));
                        tables.First().Append(tr);
                    }


                }
            }
            catch (Exception ex)
            {
                parentWebPart.m_errorMessage = ex.Message;
            }

        }

        public Drawing.TableCell CreateTableCell(string text)
        {
            Drawing.TableCell tableCell1 = new Drawing.TableCell();

            Drawing.TextBody textBody1 = new Drawing.TextBody();
            Drawing.BodyProperties bodyProperties1 = new Drawing.BodyProperties();
            Drawing.ListStyle listStyle1 = new Drawing.ListStyle();

            Drawing.Paragraph paragraph1 = new Drawing.Paragraph();

            Drawing.Run run1 = new Drawing.Run();
            Drawing.RunProperties runProperties1 = new Drawing.RunProperties() { Language = "en-US", Dirty = false, SmartTagClean = false };
            Drawing.Text text1 = new Drawing.Text();
            text1.Text = text;

            run1.Append(runProperties1);
            run1.Append(text1);
            Drawing.EndParagraphRunProperties endParagraphRunProperties1 = new Drawing.EndParagraphRunProperties() { Language = "en-US", Dirty = false };

            paragraph1.Append(run1);
            paragraph1.Append(endParagraphRunProperties1);

            textBody1.Append(bodyProperties1);
            textBody1.Append(listStyle1);
            textBody1.Append(paragraph1);
            Drawing.TableCellProperties tableCellProperties1 = new Drawing.TableCellProperties();

            tableCell1.Append(textBody1);
            tableCell1.Append(tableCellProperties1);
            return tableCell1;
        }



        // Creates an Paragraph instance and adds its children.
        public Drawing.Paragraph GenerateParagraph(string text)
        {
            Drawing.Paragraph paragraph1 = new Drawing.Paragraph();

            Drawing.Run run1 = new Drawing.Run();
            Drawing.RunProperties runProperties1 = new Drawing.RunProperties() { Language = "en-US", Dirty = false, SmartTagClean = false };
            Drawing.Text text1 = new Drawing.Text();
            text1.Text = text;

            run1.Append(runProperties1);
            run1.Append(text1);
            
            paragraph1.Append(run1);
            return paragraph1;
        }


        // <summary>
        // Get the title string of the slide.
        // </summary>
        // <param name="slidePart">The slide.</param>
        // <returns>Returns a string that represents the title of the slide. If the slide has no title, returns nothing.</returns>
        public static string GetSlideTitle(SlidePart slidePart)
        {
            if (slidePart == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            // Declare a paragraph separator.
            string paragraphSeparator = null;

            if (slidePart.Slide != null)
            {
                // Find all the title shapes.
                var shapes = from shape in slidePart.Slide.Descendants<Shape>()
                             where IsTitleShape(shape)
                             select shape;

                StringBuilder paragraphText = new StringBuilder();

                foreach (var shape in shapes)
                {
                    // Get the text in each paragraph in this shape.
                    foreach (var paragraph in shape.TextBody.Descendants<Drawing.Paragraph>())
                    {
                        // Add a line break.
                        paragraphText.Append(paragraphSeparator);

                        foreach (var text in paragraph.Descendants<Drawing.Text>())
                        {
                            paragraphText.Append(text.Text);
                        }

                        paragraphSeparator = "\n";
                    }
                }

                return paragraphText.ToString();
            }

            return string.Empty;
        }

        // <summary>
        // Determines whether the shape is a title shape.
        // </summary>
        // <param name="shape">The shape to test.</param>
        // <returns>True if the shape is a title shape.</returns>
        private static bool IsTitleShape(Shape shape)
        {
            var placeholderShape = shape.NonVisualShapeProperties.ApplicationNonVisualDrawingProperties.GetFirstChild<PlaceholderShape>();
            if (placeholderShape != null && placeholderShape.Type != null && placeholderShape.Type.HasValue)
            {
                switch ((PlaceholderValues)placeholderShape.Type)
                {
                    // Any title shape.
                    case PlaceholderValues.Title:

                    // A centered title.
                    case PlaceholderValues.CenteredTitle:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }





    }
}
