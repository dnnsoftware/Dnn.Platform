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
var binDir = 'bin/Release/',
    compiledModulesDir = '../../CompiledModules/';

/*
    Modules
    Require the modules we need.
*/
var gulp = require('gulp'),
    clean = require('gulp-clean'),
    zip = require('gulp-zip');

/*
    Visual Studio Integration
    These tasks are to provide integration with Visual Studio through pre and
    post build events.
*/
gulp.task('pre-build-Release', [
    'clean-bin'
]);
gulp.task('post-build-Release', [
    'build-release'
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
            `${binDir}*`,
            {
                read: false
            })
            .pipe(clean());
    }
);

/*
    Build Release
    Create a release zip.
*/
gulp.task('build-release',
    function () {
        return gulp.src(
            [
                `${binDir}**`
            ])

            // Zip in to install zip.
            .pipe(zip(`DeployClient_${CONFIG.MODULE_VERSION}.zip`))

            // Move to compiled modules folder.
            .pipe(gulp.dest(compiledModulesDir));
    }
);
