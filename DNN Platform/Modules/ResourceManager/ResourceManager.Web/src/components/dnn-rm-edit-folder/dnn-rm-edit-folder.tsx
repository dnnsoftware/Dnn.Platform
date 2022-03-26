import { IRoleGroup } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface';
import { IRole } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface';
import { Component, Element, Host, h, State, Prop } from '@stencil/core';
import state from '../../store/store';
import { FolderDetails, ItemsClient } from '../../services/ItemsClient';
@Component({
  tag: 'dnn-rm-edit-folder',
  styleUrl: 'dnn-rm-edit-folder.scss',
  shadow: true,
})
export class DnnRmEditFolder {
  @Prop() folderId!: number;

  @Element() el: HTMLDnnRmEditFolderElement;

  @State() roleGroups: IRoleGroup[] = [];
  @State() roles: IRole[] = [];
  @State() folderIconUrl: string;
  @State() private folderDetails: FolderDetails;

  private itemsClient: ItemsClient;

  constructor() {
    this.itemsClient = new ItemsClient(state.moduleId);
  }

  componentWillLoad() {
    this.itemsClient.getFolderDetails(this.folderId)
      .then(data => this.folderDetails = data)
      .catch(error => alert(error));
    this.itemsClient.getFolderIconUrl(this.folderId)
      .then(data => this.folderIconUrl = data)
      .catch(error => alert(error));
  }

  private handleCancel(): void {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    modal.hide().then(() => {
      setTimeout(() => {
        document.body.removeChild(modal);
      }, 300);
    });
  }

  private handleSave(): void {
    console.log(this.folderDetails);
  }

  render() {
    return (
      <Host>
        <h2>{state.localization?.Edit}</h2>
        <dnn-tabs>
          <dnn-tab tabTitle={state.localization?.General}>
            <div class="general">
              <div class="left">
                {this.folderIconUrl &&
                  <img src={this.folderIconUrl} />
                }
                {this.folderDetails &&
                  <div class="form">
                    <label>{state.localization?.FolderId}</label>
                    <span>{this.folderDetails.folderId}</span>

                    <label>{state.localization?.Created}</label>
                    <span>{this.folderDetails.createdBy}</span>

                    <label>{state.localization?.CreatedOnDate}</label>
                    <span>{this.folderDetails.createdOnDate}</span>

                    <label>{state.localization?.LastModified}</label>
                    <span>{this.folderDetails.lastModifiedBy}</span>

                    <label>{state.localization?.LastModifiedOnDate}</label>
                    <span>{this.folderDetails.lastModifiedOnDate}</span>
                  </div>
                }
              </div>
              <div class="right">
                <div class="form">
                  <label>{state.localization?.Name}</label>
                  <input type="text"
                    value={this.folderDetails?.folderName}
                    onInput={e =>
                      this.folderDetails = {
                        ...this.folderDetails,
                        folderName: (e.target as HTMLInputElement).value,
                      }
                    }
                  />
                </div>
              </div>
            </div>
          </dnn-tab>
          <dnn-tab tabTitle={state.localization?.Permissions}>
            {this.folderDetails && this.folderDetails.permissions && this.roleGroups && this.roles && 
              <dnn-permissions-grid
                permissions={this.folderDetails.permissions}
                roleGroups={this.roleGroups}
                roles={this.roles}/>
            }
          </dnn-tab>
        </dnn-tabs>
        <div class="controls">
          <dnn-button
            type="primary"
            reversed
            onClick={() => this.handleCancel()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            type="primary"
            onClick={() => this.handleSave()}
          >
            {state.localization.Save}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
