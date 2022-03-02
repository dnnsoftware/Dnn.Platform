import {Item} from "../services/ItemsClient";
import state from "../store/store";

export function isItemSelected(item: Item)
{
  return state.selectedItems && state.selectedItems.includes(item);
}

export function toggleItemSelected(item: Item): void {
    if (isItemSelected(item)){
      state.selectedItems = state.selectedItems.filter(i => i != item);
    }
    else{
      state.selectedItems = [...state.selectedItems, item];
    }
  }