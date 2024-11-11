export default class StylesClient {
    private serviceRoot: string;

    constructor() {
        const dnnStyles = window.dnn as unknown as IDnnWrapper;
        const config = dnnStyles.initStyles();
        if (!config) {
            throw new Error("dnn.initStyles() is not defined.");
        }
        if (!config.utility) {
            throw new Error("dnn.initStyles().utility is not defined.");
        }

        const sf = config.utility.sf;
        sf.moduleRoot = "PersonaBar";
        this.serviceRoot = sf.getServiceRoot();
        this.serviceRoot += "Styles/";
    }

    getStyles(){
        return new Promise<IDnnStyles>((resolve, reject) => {
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
export interface IDnnStyles {
    ColorPrimary: IDnnColorInfo;
    ColorPrimaryLight: IDnnColorInfo;
    ColorPrimaryDark: IDnnColorInfo;
    ColorPrimaryContrast: IDnnColorInfo;
    ColorSecondary: IDnnColorInfo;
    ColorSecondaryLight: IDnnColorInfo;
    ColorSecondaryDark: IDnnColorInfo;
    ColorSecondaryContrast: IDnnColorInfo;
    ColorTertiary: IDnnColorInfo;
    ColorTertiaryLight: IDnnColorInfo;
    ColorTertiaryDark: IDnnColorInfo;
    ColorTertiaryContrast: IDnnColorInfo;
    ColorNeutral: IDnnColorInfo;
    ColorNeutralLight: IDnnColorInfo;
    ColorNeutralDark: IDnnColorInfo;
    ColorNeutralContrast: IDnnColorInfo;
    ColorBackground: IDnnColorInfo;
    ColorBackgroundLight: IDnnColorInfo;
    ColorBackgroundDark: IDnnColorInfo;
    ColorBackgroundContrast: IDnnColorInfo;
    ColorForeground: IDnnColorInfo;
    ColorForegroundLight: IDnnColorInfo;
    ColorForegroundDark: IDnnColorInfo;
    ColorForegroundContrast: IDnnColorInfo;
    ColorInfo: IDnnColorInfo;
    ColorInfoLight: IDnnColorInfo;
    ColorInfoDark: IDnnColorInfo;
    ColorInfoContrast: IDnnColorInfo;
    ColorSuccess: IDnnColorInfo;
    ColorSuccessLight: IDnnColorInfo;
    ColorSuccessDark: IDnnColorInfo;
    ColorSuccessContrast: IDnnColorInfo;
    ColorWarning: IDnnColorInfo;
    ColorWarningLight: IDnnColorInfo;
    ColorWarningDark: IDnnColorInfo;
    ColorWarningContrast: IDnnColorInfo;
    ColorDanger: IDnnColorInfo;
    ColorDangerLight: IDnnColorInfo;
    ColorDangerDark: IDnnColorInfo;
    ColorDangerContrast: IDnnColorInfo;
    ControlsRadius: number;
    ControlsPadding: number;
    BaseFontSize: number;
}

export interface IDnnColorInfo {
    Red: number;
    Green: number;
    Blue: number;
    HexValue: string;
    MinifiedHex: string;
}