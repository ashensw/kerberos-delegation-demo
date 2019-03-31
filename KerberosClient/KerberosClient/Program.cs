using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.Script.Serialization;
using System.IO;
using System.Web;
using System.Net;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens;
using System.IdentityModel.Selectors;
using System.Net.Http;
using System.DirectoryServices.ActiveDirectory;
using System.Threading;

namespace KerberosClient
{
    class Program
    {
        static string tokenEndpoint = "http://apim.com:8280/token";
        static string apiURL = "http://apim.com:8280/demo/1.0.0/api/customer";
        static string apimSPN = "http/apim.com@WSO2.TEST";
        static string username = "TcY6H40VdgwdGyV1haxwtS9zxT8a";
        static string password = "2Fs1lm4hOZsPBe5ntyE50EfijkAa";

        static void Main(string[] args)
        {
           
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Kerberos Client...");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Configurations...");
            Console.WriteLine();
            Thread.Sleep(100);
            Console.WriteLine("Token Endpoint: " + tokenEndpoint);
            Thread.Sleep(100);
            Console.WriteLine("APIM SPN      : " + apimSPN);
            Thread.Sleep(100);
            Console.WriteLine("API URL       : " +apiURL);
            Console.WriteLine();
            Thread.Sleep(500);

            Console.Write("Start? (Y/N) ");
            
            string input = Console.ReadLine();
            if (string.Equals("N", input, StringComparison.OrdinalIgnoreCase))
                
            {
                Environment.Exit(0);
            }

            Console.WriteLine();
            string kerberosTicket = getKerberosTickerFromKDC();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Kerberos Ticket: ");
            Console.ResetColor();
            Console.WriteLine(kerberosTicket);
            
            Console.WriteLine();
            string accessToken = getOauth2Token(kerberosTicket);
            //string accessToken = getOauth2TokenPWGrant();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Access Token: ");
            Console.ResetColor();
            Console.WriteLine(accessToken);
            Console.WriteLine();

            getAPIResponse(accessToken);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Press Any Key To Exit...");
            Console.ReadKey();

        }

 

        static void getAPIResponse(string accessToken)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Access the API with Kerberos protected backend...");
            Console.ResetColor();
            Console.WriteLine();
            try
            {
                Uri uri = new Uri(apiURL);
                AuthenticationManager.CustomTargetNameDictionary.Add(apiURL, apimSPN);
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);
                req.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                req.Credentials = CredentialCache.DefaultCredentials;
                req.Headers.Add("X-APIM-Auth: Bearer " + accessToken);
                req.ImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    StreamReader sr = new StreamReader(res.GetResponseStream());

                    Console.WriteLine(res.StatusCode);
                    Console.WriteLine();
                    Console.WriteLine(sr.ReadToEnd());
                    Console.WriteLine();

                }
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }
        }


        static string getKerberosTickerFromKDC()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Obtaining a Kerberos ticket...");
            Console.ResetColor();
            AppDomain.CurrentDomain.SetPrincipalPolicy(System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
            var domain = Domain.GetCurrentDomain().ToString();

            using (var domainContext = new PrincipalContext(ContextType.Domain, domain))
            {
                
                KerberosSecurityTokenProvider tokenProvider = new KerberosSecurityTokenProvider(apimSPN, System.Security.Principal.TokenImpersonationLevel.Impersonation, CredentialCache.DefaultNetworkCredentials);
                KerberosRequestorSecurityToken securityToken = tokenProvider.GetToken(TimeSpan.FromMinutes(5)) as KerberosRequestorSecurityToken;
                string serviceToken = Convert.ToBase64String(securityToken.GetRequest());

                return serviceToken;
            }
        }

        static string getBasicAuthHeader()
        {
            string authInfo = username + ":" + password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            return "Basic " + authInfo;
        }


        static string getOauth2Token(string ticket)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Obtaining an access token token via kerberos grant...");
            Console.ResetColor();
            
            string encodedToken = Uri.EscapeDataString(ticket);
            string postData = "grant_type=kerberos&kerberos_realm=wso2.test&kerberos_token=" + encodedToken;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            WebRequest request = WebRequest.Create(tokenEndpoint);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Authorization"] = getBasicAuthHeader();
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseFromServer = reader.ReadToEnd();
            var jss = new JavaScriptSerializer();
            Dictionary<string, string> data = jss.Deserialize<Dictionary<string, string>>(responseFromServer);

            reader.Close();
            dataStream.Close();
            response.Close();

            return data["access_token"].ToString();
        }

        static string getOauth2TokenPWGrant()
        {

            string postData = "grant_type=password&username=admin&password=admin";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            WebRequest request = WebRequest.Create(tokenEndpoint);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers["Authorization"] = getBasicAuthHeader();
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            WebResponse response = request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string responseFromServer = reader.ReadToEnd();
            var jss = new JavaScriptSerializer();
            Dictionary<string, string> data = jss.Deserialize<Dictionary<string, string>>(responseFromServer);

            reader.Close();
            dataStream.Close();
            response.Close();

            return data["access_token"].ToString();
        }
    }
}
