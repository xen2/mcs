﻿<%@ Control Language="C#" CodeFile="Byte.ascx.cs" Inherits="Byte_Field" %>

<span class="field"><%= Column.Name %></span>: <span class="byteTemplate"><asp:Literal runat="server" ID="Literal1" Text="<%# FieldValueString %>" /></span>