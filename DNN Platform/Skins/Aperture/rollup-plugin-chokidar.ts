import chokidar from 'chokidar';
import { Plugin } from 'rollup';

interface ChokidarWatchOptions {
    paths: string | string[];
    onChange: () => void;
}

export default function chokidarWatch(options: ChokidarWatchOptions): Plugin {
    return {
        name: 'chokidar-watch',
        buildStart() {
            const { paths, onChange } = options;

            if (!paths || !onChange) {
                throw new Error('You must specify "paths" and "onChange" options for the chokidar-watch plugin.');
            }

            const watcher = chokidar.watch(paths, {
                ignored: /node_modules/,
                persistent: true,
                ignoreInitial: true,
                followSymlinks: true,
                cwd: '.',
                disableGlobbing: false,
                usePolling: true,
                interval: 100,
                binaryInterval: 300,
                depth: 99,
                awaitWriteFinish: {
                    stabilityThreshold: 2000,
                    pollInterval: 100,
                },
            });

            watcher.on('all', (event, path) => {
                console.log(`File ${path} changed: ${event}`);
                onChange();
            });
        },
    };
}
