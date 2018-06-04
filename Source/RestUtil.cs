using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using Verse;

namespace ModSyncRW
{
    public delegate void IsConnectedCallback(bool isConnected);
    public delegate void RequestCallback(XmlDocument text);

    static class RestUtil
    {
        const int TIMEOUT_TIME = 5000;
        public static void CheckForInternetConnectionAsync(IsConnectedCallback callback)
        {
            Initialize();
            Thread t = new Thread(() =>
            {
                try
                {
                    WebRequest request = WebRequest.Create("http://www.google.com");
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    callback(response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted));
                }
                catch (Exception e)
                {
                    Log.Warning("ModSyncRW: Error connecting to internet: " + e.GetType().Name + " " + e.Message);
                    callback(false);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static void GetAboutXml(string uri, IsConnectedCallback callback)
        {
            Initialize();
            Thread t = new Thread(() =>
            {
                try
                {
                    WebRequest request = WebRequest.Create(uri);
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    callback(response != null && (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted));
                }
                catch (Exception e)
                {
                    Log.Warning("Failed to find About.xml in host: [" + e.GetType().Name + "]\n" + e.GetType() + " " + e.Message);
                    callback(false);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static void GetModSyncXml(string uri, RequestCallback callback)
        {
            Initialize();
            Thread t = new Thread(() =>
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(uri);
                    callback(xml);
                }
                catch (Exception e)
                {
                    Log.Warning("Failed to load xml remotely from [" + uri + "]\n" + e.GetType() + " " + e.Message);
                    callback(null);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private static bool isInitialized = false;
        private static void Initialize()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }
        }
    }
}
