import esbuild from "esbuild";
import path from "path";
import settings from "../../../settings.local.json";
import fs from "fs";
import sass from "sass";
import postcss from "postcss";
import postcssImport from "postcss-import";
import autoprefixer from "autoprefixer";
import cssnano from "cssnano";
import postcssBanner from "postcss-banner";
import chokidar from "chokidar";
import bs from "browser-sync";
import packageJson from "./package.json";
import { glob, globSync } from "glob";
import { Zip } from 'zip-lib';


const browserSync = bs.create("Aperture");
const mode = process.argv.includes("--watch") ? "watch" : "build";
const deploy = settings.WebsitePath.length > 0;

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const tempDir = "./temp";
const containersDist = deploy
  ? settings.WebsitePath + '/Portals/_default/Containers/' + packageName
  : `${tempDir}/Containers`;
const skinDist = deploy
  ? settings.WebsitePath + '/Portals/_default/Skins/' + packageName
  : `${tempDir}/Skin`;

/** Configuration for a file that needs transpiling. */
interface TranspiledFileConfig {
    input: string;
    output: string;
}

/** Configuration for a static file that needs to be copied. */
interface StaticFileConfig {
  /** Glob pattern for files to be copied. */
  src: string;
  /** Destination Folder. */
  dest: string;
}

const transpiledFiles: TranspiledFileConfig[] = [
    {
        input: "src/scripts/main.ts",
        output: path.resolve(skinDist, "js/skin.min.js"),
    },
    {
        input: "src/scss/style.scss",
        output: path.resolve(skinDist, "css/skin.min.css"),
    },
];

const copyFiles: StaticFileConfig[] = [
  { src: "containers/*", dest: containersDist },
  { src: "menus/desktop/*", dest: skinDist + "/menus/desktop" },
  { src: "menus/footer/*", dest: skinDist + "/menus/footer" },
  { src: "menus/mobile/*", dest: skinDist + "/menus/mobile" },
  { src: "partials/*", dest: skinDist + "/partials" },
  { src: "src/fonts/*", dest: skinDist + "/fonts" },
  { src: "src/images/*", dest: skinDist + "/images" },
  { src: "*.{ascx,png,dnn,xml,txt}", dest: skinDist },
];

/** Normalizes a path (windows vs linux, etc.) */
function normalizePath(filePath: string): string {
  return path.resolve(filePath).replace(/\\/g, "/");
}

/** Copies files while preserving folder structure */
function copyFilesPreservingStructure(copyConfig: StaticFileConfig[]): void {
  copyConfig.forEach(entry => {
    console.log(`Copying files from ${entry.src} to ${entry.dest}...`);
    
    globSync(entry.src).forEach(file => {
        const fileName = path.basename(file);
        const destFile = path.join(entry.dest, fileName);
        ensureDirectoryExists(destFile);
        fs.copyFileSync(file, destFile);
        console.log(`Copied ${file} → ${destFile}`);
    });
  });
}

