import esbuild from "esbuild";
import * as sass from "sass";
import path from "path";
import fs from "fs";
import chokidar from "chokidar";
import postcss from "postcss";
import autoprefixer from "autoprefixer";
import cssnano from "cssnano";
import settings from "../../settings.local.json";

// Define input and output file types
interface FileConfig {
  input: string;
  output: string;
}

const mode = process.argv.includes("--watch") ? "watch" : "build";

// List of files to process
const files: FileConfig[] = [
  {
    input: "src/styles/default-css/10.0.0/default.scss",
    output:
      mode === "build"
      ? path.relative(process.cwd(), path.resolve("../Website/Resources/Shared/stylesheets/dnndefault/10.0.0/default.css"))
      : path.resolve(settings.WebsitePath, "Resources/Shared/stylesheets/dnndefault/10.0.0/default.css") },
  // { input: "src/scripts/test.ts", output: "dist/scripts/test.js" },
];

// Helper function to ensure directories exist
function ensureDirectoryExists(filePath: string): void {
  const dir = path.dirname(filePath);
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
}

function normalizePath(filePath: string): string {
    return filePath.split(path.sep).join(path.posix.sep);
}

// Compile SCSS to CSS with sourcemaps
async function buildScss(input: string, output: string): Promise<void> {
  try {
    // Read the SCSS file
    const scss = fs.readFileSync(input, "utf-8");
    const result = sass.compileString(
        scss,
        {
            sourceMap: true,
            sourceMapIncludeSources: true,
        }
    );

    const postcssResult = await postcss([
        autoprefixer,
        cssnano,
    ])
    .process(result.css, {
        from: input,
        to: output,
        map: {
            inline: false,
            annotation: `${path.basename(output)}.map`,
            sourcesContent: true,
        },
    });

    ensureDirectoryExists(output);
    const banner = "/*\n* This file is generated. Do not edit it directly.\n* Changes will be overwritten upon upgrades.\n*/\n";
    fs.writeFileSync(output, `${banner}${postcssResult.css}`);
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

// Bundle TypeScript/JavaScript with esbuild
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

// Process all files
async function buildAll(): Promise<void> {
  for (const { input, output } of files) {
    if (input.endsWith(".scss")) {
      await buildScss(input, output);
    } else if (input.endsWith(".ts") || input.endsWith(".js")) {
      await buildJs(input, output);
    }
  }
}

// Watch for changes (optional)
function watchFiles(): void {
  const watcher = chokidar.watch("src", { ignoreInitial: true });

  watcher.on("all", async (event, filePath) => {
    const normalizedFilePath = normalizePath(filePath);
    console.log(`File ${event}: ${normalizedFilePath}`);

    const match = files.find((f) => f.input === normalizedFilePath);
    if (match) {
      if (normalizedFilePath.endsWith(".scss")) {
        await buildScss(match.input, match.output);
      } else if (normalizedFilePath.endsWith(".ts") || normalizedFilePath.endsWith(".js")) {
        await buildJs(match.input, match.output);
      }
    }
  });
}

// Entry point
const args = process.argv.slice(2);
if (args.includes("--watch")) {
  console.log("Watching for file changes...");
  watchFiles();
} else {
  buildAll();
}
