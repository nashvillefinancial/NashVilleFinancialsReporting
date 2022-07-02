<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="LeadGenDashboard.aspx.cs" Inherits="LeadGenDashboard" %>

<%@ Register Assembly="DevExpress.Dashboard.v21.2.Web.WebForms, Version=21.2.5.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.DashboardWeb" TagPrefix="dx" %>

<!DOCTYPE html>


<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dashboard</title>
    <style type="text/css">
        html, body, form {
            height: 100%;
            margin: 0;
            padding: 0;
            overflow: auto;
        }
    </style>
    <script type="text/javascript">
        function onBeforeRender(s, e) {

            var dashboardControl = s.GetDashboardControl();
            dashboardControl.registerExtension(new DevExpress.Dashboard.DashboardPanelExtension(dashboardControl));
            var viewerApiExtension = dashboardControl.findExtension('viewer-api');

            //console.log(viewerApiExtension);
            if (viewerApiExtension) {
                viewerApiExtension._onitemWidgetCreated = onItemWidgetOptionsPrepared;
                viewerApiExtension.onItemWidgetUpdating = onItemWidgetOptionsPrepared;
                viewerApiExtension.onItemWidgetUpdated = onItemWidgetOptionsPrepared;
                viewerApiExtension.on('itemWidgetOptionsPrepared', onItemWidgetOptionsPrepared);
            }

        }


        function onItemWidgetOptionsPrepared(args) {
            if (args != null && args.itemName.includes("pieDashboardItem")) {
                console.log('inside');
                var total = 0;
                for (let val of args.options.dataSource) {
                    total += val.y;
                }

                args.options.title.text = 'TOTAL ' + new Intl.NumberFormat().format(total);
                return;
            }
            //console.log(args);

            if (args !== null && args.itemName.includes("chartDashboardItem")) {
                args.options.tooltip.customizeTooltip = function (arg) {
                    var series = args.options.series.find(function (s) { return s.name === arg.seriesName; });

                    var argumentPoint = arg.point.tag.axisPoint;
                    var seriesPoint = series.tag.axisPoint;
                    var itemData = webViewer.GetItemData(args.itemName);
                    var dataSlice = itemData.GetSlice(argumentPoint);
                    dataSlice = dataSlice.GetSlice(seriesPoint);

                    var text = "";
                    $.each(itemData.GetMeasures(), function (_, measure) {
                        var measureValue = dataSlice.GetMeasureValue(measure.Id);
                        text = text + "" + measure.Name + ": " + measureValue.GetValue() + "<br />";
                    });

                    var result = customizeTooltip(arg);
                    // result.html = result.html + text;
                    result.html = text;
                    return result;
                };

            }
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <dx:ASPxDashboard ID="ASPxDashboard1" ClientInstanceName="webViewer"  OnConfigureItemDataCalculation="ASPxDashboard1_ConfigureItemDataCalculation1" ResizeByTimer="true" WorkingMode="ViewerOnly" LoadDefaultDashboard="true" AllowCreateNewDashboard="false" AllowOpenDashboard="true" OnDataLoading="ASPxDashboard1_DataLoading" runat="server">
                <ClientSideEvents BeforeRender="onBeforeRender" CallbackError="function(s, e) {
                    console.log(e);
                    alert(e.Message);
    }" />
            </dx:ASPxDashboard>
        </div>
    </form>
</body>
</html>
