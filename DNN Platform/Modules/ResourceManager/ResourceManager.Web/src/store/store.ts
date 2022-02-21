import { createStore } from "@stencil/store";
import { GetFoldersResponse } from "../services/InternalServicesClient";
import { GetFolderContentResponse } from "../services/ItemsClient";
import { LocalizedStrings } from "../services/LocalizationClient";

const { state } = createStore<{
    moduleId: number;
    rootFolders?: GetFoldersResponse;
    currentItems?: GetFolderContentResponse;
    layout: "list" | "card";
    localization?: LocalizedStrings;
}>({
    moduleId: -1,
    layout: "list",
});

export default state;