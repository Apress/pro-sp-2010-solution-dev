<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BuildCustomerDoc.aspx.cs" Inherits="CustomerDocumentsFeature.Layouts.CustomerDocumentsFeature.BuildCustomerDoc" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
<table border="1" cellpadding="5" cellspacing="0" style="width:100%; font-size: 9pt" >
        <tr>
            <td>Document Type:</td>
            <td><asp:DropDownList ID="lstContentTypes"  runat="server" EnableViewState="true"/></td>
        </tr>
         <tr>
            <td>New File Name:</td>
            <td><asp:TextBox ID="txtFileName"  runat="server" EnableViewState="true"/></td>
        </tr>
        <tr>
            <td></td>
            <td><asp:Button ID="btnCancel"  Text="Return" runat="server" />&nbsp;<asp:Button ID="btnOK" Text="Generate" runat="server" /></td>
        </tr>

    </table>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="true" Text="Your new Document has been saved to the CustomerDocuments library" />
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Build Customer Doc
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Build Customer Doc
</asp:Content>
