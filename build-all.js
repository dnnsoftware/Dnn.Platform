const path = require("path");
const fs = require("fs");
const child_process = require("child_process");

const root = process.cwd(); 
yarn_build_recursive(root);

// Recurses into a folder
function yarn_build_recursive(folder)
{
    const has_package_json = fs.existsSync(path.join(folder, "package.json"));

    // Abort if there's no `package.json` in this folder and it's not a "packages" folder
    if (!has_package_json && path.basename(folder) !== "packages")
    {
        return;
    }

    // If there is `package.json` in this folder then perform `yarn install`.
    //
    // Skip the root folder, because there is nothing to build in the root folder
    // Hence the `folder !== root` condition.
    //
    if (has_package_json && folder !== root)
    {
        yarn_build(folder);
    }

    // Recurse into subfolders
    for (let subfolder of subfolders(folder))
    {
        yarn_build_recursive(subfolder);
    }
}

function console_log_performingAction(folder)
{
    console.log("===================================================================");
    console.log(`Performing "yarn run prepublishOnly" inside ${folder === root ? "root folder" : "./" + path.relative(root, folder)}`);
    console.log("===================================================================");
}

// Performs `yarn install`
function yarn_build(where)
{
    console_log_performingAction(where);
    child_process.execSync("yarn run prepublishOnly", { cwd: where, env: process.env, stdio: "inherit" });
}

// Lists subfolders in a folder
function subfolders(folder)
{
    return fs.readdirSync(folder)
        .filter(subfolder => fs.statSync(path.join(folder, subfolder)).isDirectory())
        .filter(subfolder => subfolder !== "node_modules" && subfolder[0] !== ".")
        .map(subfolder => path.join(folder, subfolder));
}