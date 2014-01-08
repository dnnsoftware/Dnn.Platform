using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DNNSelenium.Common.BaseClasses;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.Tests.P1
{

    [TestFixture]
    [Category("P1")]
    public abstract class P1AdminEventViewer : CommonTestSteps
    {


        protected abstract string DataFileLocation { get; }



        [TestFixtureSetUp]
        public void LoginToSite()
        {
            XDocument doc = XDocument.Load(DataFileLocation);

            XElement settings = doc.Document.Element("Tests").Element("settings");
            XElement testSettings = doc.Document.Element("Tests").Element("adminEventViewer");

            _driver = StartBrowser(settings.Attribute("browser").Value);
            _baseUrl = settings.Attribute("baseURL").Value;

            string testName = testSettings.Attribute("name").Value;

            Trace.WriteLine(BasePage.RunningTestKeyWord + "'" + testName + "'");
            Trace.WriteLine(BasePage.PreconditionsKeyWord);

            OpenMainPageAndLoginAsHost();

            string[] logsettingitems = { "Cache Error", 
                                            "Cache Item Overflow", 
                                            "Folder created", 
                                            "Folder deleted", 
                                            "Folder updated",
                                            "Host Alert",
                                            "Host setting created",
                                            "Host setting deleted",
                                            "Host setting updated"};
            //Edit Log Settings
            var ev = new AdminEventViewerPage(_driver);
            ev.OpenUsingButtons(_baseUrl);
            ev.ClearEventViewer();
            ev.SetupEventViewer(logsettingitems);
            //_logContent = LogContent();
        }

        //[TestFixtureTearDown]
        public void Cleanup()
        {
            VerifyLogs(_logContent);
        }

        
        [Test]
        public void Test01_VerifyLoginSuperuser()
        {
            OpenMainPageAndLoginAsHost();

            try
            {
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);

                IWebElement elem = ap.WaitForElement(
                    By.XPath("//div[contains(text(), 'Login - Superuser')]"));
                elem.Click();
                Assert.IsTrue(elem.Text.Contains("Login - Superuser"));

                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }

        }

        [Test]
        public void Test02_SuperuserCreateNewUserEvent()
        {
            OpenMainPageAndLoginAsHost();
            var up = new ManageUsersPage(_driver);
            string username = "bn000";
            string displayname = "bnzero";
            string email = "bn000@dnn.com";
            try
            {
                up.OpenUsingControlPanel(_baseUrl);
                up.AddNewUser(username, displayname, email, "dnnhost");

                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                IWebElement elem = ap.WaitForElement(
                    By.XPath("//div[contains(text(), 'UserName " + username + " Email " + email + "')]"));
                elem.Click();
                Assert.IsTrue(elem.Text.Contains("UserName " + username + " Email " + email));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                up.OpenUsingControlPanel(_baseUrl);
                up.DeleteUser(username);
                up.RemoveDeletedUsers();
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test03_SuperuserCreateNewSuperUserEvent()
        {
            OpenMainPageAndLoginAsHost();
            var hsua = new HostSuperUserAccountsPage(_driver);
            string username = "bnsuper";
            string displayname = "bnsuperduper";
            string email = "bnsuper@dnn.com";
            try
            {
                //hsua.OpenUsingControlPanel(_baseUrl);
                hsua.OpenUsingButtons(_baseUrl);
                hsua.AddNewUser(username, displayname, email, "dnnhost");
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                IWebElement elem = ap.WaitForElement(
                    By.XPath("//div[contains(text(), 'UserName " + username + " Email " + email + "')]"));

                elem.Click();
                Assert.IsTrue(elem.Text.Contains("UserName " + username + " Email " + email));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                hsua.OpenUsingButtons(_baseUrl);
                hsua.DeleteUser(username);
                hsua.RemoveDeletedUser(username);
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test04_SuperuserCreateFolderInHostFileManagementEvent()
        {
            OpenMainPageAndLoginAsHost();
            var hfmp = new HostFileManagementPage(_driver);
            string foldertype = "Standard";
            string foldername = "bnFolder";
            string folderxpath = "//span[contains(text(), '" + foldername + "')]";
            try
            {
                //hfmp.OpenUsingControlPanel(_baseUrl);
                hfmp.OpenUsingButtons(_baseUrl);
                if (hfmp.ElementPresent(By.XPath(folderxpath))) { hfmp.DeleteFolderFromTreeView(foldername); }
                hfmp.CreateFolder(foldertype, foldername);
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                string lookfor = "//div[contains(text(), 'logdetail ')]";
                IWebElement elem = ap.WaitForElement(By.XPath(lookfor));

                elem.Click();
                string key = "<displayname>" + foldername + "</displayname><folderpath>" +
                             foldername + "/</folderpath><displaypath>" + foldername + "/</displaypath>";
                string openlookfor = "//p[contains(text(), '" + key + "')]";
                IWebElement openelem = ap.WaitForElement(By.XPath(openlookfor)).WaitTillVisible();
                string txt = openelem.Text;
                Assert.IsTrue(openelem.Text.Contains(key));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test05_SuperuserDeleteFolderInHostFileManagementEvent()
        {
            OpenMainPageAndLoginAsHost();
            var hfmp = new HostFileManagementPage(_driver);
            string foldertype = "Standard";
            string foldername = "bnFolder";
            string folderxpath = "//span[contains(text(), '" + foldername + "')]";
            try
            {
                //hfmp.OpenUsingControlPanel(_baseUrl);
                hfmp.OpenUsingButtons(_baseUrl);
                if (!hfmp.ElementPresent(By.XPath(folderxpath))) { hfmp.CreateFolder(foldertype, foldername); }
                hfmp.DeleteFolderFromTreeView(foldername);
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                string lookfor = "//div[contains(text(), 'logdetail ')]";
                IWebElement elem = ap.WaitForElement(By.XPath(lookfor));

                elem.Click();
                string key = "<displayname>" + foldername + "</displayname><folderpath>" +
                             foldername + "/</folderpath><displaypath>" + foldername + "/</displaypath>";
                string openlookfor = "//p[contains(text(), '" + key + "')]";
                IWebElement openelem = ap.WaitForElement(By.XPath(openlookfor)).WaitTillVisible();
                string txt = openelem.Text;
                Assert.IsTrue(openelem.Text.Contains(key));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test06_SuperuserCreateNewSiteInSiteManagementEvent()
        {
            OpenMainPageAndLoginAsHost();
            var hsmp = new HostSiteManagementPage(_driver);

            string sitename = "bnweb.dnndev.me";
            string title = "BN Site";
            string sitexpath = "//span[contains(text(), '" + title + "')]";
            try
            {
                //hsmp.OpenUsingControlPanel(_baseUrl);
                hsmp.OpenUsingButtons(_baseUrl);
                if (hsmp.ElementPresent(By.XPath(sitexpath))) { hsmp.DeleteSite(sitename); }
                //hsmp.AddNewChildSite(_baseUrl, sitename, title);
                hsmp.AddNewParentSite(sitename, title, "Default Website");
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                string lookfor = "//div[contains(text(), 'Install Portal " + title + "')]";
                IWebElement elem = ap.WaitForElement(By.XPath(lookfor));

                elem.Click();
                Assert.IsTrue(elem.Text.Contains("Install Portal " + title));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        private const string adminusr = "bnAdmin";
        private const string admindisp = "BN Admin";
        private const string adminemail = "bnadmin@dnn.com";
        private const string adminpswrd = "dnnhost";
        
        [Test]
        public void Test07_VerifyLoginAdmin()
        {
            LoginCreateIfNeeded(adminusr, adminpswrd, admindisp, adminemail);

            try
            {
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                IWebElement elem = ap.WaitForElement(
                    By.XPath("//div[contains(text(), 'Login Success')]"));
                elem.Click();
                Assert.IsTrue(ap.ElementPresent(By.XPath("//div[contains(text(), '" + adminusr + "')]")));

                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }

        }

        [Test]
        public void Test08_AdminCreateNewUserEvent()
        {
            LoginCreateIfNeeded(adminusr, adminpswrd, admindisp, adminemail);

            var up = new ManageUsersPage(_driver);
            string username = "createdByAdmin";
            string displayname = "created by admin";
            string email = "createdByAdmin@dnn.com";
            try
            {
                up.OpenUsingControlPanel(_baseUrl);
                up.AddNewUser(username, displayname, email, "dnnhost");

                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                IWebElement elem = ap.WaitForElement(
                    By.XPath("//div[contains(text(), 'UserName " + username + " Email ')]"));
                elem.Click();
                Assert.IsTrue(elem.Text.Contains("UserName " + username + " Email "));

                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                //Delete the user created by bnAdmin
                up.OpenUsingControlPanel(_baseUrl);
                up.DeleteUser(username);
                up.RemoveDeletedUsers();
                System.Threading.Thread.Sleep(2000);
            }
        }
        
        [Test]
        public void Test09_AdminCreateFolderInAdminFileManagementEvent()
        {
            LoginCreateIfNeeded(adminusr, adminpswrd, admindisp, adminemail);

            var afmp = new AdminFileManagementPage(_driver);
            string foldertype = "Standard";
            string foldername = "bnByAdminFolder";
            string folderxpath = "//span[contains(text(), '" + foldername + "')]";
            try
            {
                //hfmp.OpenUsingControlPanel(_baseUrl);
                afmp.OpenUsingButtons(_baseUrl);
                if (afmp.ElementPresent(By.XPath(folderxpath))) { afmp.DeleteFolderFromTreeView(foldername); }
                afmp.CreateFolder(foldertype, foldername);
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                string lookfor = "//div[contains(text(), 'logdetail ')]";
                IWebElement elem = ap.WaitForElement(By.XPath(lookfor));

                elem.Click();
                string key = "<displayname>" + foldername + "</displayname><folderpath>" +
                             foldername + "/</folderpath><displaypath>" + foldername + "/</displaypath>";
                string openlookfor = "//p[contains(text(), '" + key + "')]";
                IWebElement openelem = ap.WaitForElement(By.XPath(openlookfor)).WaitTillVisible();
                string txt = openelem.Text;
                Assert.IsTrue(openelem.Text.Contains(key));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test10_AdminDeleteFolderInAdminFileManagementEvent()
        {
            LoginCreateIfNeeded(adminusr, adminpswrd, admindisp, adminemail);

            var afmp = new AdminFileManagementPage(_driver);
            string foldertype = "Standard";
            string foldername = "bnByAdminFolder";
            string folderxpath = "//span[contains(text(), '" + foldername + "')]";
            try
            {
                //afmp.OpenUsingControlPanel(_baseUrl);
                afmp.OpenUsingButtons(_baseUrl);
                if (!afmp.ElementPresent(By.XPath(folderxpath))) { afmp.CreateFolder(foldertype, foldername); }
                afmp.DeleteFolderFromTreeView(foldername);
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingControlPanel(_baseUrl);
                string lookfor = "//div[contains(text(), 'logdetail ')]";
                IWebElement elem = ap.WaitForElement(By.XPath(lookfor));

                elem.Click();
                string key = "<displayname>" + foldername + "</displayname><folderpath>" +
                             foldername + "/</folderpath><displaypath>" + foldername + "/</displaypath>";
                string openlookfor = "//p[contains(text(), '" + key + "')]";
                IWebElement openelem = ap.WaitForElement(By.XPath(openlookfor)).WaitTillVisible();
                string txt = openelem.Text;
                Assert.IsTrue(openelem.Text.Contains(key));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }

        [Test]
        public void Test11_SuperuserDeleteAdminUserEvent()
        {
            //LoginCreateIfNeeded(adminusr, adminpswrd, admindisp, adminemail);
            //Logout();
            OpenMainPageAndLoginAsHost();
            var mup = new ManageUsersPage(_driver);
            try
            {
                mup.OpenUsingControlPanel(_baseUrl);
                System.Threading.Thread.Sleep(1000);
                mup.DeleteUser(adminusr);
                System.Threading.Thread.Sleep(1000);
                mup.RemoveDeletedUser(adminusr);
                System.Threading.Thread.Sleep(1000);
                var ap = new AdminEventViewerPage(_driver);
                ap.OpenUsingButtons(_baseUrl);
                string xpath = "//div[div[contains(text(), 'User Removed')]]/div[contains(text(), 'Username " + adminusr + "')]";
                IWebElement elem = ap.WaitForElement(By.XPath(xpath));

                elem.Click();
                Assert.IsTrue(elem.Text.Contains("Username " + adminusr));
                elem.Click();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                Assert.Fail(ex.ToString());
            }
            finally
            {
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
}
