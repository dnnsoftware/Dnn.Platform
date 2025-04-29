import state from "../store/store";
export class SortField {
  readonly itemName: SortFieldInfo;
  readonly lastModifiedOnDate: SortFieldInfo;
  readonly size: SortFieldInfo;
  readonly parentFolder: SortFieldInfo;
  readonly createdOnDate: SortFieldInfo;

  constructor() {
    this.itemName = new SortFieldInfo("ItemName");
    this.lastModifiedOnDate = new SortFieldInfo("LastModifiedOnDate");
    this.size = new SortFieldInfo("Size");
    this.parentFolder = new SortFieldInfo("ParentFolder");
    this.createdOnDate = new SortFieldInfo("CreatedOnDate");
  }
}

export class SortFieldInfo {
  readonly sortKey: string;

  get localizedName() {
    return state.localization[`SortField_${this.sortKey}`];
  }

  constructor(sortKey: string) {
    this.sortKey = sortKey;
  }
}

export const sortField = new SortField();
