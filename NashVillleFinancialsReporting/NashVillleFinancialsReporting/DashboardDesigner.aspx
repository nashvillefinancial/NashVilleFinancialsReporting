﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DashboardDesigner.aspx.cs" Inherits="DashboardDesigner" %>

<%@ Register Assembly="DevExpress.Dashboard.v19.1.Web.WebForms" Namespace="DevExpress.DashboardWeb" TagPrefix="dx" %>

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
            var viewerApiExtension = dashboardControl.findExtension('viewer-api');

            //console.log(viewerApiExtension);
            if (viewerApiExtension) {
                viewerApiExtension._options.onItemWidgetCreated = onItemWidgetOptionsPrepared;
                viewerApiExtension._options.onItemWidgetUpdating = onItemWidgetOptionsPrepared;
                viewerApiExtension._options.onItemWidgetUpdated = onItemWidgetOptionsPrepared;
                //viewerApiExtension.on('itemWidgetOptionsPrepared', onItemWidgetOptionsPrepared);
            }

        }

      
        function onItemWidgetOptionsPrepared(args) {

            var widget = args.getWidget()[0];
            if(widget === null || widget === undefined){
                widget = args.getWidget();
            }
            
            
           
            if (widget !== null && widget !== undefined && widget.NAME === "dxPieChart") {
                var total = 0;
                for (let val of widget._options.dataSource) {
                    total += val.y;
                }

                var title = widget._options.title;
                //title.text = "Ammad";
               // console.log('-----');

                //console.log(title);
                widget._title._options.text = 'TOTAL ' + new Intl.NumberFormat().format(total);
                if (!title.hasTotal) {
                    title.text += ' ' + total;
                    title.hasTotal = true;
                    widget._options.title = title;

                }
            }

            console.log(args);
            if (widget !== null && widget !== undefined && widget.NAME === "dxChart") {
                console.log('inside chart');
                var chart = widget;
                var chartOptions = chart.option();
                var customizeTooltip = chart.option('tooltip.customizeTooltip');
                chartOptions.tooltip.customizeTooltip = function (arg) {
                    console.log('inside customizeTooltip');
                    var series = chartOptions.series.find(function (s) { return s.name === arg.seriesName; });

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
                    result.html =text;
                    return result;
                };
                chart.option(chartOptions);
               
            }
            

           

        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div>
           
            <dx:ASPxDashboard ID="ASPxDashboard1" ClientInstanceName="webViewer" OnConfigureItemDataCalculation="ASPxDashboard1_ConfigureItemDataCalculation1" OnDataLoading="ASPxDashboard1_DataLoading"  runat="server">
                <ClientSideEvents BeforeRender="onBeforeRender" />
            </dx:ASPxDashboard>
        </div>
    </form>
</body>
</html>
