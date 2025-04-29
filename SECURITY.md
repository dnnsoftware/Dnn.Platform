# Security Policy

## Reporting a Vulnerability

We make every effort to ensure speedy analysis of reported issues and, where required, provide workarounds and updated application releases to fix them. If you see suspected issues/security scan results please report them via [the GitHub feature for reporting a security vulnerability](https://github.com/dnnsoftware/Dnn.Platform/security/advisories/new).

All submitted information is viewed only by members of the DNN Security Task Force, and will not be discussed outside the Task Force without the permission of the person/company who reported the issue. Each confirmed issue is assigned a severity level (critical, moderate, or low) corresponding to its potential impact on the security of DNN installations.

- **Critical** means the issue can be exploited by a remote attacker to gain access to DNN data or functionality. All critical issue security bulletins include a recommended workaround or fix that should be applied as soon as possible.
- **Moderate** means the issue can compromise data or functionality on a portal/website only if some other condition is met (e.g. a particular module or a user within a particular role is required). Moderate issue security bulletins typically include recommended actions to resolve the issue.
- **Low** means the issue is very difficult to exploit or has a limited potential impact.

Once an issue has been resolved via a public release of DNN Platform, the release notes on GitHub are updated to reflect security bulletins exist for the release. Additionally, the [DNN Security Center](https://dnncommunity.org/security) is updated with the vulnerability details. We strongly suggest using the "Watch" option on GitHub for "Releases" at a minimum to receive notifications of DNN Platform releases.

## Coordinated Disclosure & Timelines

DNN Platform follows a policy of coordinated disclosure, meaning we aim to provide clear and timely communication about security issues after a fix has been implemented and released.

As an open-source project maintained by a volunteer-driven community, we cannot guarantee release dates for patches. While we take all reports seriously, the time required to validate, fix, test, and publish a release depends entirely on the availability of community contributors.

If you are a security researcher, we appreciate your patience and understanding. DNN Platform is not maintained by a commercial entity with dedicated security engineers on staff. Instead, it is maintained by the DNN Community and our ability to respond depends on the time and effort of volunteers. You can help speed up resolution by:

- Providing detailed reproduction steps.
- Suggesting mitigation strategies.
- Contributing a patch or fix via a pull request (let us know if you are interested and we will be happy to collaborate securely).

We understand the desire to know which version will include the fix and when it will be disclosed, but please note that we are unable to commit to specific timelines. We will know more about which version the fix is targeting once it is implemented and verified.

## Public Disclosure and Notifications

Once a fix has been released, the vulnerability will be documented in the [DNN Security Center](https://dnncommunity.org/security), and in the [GitHub repository security advisory tab](https://github.com/dnnsoftware/Dnn.Platform/security/advisories).

We typically wait about 30 days after a release before disclosing any details of a resolved vulnerability. This delay allows site administrators time to apply updates before specific exploitation techniques are made public. However, this timing can vary depending on several factors.

As a general policy, DNN Platform does not issue Hot Fix releases to prior versions of DNN Platform. If a remediation is possible via configuration it shall be noted as applicable in the posted bulletins.