/** Helper function to ensure directories exist */
function ensureDirectoryExists(filePath: string): void {
  console.log("Creating directory: ", filePath);
  const dir = path.dirname(filePath);
  console.log("Directory: ", dir);
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

/** Compile SCSS to CSS with sourcemaps */
async function buildScss(input: string, output: string): Promise<void> {
  try {
    const result = sass.compile(
        input,
        {
          sourceMap: true,
          sourceMapIncludeSources: true,
        }
    );

    const cssWithBanner = result.css;
    const postcssResult = await postcss([
        postcssImport(),
        autoprefixer,
        cssnano,
        postcssBanner({
          banner: "This file is generated.\nDo not edit it directly.\nChanges will be overwritten upon upgrades.\n",
          important: true,
        })
    ])
    .process(cssWithBanner, {
        from: normalizePath(input),
        to: normalizePath(output),
        map: {
            inline: false,
            annotation: `${path.basename(output)}.map`,
            prev: result.sourceMap,
            sourcesContent: true,
        },
    });

    ensureDirectoryExists(output);
    console.log(`Writing CSS to: ${output}`);
    fs.writeFileSync(output, postcssResult.css);
    console.log(`CSS written to: ${output}`);

    if (postcssResult.map) {
        const sourcemapPath = `${output}.map`;
        fs.writeFileSync(sourcemapPath, JSON.stringify(postcssResult.map));
        console.log(`Sourcemap written to: ${sourcemapPath}`);
    }
  } catch (error) {
    console.error(`Error compiling SCSS for ${input}:`, error);
  }
}
  
/** Bundle TypeScript/JavaScript with esbuild */
async function buildJs(input: string, output: string): Promise<void> {
  try {
    ensureDirectoryExists(output);
    await esbuild.build({
      entryPoints: [input],
      outfile: output,
      bundle: true,
      sourcemap: true,
      format: "iife",
      target: "es2018",
      minify: true,
    });
    console.log(`JS written to: ${output}`);
  } catch (error) {
    console.error(`Error bundling JS for ${input}:`, error);
  }
}

/** Process all files */
async function buildAll(firstRun = true): Promise<void> {
  for (const { input, output } of transpiledFiles) {
      if (input.endsWith(".scss")) {
          await buildScss(input, output);
      } else if (input.endsWith(".ts") || input.endsWith(".js")) {
          await buildJs(input, output);
      }
  }

  if (firstRun){
    copyFilesPreservingStructure(copyFiles);
  }
}


/** Watch for changes (optional) */
function watchFiles(): void {
  const watcher = chokidar.watch([
    ...globSync(['*.ascx', '*.txt', '*.png']),
    "./src",
    "./containers",
    "./menus",
    "./partials",
  ],
  {
    ignored: /(^|[\/\\])\../, // Ignore dotfiles
    persistent: true,
    ignoreInitial: true,
  });
  buildAll();
  if (!browserSync.active) {
    browserSync.init({
      proxy: settings.WebsiteUrl,
      rewriteRules: [
          {
              match: /w\[".*"].*/g,
              fn: (req, _res, match: string) => {
                  return match.replace(/(http:\/\/|https:\/\/)[a-zA-Z0-9.-]+\//g, `//${req.headers.host}/`);
              }
          },
      ],
      files: [
          skinDist + '/**/*',
          containersDist + '/**/*',
      ],
      injectChanges: true,
      middleware: function (_req, res, next) {
        res.setHeader("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
        res.setHeader("Pragma", "no-cache");
        res.setHeader("Expires", "0");
        next();
      }
    });
  }
  watcher.on("all", async (event, filePath) => {
      console.log(`File ${filePath} changed (${event}). Rebuilding...`);
      await buildAll(false);
      const isStaticFile = copyFiles.some(entry => globSync(entry.src).includes(filePath));
      if (isStaticFile) {
        copyFilesPreservingStructure(copyFiles);
      }
      browserSync.reload();
  });
}

function deleteDirectoryWithRetry(path: string, retries = 5, delay = 1000) {
  try {
      fs.rmSync(path, { recursive: true, force: true });
  } catch (err: any) {
      if (retries > 0 && err.code === 'EBUSY') {
          setTimeout(() => deleteDirectoryWithRetry(path, retries - 1, delay + 1000), delay);
      } else {
          throw err; // Rethrow the error if retries are exhausted or it's another error
      }
  }
}

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

/** Copies a file to a certain path. */
function copyFileToPath(src: string, dest: string): void {
  // Ensure that the destination directory exists
  const dir = path.dirname(dest);
  if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
  }

  // Now that we know the directory exists, copy the file
  fs.copyFileSync(src, dest);
}

/** Package files */
async function packageFiles(): Promise<void> {
  const artifactsDir = "./artifacts";
  ensureEmptyDirectory(artifactsDir);
  const stagingDir = `${artifactsDir}/staging`;
  ensureEmptyDirectory(stagingDir);
  
  // Skin resources
  console.log("Packaging skin resources...");
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
  console.log("Skin resources packaged.");

  // Container resources
  console.log("Packaging container resources...");
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
  console.log("Containers resources packaged.");

  // Root files
  console.log("Packaging root files...");
  var rootResources = await glob(
      [
          `${skinDist}/*.png`,
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
  console.log("Root files packaged.");

  // Package ZIP
  console.log("Creating install package...");
  zip = new Zip();
  zip.addFolder(stagingDir);
  var packageName = `Aperture_${packageJson.version}_install.zip`;
  var packagePath = `${artifactsDir}/${packageName}`;
  await zip.archive(packagePath);
  deleteDirectoryWithRetry(stagingDir);
  console.log("Install package created.");

  // Copy package to Install/Skin
  console.log("Copying package to Install/Skin...");
  var skinInstallPath = "../../../Website/Install/Skin/";
  console.log(`Copying ${packageName} to ${skinInstallPath}`);
  var destinationPath = `${skinInstallPath}/${packageName}`
  ensureDirectoryExists(destinationPath);
  fs.copyFileSync(
      packagePath,
      destinationPath);
  deleteDirectoryWithRetry(artifactsDir);
  console.log(`Package copied to ${skinInstallPath}/${packageName}.`);
}

// THE FUN STARTS HERE - MAIN ENTRY POINT
deleteDirectoryWithRetry(tempDir);
if (mode === "watch") {
    console.log("Watching for changes...");
    watchFiles();
}
else{
    console.log("Building...");
    buildAll()
    .then(() => {
      console.log("Build complete, packaging...");
      packageFiles()
      .then(() => {
        console.log("Packaging complete.");
      });
    });
}
