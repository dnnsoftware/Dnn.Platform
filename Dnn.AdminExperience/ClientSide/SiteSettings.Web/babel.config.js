// Used by Jest only (babel-jest); the production bundle is built by rsbuild/SWC.
module.exports = {
    env: {
        test: {
            presets: [["@babel/preset-env", { targets: { node: "current" } }]],
        },
    },
};
