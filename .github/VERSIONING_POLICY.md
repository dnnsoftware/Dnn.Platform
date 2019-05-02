# DNN Platform Versioning and Deprecation Policies
The DNN Platform follows a semantic versioning process for releases, in a manner to better communicate expectations of releases and their potential impacts to users of the platform.

##Semantic Versioning
The DNN Community adopted the current semantic version policy in July of 2018.  Releases before this date may follow different standards.

### Major Releases (Ex 10.0.0)
A major release is as the name implies, a release with major changes.  These changes might include new features, breaking changes, or other larger changes.  Each major release will come with release notes that outline the nature of any known breaking changes.

Major releases are also the time that platform requirements might be changed, such as requiring a new edition of SQL Server or otherwise.

### Minor Releases (Ex 10.1, 10.2, 10.x)
A minor might contain smaller new features and enhancements, but will not introduce any breaking API changes, nor will it change the requirements of the hosting environment or platform to run the application.  

### Revision Releases (Ex 10.1.1, 10.1.2, 10.1.x)
These releases are created primarily to contain hot-fix style improvements from prior releases.  Any bugs or security issues identified, or missing UI/UX features from a Minor/Major release might be added to a revision release.  Similar to a Minor release a Revision release will not contain any known breaking changes.

## API Deprecation Policy
The DNN Platform project is in a state of transition, continuing to modernize the API and work towards a transition to .NET Core.  To this point, it will be necessary for the project to remove public API's.  This will be done methodically, allowing developers to transition away from the older code with time to properly respond to change.
Any API method to be removed will be flagged as deprecated in a release, major, minor or revision, and will be identified to be removed by a specific version.  This will be done using a C# annotation with a comment similar to the following "Deprecated in x.x.x.  Scheduled for removal in vy.0.0, use ____ instead".  The version number of "y" in this example must be 2 major versions ahead.  
Therefore, an API marked as Deprecated in 9.2.1 can only be removed in version 11.0.  Additionally, methods marked for removal in a version will GUARANTEED be removed in that revision.
> Example: [Obsolete("Deprecated in DotNetNuke 7.0. This function has been replaced by AddUserRole with additional params. Scheduled removal in v10.0.0.")]

### Special DNN 10.x Cleanup
A number of legacy APIs have been marked as deprecated for more than 7 years and not yet removed.  To continue to clean the API structure a final cleanup is being completed as part of the 10.x release.  All of these API's are more than 2 major revisions older, however, have non-standard indicators for the Obsolete attribute.  These will be removed in 10.x along with other expected removals.
Lastly, each Major release will contain release notes outlining every API method removed.  More information can be found [in this blog post](https://www.dnnsoftware.com/community-blog/cid/156712/moving-forward-dnn-platform-100-growing-pains-lead-to-improvement)



