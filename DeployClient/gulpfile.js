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
gulp.task('pre-build-Release', cleanBin);
gulp.task('post-build-Release', buildRelease);

gulp.task('pre-build-Debug', noOp);
gulp.task('post-build-Debug', noOp);

gulp.task('pre-build-Clients', noOp);
gulp.task('post-build-Clients', noOp);

/*
    Clean Bin
    You can't rely on Visual Studio to clean the bin folder, even when calling
    for the project to be cleaned. This task cleans the bin.
    Doesn't bother reading the directory as that slows the task down.
*/
function cleanBin() {
    return gulp.src(
        `${binDir}*`,
        {
            read: false
        })
        .pipe(clean());
}

/*
    Build Release
    Create a release zip.
*/
function buildRelease() {
    return gulp.src(
        [
            `${binDir}**`
        ])

        // Zip in to install zip.
        .pipe(zip(`DeployClient_${CONFIG.MODULE_VERSION}.zip`))

        // Move to compiled modules folder.
        .pipe(gulp.dest(compiledModulesDir));
}

function noOp(done) {
    done();
}