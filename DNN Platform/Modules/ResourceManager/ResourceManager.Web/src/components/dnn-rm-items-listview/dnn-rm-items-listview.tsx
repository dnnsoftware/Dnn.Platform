import { Component, Host, h } from '@stencil/core';
import state from "../../store/store";

@Component({
  tag: 'dnn-rm-items-listview',
  styleUrl: 'dnn-rm-items-listview.scss',
  shadow: true,
})
export class DnnRmItemsListview {

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

  render() {
    return (
      <Host>
        {state.currentItems &&
          <table>
            <thead>
              <tr>
                <td></td>
                <td>{state.localization?.Name}</td>
                <td>{state.localization?.Created}</td>
                <td>{state.localization?.LastModified}</td>
                <td>{state.localization?.Size}</td>
              </tr>
            </thead>
            <tbody>
              {state.currentItems.items?.map(item =>
                <tr>
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
