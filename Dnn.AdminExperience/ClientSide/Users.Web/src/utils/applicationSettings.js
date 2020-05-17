const applicationSettings = {
    init(applicationSettings) {
        if (!applicationSettings) {
            this.applicationSettings = {};
        }
        this.applicationSettings = applicationSettings;      
    },
    applicationSettings: null
};
export default applicationSettings;