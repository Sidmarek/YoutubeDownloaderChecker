using CefSharp;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace YoutubeDownloaderChecker
{
    class FacebookVideoDownloaderManagerBase
    {
        const string youTubeQueryBase = "https://www.youtube.com/results?search_query=";

        public static Task LoadPageAsync(IWebBrowser browser)
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                //Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            return tcs.Task;
        }

        public static async Task<string> GetFacebookHtmlAsync(ChromeDriver chromeDriver, string url)
        {
            chromeDriver.Url = url;
            return chromeDriver.PageSource;
        }

        public static string GetResultHtml(string query)
        {
            string resultString;
            WebRequest request = WebRequest.Create(youTubeQueryBase + query);

            WebResponse response = request.GetResponse();
            WebRequest requestOfResponse = WebRequest.Create(response.ResponseUri);
            WebResponse responsefResponse = requestOfResponse.GetResponse();
            response.Close();
            Stream dataStream = responsefResponse.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);

            resultString = reader.ReadToEnd();
            reader.Close();
            return resultString;
        }

        public static void LoginIntoFacebook(ChromeDriver executeDriver)
        {
            executeDriver.Url = "https://facebook.com";
            var buttons = executeDriver.FindElementsByTagName("button");

            foreach (var button in buttons)
            {
                var title = button.GetAttribute("title");
                if (!string.IsNullOrEmpty(title))
                {
                    if (title.Contains("cookies"))
                    {
                        button.Click();
                    }
                }
            }

            //var cookiesSecondAccept = executeDriver.FindElementsByTagName("button");
            //cookiesSecondAccept[4].Click();
            var emailElement = executeDriver.FindElementById("email");
            emailElement.Click();
            emailElement.SendKeys($"chodec.marek@gmail.com");
            var passwordElement = executeDriver.FindElementById("pass");
            passwordElement.Click();
            passwordElement.SendKeys("Vahrai1EVee");
            var loginButtons = executeDriver.FindElementsByTagName("button");
            loginButtons[0].Click();
        }
    }
}