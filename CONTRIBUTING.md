# How to contribute

Community contributions are essential part of any open source project. The
community has access to a large number of unique configurations which would
be extremely difficult for the core maintainers to reproduce. We want to keep 
it as easy as possible to contribute changes that get things working in your 
environment. There are a few guidelines that we need contributors to follow 
so that we can have a chance of keeping on top of things.

## Getting Started

* Make sure you have a [DNN Tracker account](https://dnntracker.atlassian.net/secure/Signup!default.jspa)
* Make sure you have a [GitHub account](https://github.com/signup/free)
* [Submit a ticket](https://dnntracker.atlassian.net/secure/CreateIssue!default.jspa) for your issue, assuming one does not already exist. If you have the rights, you should assign yourself to the issue and click 'start progress' to indicate that the issue is underway. If you do not have that ability please add a comment noting that a pull request will be submitted for the issue, and the engineering team will handle the assignment. After review, if the pull request is accepted, we will mark the issue as resolved and assign it to a release so it can be QA'ed.
  * Clearly describe the issue including steps to reproduce when it is a bug.
  * Make sure you fill in the earliest version that you know has the issue.
* Fork the repository on GitHub

## Making Changes

* Create a topic branch from where you want to base your work.
  * This is usually the 'development' branch.
  * Release branches should only be targeted by official committers.
  * To quickly create a topic branch based on development; `git checkout -b my_contribution development`
* Make commits of logical units.
* Check for unnecessary whitespace with `git diff --check` before committing.
* Make sure your commit messages are in the proper format.

````
    (DNN-####) Make the example in CONTRIBUTING imperative and concrete

    Without this patch applied the example commit message in the CONTRIBUTING
    document is not a concrete example.  This is a problem because the
    contributor is left to imagine what the commit message should look like
    based on a description rather than an example.  This patch fixes the
    problem by making the example concrete and imperative.

    The first line is a real life imperative statement with a ticket number
    from our issue tracker.  The body describes the behavior without the patch,
    why this is a problem, and how the patch fixes the problem when applied.
````

* For bonus points run and add unit tests
	* Make sure you have added the necessary tests for your changes.
	* Run _all_ the tests to assure nothing else was accidentally broken.


## Making Trivial Changes

### Documentation

For changes of a trivial nature to comments and documentation, it is not
always necessary to create a new ticket in the issue tracker. In this case, it is
appropriate to start the first line of a commit with '(doc)' instead of
a ticket number. 

````
    (doc) Add documentation commit example to CONTRIBUTING

    There is no example for contributing a documentation commit
    to the DNN repository. This is a problem because the contributor
    is left to assume how a commit of this nature may appear.

    The first line is a real life imperative statement with '(doc)' in
    place of what would have been the ticket number in a 
    non-documentation related commit. The body describes the nature of
    the new documentation or comments added.
````

## Submitting Changes

<<<<<<< HEAD
* Sign the [Contributor License Agreement](http://www.dnnsoftware.com/Portals/0/Community/CLA/DNN - CLA.docx).
=======
* Sign the [Contributor License Agreement](http://www.dnnsoftware.com).
>>>>>>> upstream/master
* Push your changes to a topic branch in your fork of the repository.
* Submit a pull request to the DNN.Platform repository in the DNNSoftware organization.
* The committers will handle updating the associated issue in the DNN Tracker to ensure it gets the necessary code review and QA.

## Acceptance of your Changes
* We have a group of fellow developers that review pull requests submitted by developers like yourself.
* If your changes look good, then changes are merged to an appropriate release.
* We may ask you to make further changes or reject the change (with proper reasonsing - we hope that's not the case though).
* You should get an email notification as we complete processing of your pull request.

## Downloading latest package with your changes
* As soon as changes are accepted, our team city build server gets into action.
* New builds are usually available within 10 minutes of acceptance.
* You can download the latest build by logging as a 'guest user' at: https://build.dnnsoftware.com/

# Additional Resources

* [Issue tracker (Jira)](https://dnntracker.atlassian.net/)
<<<<<<< HEAD
* [Contributor License Agreement](http://www.dnnsoftware.com/Portals/0/Community/CLA/DNN - CLA.docx)
=======
* [Contributor License Agreement](http://www.dnnsoftware.com)
>>>>>>> upstream/master
* [General GitHub documentation](http://help.github.com/)
* [GitHub pull request documentation](http://help.github.com/send-pull-requests/)
