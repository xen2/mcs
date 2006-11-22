<%--
// Mainsoft.Web.Administration - Site administration utility
// Authors:
//  Klain Yoni <yonik@mainsoft.com>
//
// Mainsoft.Web.Administration - Site administration utility
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. --%>
<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Util.Master" CodeBehind="Default.aspx.cs" Inherits="Mainsoft.Web.Administration.Default" %>
<asp:Content runat="server" ContentPlaceHolderID="Main">
    <table>
        <tr>
            <td>
                You can use the Web Site Administration Tool to manage all the security settings for your application. You can set up users and passwords (authentication), create roles (groups of users), and create permissions (rules for controlling access to parts of your application).
                <br /><br /><br />
            </td>
        </tr>
        <tr>
            <td>
                <table class="innertable" width="70%" >
                    <tr>
                        <td class="controlheader">
                            Users
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Created users : <%= User_count %>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/CreateUser.aspx">Create user</asp:HyperLink>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink2" runat="server" NavigateUrl="~/ManageUser.aspx">Manage users</asp:HyperLink>
                        </td>
                    </tr>
                </table> 
                <br /><br /><br />
            </td>
        </tr>
        <tr>
            <td>
                <table class="innertable" width="70%">
                    <tr>
                        <td class="controlheader">
                            Roles 
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Existing roles : <%= Roles_count %>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HyperLink ID="HyperLink3" runat="server" NavigateUrl="~/CreateRole.aspx">Create or manage roles</asp:HyperLink>
                        </td>
                    </tr>
                </table>
                <br /><br /><br />
            </td>
        </tr>
    </table>
</asp:Content>