// 
// System.Web.Hosting
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   (class signature from Bob Smith <bob@thestuff.net> (C) )
//
using System;
using System.IO;
using System.Text;

namespace System.Web.Hosting {
   [MonoTODO("Implement security demands on the path usage functions (and review)")]
   public class SimpleWorkerRequest : HttpWorkerRequest {
      private string _Page;
      private string _Query;
      private string _PathInfo;
      private string _AppVirtualPath;
      private string _AppPhysicalPath;
      private string _AppInstallPath;
      private TextWriter   _Output;
      private bool _HasInstallInfo;

      private SimpleWorkerRequest() {
      }

      public SimpleWorkerRequest(string Page, string Query, TextWriter Output) {
         _Page = Page;
         _Query = Query;

         _AppVirtualPath = System.Threading.Thread.GetDomain().GetData(".ASP.Net.App.VirtualPath").ToString();
         _AppInstallPath = AppDomain.CurrentDomain.GetData(".ASP.Net.App.InstallPath").ToString();
         _AppPhysicalPath = CheckAndAddSlash(AppDomain.CurrentDomain.GetData(".ASP.Net.App.Path").ToString());

         _Output = Output;

         if (_AppPhysicalPath == null) {
            // needs to be in a initialized app domain
            throw new HttpException("Invalid app domain");
         }

         _HasInstallInfo = true;
      }

      public SimpleWorkerRequest(string AppVirtualPath, string AppPhysicalPath, string Page, string Query, TextWriter Output) {
         if (AppDomain.CurrentDomain.GetData(".ASP.Net.App.Path") == null) {
            // needs to be in a initialized app domain
            throw new HttpException("Invalid app domain");
         }

         _Page = Page;
         _Query = Query;
         _AppVirtualPath = AppVirtualPath;
         _AppPhysicalPath = CheckAndAddSlash(AppPhysicalPath);
         _Output = Output;

         _HasInstallInfo = true;
      }

      [MonoTODO("Implement security")]
      public override string MachineInstallDirectory {
         get {
            if (_HasInstallInfo) {
               return _AppInstallPath;
            }

            return null;
         }
      }

      [MonoTODO("Get config path from Web.Config class")]
      public override string MachineConfigPath {
         get {
            return MachineConfigPath;
         }
      }

      public override void EndOfRequest() {
      }

      public override void FlushResponse(bool finalFlush) {
         _Output.Flush();
      }

      public override string GetAppPath() {
         return _AppVirtualPath;
      }

      [MonoTODO("Implement security")]
      public override string GetAppPathTranslated() {
         return _AppPhysicalPath;
      }

      public override string GetFilePath() {
         return CreatePath(false);
      }

      [MonoTODO("Implement security")]
      public override string GetFilePathTranslated() {
         return _AppPhysicalPath + _Page.Replace('/', '\\');
      }

      public override string GetHttpVerbName() {
         return "GET";
      }

      public override string GetHttpVersion() {
         return "HTTP/1.0";
      }

      public override string GetLocalAddress() {
         return "127.0.0.1";
      }

      public override int GetLocalPort() {
         return 80;
      }

      [MonoTODO("Implement security")]
      public override string GetPathInfo() {
         if (null != _PathInfo) {
            return _PathInfo;
         }

         return System.String.Empty;
      }

      public override string GetQueryString() {
         return _Query;
      }

      public override string GetRawUrl() {
         if (null != _Query && _Query.Length > 0) {
            return CreatePath(true) + "?" + _Query;
         }

         return CreatePath(true);
      }

      public override string GetRemoteAddress() {
         return "127.0.0.1";
      }

      public override int GetRemotePort() {
         return 0;
      }

      public override string GetServerVariable(string name) {
         return System.String.Empty;
      }

      public override string GetUriPath() {
         return CreatePath(true);
      }

      public override IntPtr GetUserToken() {
         return System.IntPtr.Zero;
      }

      public override string MapPath(string path) {
         string sPath = _AppPhysicalPath.Substring(0, _AppPhysicalPath.Length - 1);
         if (path != null && path.Length > 0 && path != "/") {
            return sPath;
         }

         if (path.StartsWith(_AppVirtualPath)) {
            return sPath + path.Substring(_AppVirtualPath.Length).Replace('/', '\\');
         }

         return null;
      }

      public override void SendKnownResponseHeader(int index, string value) {
      }

      public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
      }

      public override void SendResponseFromFile(string filename, long offset, long length) {
      }

      public override void SendResponseFromMemory(byte[] data, int length) {
         _Output.Write(Encoding.Default.GetChars(data, 0, length));
      }

      public override void SendStatus(int statusCode, string statusDescription) {
      }

      public override void SendUnknownResponseHeader(string name, string value) {
      }

      // Create's a path string
      private string CheckAndAddSlash(string sPath) {
         if (null == sPath) {
            return null;
         }

         if (!sPath.EndsWith("\\")) {
            return sPath + "\\";
         }

         return sPath;
      }

      // Create's a path string
      private string CreatePath(bool bIncludePathInfo) {
         string sPath;

         if ("/" == _AppVirtualPath) {
            sPath = _AppVirtualPath + "/" + _Page;
         } else {
            sPath = "/" + _Page;
         }

         if (bIncludePathInfo && null != _PathInfo) {
            return sPath + _PathInfo;
         }

         return sPath;
      }

      // Parses out the string after / know as the "path info"
      private void ParsePathInfo() {
         int iPos = _Page.IndexOf("/");
         if (iPos >= 0) {
            _PathInfo = _Page.Substring(iPos);
            _Page = _Page.Substring(0, iPos);
         }
      }
   }
}
