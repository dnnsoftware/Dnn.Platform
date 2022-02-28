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
    itemsSearchTerm?: string;
    pageSize: number;
    lastSearchRequestedPage: number;
}>({
    moduleId: -1,
    layout: "list",
    pageSize: 20,
    lastSearchRequestedPage: 1,
});

export default state;