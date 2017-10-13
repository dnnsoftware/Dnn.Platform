/// <binding Clean='clean-bin' />

/*
    Configuration
    Load the project configuration file.
*/
var CONFIG = require('./project.config');

/*
    Paths
    Create all the paths we need ahead of time.
*/
var clientsDir = 'Clients/',
    srcDir = 'src/',
    distDir = 'dist/',
    binDir = 'bin/',
    webModuleDir = `${CONFIG.WEBSITE_PATH}DesktopModules/Cantarus/${CONFIG.MODULE_NAME}/`,
    webBinDir = `${CONFIG.WEBSITE_PATH}bin/`,
    compiledModulesDir = '../../CompiledModules/';

/*
    Modules
    Require the modules we need.
*/
var fs = require('fs'),
    path = require('path'),
    merge = require('merge-stream'),
    gulp = require('gulp'),
    clean = require('gulp-clean'),
    webpack = require('webpack-stream'),
    zip = require('gulp-zip'),
    addsrc = require('gulp-add-src');

/*
    Get Clients
    Gets each of the client folders which exist in the clients directory.
*/
function getClients(dir) {
    return fs.readdirSync(dir)
        .filter(function (file) {
            return fs.statSync(path.join(dir, file)).isDirectory();
        });
}

/*
    Js Entry Point
    Finds a suitable app entry point in the passed directory. Priority is as
    follows:

    - index.js
    - app.js
    - a lone .js file
*/
function getJsEntryPoint(dir) {

    // Get all js files in the directory.
    var jsFiles = fs.readdirSync(dir)
        .filter(function (file) {
            return fs.statSync(path.join(dir, file)).isFile();
        })
        .filter(function (file) {
            return file.toLowerCase().endsWith('.js');
        });

    // Try and find index and app files.
    var indexFile,
        appFile;

    jsFiles.forEach(function (file) {

        var lowerFile = file.toLowerCase();

        if (lowerFile === 'index.js') {
            indexFile = file;
        }

        if (lowerFile === 'app.js') {
            appFile = file;
        }
    });

    // Found an index file?
    if (indexFile) {
        return indexFile;
    }

    // Found an app file?
    if (appFile) {
        return appFile;
    }

    // Single file?
    if (jsFiles.length === 1) {
        return jsFiles[0];
    }

    return undefined;
}

/*
    Css Entry Point
    Finds a suitable styling entry point in the passed directory. Priority is
    as follows:

    - main.(less/sass)
    - a lone less or sass file
*/
function getCssEntryPoint(dir) {

    // Get all less/sass files in the directory.
    var stylingFiles = fs.readdirSync(dir)
        .filter(function (file) {
            return fs.statSync(path.join(dir, file)).isFile();
        })
        .filter(function (file) {
            return file.toLowerCase().endsWith('.less')
                || file.toLowerCase().endsWith('.scss');
        });

    // Get just the main files.
    var mainFiles = fs.readdirSync(dir)
        .filter(function (file) {
            return fs.statSync(path.join(dir, file)).isFile();
        })
        .filter(function (file) {
            return file.toLowerCase().endsWith('.less')
                || file.toLowerCase().endsWith('.scss');
        })
        .filter(function (file) {
            return file.toLowerCase().indexOf('main') > -1;
        });

    // Exactly one main file?
    if (mainFiles.length === 1) {
        return mainFiles[0];
    }

    // Exactly one styling file?
    if (stylingFiles.length === 1) {
        return stylingFiles[0];
    }

    return undefined;
}

/*
    Visual Studio Integration
    These tasks are to provide integration with Visual Studio through pre and
    post build events.
*/
gulp.task('pre-build-Release', []);
gulp.task('post-build-Release', [
    'bundle',
    'build-install'
]);

gulp.task('pre-build-Debug', []);
gulp.task('post-build-Debug', [
    'bundle',
    'copy-clients',
    'copy-module'
]);

gulp.task('pre-build-Clients', []);
gulp.task('post-build-Clients', [
    'bundle',
    'copy-clients'
]);

/*
    Clean Bin
    You can't rely on Visual Studio to clean the bin folder, even when calling
    for the project to be cleaned. This task cleans the bin.
    Doesn't bother reading the directory as that slows the task down.
*/
gulp.task('clean-bin',
    function () {
        return gulp.src(
            binDir,
            {
                read: false
            })
            .pipe(clean());
    }
);

/*
    Clean Dist
    Empty the dist folder of each client.
    Doesn't bother reading the directory as that slows the task down.
*/
gulp.task('clean-dist',
    function () {

        // Get list of clients we have to clean.
        var clients = getClients(clientsDir);

        // Delete dist folder in each client.
        var tasks = clients.map(function (folder) {
            return gulp.src(
                `${clientsDir}${folder}/${distDir}`,
                {
                    read: false
                })
                .pipe(clean());
        });

        return merge(tasks);
    }
);

