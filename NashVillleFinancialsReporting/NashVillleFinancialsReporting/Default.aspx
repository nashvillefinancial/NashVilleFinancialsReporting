<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:TextBox ID="txtQuery" runat="server"></asp:TextBox>
            <asp:Button Text="Load Data" runat="server" OnClick="Unnamed_Click" />
            <asp:Label ID="lblTime" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>
