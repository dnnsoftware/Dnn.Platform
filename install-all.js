const path = require("path");
const fs = require("fs");
const child_process = require("child_process");

const root = process.cwd(); 
yarn_install_innerDependencies();
yarn_install_recursive(root);

// Since this script is intended to be run as a "preinstall" command,
// it will do `yarn install` automatically inside the root folder in the end.
console.log("===================================================================");
console.log("Performing \"yarn install\" inside root folder");
console.log("===================================================================");

// Recurses into a folder
function yarn_install_recursive(folder)
{
    const has_package_json = fs.existsSync(path.join(folder, "package.json"));

    // Abort if there's no `package.json` in this folder and it's not a "packages" folder
    if (!has_package_json && path.basename(folder) !== "packages")
    {
        return;
    }

    // If there is `package.json` in this folder then perform `yarn install`.
    //
    // Since this script is intended to be run as a "preinstall" command,
    // skip the root folder, because it will be `yarn install`ed in the end.
    // Hence the `folder !== root` condition.
    //
    if (has_package_json && folder !== root)
    {
        yarn_install(folder);
    }

    // Recurse into subfolders
    for (let subfolder of subfolders(folder))
    {
        yarn_install_recursive(subfolder);
    }
}

function console_log_performingInstall(folder)
{
    console.log("===================================================================");
    console.log(`Performing "yarn install" inside ${folder === root ? "root folder" : "./" + path.relative(root, folder)}`);
    console.log("===================================================================");
}

// Performs `yarn install`for all the inner dependencies
function yarn_install_innerDependencies()
{
    yarn_install(path.resolve("EslintConfigDnn"));
    yarn_install(path.resolve("GlobalStyles"));
    yarn_install(path.resolve("SvgIcons"));
    yarn_install(path.resolve("Tooltip"));
    yarn_install(path.resolve("Label"));
}

// Performs `yarn install`
function yarn_install(where)
{
    console_log_performingInstall(where);
    child_process.execSync("yarn install", { cwd: where, env: process.env, stdio: "inherit" });
}

// Lists subfolders in a folder
function subfolders(folder)
{
    return fs.readdirSync(folder)
        .filter(subfolder => fs.statSync(path.join(folder, subfolder)).isDirectory())
        .filter(subfolder => subfolder !== "node_modules" && subfolder[0] !== ".")
        .map(subfolder => path.join(folder, subfolder));
}