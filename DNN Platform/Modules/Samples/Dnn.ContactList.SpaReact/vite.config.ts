import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';
import { readFileSync } from 'fs';

// Read settings file
let settings: { WebsitePath?: string } = {};
try {
  const settingsPath = resolve(__dirname, '../../../../settings.local.json');
  const settingsContent = readFileSync(settingsPath, 'utf-8');
  settings = JSON.parse(settingsContent);
} catch (err) {
  // settings.local.json is optional
  console.warn('settings.local.json not found, using defaults');
}


export default defineConfig(({ mode }) => {
  // For development/watch mode, you can set a custom path to your local DNN installation
  // For production builds, output to the scripts folder for packaging
  const isDevelopment = mode === 'development';
  const outDir = isDevelopment
    ? settings.WebsitePath
      ? `${settings.WebsitePath}/DesktopModules/Dnn/ContactListSpaReact/scripts`
      : 'scripts'
    : 'scripts';

  return {
    plugins: [react()],
    define: {
      'process.env': JSON.stringify({
        NODE_ENV: mode === 'development' ? 'development' : 'production'
      }),
      'process.env.NODE_ENV': JSON.stringify(mode === 'development' ? 'development' : 'production'),
      'global': 'globalThis'
    },
    build: {
      outDir,
      emptyOutDir: false,
      sourcemap: isDevelopment,
      minify: !isDevelopment,
      lib: {
        entry: resolve(__dirname, 'src/main.tsx'),
        name: 'ContactList',
        formats: ['iife'],
        fileName: () => 'contact-list.js'
      },
      rollupOptions: {
        external: [],
        output: {
          inlineDynamicImports: true,
          globals: {},
          intro: `var process = { env: { NODE_ENV: '${mode === 'development' ? 'development' : 'production'}' } };`
        }
      },
      watch: isDevelopment ? {} : null
    }
  };
});