/*
    Bundle
    Use webpack to bundle all of the resources for each client.

    Task Dependencies:
    - clean-dist, empties the dist directory prior to bundling.
*/
gulp.task('bundle',
    [
        'clean-dist'
    ],
    function () {

        // Get list of clients we want to pass to webpack.
        var clients = getClients(clientsDir);

        // Load the base config.
        var wpConfig = require('./webpack.base.js')();

        // Clear entry points.
        wpConfig.entry = {};

        // Add each client.
        clients.map(function (folder) {

            // Build our client relative paths.
            var clientPath = `./${clientsDir}${folder}/`,
                jsPath = `${clientPath}${srcDir}js/`,
                cssPath = `${clientPath}${srcDir}css/`,
                distPath = `${folder}/${distDir}`;

            // Try find app entry point.
            var appEntryPoint = getJsEntryPoint(jsPath);

            // Do we have one?
            if (appEntryPoint) {

                // Add it.
                wpConfig.entry[`${distPath}${folder}.bundle`] = [
                    'babel-polyfill',
                    `${jsPath}${appEntryPoint}`
                ];
            }

            // Try find css entry point.
            var cssEntryPoint = getCssEntryPoint(cssPath);

            // Do we have one?
            if (cssEntryPoint) {

                // Add it.
                wpConfig.entry[`${distPath}${folder}.styles`] = [
                    `${cssPath}${cssEntryPoint}`
                ];
            }
            
        });

        // Run webpack.
        return webpack(wpConfig)
            .pipe(gulp.dest(clientsDir));
    }
);

/*
    Copy Clients
    For lighter development where we don't want to recycle the application pool
    it's important that the front end clients are copied separately from the
    rest of the module.

    Task Dependencies:
    - bundle, no point copying the front end clients if they doesn't exist.
*/
gulp.task('copy-clients',
    [
        'bundle'
    ],
    function () {
        return gulp.src(
            [
                // Include entire clients directory, except source, less and sass.
                `${clientsDir}*/**`,
                `!${clientsDir}**/${srcDir}**/*.js`,
                `!${clientsDir}**/*.scss`,
                `!${clientsDir}**/*.less`
            ],
            {
                base: '.',
                nodir: true
            })
            .pipe(gulp.dest(webModuleDir));
    }
);

/*
    Copy Module
    All of the module files need to be moved across to the module folder in the
    website.

    Task Dependencies:
    - copy-bin, we need to update the module dll too.
*/
gulp.task('copy-module',
    [
        'copy-bin'
    ],
    function () {
        return gulp.src(
            [
                // Exclude package management.
                '!node_modules/**',
                '!package.json',
                '!packages.config',

                // Exclude build chain.
                '!gulpfile.js',
                '!webpack.base.js',
                '!.babelrc',
                '!project.config.js',

                // Exclude clients.
                '!Clients/**',

                // Include all the standard stuff.
                '**/*.ascx',
                '**/*.asmx',
                '**/*.css',
                '**/*.html',
                '**/*.htm',
                '**/*.resx',
                '**/*.aspx',
                '**/*.js',
                'Images/*.*'
            ],
            {
                base: '.',
                nodir: true
            })
            .pipe(gulp.dest(webModuleDir));
    }
);

/*
    Copy Bin
    Dll files need to be copied to the website bin folder.
*/
gulp.task('copy-bin',
    function () {
        return gulp.src(
            `${binDir}**`,
            {
                base: 'bin'
            })
            .pipe(gulp.dest(webBinDir));
    }
);

/*
    Build Install
    Create a DNN install package for this module.
*/
gulp.task('build-install',
    [
        'bundle'
    ],
    function () {
        return gulp.src(
            [
                // Exclude package management.
                '!node_modules/**',
                '!package.json',
                '!packages.config',

                // Exclude build chain.
                '!gulpfile.js',
                '!webpack.base.js',
                '!.babelrc',
                '!project.config.js',

                // Include entire clients directory, except source, less and sass.
                `${clientsDir}*/**`,
                `!${clientsDir}**/${srcDir}**/*.js`,
                `!${clientsDir}**/*.scss`,
                `!${clientsDir}**/*.less`,

                // Include all the standard stuff.
                '**/*.ascx',
                '**/*.asmx',
                '**/*.css',
                '**/*.html',
                '**/*.htm',
                '**/*.resx',
                '**/*.aspx',
                '**/*.js',
                'Images/*.*'
            ],
            {
                base: '.',
                nodir: true
            })

            // Zip in to resources zip.
            .pipe(zip('Resources.zip'))

            // Add additional install files to stream.
            .pipe(addsrc(
                [
                    // Exclude .pdb file.
                    `!${binDir}**/*.pdb`,

                    `${binDir}**`,
                    'SqlDataProviders/**',
                    '*.dnn',
                    'License.txt'
                ],
                {
                    base: '.',
                    nodir: true
                })
            )

            // Zip in to install zip.
            .pipe(zip(`Cantarus_${CONFIG.MODULE_NAME}_${CONFIG.MODULE_VERSION}_Install.zip`))

            // Move to compiled modules folder.
            .pipe(gulp.dest(compiledModulesDir));
    }
);
