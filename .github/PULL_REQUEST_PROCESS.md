# DNN Platform Pull Request Proceseses

The goal of this document is to set standards for the review, processing, and approval of all Pull requests into the DNN_Platform GitHub repository, as well as associated commmunity open-source repositories.  These rules apply to ALL pull requests, regardless of the submitter, or complexity of the change.

## Prerequisites
The following items must all be true prior to a pull request being submitted to the DNN_Platform, if any of the following items are not true the pull request will be returned for edits referencing the specific missing item(s).

* Change should represent an entire solution for the issue at hand.  Partial requests will NOT be processed.
* Change should have a supporting issue logged on the DNN_Platform GitHub account, documenting the issue resolved, following the procedures outlined on the [Contribute Page](CONTRIBUTING.md)
* If your change was to an area that already was covered by tests those tests must be updated.  New tests for areas currently un-tested are appreciated
* **Exception**: Security items can be addressed individually, and should be initially communicated to the security@dnnsoftware.com email address for coordination
* Pull request comment should contain at a minimum the following details (When creating, a default template will also prompt you for the proper information)
  * Issue #, referenced such as “Fixes #24”
  * Release Note: Suggested release note for the change such as “Improvement of user profile to improve performance when searching.”
  * Testing Steps: Suggested testing steps for validation of the change
* Pull request must pass the Continuous Integration (CI) Build.  Failure of the build will result in no futher review of the pull request until the build has been corrected.
* The .NET Foundation [Contributor License Agreement (CLA)] (https://cla.dotnetfoundation.org/) must be signed prior to code review.  An automated process will prompt for this as part of the Pull Request process.

> NOTE: Changes to methods that have been marked as [Obsolete] will typically not be accepted due to inherit risks associated with code scheduled for removal.  If a critical bug/issue exists, please be sure to document & discuss using an RFC issue prior to submitting your contribution to ensure alignment with acceptance criteria.

## Code Review Process
Community review of submitted pull requests is encouraged, and all pull requests must be reviewed by at least **two (2)** designated approvers before the change can be approved.  Once approved by two unique designated approvers, the change can be merged. An approver should NEVER merge their own change, even with two supporting approvals.  

### Designated Approvers
At the current time the following community members are designated approvers.

* Mitchel Sellers ([mitchelsellers](https://github.com/mitchelsellers)) - Community Technology Advisory Group Lead
* Oliver Hine ([ohine](https://github.com/ohine))
* Brian Dukes ([bdukes](https://github.com/bdukes))
* Peter Donker ([donker](https://github.com/donker)) - Community Developer Advisory Group Lead
* Daniel Valadas ([valadas](https://github.com/valadas))
* Matt Rutledge ([mtrutledge](https://github.com/mtrutledge))
* Vicenç Masanas ([vmasanas](https://github.com/vmasanas))
* Erik van Ballegoij ([erikvb](https://github.com/erikvb))

Additionally, the following individuals from ESW/DNN Corp are approved reviewers.

* Daniel Aguilera ([daguiler](https://github.com/daguiler)) - CTO
* Ash Prasad ([ashishpd](https://github.com/ashishpd)) - VP of Engineering

### Review Minimums
An individual performing the code review should validate at a minimum the following.

* The source should be able to be cloned, compiled, and all tests ran without error
* The issue identified should no longer be apparent, and testing steps validated
* The change should be free from visible errors, and adherence to recommended best practices should be validated.
* All tests are passing successfully (Confirmed with the automated build)

If a reviewer has suggestions for improvement, those should be noted in the pull request and revision requested.  If the pull request is acceptable “as is” simply noting, via a comment, “I reviewed this, no concerns” will document the completion of a review, this statement is preferred to the shorthand LGTM (Looks good to me) for clarity.

*If you have questions about a pull request or an idea for a pull request, please reach out to one of the approvers before submitting to ensure a streamlined process.*

## Merging & Closing of Requests
Once a pull request has been reviewed by two designated approvers it may be merged and the pull request closed.

> This **_MUST NOT_** be done by the submitter of the Pull Request for ANY reason!  

### Execption for Build Process Pull Requests
A special exception has been granted to reduce the number of Designed Approval reviews to 1 for all pull requests related to Build Support when incrementing versions in prepration of a release, or reconfiguration of the environment for the next version. 

Additionally a special exception has been granted to merge pull requests created by the Designated Approvers for the purposes of keeping release branches in sync when the pull request ONLY moves previously approved changes.  Such as moving bug fixes from a minor release into the next major release, or otherwise.

### Milestone (Version) Targeted
We follow the process outlined in the [Versioning Policy](VERSIONING_POLICY.md) with regards to major, minor, and revision releases.  As items are reviewed and approved they will be assigned to a milestone based on their impact and risk.  If, when submitting, a specific release is requested coordinate the scope of change with the Designated Approvers or technology advisory group to ensure your item will meet the requirements for the targeted release.  We work diligently to accept all pull requests, but ultimately must keep the platform stable and predictable for all users.

### Stale Pull Requests

The review team will work to respond to all pull requests in a timely fashion.  If changes or additional information is requested a pull request will remain open allowing the submitter to update their contribution accordingly.  If a request for additional information or changes is not completed with 90 days of request the Pull Request will be closed to keep the pipeline clear.  Once the needed information has been gathered the information can be re-submitted via a new Pull Request.  

For expedited processing you may reference the prior Pull Request.

### Items for Future Releases
If an item was submitted that will be integrated into a future release that is not currently in the development pipeline it is possible that the Pull Request will remain open. 

In this situation the reviewing team will approve the request, tag the request with a specific version milestone and add a comment noting when and why it will be included in the particularly identified release.

This most often will apply to technology or dependency changes that require alignment with Major, Minor, Revision build inclusion.  
