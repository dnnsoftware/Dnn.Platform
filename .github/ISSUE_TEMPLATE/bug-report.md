---
name: Bug Report
about: Report a bug in DNN Platform
labels: "Status: New"
assignees: dnnsoftware/triage
---
<!-- 
  If you need community support or would like to solicit a Request for Comments (RFC), please post to the DNN Community forums at https://dnncommunity.org/forums for now.  In the future, we are planning to implement a more robust solution for cultivating new ideas and nuturing these from concept to creation.  We will update this template when this solution is generally available.  In the meantime, we appreciate your patience as we endeavor to streamline our GitHub focus and efforts.
  
  Please read the CONTRIBUTING guidelines at https://github.com/dnnsoftware/Dnn.Platform/blob/develop/CONTRIBUTING.md prior to submitting an issue.

  Any potential security issues SHOULD NOT be posted on GitHub.  Instead, please send an email to security@dnnsoftware.com.
-->
## Description of bug
GetUserRequestIPAddress has a bug since it assumes that all incoming requests are IPv4 addresses. This causes bugs in all downstream IP address handling. One specific example is the LOGIN_SUCCESS event log entry does not include the IPv6 address (IPv4 addresses are working). My proposed change checks if the request is coming from a valid IPv4 address and returns it if it is IPv4. If not, it checks if it is an IPv6 address and returns it or string.Empty if the IP address is not valid.

## Steps to reproduce
Login using an IPv6 address and the LOGIN_SUCCESS log message does not have the IPv6 address.

## Current behavior
The LOGIN_SUCCESS log message does not have the IPv6 address.  Only the IPv4 address is shown.

## Expected behavior
The IPv6 IP address should show up in the log.

## Screenshots


## Error information


## Additional context


## Affected version
<!-- 
Please add X in at least one of the boxes as appropriate. In order for an issue to be accepted, a developer needs to be able to reproduce the issue on a currently supported version. If you are looking for a workaround for an issue with an older version, please visit the forums at https://dnncommunity.org/forums
-->
* [X] 10.00.00 alpha build
* [X] 09.08.00 release candidate
* [X] 09.07.02 release candidate

## Affected browser
<!-- 
  Check all that apply, and add more if necessary. As appropriate, please specify the exact version(s) of the browser and operating system.
-->
* [ ] Chrome
* [ ] Firefox
* [ ] Safari
* [ ] Internet Explorer 11
* [ ] Microsoft Edge (Classic)
* [ ] Microsoft Edge Chromium
