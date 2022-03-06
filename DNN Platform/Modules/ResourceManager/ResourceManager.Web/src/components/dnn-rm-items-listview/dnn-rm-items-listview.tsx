import { Component, Host, h, Prop, Element } from '@stencil/core';
import { GetFolderContentResponse, Item } from '../../services/ItemsClient'
import state from '../../store/store';
import { selectionUtilities } from "../../utilities/selection-utilities";

@Component({
  tag: 'dnn-rm-items-listview',
  styleUrl: 'dnn-rm-items-listview.scss',
  shadow: true,
})
export class DnnRmItemsListview {

  /** The list of current items. */
  @Prop() currentItems!: GetFolderContentResponse;

  @Element() el: HTMLDnnRmItemsListviewElement;

  componentWillLoad() {
    document.addEventListener("click", this.dismissContextMenu.bind(this));
  }

  disconnectedCallback() {
    document.removeEventListener("click", this.disconnectedCallback.bind(this));
  }

  private dismissContextMenu() {
    const existingMenus = this.el.shadowRoot.querySelectorAll("dnn-collapsible");
    existingMenus?.forEach(existingMenu => this.el.shadowRoot.removeChild(existingMenu));
  }
  

  private getLocalDateString(dateString: string) {
    const date = new Date(dateString);
    return <div class="date">
      <span>{date.toLocaleDateString()}</span>
      <span>{date.toLocaleTimeString()}</span>
    </div>
  }

  private getFileSize(fileSize: number) {
    if (fileSize == undefined || fileSize == undefined){
      return "";
    }
    
    if (fileSize < 1024){
      return fileSize.toString() + " B";
    }
    
    if (fileSize < 1048576 ){
      return Math.round(fileSize / 1024).toString() + " KB";
    }
    
    return Math.round(fileSize / 3221225472).toString() + " MB";
  }

  private handleRowKeyDown(e: KeyboardEvent, item: Item): void {
    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        ((e.target as HTMLTableRowElement).nextElementSibling as HTMLTableRowElement)?.focus();
        break;
      case "ArrowUp":
        e.preventDefault();
        ((e.target as HTMLTableRowElement).previousElementSibling as HTMLTableRowElement)?.focus();
        break;
      case " ":
      case "Enter":
        e.preventDefault();
        selectionUtilities.toggleItemSelected(item);
        break;
      default:
        break;
    }
  }

  private handleContextMenu(e: MouseEvent, item: Item): void {
    e.preventDefault();
    state.selectedItems = [];
    this.dismissContextMenu();
    if (item.isFolder){
      const collapsible = document.createElement("dnn-collapsible");
      const folderContextMenu = document.createElement("dnn-rm-folder-context-menu");
      folderContextMenu.clickedFolderId = item.itemId;
      collapsible.appendChild(folderContextMenu);
      collapsible.style.left = `${e.pageX}px`;
      collapsible.style.top = `${e.pageY}px`;
      collapsible.style.display = "block";
      this.el.shadowRoot.appendChild(collapsible);
      setTimeout(() => {
        collapsible.expanded = true;
      }, 100);
      return;
    }
  }

  render() {
    return (
      <Host>
        {this.currentItems &&
          <table>
            <thead>
              <tr>
                <td></td>
                <td></td>
                <td>{state.localization?.Name}</td>
                <td>{state.localization?.Created}</td>
                <td>{state.localization?.LastModified}</td>
                <td>{state.localization?.Size}</td>
              </tr>
            </thead>
            <tbody>
              {this.currentItems.items?.map(item =>
                <tr
                  class={selectionUtilities.isItemSelected(item) ? "selected" : ""}
                  tabIndex={0}
                  onKeyDown={e => this.handleRowKeyDown(e, item)}
                  onClick={() => selectionUtilities.toggleItemSelected(item)}
                  onContextMenu={e => this.handleContextMenu(e, item)}
                >
                  <td class="radio">
                    {selectionUtilities.isItemSelected(item) &&
                      <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 7c-2.76 0-5 2.24-5 5s2.24 5 5 5 5-2.24 5-5-2.24-5-5-5zm0-5C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
                    }
                    {!selectionUtilities.isItemSelected(item) &&
                      <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"/><path d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z"/></svg>
                    }
                  </td>
                  <td><img src={item.iconUrl} /></td>
                  <td>{item.itemName}</td>
                  <td>{this.getLocalDateString(item.createdOn)}</td>
                  <td>{this.getLocalDateString(item.modifiedOn)}</td>
                  <td class="size">{this.getFileSize(item.fileSize)}</td>
                </tr>
              )}
            </tbody>
          </table>
        }
      </Host>
    );
  }
}
