# This script should only be run when releasing a new version to npm from VSTS.
# Bump the version and publish
npm version patch -m "Releasing version %s of the DNN React Common Library"
npm publish --access public