import { Component, Host, h, State } from '@stencil/core';
import state from '../../store/store';
import { sortField, SortFieldInfo } from "../../enums/SortField";
import { sortOrder } from "../../enums/SortOrder";
import { InternalServicesClient } from '../../services/InternalServicesClient';
import { ItemsClient } from '../../services/ItemsClient';
@Component({
  tag: 'dnn-rm-actions-bar',
  styleUrl: 'dnn-rm-actions-bar.scss',
  shadow: true,
})
export class DnnRmActionsBar {
  
  @State() sortDropdownExpanded: boolean = false;
  @State() syncDropdownExpanded: boolean = false;

  private internalServicesClient: InternalServicesClient;
  private itemsClient: ItemsClient;

  constructor(){
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
    this.itemsClient = new ItemsClient(state.moduleId);
  }

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

  private renderSortOrderButtonIcon(){
    if (state.sortOrder.sortOrderKey == sortOrder.ascending.sortOrderKey) {
      return <path d="M10 6h4v2h-4v-2zm-2 5h8v2h-8v-2zm-2 5h12v2h-12v-2z"/>
    }
    else {
      return <path d="M6 6h12v2h-12v-2zm2 5h8v2h-8v-2zm2 5h4v2h-4v-2z"/>
    }
  }

  private async syncFolderContent(recursive: boolean = false) {
    try {
      await this.itemsClient.syncFolderContent(
        state.currentItems.folder.folderId,
        0,
        state.sortField,
        recursive);
      await this.getFolderContent();
    } catch (error) {
      alert(error);
    }
  }


  private async getFolderContent() {
    try {
      await this.getFolders();
      state.currentItems = await this.itemsClient.getFolderContent(
        state.currentItems.folder.folderId,
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
      
    } catch (error) {
      alert(error);
    }
  }

  private async getFolders() {
    try {
      state.rootFolders = await this.internalServicesClient.getFolders(state.settings.HomeFolderId);
    } catch (error) {
      alert(error);
    }
  }

  private toggleSortOrder() {
    state.sortOrder = (state.sortOrder.sortOrderKey == sortOrder.ascending.sortOrderKey) ? sortOrder.descending : sortOrder.ascending;
  }

  render() {
    return (
      <Host>
        <dnn-vertical-overflow-menu>
          {state.selectedItems && state.selectedItems.length == 0 && state.currentItems && state.currentItems.hasAddFoldersPermission &&
              <dnn-action-create-folder />
          }
          {state.selectedItems && state.selectedItems.length == 0 && state.currentItems && state.currentItems.hasAddFilesPermission &&
              <dnn-action-upload-file />
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
          {state.selectedItems.length == 1 && !state.selectedItems[0].isFolder &&
            <dnn-action-open-file item={state.selectedItems[0]}/>
          }
          {state.selectedItems.length == 1 && !state.selectedItems[0].isFolder &&
            <dnn-action-download-item item={state.selectedItems[0]}/>
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
              onClick={() => 
                {
                  this.syncDropdownExpanded = false;
                  this.sortDropdownExpanded = !this.sortDropdownExpanded;
                }
              }
            >
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M3 18h6v-2H3v2zM3 6v2h18V6H3zm0 7h12v-2H3v2z"/></svg>
              <span>{state.localization.Sort}</span>
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M7 10l5 5 5-5z"/></svg>
            </button>
            <button id="sort-order-button"
              title={`Order: ${state.sortOrder.localizedName}`}
              onClick={() =>
                {
                     this.toggleSortOrder();
                     void this.getFolderContent();
                }
              }
            >
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000">
                  <path d="M0 0h24v24H0z" fill="none"/>
                  {this.renderSortOrderButtonIcon()}
              </svg>
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
          <div class="sync">
            <button
              onClick={() =>
                {
                  this.sortDropdownExpanded = false;
                  this.syncDropdownExpanded = !this.syncDropdownExpanded;
                }
              }
            >
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path xmlns="http://www.w3.org/2000/svg" d="M14 4H20V6H17.25L17.65 6.35Q18.875 7.575 19.438 9.012Q20 10.45 20 11.95Q20 14.725 18.337 16.887Q16.675 19.05 14 19.75V17.65Q15.8 17 16.9 15.438Q18 13.875 18 11.95Q18 10.825 17.575 9.762Q17.15 8.7 16.25 7.8L16 7.55V10H14ZM10 20H4V18H6.75L6.35 17.65Q5.05 16.5 4.525 15.025Q4 13.55 4 12.05Q4 9.275 5.662 7.112Q7.325 4.95 10 4.25V6.35Q8.2 7 7.1 8.562Q6 10.125 6 12.05Q6 13.175 6.425 14.237Q6.85 15.3 7.75 16.2L8 16.45V14H10Z"/></svg>
              <span>{state.localization.Sync}</span>
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M7 10l5 5 5-5z"/></svg>
            </button>
            <dnn-collapsible expanded={this.syncDropdownExpanded}>
              <div class="dropdown">
                <button
                  onClick={() =>
                  {
                    void this.getFolderContent();
                    this.syncDropdownExpanded = !this.syncDropdownExpanded;
                  }}
                >
                  <span>{state.localization.Refresh}</span>
                </button>
                <button
                  onClick={() =>
                  {
                    void this.syncFolderContent();
                    this.syncDropdownExpanded = !this.syncDropdownExpanded;
                  }}
                >
                  <span>{state.localization.SyncThisFolder}</span>
                </button>
                <button
                  onClick={() =>
                  {
                    void this.syncFolderContent(true);
                    this.syncDropdownExpanded = !this.syncDropdownExpanded;
                  }}
                >
                  <span>{state.localization.SyncThisFolderAndSubfolders}</span>
                </button>
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
