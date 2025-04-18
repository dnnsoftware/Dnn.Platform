<%@ Page Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.Services.Install.UpgradeWizard" CodeBehind="UpgradeWizard.aspx.cs" %>

<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" Namespace="DotNetNuke.Web.UI.WebControls.Internal" Assembly="DotNetNuke.Web" %>
<%@ Import Namespace="DotNetNuke.Services.Localization" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" lang="en-US">
<head runat="server">
  <title></title>
  <asp:PlaceHolder runat="server" ID="ClientDependencyHeadCss"></asp:PlaceHolder>
  <asp:PlaceHolder runat="server" ID="ClientDependencyHeadJs"></asp:PlaceHolder>
  <link rel="stylesheet" type="text/css" class="needVer" href="../Resources/Shared/stylesheets/dnndefault/10.0.0/default.css?refresh" />
  <link rel="stylesheet" type="text/css" class="needVer" href="Install.css?refresh" />
  <script type="text/javascript" src="../Resources/Shared/scripts/jquery/jquery.min.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
  <script type="text/javascript" src="../Resources/Shared/scripts/jquery/jquery-migrate.min.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
  <script type="text/javascript" src="../Resources/Shared/Scripts/jquery/jquery-ui.min.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
  <script type="text/javascript" src="../Resources/Shared/Scripts/jquery/jquery.hoverIntent.min.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
  <asp:PlaceHolder ID="SCRIPTS" runat="server"></asp:PlaceHolder>
