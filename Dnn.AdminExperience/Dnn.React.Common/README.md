[![Build status](https://ci.appveyor.com/api/projects/status/er8qc8a7323ctfb1?svg=true)](https://ci.appveyor.com/project/DnnAutomation/dnn-react-common)

## DNN React Common Components
DNN React Common is a library of common React components that can be used in [DNN.Platform](https://github.com/dnnsoftware/Dnn.Platform/) Persona Bar and Module projects.

## Installation
DNN React Common uses [Yarn](https://yarnpkg.com/) as the package manager. To install the components run

```
yarn install @dnnsoftware/dnn-react-common
```
## Usage
Once installed you can reference any of the components by doing
```
import { Label, SingleLineInputWithError } from "dnn-react-common";
```
## StoryBook
DNN React Common uses the [StoryBook.js](https://storybook.js.org/) library to create a StoryBook. Every component in the project contains a ```.stories.js``` file. This allows you to test the components in a isolated environment and see what the components are doing. To run the StoryBook run

```
yarn run storybook
```

## Building the Components
To build the components you just need to run
```
yarn run build
```

## License
 
The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

Font Awesome Icons By Font Awesome are licensed under CC BY 4.0 [https://creativecommons.org/licenses/by/4.0/](https://creativecommons.org/licenses/by/4.0/)
