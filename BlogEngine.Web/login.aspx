<%@ Page Language="C#" AutoEventWireup="true" CodeFile="login.aspx.cs" Inherits="login" Title="Sign in" %>

 
<asp:Content ID="Content1" ContentPlaceHolderID="cphBody" Runat="Server">
<asp:Login ID="Login1" runat="server" class="loginbox" />

<div style="text-align:center">
  <asp:changepassword runat="server" id="changepassword1" visible="false" />
</div>
</asp:Content>