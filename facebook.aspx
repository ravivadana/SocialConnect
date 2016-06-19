<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="facebook.aspx.cs" Inherits="SocialConnectClient.facebook" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
      <a href="https://www.facebook.com/dialog/oauth?client_id=255642851472510&redirect_uri=<%=Request.Url.AbsoluteUri %>/&scope=publish_stream&display=popup">Facebook</a>
                        
        <hr />
        <asp:Panel runat="server" ID="pnlTweet">
            <asp:TextBox ID="txtMessage" runat="server" TextMode="MultiLine" Height="50"></asp:TextBox><br />
           
            <br />
            <asp:Button ID="btnPost" runat="server" Text="Post"  />
        </asp:Panel>
    </form>
</body>
</html>
