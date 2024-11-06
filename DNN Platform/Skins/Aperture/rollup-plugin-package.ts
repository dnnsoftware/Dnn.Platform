import { Plugin } from 'rollup';
import * as fs from 'fs';
import * as path from 'path';
import { glob } from 'glob';
import { Zip } from 'zip-lib';

interface RollupPluginDnnPackageOptions
{
    name: string;
    version: string;
    destinationDirectory: string;
};

type RollupPluginDnnPackage = (dnnPackageOptions: RollupPluginDnnPackageOptions) => Plugin;

function ensureEmptyDirectory(dirPath: string): void {
    // Ensure that the directory exists
    if (!fs.existsSync(dirPath)) {
        // Directory does not exist, create it
        fs.mkdirSync(dirPath, { recursive: true });
        return;
    }

    // Directory exists, empty it
    const files = fs.readdirSync(dirPath);

    for (const file of files) {
        const currentPath = path.join(dirPath, file);
        if (fs.lstatSync(currentPath).isDirectory()) {
            // Recursive call for directories
            ensureEmptyDirectory(currentPath);
            // After emptying the subdirectory, remove it
            deleteDirectoryWithRetry(currentPath);
        } else {
            // Delete file
            fs.unlinkSync(currentPath);
        }
    }
}

function copyFileToPath(src: string, dest: string): void {
    // Ensure that the destination directory exists
    const dir = path.dirname(dest);
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }

    // Now that we know the directory exists, copy the file
    fs.copyFileSync(src, dest);
}

function deleteDirectoryWithRetry(path: string, retries = 5, delay = 1000) {
    try {
        fs.rmSync(path, { recursive: true, force: true });
    } catch (err: any) {
        if (retries > 0 && err.code === 'EBUSY') {
            setTimeout(() => deleteDirectoryWithRetry(path, retries - 1, delay), delay);
        } else {
            throw err; // Rethrow the error if retries are exhausted or it's another error
        }
    }
}

const dnnPackage: RollupPluginDnnPackage = (dnnPackageOptions) =>
{
    return {
        name: 'rollup-plugin-dnn-package',
        async writeBundle(options, _bundle)
        {
            const skinDist = options.dir as string;
            const containersDist = skinDist.replace('/Skins/', '/Containers/');
            const artifactsDir = "./artifacts";
            ensureEmptyDirectory(artifactsDir);
            const stagingDir = `${artifactsDir}/staging`;
            ensureEmptyDirectory(stagingDir);
            
            // Skin resources
            var skinResources = await glob(
                [
                    `${skinDist}/css/**/*`,
                    `${skinDist}/fonts/**/*`,
                    `${skinDist}/js/**/*`,
                    `${skinDist}/menus/**/*`,
                    `${skinDist}/patials/**/*`,
                    `${skinDist}/**/*.ascx`,
                    `${skinDist}/**/*.xml`,
                    `${skinDist}/**/*.png`,
                ],
                { nodir: true }
            );
            skinResources.forEach((skinResource) => {
                const relativePath = path.relative(skinDist, skinResource).replace(/\\/g, '/');
                const targetPath = path.resolve(`${stagingDir}/skinResources/${relativePath}`);
                copyFileToPath(skinResource, targetPath);
            });
            let zip = new Zip();
            zip.addFolder(`${stagingDir}/skinResources`);
            await zip.archive(`${stagingDir}/skin.zip`)
            deleteDirectoryWithRetry(`${stagingDir}/skinResources`);

            // Container resources
            var containerResources = await glob(
                [
                    `${containersDist}/**/*`,
                ],
                { nodir: true }
            );
            containerResources.forEach((containerResource) => {
                const relativePath = path.relative(containersDist, containerResource).replace(/\\/g, '/');
                const targetPath = path.resolve(`${stagingDir}/containerResources/${relativePath}`);
                copyFileToPath(containerResource, targetPath);
            });
            zip = new Zip();
            zip.addFolder(`${stagingDir}/containerResources`);
            await zip.archive(`${stagingDir}/container.zip`);
            deleteDirectoryWithRetry(`${stagingDir}/containerResources`);

            // Root files
            var rootResources = await glob(
                [
                    `${skinDist}/*.png`,
                    `${skinDist}/LICENSE`,
                    `${skinDist}/*.txt`,
                    `${skinDist}/*.dnn`,
                ],
                { nodir: true }
            );
            rootResources.forEach((rootResource) => {
                const relativePath = path.relative(skinDist, rootResource).replace(/\\/g, '/');
                const targetPath = path.resolve(`${stagingDir}/${relativePath}`);
                copyFileToPath(rootResource, targetPath);
            });

            // Package ZIP
            zip = new Zip();
            zip.addFolder(stagingDir);
            var packageName = `${dnnPackageOptions.name}_${dnnPackageOptions.version}_install.zip`;
            var packagePath = `${artifactsDir}/${packageName}`;
            await zip.archive(packagePath);
            deleteDirectoryWithRetry(stagingDir);

            var skinInstallPath = `${path.resolve(dnnPackageOptions.destinationDirectory)}`;
            console.log(`Copying ${packageName} to ${skinInstallPath}`);
            fs.copyFileSync(
                packagePath,
                `${skinInstallPath}/${packageName}`);
            deleteDirectoryWithRetry(artifactsDir);
        },
    };
}

export default dnnPackage;