const webpack = require("webpack");
const packageJson = require("./package.json");
const isProduction = process.env.NODE_ENV === "production";
const languages = {
    "en": null
    // TODO: create locallizaton files per language 
    // "de": require("./localizations/de.json"),
    // "es": require("./localizations/es.json"),
    // "fr": require("./localizations/fr.json"),
    // "it": require("./localizations/it.json"),
    // "nl": require("./localizations/nl.json")
};

const webpackExternals = require("dnn-webpack-externals");

module.exports = {
    entry: "./src/main.jsx",
    output: {
        path: "../admin/personaBar/scripts/bundles/",
        filename: "licensing-bundle.js",
        publicPath: isProduction ? "" : "http://localhost:8080/dist/"
    },

    module: {
        loaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loaders: ["react-hot-loader", "babel-loader"] },
            { test: /\.less$/, loader: "style-loader!css-loader!less-loader" },
            { test: /\.(ttf|woff)$/, loader: "url-loader?limit=8192" }
        ],

        preLoaders: [
            { test: /\.(js|jsx)$/, exclude: /node_modules/, loader: "eslint-loader" }
        ]
    },

    resolve: {
        extensions: ["", ".js", ".json", ".jsx"]
    },
	
	externals: {
        "react": "dnn.nodeModules.React",
        "react/lib/ReactMount": "dnn.nodeModules.ReactMount",
        "react/lib/ReactComponentWithPureRenderMixin": "dnn.nodeModules.ReactComponentWithPureRenderMixin",
        "redux": "dnn.nodeModules.Redux",
        "react-redux": "dnn.nodeModules.ReactRedux",
        "react-dom": "dnn.nodeModules.ReactDOM",
        "react-tabs": "dnn.nodeModules.ReactTabs",
        "redux-devtools": "window.dnn.nodeModules.ReduxDevTools",
        "redux-devtools-dock-monitor": "window.dnn.nodeModules.ReduxDevToolsDockMonitor",
        "redux-devtools-log-monitor": "window.dnn.nodeModules.ReduxDevToolsLogMonitor",
        "redux-immutable-state-invariant": "window.dnn.nodeModules.ReduxImmutableStateInvariant",
        "redux-thunk": "window.dnn.nodeModules.ReduxThunk",
        "react-collapse": "window.dnn.nodeModules.ReactCollapse",
        "react-modal": "window.dnn.nodeModules.ReactModal",
        "react-custom-scrollbars": "window.dnn.nodeModules.ReactCustomScrollBars",
        "dnn-button": "dnn.nodeModules.CommonComponents.Button",
        "dnn-checkbox": "dnn.nodeModules.CommonComponents.Checkbox",
        "dnn-collapsible-row": "dnn.nodeModules.CommonComponents.CollapsibleRow",
        "dnn-dropdown": "dnn.nodeModules.CommonComponents.Dropdown",
        "dnn-editable-field": "dnn.nodeModules.CommonComponents.EditableField",
        "dnn-grid-cell": "dnn.nodeModules.CommonComponents.GridCell",
        "dnn-grid-system": "dnn.nodeModules.CommonComponents.GridSystem",
        "dnn-input-group": "dnn.nodeModules.CommonComponents.InputGroup",
        "dnn-label": "dnn.nodeModules.CommonComponents.Label",
        "dnn-multi-line-input": "dnn.nodeModules.CommonComponents.MultiLineInput",
        "dnn-multi-line-input-with-error": "dnn.nodeModules.CommonComponents.MultiLineInputWithError",
        "dnn-persona-bar-page": "dnn.nodeModules.CommonComponents.PersonaBarPage",
        "dnn-radio-buttons": "dnn.nodeModules.CommonComponents.RadioButtons",
        "dnn-search-box": "dnn.nodeModules.CommonComponents.SearchBox",
        "dnn-select": "dnn.nodeModules.CommonComponents.Select",
        "dnn-single-line-input": "dnn.nodeModules.CommonComponents.SingleLineInput",
        "dnn-single-line-input-with-error": "dnn.nodeModules.CommonComponents.SingleLineInputWithError",
        "dnn-social-panel-body": "dnn.nodeModules.CommonComponents.SocialPanelBody",
        "dnn-social-panel-header": "dnn.nodeModules.CommonComponents.SocialPanelHeader",
        "dnn-svg-icons": "dnn.nodeModules.CommonComponents.SvgIcons",
        "dnn-switch": "dnn.nodeModules.CommonComponents.Switch",
        "dnn-tabs": "dnn.nodeModules.CommonComponents.Tabs",
        "dnn-tags": "dnn.nodeModules.CommonComponents.Tags",
        "dnn-text-overflow-wrapper": "dnn.nodeModules.CommonComponents.TextOverflowWrapper",
        "dnn-tooltip": "dnn.nodeModules.CommonComponents.Tooltip",
        "dnn-page-picker": "dnn.nodeModules.CommonComponents.PagePicker"
},

    plugins: isProduction ? [
        new webpack.optimize.UglifyJsPlugin(),
        new webpack.optimize.DedupePlugin(),
        new webpack.DefinePlugin({
            VERSION: JSON.stringify(packageJson.version),
            "process.env": {
                "NODE_ENV": JSON.stringify("production")
            }
        })
    ] : [
            new webpack.DefinePlugin({
                VERSION: JSON.stringify(packageJson.version)
            })
        ]
};