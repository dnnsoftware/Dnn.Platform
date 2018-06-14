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

## Code Review Process
Community review of submitted pull requests is encouraged, and all pull requests must be reviewed by at least **two (2)** community approvers before the change can be approved.  Once approved by two unique individuals the change can be merged by the secondary approval.  At the current time the following community members are designated approvers.

* Mitchel Sellers ([mitchelsellers](https://github.com/mitchelsellers)) - Community Technology Advisory Group Lead
* Oliver Hine ([ohine](https://github.com/ohine))
* Brian Dukes ([bdukes](https://github.com/bdukes))
* Peter Donker ([donker](https://github.com/donker)) - Community Developer Advisory Group Lead
* Vicnec Masanas ([vmasanas](https://github.com/vmasanas))
* Erik van Ballegoij ([erikvb](https://github.com/erikvb))

Additionally the following individuals from ESW/DNN Corp are approved reviewers.

* Tomasz Pluskiewicz ([tpluscode](https://github.com/tpluscode)) - CTO
* Ash Prasad ([ashishpd](https://github.com/ashishpd)) - VP of Engineering

An individual performing the code review should validate at a minimum the following.

* The source should be able to be cloned, compiled, and all tests ran without error
* The issue identified should no longer be apparent, and testing steps validated
* The change should be free from visible errors, and adherence to recommended best practices should be validated.
* All tests are passing successfully (Confirmed with automated build)

If a reviewer has suggestions for improvement, those should be noted in the pull request and revision requested.  If the pull request is acceptable “as is” simply noting, via a comment, “I reviewed this, no concerns” will document the completion of a review, this statement is preferred to the shorthand LGTM (Looks good to me) for clarity.

*If you have questions about a pull request, or an idea for a pull request, please reach out to one of the approvers before submitting to ensure a streamlined process.*

## Approval & Merging Process
Once a pull request has been reviewed by two individuals it may be merged.

This **_MUST NOT_** be done by the submitter of the Pull Request for ANY reason!  

