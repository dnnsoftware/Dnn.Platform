const fs = require('fs');
const path = require('path');
const proc = require("child_process");

const skipList = ['GlobalStyles', 'node_modules'];


const isProjectFolder = (cwd) => {
    const fullpath = path.resolve(cwd);
    const pkg = `${fullpath}\\package.json`;
    return fs.existsSync(pkg);
};

const cleanup = (items) => {
    const promises = [];
    items.filter((item) => !skipList.includes(item)).forEach((item) => {
        const cwd = `./${item}`;
        const stats = fs.lstatSync(cwd);

        if (stats.isDirectory() && isProjectFolder(cwd)) {

            let out = proc.execSync("rimraf node_modules lib yarn.lock package-lock.json", {cwd, shell: true});
            console.log(`Cleanup folder ${cwd}: ${out.toString()}`);
        }
    });
};

fs.readdir(".", function (err, items) {
    cleanup(items);
});