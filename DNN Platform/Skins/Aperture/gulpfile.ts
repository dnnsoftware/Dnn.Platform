import * as gulp from 'gulp';

const 
    autoprefixer  = require('gulp-autoprefixer'),
    browserify    = require('browserify'),
    bs            = require('browser-sync').create(),
    clean         = require('gulp-clean'),
    cleanCSS      = require('gulp-clean-css'),
    localSettings = require('../../../settings.local.json'),
    log           = require('fancy-log'),
    packageJson   = require('./package.json'),
    rename        = require('gulp-rename'),
    sourcemaps    = require('gulp-sourcemaps'),
    sass          = require('gulp-sass')(require('sass')),
    tsify         = require('tsify'),
    uglify        = require('gulp-uglify'),
    buffer        = require('vinyl-buffer'),
    source        = require('vinyl-source-stream'),
    zip           = require('gulp-zip');

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const dist = localSettings.WebsitePath + '/Portals/_default/Skins/' + packageName;

const paths = {
  fonts: {
    src: './src/fonts/*',
    dest: dist + '/fonts/'
  },
  normalize: {
    src: '../../../node_modules/normalize.css/normalize.css',
    dest: dist + '/css/'
  },
  images: {
    src: './src/images/**/*.{jpg,jpeg,png,gif,svg,webp}',
    dest: dist + '/images/'
  },
  rootImages: {
    src: './*.{jpg,jpeg,png,gif,svg,webp}',
    dest: dist + '/'
  },
  styles: {
    src: './src/scss/**/*.scss',
    dest: dist + '/css/'
  },
  scripts: {
    src: './src/js/*.js',
    dest: dist + '/js/'
  },
  layouts: {
    src: ['./**/*.{ascx,xml,cshtml}', '!**/containers/**/*'],
    dest: dist + '/'
  },
  txtFiles: {
    src: ['./*.txt', './LICENSE'],
    dest: dist + '/'
  },
  containers: {
    src: './containers/*',
    dest: dist + '/../../Containers/' + packageName + '/'
  },
  manifest: {
    src: './manifest.dnn',
    dest: dist,
  },
  zipSkin: {
    src: [dist + '/**/*', '!' + dist + '/*.{dnn,png,txt}', '!' + dist + '!/LICENSE'],
    zipfile: 'skin.zip',
    dest: './temp/'
  },
  zipContainers: {
    src: './containers/**/*',
    zipfile: 'container.zip',
    dest: './temp/'
  },
  zipPackage: {
    src: ['./temp/*.zip','*.{dnn,png,txt}', 'LICENSE'],
    zipfile: packageName + '\_' + packageJson.version + '\_install.zip',
    dest: '../../../Website/Install/Skin/'
  },
  cleanTemp: {
    src: './temp/',
  },
  cleanDist: {
    src: dist,
  }
};
    

/*------------------------------------------------------*/
/* INIT TASKS ------------------------------------------*/
/*------------------------------------------------------*/
// Copy fonts from src/fonts to dist/fonts
function fontsInit() {
  let nSrc=0;
  return gulp.src(paths.fonts.src)
    .pipe(gulp.dest(paths.fonts.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'font files distributed!');
    })
}

// Compile normalize.css from node_modules and copy to dist/js
function normalizeInit() {
  let nSrc=0;
  return gulp.src(paths.normalize.src, { sourcemaps: true })
  .pipe(sass({outputStyle: 'compressed'}).on('error', sass.logError))
  .pipe(cleanCSS())
  .pipe(rename({suffix: '.min'}))
  .pipe(autoprefixer())
  .pipe(gulp.dest(paths.normalize.dest, { sourcemaps: '.' }))
  .on('data', function() { nSrc+=1; })
  .on('end', function() {
    log(nSrc, 'normalize files distributed!');
  })
}

/*------------------------------------------------------*/
/* END INIT TASKS --------------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* IMAGE TASKS -----------------------------------------*/
/*------------------------------------------------------*/
// Copy root images to dist
function rootImages() {
  let nSrc=0;
  return gulp.src(paths.rootImages.src, {since: gulp.lastRun(images)})
		.pipe(gulp.dest(paths.rootImages.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'root images distributed!');
    })
}

// Copy images to dist/images
function images() {
  let nSrc=0;
  return gulp.src(paths.images.src, {since: gulp.lastRun(images)})
		.pipe(gulp.dest(paths.images.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'images distributed!');
    })
}
/*------------------------------------------------------*/
/* END IMAGE TASKS -------------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* STYLES TASKS ----------------------------------------*/
/*------------------------------------------------------*/
// Compile custom SCSS to CSS and copy to dist/css
function styles() {
  return gulp.src(paths.styles.src, { sourcemaps: true })
  .pipe(sass({includePaths: ['./node_modules']},{outputStyle: 'compressed'}).on('error', sass.logError))
  .pipe(cleanCSS())
  .pipe(rename({suffix: '.min'}))
  .pipe(autoprefixer())
  .pipe(gulp.dest(paths.styles.dest, { sourcemaps: '.' }))
  .on('end', function() {
    log('SCSS compiled, minified, and distributed!');
  })
}
/*------------------------------------------------------*/
/* END STYLES TASKS ------------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* SCRIPTS TASKS ---------------------------------------*/
/*------------------------------------------------------*/
// Compile custom JS and copy to dist/js
function scripts() {
  return browserify({
    basedir: '.',
    debug: true,
    entries: ['./src/scripts/main.ts'],
    cache: {},
    packageCache: {}
  })
  .plugin(tsify)
  .transform('babelify', {
      presets: ['env'],
      extensions: ['.ts']
  })
  .bundle()
  .pipe(source('skin.min.js'))
  .pipe(buffer())
  .pipe(sourcemaps.init({loadMaps: true}))
  .pipe(uglify())
  .pipe(sourcemaps.write('./'))
  .pipe(gulp.dest(paths.scripts.dest))
  .on('end', function() {
    log('scripts transpiled and distributed!');
  })
}
/*------------------------------------------------------*/
/* END SCRIPTS TASKS -----------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* DNN TASKS -------------------------------------------*/
/*------------------------------------------------------*/
// Copy containers to proper DNN theme containers folder
function containers() {
  let nSrc=0;
  return gulp.src(paths.containers.src)
    .pipe(gulp.dest(paths.containers.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'container files distributed!');
    })
}

