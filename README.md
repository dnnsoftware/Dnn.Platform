[![Build status](https://ci.appveyor.com/api/projects/status/er8qc8a7323ctfb1?svg=true)](https://ci.appveyor.com/project/DnnAutomation/dnn-react-common)

>This documentation is based in the info from 
http://docs.myget.org/docs/reference/myget-npm-support

## Configure local npm to use the DNN public repository
From the command line, the following command must be executed:
```
npm config set registry https://www.myget.org/F/dnn-software-public/npm/
```

## Installing a package from the private repository

To install a package, execute the regular npm command:

```
npm install packagename
```

This will search the package in the private repository, and if not found, it will then use the public npm repository

## Compiling a package

To compile a package from the source, it is needed to install all its dependencies in the package folder

```
cd package
npm install
```

Then the build script should be run to generate the compiled js in the lib subfolder

```
npm run build
```

### Note:
[Yarn](https://yarnpkg.com/) is another dependency management tool that can be used for building these packages. If you prefer to use this tool instead of `npm`, then you need only to replace the `npm` with `yarn` in the various commnads above.