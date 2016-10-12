# Dnn.PersonaBar.Boilerplate.ReactRedux

Boilerplate code for Persona Bar - Quick set up with useful components such as Tabs & Pane Navigation. 
Pull this code to get started quickly on a new persona bar module. Built using React/Redux.

# Requirements

1.) The first thing needed is the application-container, which is where our react application attaches to.
    This container should have a data-init-callback parameter, which defines the JS function which we use to
    pass in certain parameters, such as the utility object from the persona bar.

![HTML Requirements](/readme/htmlRequirements.png "HTML Requirements")

2.) We also need to have a loadScript function, which loads our bundle and also define the data-init-callback function specified in the HTML above. This is where we are able to pass in stuff like the utility object to our React application, to allow proper API calls. This change is done in the .JS file of your module.

![HTML Requirements](/readme/jsRequirements.png "JS Requirements")

3.) NodeJS

# Getting started

1.) Pull the repository.

2.) Our react boilerplate template uses private myget packages, so we need to authenticate node before running npm install.
    We have two options - pre-authenticated URL or specifying login information.
    
    a.) Pre-authenticated URL:
    npm config set registry https://www.myget.org/F/dnn-software-private/auth/53bf0254-cdf2-4f2b-9fd6-6a10b0ceaf68/npm/
    
    b.) To set a proper username and password to install and push packages, follow the instructions on 
    https://github.com/dnnsoftware/Dnn.Evoq.Microservices.UI.Common/edit/master/readme.md
    
3.) Once our credentials have been established, we can then run npm install or npm i.

4.) Once the packages are installed, run webpack by doing npm run webpack. This will spit up a dev server with the bundle hosted, and you can now start developing locally.

# Notes

1.) The instructions on this readme are for development purposes only. To deploy on production, we have to set up MyGet and Visual Studio Team Services to establish an automatic building process. (Not done yet)

2.) You should change the package name from boilerplate to the appropriate name.

![HTML Requirements](/readme/packageName.PNG "Package Name")

3.) Rename boilerplate to your application name.

![HTML Requirements](/readme/boilerplateApp.png "Boiler Plate App")
