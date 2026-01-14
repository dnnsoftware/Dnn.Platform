const { emptyDirSync, copySync } = require('fs-extra');
const path = require('path');

// Paths
const src = path.resolve(__dirname, 'dist');
const dest = path.resolve(__dirname, '../../Dnn.PersonaBar.Extensions/admin/personaBar/Dnn.Styles/scripts/dist');

try {
  console.log(`Cleaning destination directory: ${dest}`);
  emptyDirSync(dest); // Clean the destination directory

  console.log(`Copying files from ${src} to ${dest}...`);
  copySync(src, dest); // Copy the files
  console.log('Clean and copy operation completed successfully!');
} catch (error) {
  console.error('Error during clean and copy operation:', error);
  process.exit(1); // Exit with error code
}