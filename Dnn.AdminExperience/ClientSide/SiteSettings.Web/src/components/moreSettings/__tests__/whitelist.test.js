import {
    WhitelistOptions,
    areEquivalent,
    getInitialCustomWhitelist,
    getWhitelistForOption,
    getWhitelistOption,
    normalizeExtensions,
} from "../whitelist";

const hostDefault = "jpg,jpeg,jpe,gif,bmp,png,svg,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,ico,avi,mpg,mpeg,mp3,wmv,mov,wav,mp4,webm,ogv,export";
const images = "bmp,gif,ico,jpeg,jpg,jpe,png,svg";

function settings(allowed, extra) {
    return Object.assign({
        AllowedExtensionsWhitelist: allowed,
        HostAllowedExtensionsWhitelists: hostDefault,
        ImageExtensionsList: images,
    }, extra);
}

describe("More settings whitelist helpers", () => {
    it("normalizes extension lists", () => {
        expect(normalizeExtensions(" JPG, .png,,gif,png ")).toEqual(["gif", "jpg", "png"]);
        expect(normalizeExtensions(undefined)).toEqual([]);
        expect(normalizeExtensions("")).toEqual([]);
    });

    it("compares lists regardless of order, case and duplicates", () => {
        expect(areEquivalent("jpg,png", "PNG, jpg,jpg")).toBe(true);
        expect(areEquivalent("jpg,png", "jpg")).toBe(false);
    });

    it("prefers the option computed by the server", () => {
        expect(getWhitelistOption(settings(hostDefault, { AllowedExtensionsWhitelistOption: WhitelistOptions.Custom })))
            .toBe(WhitelistOptions.Custom);
    });

    it("detects the option when the server did not provide one", () => {
        expect(getWhitelistOption(settings(hostDefault))).toBe(WhitelistOptions.Default);
        expect(getWhitelistOption(settings("jpg,jpeg,jpe,gif,bmp,png,svg,ico"))).toBe(WhitelistOptions.OnlyImages);
        expect(getWhitelistOption(settings("jpg,webp"))).toBe(WhitelistOptions.Custom);
        expect(getWhitelistOption(undefined)).toBe(WhitelistOptions.Default);
    });

    it("never treats an empty image list as 'Only Images'", () => {
        expect(getWhitelistOption(settings("", { ImageExtensionsList: "" }))).toBe(WhitelistOptions.Custom);
    });

    it("starts the custom list from the host default when the site uses Default or Only Images", () => {
        expect(getInitialCustomWhitelist(settings(images))).toBe(hostDefault);
        expect(getInitialCustomWhitelist(settings(hostDefault))).toBe(hostDefault);
    });

    it("keeps the site's custom list as the custom starting point", () => {
        expect(getInitialCustomWhitelist(settings("jpg,pdf"))).toBe("jpg,pdf");
    });

    it("returns the list for each option", () => {
        const s = settings(images);
        expect(getWhitelistForOption(WhitelistOptions.Default, s, "jpg")).toBe(hostDefault);
        expect(getWhitelistForOption(WhitelistOptions.OnlyImages, s, "jpg")).toBe(images);
        expect(getWhitelistForOption(WhitelistOptions.Custom, s, "jpg,pdf")).toBe("jpg,pdf");
        expect(getWhitelistForOption(WhitelistOptions.Custom, s, undefined)).toBe(hostDefault);
    });

    it("switching Only Images -> Custom shows the full default list (issue #6727)", () => {
        const s = settings(images);
        const custom = getInitialCustomWhitelist(s);
        const onlyImages = Object.assign({}, s, {
            AllowedExtensionsWhitelist: getWhitelistForOption(WhitelistOptions.OnlyImages, s, custom),
        });
        expect(getWhitelistForOption(WhitelistOptions.Custom, onlyImages, custom)).toBe(hostDefault);
    });
});
