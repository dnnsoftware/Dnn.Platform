/// <binding AfterBuild='clean-msbuild' Clean='clean-bin' />

var promise = require('bluebird'),
    gulp = require('gulp'),
    clean = require('gulp-clean'),
    concatenate = require('gulp-concat'),
    minify = require('gulp-minify'),
    browserify = require('gulp-browserify'),
    less = require('gulp-less'),
    cleanCss = require('gulp-clean-css');

var srcDir = './Angular/src/',
    distDir = './Angular/dist/',
    tempBuildDir = './msbuild-tmp/',
    binDir = './bin',
    moduleDir = '../../Website/DesktopModules/Cantarus/OneSpace/Angular/dist/',
    cssDir = './css/';


/*
    Integration Points
    These tasks provide integration points for Visual Studio but they can also
    be used directly where required.
*/

// Release build configuration tasks.
gulp.task('pre-msbuild-Release', [
        'clean-dist',
        'concatenate',
        'compile-less',
        'browserify',
        'minify',
        'clean-msbuild'
    ],
    function () {
        return gulp.src([
            '!' + tempBuildDir,
            '!./node_modules/**',
            './**/*'
        ])
        .pipe(gulp.dest(tempBuildDir));
    });

gulp.task('post-msbuild-Release', [
    //'clean-msbuild'
]);

// Debug build configuration tasks.
gulp.task('pre-msbuild-Debug', [
        'clean-dist',
        'concatenate',
        'compile-less',
        'browserify',
        'clean-msbuild'
    ],
    function () {
        return gulp.src([
            '!' + tempBuildDir,
            '!./node_modules/**',
            './**/*'
        ])
        .pipe(gulp.dest(tempBuildDir));
    });

gulp.task('post-msbuild-Debug', [
    //'clean-msbuild'
]);

// Angular build configuration tasks.
gulp.task('pre-msbuild-Angular', [
        'clean-dist',
        'concatenate',
        'compile-less',
        'browserify'
    ],
    function () {
        return gulp.src([
            distDir + '*'
        ])
        .pipe(gulp.dest(moduleDir));
    });

gulp.task('post-msbuild-Angular', function () { });

// Cleans up temporary build folder.
// Gets rid of the temporary build folder created for MSBuild.
gulp.task('clean-msbuild', function () {
    return gulp.src(
        tempBuildDir,
        {
            read: false
        }
    )
    .pipe(clean());
});

// Builds the Angular application.
// See the list of tasks which this task executes for explanation.
gulp.task('build', [
    'clean-dist',
    'concatenate',
    'browserify',
    'minify',
    'clean-msbuild'
]);

// Cleans the bin folder.
// You can't rely on Visual Studio to clean the bin folder, even when calling
// for the project to be cleaned. This task cleans the bin.
// Doesn't bother reading the directory as that slows the task down.
gulp.task('clean-bin', function () {
    return gulp.src(
        binDir,
        {
            read: false
        }
    )
    .pipe(clean());
});


/*
    Tasks
    Individual tasks.
*/

// Cleans the distribution folder.
// Doesn't bother reading the directory as that slows the task down.
gulp.task('clean-dist', function () {
    return gulp.src(
        distDir + '**/*',
        {
            read: false
        }
    )
    .pipe(clean());
});

// Concatenates all the application source files in to the dist folder.
gulp.task('concatenate', ['clean-dist'], function () {
    return promise.all([
        new promise(function (resolve) {
            gulp.src([
                '!' + srcDir + '**/bundle.js',
                srcDir + 'app.js',
                srcDir + '**/*.js'
            ])
            .pipe(concatenate('poly-deploy.js'))
            .pipe(gulp.dest(distDir))
            .on('end', resolve);
        })
    ]);
});

// Compiles the LESS files into agent.css and student.css
gulp.task('compile-less', [], function () {
    return promise.all([
        new promise(function (resolve) {
            gulp.src([
                cssDir + '**/*.less'
            ])
            .pipe(less({
                relativeUrls: true
            }))
            .pipe(concatenate('poly-deploy.css'))
            .pipe(gulp.dest(cssDir))
            .on('end', resolve);
        })
    ]);
});

// Bundles up dependencies for each module angular module.
// This currently feels a bit messy because of the use of promises, maybe it
// would be better split in to two separate tasks?
gulp.task('browserify', ['clean-dist'], function () {
    return promise.all([
        new promise(function (resolve) {
            gulp.src([
                srcDir + 'bundle.js'
            ])
            .pipe(concatenate('poly-deploy.bundle.js'))
            .pipe(browserify({
                insertGlobals: false
            }))
            .pipe(gulp.dest(distDir))
            .on('end', resolve);
        })
    ]);
});

// Minifies the javascript and CSS.
// Overwrites the source so there is no need to change betwene src and min.
gulp.task('minify', ['compile-less', 'concatenate', 'browserify'], function () {
    return promise.all([
        new promise(function (resolve) {
            gulp.src([
                distDir + '**/*.js'
            ])
            .pipe(minify({
                ext: {
                    src: '.js',
                    min: '.min.js'
                },
                noSource: true
            }))
            .pipe(gulp.dest(distDir))
            .on('end', resolve);
        }),
        new promise(function (resolve) {
            gulp.src([
                cssDir + '**/*.css'
            ])
            .pipe(concatenate('poly-deploy.min.css'))
            .pipe(cleanCss())
            .pipe(gulp.dest(cssDir))
            .on('end', resolve);
        })
    ]);
});
