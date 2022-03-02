import { createStore } from "@stencil/store";
import { SortFieldInfo } from "../enums/SortField";
import { GetFoldersResponse } from "../services/InternalServicesClient";
import { GetFolderContentResponse, Item } from "../services/ItemsClient";
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
    sortField?: SortFieldInfo;
    selectedItems: Item[];
}>({
    moduleId: -1,
    layout: "list",
    pageSize: 50,
    lastSearchRequestedPage: 1,
    selectedItems: [],
});

export default state;