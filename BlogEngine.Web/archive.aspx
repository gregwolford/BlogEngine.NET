<%@ Page Language="C#" AutoEventWireup="true" CodeFile="archive.aspx.cs" Inherits="archive" EnableViewState="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="cphBody" Runat="Server">
  <div id="archive">
    <h1><%=Resources.labels.archive %></h1>
    <ul runat="server" id="ulMenu" />
    <asp:placeholder runat="server" id="phArchive" />
    <br />

    <h2>Total</h2>
    <span><asp:literal runat="server" id="ltPosts" /></span><br />
    <span><asp:literal runat="server" id="ltComments" /></span><br />
    <span><asp:literal runat="server" id="ltRaters" /></span>
  </div>
</asp:Content>