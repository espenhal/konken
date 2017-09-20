﻿using System;
using OpenQA.Selenium.PhantomJS;

namespace scraper
{
    public class Scraper
    {
        public static string GetHtml(string url, string htmlElementToFind = null, bool throwErrorOnElementNotFound = true)
        {
            string html = "";

            using (var driver = new PhantomJSDriver())
            {
                try
                {
                    driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 7));

                    driver.Navigate().GoToUrl(url);

                    var element = htmlElementToFind == null
                        ? driver.FindElementByTagName("html")
                        : driver.FindElementById(htmlElementToFind);

                    if (element != null)
                    {
                        html = (string)driver.ExecuteScript("return arguments[0].outerHTML;", element);
                    }
                }
                catch (Exception e)
                {
                    //just for debugging
                }
                finally
                {
                    driver.Quit();
                }
            }

            if (throwErrorOnElementNotFound && string.IsNullOrEmpty(html))
                throw new Exception("PhantomJSDriver fails!");

            return html;
        }

        public static string GetHtmlByXPath(string url, string xpath, bool throwErrorOnElementNotFound = true)
        {
            string html = "";

            using (var driver = new PhantomJSDriver())
            {
                try
                {
                    driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 7));

                    driver.Navigate().GoToUrl(url);

                    var element = driver.FindElementByXPath(xpath);

                    if (element != null)
                    {
                        html = (string)driver.ExecuteScript("return arguments[0].outerHTML;", element);
                    }
                }
                catch (Exception e)
                {
                    //just for debugging
                }
                finally
                {
                    driver.Quit();
                }
            }

            if (throwErrorOnElementNotFound && string.IsNullOrEmpty(html))
                throw new Exception("PhantomJSDriver fails!");

            return html;
        }
    }
}