// Copy layout files to dist
function layouts() {
  let nSrc=0;
  return gulp.src(paths.layouts.src, {since: gulp.lastRun(layouts)})
		.pipe(gulp.dest(paths.layouts.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'layouts distributed!');
    })
}

// Copy txt files to dist
function txtFiles() {
  let nSrc=0;
  return gulp.src(paths.txtFiles.src, {since: gulp.lastRun(txtFiles)})
		.pipe(gulp.dest(paths.txtFiles.dest))
    .on('data', function() { nSrc+=1; })
    .on('end', function() {
      log(nSrc, 'txt files distributed!');
    })
}

// Update manifest.dnn
function manifest() {
  return gulp.src(paths.manifest.src)
    .pipe(gulp.dest(paths.manifest.dest))
    .on('end', function() {
      log('DNN manifest updated!');
    })
}
/*------------------------------------------------------*/
/* END DNN TASKS ---------------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* MAINTENANCE TASKS -----------------------------------*/
/*------------------------------------------------------*/
// Clean up dist folder
function cleanDist() {
  return gulp.src(paths.cleanDist.src, { allowEmpty: true })
    .pipe(clean({force: true}))
    .on('end', function() {
      log('dist folder cleaned up!');
    })
}
/*------------------------------------------------------*/
/* END MAINTENANCE TASKS -------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* PACKAGING TASKS -------------------------------------*/
/*------------------------------------------------------*/
// ZIP contents of skin folder
function zipSkin() {
  return gulp.src(paths.zipSkin.src)
    .pipe(zip(paths.zipSkin.zipfile))
    .pipe(gulp.dest(paths.zipSkin.dest))
    .on('end', function() {
      log('zipSkin temporarily created!');
    })
}

// ZIP contents of containers folder
function zipContainers() {
  return gulp.src(paths.zipContainers.src)
    .pipe(zip(paths.zipContainers.zipfile))
    .pipe(gulp.dest(paths.zipContainers.dest))
    .on('end', function() {
      log('zipContainers temporarily created!');
    })
}

// git zipTemp
const zipTemp = gulp.series(zipSkin, zipContainers);

// Assemble files into DNN theme install package
function zipPackage() { 
  return gulp.src(paths.zipPackage.src)
    .pipe(zip(paths.zipPackage.zipfile))
    .pipe(gulp.dest(paths.zipPackage.dest))
    .on('end', function() {
      log('Theme install package created!');
    })
}

// Clean temp folder
function cleanTemp() {
  return gulp.src(paths.cleanTemp.src, { allowEmpty: true })
    .pipe(clean())
    .on('end', function() {
      log('temp folder cleaned up!');
    })
}
/*------------------------------------------------------*/
/* END PACKAGING TASKS ---------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* DEV TASKS -------------------------------------------*/
/*------------------------------------------------------*/
//gulp serve
function serve() {
  bs.init({
      proxy: "dnn.loc"
  });
  gulp.watch(paths.images.src, images).on('change', bs.reload);
  gulp.watch(paths.styles.src, styles).on('change', bs.reload);
  gulp.watch(paths.scripts.src, scripts).on('change', bs.reload);
  gulp.watch(paths.containers.src, containers).on('change', bs.reload);
}

// gulp watch
function watch() {
  gulp.watch(paths.images.src, images);
  gulp.watch(paths.styles.src, styles);
  gulp.watch(paths.scripts.src, scripts);
  gulp.watch(paths.containers.src, containers);
}

// gulp init
const init = gulp.series(fontsInit, normalizeInit);

// gulp build
const build = gulp.series(cleanDist, init, styles, scripts, rootImages, images, layouts, txtFiles, containers, manifest);

// gulp packageTheme
const packageTheme = gulp.series(cleanTemp, build, zipTemp, zipPackage, cleanTemp);
/*------------------------------------------------------*/
/* END DEV TASKS ---------------------------------------*/
/*------------------------------------------------------*/


/*------------------------------------------------------*/
/* EXPORT TASKS ----------------------------------------*/
/*------------------------------------------------------*/
// You can use CommonJS `exports` module notation to declare tasks
exports.fontsInit = fontsInit;
exports.normalizeInit = normalizeInit;
exports.rootImages = rootImages;
exports.images = images;
exports.styles = styles;
exports.scripts = scripts;
exports.layouts = layouts;
exports.txtFiles = txtFiles;
exports.containers = containers;
exports.manifest = manifest;
exports.cleanDist = cleanDist;
exports.zipSkin = zipSkin;
exports.zipContainers = zipContainers;
exports.zipTemp = zipTemp;
exports.zipPackage = zipPackage;
exports.cleanTemp = cleanTemp;
exports.serve = serve;
exports.watch = watch;
exports.init = init;
exports.build = build;
exports.packageTheme = packageTheme;

// Define default task that can be called by just running `gulp` from cli
exports.default = build;
/*------------------------------------------------------*/
/* END EXPORT TASKS ------------------------------------*/
/*------------------------------------------------------*/
