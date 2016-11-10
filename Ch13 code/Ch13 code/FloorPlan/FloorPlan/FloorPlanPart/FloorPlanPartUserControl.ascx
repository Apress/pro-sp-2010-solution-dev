<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FloorPlanPartUserControl.ascx.cs" Inherits="FloorPlan.FloorPlanPart.FloorPlanPartUserControl" %>
<SharePoint:ScriptLink runat="server" Name="sp.js" LoadAfterUI="true" Localizable="false" />
<asp:ScriptManagerProxy runat="server">
    <Services>
        <asp:ServiceReference Path="/_vti_bin/userprofileservice.asmx" />
    </Services>
</asp:ScriptManagerProxy>
<style type="text/css">
    .heading {padding:5px;background:#000;color:#fff;font-weight:bold;text-align:center;margin-bottom:10px;}
    .hidden {position:absolute;visibility:hidden;}
    .visible {position:relative;visibility:visible;}
    #userPhoto {float:left;padding-right:5px;}
</style>
<script language="ecmascript" type="text/ecmascript">
    var vwaControl; //Visio Web Access control
    var vwaPage; // current page in the diagram
    var vwaShapes; // shape collection

    // ajax call ensures webpart has rendered
    Sys.Application.add_load(onApplicationLoad);

    function onApplicationLoad() {
        // get the VWA control on the page
        vwaControl = new Vwa.VwaControl('WebPartWPQ2');

        // add handler which will fire once the diagram has loaded
        vwaControl.addHandler('diagramcomplete', onDiagramComplete);

        // wire up handlers for mouse events
        vwaControl.addHandler('shapeselectionchanged', onShapeSelectionChanged);
        vwaControl.addHandler('shapemouseenter', onShapeMouseEnter);
        vwaControl.addHandler('shapemouseleave', onShapeMouseLeave);
    }

    function onDiagramComplete() {
        vwaPage = vwaControl.getActivePage(); // page
        vwaShapes = vwaPage.getShapes(); // shape collection
    }


    function onShapeMouseEnter(source, args) {
        var shape = vwaShapes.getItemById(args); // shape associated with the event

        // if this shape is a printer shape
        if (shape.getName().substring(0, 7) == 'Printer') {
            var shapeData = shape.getShapeData(); // load data related to the shape

            // define the XAML overlay
            var overLay = '<Canvas>' +
                '<Rectangle Fill="Black" Height="54" Width="200" />' +
                '<Rectangle Fill="White" Height="50" Width="196" Margin="2,2" />' +
                '<StackPanel Margin="5,5">' +
                '<TextBlock Text ="Network Name: ' + shapeData[0].value + '" />' +
                '<TextBlock Text ="IP Address: ' + shapeData[1].value + '" />' +
                '</StackPanel>' +
                '</Canvas>';

            shape.addOverlay('printerOverlay', overLay, 2, 1, 200, 54); // add overlay to drawing
        }
    }

    function onShapeMouseLeave(source, args) {
        var shape = vwaShapes.getItemById(args); // shape associated with the event

        // if this shape is a printer shape
        if (shape.getName().substring(0, 7) == 'Printer') {
            shape.removeOverlay('printerOverlay');  // remove the overlay from the drawing
        }
    }

    
    function onShapeSelectionChanged(source, args) {
        var shape = vwaShapes.getItemById(args); // shape associated with the event

        // hide all divs
        document.getElementById('nothingSelected').className = 'hidden';
        document.getElementById('officeInfo').className = 'hidden';
        document.getElementById('roomInfo').className = 'hidden';
        document.getElementById('userInfo').className = 'hidden';
        document.getElementById('notOccupied').className = 'hidden';

        // if this shape is a space shape
        if (shape.getName().substring(0, 5) == 'Space') {
            var shapeData = shape.getShapeData(); // load data related to the shape

            // if this is an office show occupant details
            if (shapeData[0].value == 'Office') {
                document.getElementById('officeNum').innerText = shapeData[5].value;
                document.getElementById('officeInfo').className = 'visible';
                if (shapeData[6].value != "") {
                    Microsoft.Office.Server.UserProfiles.UserProfileService.GetUserProfileByName(shapeData[6].value, onProfileRequestSuccess, onProfileRequestFailed);
                }
                else {
                    document.getElementById('notOccupied').className = 'visible';
                }
            }
            // else if this is a conference room show reserve dialog
            else if (shapeData[0].value == 'Conference') {
                document.getElementById('reservedBy').value = shapeData[8].value;
                document.getElementById('roomTitle').innerText = shapeData[6].value;
                document.getElementById('roomId').value = shapeData[5].value;
                document.getElementById('roomInfo').className = 'visible';
            }
        }
        // else not a space shape
        else document.getElementById('nothingSelected').className = 'visible';
    }

    function onProfileRequestSuccess(results) {
        document.getElementById('userPhoto').src = results[15].Values[0] == null ? "" : results[15].Values[0].Value;
        document.getElementById('userName').innerText = results[6].Values[0] == null ? "" : results[6].Values[0].Value;
        document.getElementById('userPhone').innerText = results[8].Values[0] == null ? "" : results[8].Values[0].Value;
        document.getElementById('userInfo').className = 'visible';
    }

    function onProfileRequestFailed(results) {
        alert('Your request for profile details failed');
        document.getElementById('nothingSelected').className = 'visible';
    }

    function updateReservation() {
        document.getElementById("resvSubmit").disabled = true; // disable save button

        // define commands, these will not execute until we call executeQueryAsync
        // get current SharePoint context
        var clientContext = SP.ClientContext.get_current(); 
        // get the current SharePoint site
        var web = clientContext.get_web(); 
        // get reference to ConferenceRooms list
        var list = web.get_lists().getByTitle('ConferenceRooms');
        // get item
        var item = list.getItemById(parseInt(document.getElementById('roomId').value)); 
        // set new value
        item.set_item('ReservedBy', document.getElementById('reservedBy').value); 
        // update item
        item.update();

        // execute commands and set callbacks
        clientContext.executeQueryAsync(Function.createDelegate(this, this.onReserveSuccess),Function.createDelegate(this, this.onReserveFailed));
    }

    function onReserveSuccess(sender, args) {
        vwaControl.refreshDiagram(); // refresh the diagram
        // reenable the button for the user
        document.getElementById("resvSubmit").disabled = false;
    }
    
    function onReserveFailed(sender, args) {
        alert('Reservation update failed');
        // reenable the button for the user
        document.getElementById("resvSubmit").disabled = false;
    }
</script>

<div style="position:relative;">
    <div id="nothingSelected" class="visible" >
        <div class="heading">Select a Room</div>
        <div>
            Select an office to view occupant details or a 
            conference room to view reservation details.
        </div> 
    </div>

    <div id="officeInfo" class="hidden">
        <div class="heading">Office #<span id="officeNum"></span></div>
        <div id="notOccupied" class="hidden">Not Occupied</div>
        <div id="userInfo" class="hidden">
            <div><img id="userPhoto" src="" alt="user photo" /></div>
            <div>Name: <span id="userName"></span></div>
            <div>Phone: <span id="userPhone"></span></div>
        </div>
    </div>

    <div id="roomInfo" class="hidden">
        <input type="hidden" id="roomId" />
        <div class="heading"><span id="roomTitle"></span> Reservation</div>
        <div>Reserved By: <input type="text" id="reservedBy" value="" /></div>
        <div style="padding:10px;text-align:right;">
            <input type="button" id="resvSubmit" value="Save" onclick="updateReservation();" />
        </div>
    </div>
</div>