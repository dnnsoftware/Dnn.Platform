import { Item } from "../services/ItemsClient";
import state from "../store/store";

/** Various utilities that help with current selection of items. */
class SelectionUtilities {
  /** Check if an item is currently selected */
  public isItemSelected(item: Item) {
    return state.selectedItems && state.selectedItems.includes(item);
  }

  /** Toggles the selection of an item. */
  public toggleItemSelected(item: Item): void {
    if (this.isItemSelected(item)) {
      state.selectedItems = state.selectedItems.filter((i) => i != item);
    } else {
      state.selectedItems = [...state.selectedItems, item];
    }
  }
}

export const selectionUtilities = new SelectionUtilities();
