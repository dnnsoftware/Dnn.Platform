declare module 'rollup-plugin-cleaner' {
    interface CleanerOptions{
        targets?: string[];
        silent?: boolean;
    }

    function cleaner(options: CleanerOptions): any;
    
    export = cleaner;
}