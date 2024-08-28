export default class StylesClient {
    private serviceRoot: string;

    constructor() {
        const config = window.dnn.initStyles();
        const sf = config.utility.sf;
        sf.moduleRoot = "PersonaBar";
        this.serviceRoot = sf.getServiceRoot();
        this.serviceRoot += "Styles/";
    }

    getStyles(){
        return new Promise<any>((resolve, reject) => {
            fetch(`${this.serviceRoot}GetStyles`, {
                method: "GET",
            })
            .then((response) => response.json())
            .then((response) => {
                resolve(response);
            })
            .catch((error) => {
                reject(error);
            });
        });
    }
}