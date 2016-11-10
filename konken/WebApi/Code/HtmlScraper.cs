﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using OpenQA.Selenium.PhantomJS;

namespace WebApi.Code
{
    public class HtmlScraper
    {
        public static string GetHtml(string url, string htmlElementToFind = null)
        {
            string html = null;

            using (var driver = new PhantomJSDriver())
            {
                try
                {
                    driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));

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
                    throw;
                }
                finally
                {
                    driver.Quit();
                }
            }

            if (string.IsNullOrEmpty(html))
                 throw new Exception("PhantomJSDriver fails!");

            return html;
        }

        public static string GetHtmlByXPath(string url, string xpath)
        {
            string html = null;

            using (var driver = new PhantomJSDriver())
            {
                try
                {
                    driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));

                    driver.Navigate().GoToUrl(url);

                    var element = driver.FindElementByXPath(xpath);

                    if (element != null)
                    {
                        html = (string)driver.ExecuteScript("return arguments[0].outerHTML;", element);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    driver.Quit();
                }
            }

            if (string.IsNullOrEmpty(html))
                throw new Exception("PhantomJSDriver fails!");

            return html;
        }
    }
}