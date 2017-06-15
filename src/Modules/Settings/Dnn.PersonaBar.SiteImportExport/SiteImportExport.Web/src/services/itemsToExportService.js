const itemsToExport = [];

const itemsToExportService = {    
    registerItemToExport(item) {
        itemsToExport.push(item);
    },
    getRegisteredItemsToExport() {
        return itemsToExport;
    }
};


export default itemsToExportService;