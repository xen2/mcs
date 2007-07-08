<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
<%--
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        // If there is a ScriptManager on the page, use it.
        // If not, throw an exception.
        ScriptManager Smgr = ScriptManager.GetCurrent(Page);
        if (Smgr == null) throw new Exception("ScriptManager not found.");
    
        ScriptReference SRef = new ScriptReference();
        
        // If you know that Smgr.ScriptPath is correct...
        SRef.Name = "Script.js";
        
        // Or, to specify an app-relative path...
        SRef.Path = "~/Scripts/Script.js";
        
        // To set ScriptMode for all scripts on the page...
        Smgr.ScriptMode = ScriptMode.Release;
        
        //Or, to set the ScriptMode just for the one script...
        SRef.ScriptMode = ScriptMode.Debug;
        
        //If they conflict, the setting on the ScriptReference wins.
        
        Smgr.Scripts.Add(SRef);
        
        SRef.Name = "Script.js";
        SRef.Assembly = "ScriptAssembly";

    }
</script>
--%>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dynamic Script References</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:scriptmanager ID="ScriptManager1" runat="server" />
        <div>
        </div>
    </form>
</body>
</html>