</head>
<body>
  <asp:PlaceHolder runat="server" ID="ClientResourceIncludes" />
  <form id="form1" runat="server">
    <asp:ScriptManager ID="scManager" runat="server" EnablePageMethods="true"></asp:ScriptManager>
    <asp:PlaceHolder ID="BodySCRIPTS" runat="server">
      <script type="text/javascript" src="../js/dnn.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
      <script type="text/javascript" src="../Resources/Shared/Scripts/dnn.jquery.js?ver=<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>"></script>
    </asp:PlaceHolder>

    <div id="languageFlags">
      <asp:LinkButton ID="lang_en_US" class="flag" runat="server" value="en-US" OnClientClick="upgradeWizard.changePageLocale('lang_en_US','en-US');"><img src="../images/flags/en-US.gif" alt="en-US" class="flagimage"/></asp:LinkButton>
      <asp:LinkButton ID="lang_de_DE" class="flag" runat="server" value="de-DE" OnClientClick="upgradeWizard.changePageLocale('lang_de_DE','de-DE');"><img src="../images/flags/de-DE.gif" alt="de-DE" class="flagimage"/></asp:LinkButton>
      <asp:LinkButton ID="lang_es_ES" class="flag" runat="server" value="es-ES" OnClientClick="upgradeWizard.changePageLocale('lang_es_ES','es-ES');"><img src="../images/flags/es-ES.gif" alt="es-ES" class="flagimage"/></asp:LinkButton>
      <asp:LinkButton ID="lang_fr_FR" class="flag" runat="server" value="fr-FR" OnClientClick="upgradeWizard.changePageLocale('lang_fr_FR','fr-FR');"><img src="../images/flags/fr-FR.gif" alt="fr-FR" class="flagimage"/></asp:LinkButton>
      <asp:LinkButton ID="lang_it_IT" class="flag" runat="server" value="it-IT" OnClientClick="upgradeWizard.changePageLocale('lang_it_IT','it-IT');"><img src="../images/flags/it-IT.gif" alt="it-IT" class="flagimage"/></asp:LinkButton>
      <asp:LinkButton ID="lang_nl_NL" class="flag" runat="server" value="nl-NL" OnClientClick="upgradeWizard.changePageLocale('lang_nl_NL','nl-NL');"><img src="../images/flags/nl-NL.gif" alt="nl-NL" class="flagimage"/></asp:LinkButton>
    </div>

    <div class="install">
      <h2 class="dnnForm dnnInstall dnnClear">
        <asp:Label ID="lblDotNetNukeUpgrade" runat="server" ResourceKey="Title" />
        <h5>
          <asp:Label ID="currentVersionLabel" runat="server" /></h5>
        <h5>
          <asp:Label ID="versionLabel" runat="server" /></h5>
        <h5>
          <asp:Label ID="versionsMatch" runat="server" /></h5>
      </h2>
      <br />
      <div class="dnnForm dnnInstall dnnClear" id="dnnInstall" runat="server">
        <hr />
        <asp:Label ID="lblIntroDetail" runat="Server" ResourceKey="BestPractices" />
      </div>

      <div id="tabs" class="dnnWizardTab">
        <ul>
          <li><a href="#upgradeAccountInfo">
            <div class="dnnWizardStep">
              <span class="dnnWizardStepNumber">1</span>
              <span class="dnnWizardStepTitle"><%= LocalizeString("AccountInfo")%></span>
              <span class="dnnWizardStepArrow"></span>
            </div>
          </a>
          </li>
          <li><a href="#upgradeSecurity">
            <div class="dnnWizardStep">
              <span class="dnnWizardStepNumber">2</span>
              <span class="dnnWizardStepTitle"><%= LocalizeString("Security")%></span>
              <span class="dnnWizardStepArrow"></span>
            </div>
          </a>
          </li>
          <li><a href="#upgradeInstallation">
            <div class="dnnWizardStep">
              <span class="dnnWizardStepNumber">3</span>
              <span class="dnnWizardStepTitle"><%= LocalizeString("Upgrade")%></span>
              <span class="dnnWizardStepArrow"></span>
            </div>
          </a>
          </li>
          <li><a href="#upgradeInstallation">
            <div class="dnnWizardStep">
              <span>
                <img id="finishImage" src="../images/finishflag.png" alt="" /></span>
              <span class="dnnWizardStepTitle"><%= LocalizeString("ViewWebsite")%></span>
            </div>
          </a>
          </li>
        </ul>
        <div class="upgradeAccountInfo dnnClear" id="upgradeAccountInfo">
          <asp:Label ID="lblAccountInfoError" runat="server" CssClass="NormalRed" />
          <div class="dnnFormItem">
            <asp:Label ID="lblUsername" runat="server" ControlName="txtUsername" ResourceKey="Username" CssClass="dnnFormRequired dnnLabel" />
            <asp:TextBox ID="txtUsername" runat="server" />
          </div>
          <div class="dnnFormItem">
            <asp:Label ID="lblPassword" runat="server" ControlName="txtPassword" ResourceKey="Password" CssClass="dnnFormRequired dnnLabel" />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" />
          </div>
          <hr />
          <ul class="dnnForm dnnActions dnnClear">
            <li>
              <asp:LinkButton ID="continueLink" runat="server" CssClass="dnnPrimaryAction dnnDisabledAction" resourcekey="NextStep" />
            </li>
          </ul>
        </div>
        <div class="upgradeSecurity dnnClear" id="upgradeSecurity">
          <asp:Label ID="lblSecurityError" runat="server" CssClass="NormalRed" />
          <div id="upgradeSecurityView">
            <span class="upgradeSecurityMessage"><%= LocalizeString("CheckingSecurity")%></span>
            <svg class="wizspinner" viewbox="0 0 50 50">
              <circle class="path" cx="25" cy="25" r="20" fill="none" stroke-width="5"></circle>
            </svg>
          </div>
          <hr />
          <ul class="dnnForm dnnActions dnnClear">
            <li>
              <asp:LinkButton ID="UpgradeNowButton" runat="server" CssClass="dnnPrimaryAction dnnDisabledAction" resourcekey="Next" />
            </li>
          </ul>
        </div>
        <div class="upgradeInstallation dnnClear" id="upgradeInstallation">
          <asp:Label ID="lblUpgradeIntroInfo" runat="server" CssClass="installIntro" ResourceKey="UpgradeIntroInfo" />
          <div id="upgrade" runat="Server" visible="True" class="dnnForm">
            <div class="dnnFormItem">
              <div id="installation-progress">
                <span id="timer"></span>&nbsp;&nbsp;&nbsp;|&nbsp;&nbsp;&nbsp;<span id="percentage" style="height: auto; max-height: 200px; overflow: auto"> </span>
                <div class="dnnProgressbar">
                  <div id="progressbar"></div>
                </div>
                <div id="installation-buttons">
                  <a id="retry" href="javascript:void(0)" class="dnnPrimaryAction"><%= LocalizeString("Retry") %></a>
                  <a id="seeLogs" href="javascript:void(0)" class="dnnSecondaryAction"><%= LocalizeString("SeeLogs") %></a>
                </div>
                <div id="installation-log-container" class="dnnScroll">
                  <div id="installation-log">
                  </div>
                </div>
              </div>
              <div id="installation-steps">
                <p class="step-notstarted" id="DatabaseUpgrade"><span class="states-icons"></span><%= LocalizeString("DatabaseUpgrade")%></p>
                <p class="step-notstarted" id="ExtensionsUpgrade"><span class="states-icons"></span><%= LocalizeString("ExtensionsUpgrade")%></p>
              </div>
              <hr />
              <asp:HyperLink ID="visitSite" runat="server" NavigateUrl="#" resourcekey="VisitWebsite" CssClass="dnnPrimaryAction visitSiteLink" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <input id="PageLocale" runat="server" name="PageLanguage" type="hidden" value="" />
    <asp:Label ID="txtErrorMessage" runat="server" />
  </form>

  <script type="text/javascript">
    var upgradeWizard = new UpgradeWizard();
    function UpgradeWizard() {
      this.accountInfo = {};
      this.changePageLocale = function (flagId, locale) {
        $('.flag').removeClass("selectedFlag");
        $('#' + flagId).addClass("selectedFlag");
        $("#PageLocale")[0].value = locale;
      };
      this.showInstallationTab = function () {
        $("#tabs").tabs('disable', 1);
        $("#tabs").tabs('enable', 2);
        $("#tabs").tabs('option', 'active', 2);
      };
      this.showSecurityTab = function () {
        $("#tabs").tabs('disable', 0);
        $("#languageFlags").hide();
        $('#<%= dnnInstall.ClientID %>').css('display', 'none');
        $("#tabs").tabs('enable', 1);
        $("#tabs").tabs('option', 'active', 1);

        PageMethods.GetSecurityTab(upgradeWizard.accountInfo, function (result) {
          if (result.Item1) {
            $('#<%= lblSecurityError.ClientID %>').text('');
                  $('#upgradeSecurityView').html(result.Item3.View);
                  $('input[name="<%= TelerikUninstallOptionClientID %>"').click(function () {
                    enable("#<%= UpgradeNowButton.ClientID %>");
                      });
                  if (result.Item3.CanProceed) {
                    enable("#<%= UpgradeNowButton.ClientID %>");
                  }
                } else {
                  $('#<%= lblSecurityError.ClientID %>').text(result.Item2);
                  $('#<%= lblSecurityError.ClientID %>').css('display', 'block');
                  setTimeout(function () { $('#<%= lblSecurityError.ClientID %>').css('display', 'none') }, 3000);
                }
              });
      };
      this.finishInstall = function () {
        upgradeWizard.stopProgressBar();
        $("#tabs").tabs('enable', 3);
        $("#tabs").tabs('option', 'active', 3);
        $("#tabs").tabs('disable', 2);
        $('#tabs ul').css('background-position', '0 -100px');
        enable('#seeLogs, #visitSite');
        $('#visitSite').attr("href", location.pathname + "?complete");
        $('#installation-steps > p').attr('class', 'step-done');
      };
      this.progressBarIntervalId = {};
      this.timerIntervalId = {};
      this.startProgressBar = function () {
        $("#timer").html('0:00 ' + '<%=LocalizeString("TimerMinutes") %>');
            var totalSeconds = 0;
            var minutes = 0;

            upgradeWizard.progressBarIntervalId = setInterval(function () {
              $.getUpgradeProgress();
            }, 500);

            upgradeWizard.timerIntervalId = setInterval(function () {
              totalSeconds = totalSeconds + 1;
              minutes = minutes + Math.floor(totalSeconds / 60);
              totalSeconds = totalSeconds % 60;
              var seconds = Math.floor(totalSeconds);
              // Pad the minutes and seconds with leading zeros, if required
              seconds = (seconds < 10 ? "0" : "") + seconds;

              $("#timer").html(minutes + ":" + seconds + ' <%=LocalizeString("TimerMinutes") %>');
              }, 1000);
      };
      this.stopProgressBar = function () {
        clearInterval(upgradeWizard.timerIntervalId);
        clearInterval(upgradeWizard.progressBarIntervalId);
      };
      this.upgrade = function () {
        var token = $('#<%= TelerikAntiForgeryTokenClientID %>').val();
            upgradeWizard.accountInfo["<%= TelerikAntiForgeryTokenClientID %>"] = token;

            $.startProgressbar();
            //Call PageMethod which triggers long running operation
            PageMethods.RunUpgrade(upgradeWizard.accountInfo, function () {
            }, function (err) {
              $.stopProgressbarOnError();
            });
            disable('#seeLogs, #visitSite, #retry');
            //Making sure that progress indicate 0
            $("#progressbar").progressbar('value', 0);
            $("#percentage").text('0% ');
            $("#timer").html('0:00 ' + '<%=LocalizeString("TimerMinutes") %>');
      };
    }

    function enable(selector) {
      $(selector).removeClass('dnnDisabledAction');
    };

    function disable(selector) {
      $(selector).addClass('dnnDisabledAction');
    };

    /*globals jQuery, window, Sys */
    (function ($, Sys) {
      $(function () {
        //update css
        $("link[class=needVer]").each(function (index, item) {
          $(item).attr("href", $(item).attr("href") + "?<%=DotNetNuke.Common.Globals.FormatVersion(ApplicationVersion)%>");
        });
        $("#tabs").on("tabscreate", function (event, ui) {
          var index = 0, selectedIndex = 0;
          $('.ui-tabs-nav li', $(this)).each(function () {
            if ($(this).hasClass('ui-tabs-active'))
              selectedIndex = index;
            index++;
          });

          $('.dnnWizardStepArrow', $(this)).eq(selectedIndex).css('background-position', '0 -299px');
          if (selectedIndex)
            $('.dnnWizardStepArrow', $(this)).eq(selectedIndex - 1).css('background-position', '0 -201px');
        });

        $("#tabs").on("tabsactivate", function (event, ui) {
          var index = ui.newTab.index();
          $('.dnnWizardStepArrow', $(this)).css('background-position', '0 -401px');
          $('.dnnWizardStepArrow', $(this)).eq(index).css('background-position', '0 -299px');
          if (index) {
            $('.dnnWizardStepArrow', $(this)).eq(index - 1).css('background-position', '0 -201px');
          }
        });
        $("#tabs").tabs();
        $("#tabs").tabs({ disabled: [1, 2, 3] });
        $('.dnnFormMessage.dnnFormError').each(function () {
          if ($(this).html().length)
            $(this).css('display', 'block');
        });

        upgradeWizard.dnnProgressbar = $(".dnnProgressbar").dnnProgressbar();
      });

      //****************************************************************************************
      // EVENT HANDLER FUNCTIONS
      //****************************************************************************************

      enable("#<%= continueLink.ClientID %>");

      //Next Step
      $('#<%= continueLink.ClientID %>').click(function () {

        if (!$(this).hasClass('dnnDisabledAction')) {
          upgradeWizard.accountInfo = {
            username: $('#<%= txtUsername.ClientID %>')[0].value,
            password: $('#<%= txtPassword.ClientID %>')[0].value
          };

              disable("#<%= continueLink.ClientID %>");
              disable('#seeLogs, #visitSite, #retry');

              PageMethods.ValidateInput(upgradeWizard.accountInfo, function (result) {
                if (result.Item1) {
                  $('#<%= lblAccountInfoError.ClientID %>').text('');
                      upgradeWizard.showSecurityTab();
                    } else {
                      $('#<%= lblAccountInfoError.ClientID %>').text(result.Item2);
                      $('#<%= lblAccountInfoError.ClientID %>').css('display', 'block');
                      setTimeout(function () { $('#<%= lblAccountInfoError.ClientID %>').css('display', 'none') }, 3000);
                      enable("#<%= continueLink.ClientID %>");
                    }
                  });
            }

            return false;
          });
      $('#<%= UpgradeNowButton.ClientID %>').click(function () {

        if (!$(this).hasClass('dnnDisabledAction')) {
          var option = $('input[name="<%= TelerikUninstallOptionClientID %>"').val();
              upgradeWizard.accountInfo["<%= TelerikUninstallOptionClientID %>"] = option;
              upgradeWizard.showInstallationTab();
              upgradeWizard.upgrade();
            }

            return false;
          });
    }(jQuery, window.Sys));
  </script>

  <!-- Progressbar -->
  <script type="text/javascript">
    $.getUpgradeProgress = function () {
      $.get(
                "<%= StatusFilename %>?" + "<%= StatusFilename %>?" + Math.random(),
        function (responseText) {
          try {
            var statuslines = responseText.split('\n');
            $.updateProgressbar(statuslines[statuslines.length - 2]);
          } catch (err) { } // ignore
        }).fail(function () {
          upgradeWizard.Status = "";
        });
    };

    $.updateProgressbar = function (status) {
      var result = jQuery.parseJSON(status);
      if (result !== null) {
        if (result.progress < $("#progressbar").progressbar('value')) return;
        //Updating progress
        $("#progressbar").progressbar('value', result.progress);
        upgradeWizard.dnnProgressbar.update(result.progress);
        $("#percentage").text(result.progress + '% ' + result.details);
        var upgradeError = result.details.toUpperCase().indexOf('<%= Localization.GetSafeJSString(LocalizeString("Error"))%>') > -1;
        if (upgradeError) {
          // go through all the result and mark error state
          var i = 0;
          for (i = 0; i < 2; i++) {
            var done = result["check" + i] === 'Done';
            if (done) {
              if (i < 1) {
                result["check" + (i + 1)] = "Error";
              }
              break;
            }
          }
          if (i == 1) {
            result.check0 = "Error";
          }
        }
        $.applyCssStyle(result.check0, $('#DatabaseUpgrade'));
        $.applyCssStyle(result.check1, $('#ExtensionsUpgrade'));

        //If operation is complete
        if (result.progress >= 100 || result.details == '<%= Localization.GetSafeJSString(LocalizeString("UpgradeDone"))%>') {
          upgradeWizard.finishInstall();
          //Enable button
          enable('#seeLogs, #visitSite');
          $('#visitSite').attr("href", location.pathname + "?complete");
          $('#installation-steps > p').attr('class', 'step-done');
        }
        //If not
        else {
          if (upgradeError) { // if error in upgrade
            $.stopProgressbarOnError();
          }
        }
      }
    };

    $.applyCssStyle = function (state, ele) {
      if (!state) state = '';
      switch (state.toLowerCase()) {
        case 'done':
          ele.attr('class', 'step-done');
          break;
        case 'running':
          ele.attr('class', 'step-running');
          break;
        case 'error':
          ele.attr('class', 'step-error');
          break;
        default:
          ele.attr('class', 'step-notstarted');
          break;
      }
    };

    //Start progress bar
    $.startProgressbar = function () {
      //Disabling button
      disable('#seeLogs, #visitSite, #retry');
      //Making sure that progress indicate 0
      $("#progressbar").progressbar().progressbar('value', 0);
      $("#percentage").text('0%');
      upgradeWizard.startProgressBar();
      $("#progressbar").removeClass('stoppedProgress');
      $("#progressbar").addClass('inProgress');
    };

    //Stop update progress bar on errors
    $.stopProgressbarOnError = function () {
      enable("#seeLogs, #retry");
      if ($.updateProgressbarTimeId) {
        clearTimeout($.updateProgressbarTimeId);
        $.updateProgressbarTimeId = null;
      }
      $('#installation-steps > p.step-running').attr('class', 'step-error');
      upgradeWizard.stopProgressBar();
      $("#progressbar").removeClass('inProgress');
      $("#progressbar").addClass('stoppedProgress');
    };

    $(document).ready(function () {
      //Page Locale
      $('.flag').removeClass("selectedFlag");
      $('.flag[value=' + $("#PageLocale")[0].value + ']').addClass("selectedFlag");

      //Progressbar and button initialization
      $("#progressbar").progressbar({ value: 0 });
      disable('#visitSite, #seeLogs, #retry');

      $("#retry").click(function (e) {
        e.preventDefault();
        if (!$(this).hasClass('dnnDisabledAction')) {
          disable('#retry');
          $('#installation-log-container').hide();
          $.startProgressbar();
          //Call PageMethod which triggers long running operation
          PageMethods.RunUpgrade(upgradeWizard.accountInfo, function () {
          }, function (err) {
            $.stopProgressbarOnError();
          });
        }
      });

      var installationLogStartLine = 0;
      var getInstallationLog = function () {
        PageMethods.GetInstallationLog(installationLogStartLine, function (result) {
          if (result) {
            if (installationLogStartLine === 0)
              $('#installation-log').html(result);
            else
              $('#installation-log').append(result);

            installationLogStartLine += 500;
            setTimeout(getInstallationLog, 100);
          } else {
            if (installationLogStartLine === 0)
              $('#installation-log').html('<%= Localization.GetSafeJSString(LocalizeString("NoInstallationLog"))%>');
          }
          $('#installation-log-container').jScrollPane();
        }, function (err) {
        });
      };

      $('#seeLogs').click(function (e) {
        e.preventDefault();
        if (!$(this).hasClass('dnnDisabledAction')) {
          disable(this);
          $('#installation-log-container').show();
          var loading = '<%= DotNetNuke.Services.Localization.Localization.GetSafeJSString(LocalizeString("LoadingInstallationLog"))%>';
          $('#installation-log').html(loading);
          getInstallationLog();
        }
      });

      $('#visitSite').click(function (e) {
        if ($(this).hasClass('dnnDisabledAction')) {
          e.preventDefault();
        } else {
          disable(this);
        }
      });
    });
  </script>

</body>
</html>
