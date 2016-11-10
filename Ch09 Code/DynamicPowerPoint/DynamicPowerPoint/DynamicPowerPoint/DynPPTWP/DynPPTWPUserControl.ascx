<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DynPPTWPUserControl.ascx.cs" Inherits="DynamicPowerPoint.DynPPTWP.DynPPTWPUserControl" %>
<asp:UpdatePanel ID="UpdatePanel1" runat="server">
<ContentTemplate>
    &nbsp;<asp:Label ID="lblInstructions" runat="server" 
        Text="Use this web part to merge the site's data into the PowerPoint template."></asp:Label>
    <br />
    <asp:Button ID="btnGenerate" runat="server" onclick="btnGenerate_Click" 
        Text="Generate" />
    <br />
    <asp:Label ID="lblErrorMessage" runat="server"></asp:Label>
</ContentTemplate>
</asp:UpdatePanel>
<asp:UpdateProgress ID="UpdateProgress1" runat="server" AssociatedUpdatePanelID="UpdatePanel1">
<ProgressTemplate>
    <asp:Image ID="Image1" runat="server" ImageUrl="_layouts/images/DynamicPowerPoint/status_anim.gif"/>
</ProgressTemplate>
</asp:UpdateProgress>
