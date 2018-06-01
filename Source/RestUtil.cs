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
                    Log.Warning("Failed to find About.xml in host: " + e.GetType().Name + " " + e.Message);
                    callback(false);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static void GetModSyncXml(string uri, RequestCallback callback)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
                    XmlDocument xml = new XmlDocument();
                    xml.Load(uri);
                    callback(xml);
                }
                catch (Exception e)
                {
                    Log.Warning("Failed to load xml remotely from " + uri + ". " + e.GetType() + " " + e.Message);
                    callback(null);
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        private static bool MyRemoteCertificateValidationCallback(
            object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }
    }
}
