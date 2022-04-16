import { Component, Host, h, State } from '@stencil/core';
import state from '../../store/store';
import { sortField, SortFieldInfo } from "../../enums/SortField";
@Component({
  tag: 'dnn-rm-actions-bar',
  styleUrl: 'dnn-rm-actions-bar.scss',
  shadow: true,
})
export class DnnRmActionsBar {
  
  @State() sortDropdownExpanded: boolean = false;

  private changeLayout(): void {
    if (state.layout == "card"){
      state.layout = "list";
      return;
    }

    state.layout = "card";
  }

  private renderSortButton(sortOption: SortFieldInfo){
    return(
      <button
        onClick={() =>
        {
            state.sortField = sortOption;
            state.currentItems = {...state.currentItems, items: []};
            this.sortDropdownExpanded = !this.sortDropdownExpanded;
        }}
      >
        {this.renderRadioButton(state.sortField == sortOption)}
        <span>{sortOption.localizedName}</span>
      </button>
    );
  }

  private renderRadioButton(checked = false){
    if (checked){
      return <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 7c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zm0-5C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
    }
    else{
      return <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
    }
  }

  render() {
    return (
      <Host>
        <dnn-vertical-overflow-menu>
          {state.selectedItems && state.selectedItems.length == 0 && state.currentItems && state.currentItems.hasAddFilesPermission &&
            // No items are currently selected
            <dnn-action-create-folder />
          }
          {state.selectedItems.length == 1 &&
            // A single item is currently selected
            state.currentItems.hasManagePermission &&
            <dnn-action-edit-item item={state.selectedItems[0]}/>
          }
          {state.selectedItems.length > 0 &&
            // One or multiple items are currently selected
            state.currentItems.hasDeletePermission &&
            [
              <dnn-action-move-items items={state.selectedItems}/>
            ,
              <dnn-action-delete-items items={state.selectedItems}/>
            ]
          }
          {state.selectedItems.length > 0 && state.selectedItems.every(i => i.isFolder && i.unlinkAllowedStatus && i.unlinkAllowedStatus != "false") &&
            <dnn-action-unlink-items items={state.selectedItems}/>
          }
          {state.selectedItems.length == 1 && !state.selectedItems[0].isFolder && location.protocol == "https:" &&
            <dnn-action-copy-url items={state.selectedItems}/>
          }
        </dnn-vertical-overflow-menu>
        <div class="right-controls">
          {state.selectedItems.length > 0 &&
            <button onClick={() => state.selectedItems = []}>
              <span>
              {state.selectedItems.length} {state.localization.Items}
              </span>
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z"/></svg>
            </button>
          }
          <div class="sort">
            <button
              onClick={() => this.sortDropdownExpanded = !this.sortDropdownExpanded}
            >
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M3 18h6v-2H3v2zM3 6v2h18V6H3zm0 7h12v-2H3v2z"/></svg>
              <span>{state.localization.Sort}</span>
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M7 10l5 5 5-5z"/></svg>
            </button>
            <dnn-collapsible expanded={this.sortDropdownExpanded}>
              <div class="dropdown">
                {this.renderSortButton(sortField.itemName)}
                {this.renderSortButton(sortField.createdOnDate)}
                {this.renderSortButton(sortField.lastModifiedOnDate)}
                {this.renderSortButton(sortField.size)}
              </div>
            </dnn-collapsible>
          </div>
          <button
            onClick={() => this.changeLayout()}
          >
            {state.layout == "card" &&
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M3 13h2v-2H3v2zm0 4h2v-2H3v2zm0-8h2V7H3v2zm4 4h14v-2H7v2zm0 4h14v-2H7v2zM7 7v2h14V7H7z"/></svg>
            }
            {state.layout == "list" &&
              <svg xmlns="http://www.w3.org/2000/svg" enable-background="new 0 0 24 24" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><rect fill="none" height="24" width="24"/><g><path d="M14.67,5v6.5H9.33V5H14.67z M15.67,11.5H21V5h-5.33V11.5z M14.67,19v-6.5H9.33V19H14.67z M15.67,12.5V19H21v-6.5H15.67z M8.33,12.5H3V19h5.33V12.5z M8.33,11.5V5H3v6.5H8.33z"/></g></svg>
            }
          </button>
        </div>
      </Host>
    );
  }
}
