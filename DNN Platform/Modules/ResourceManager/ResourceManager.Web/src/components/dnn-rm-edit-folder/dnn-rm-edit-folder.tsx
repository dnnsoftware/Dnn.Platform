import { IRoleGroup } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface';
import { IRole } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface';
import { Component, Element, Host, h, State, Prop } from '@stencil/core';
import state from '../../store/store';
import { FolderDetails, ItemsClient } from '../../services/ItemsClient';
import { IPermissions, IRolePermission } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface';
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
      .then(data => {
        this.folderDetails = {
          ...data,
          permissions: {
            ...data.permissions,
            permissionDefinitions: data.permissions.permissionDefinitions.sort(a => {
              switch (a.permissionName) {
                case "View Folder":
                  return -1;
                case "Browse Folder":
                  return 0;
                case "Write to Folder":
                  return 1;
                default:
                  break;
              }
            }),
            rolePermissions: [
              ...data.permissions.rolePermissions.map(rp => {
                return {
                  ...rp,
                  permissions: rp.permissions.sort(a => {
                    switch (a.permissionName) {
                      case "View Folder":
                        return -1;
                      case "Browse Folder":
                        return 0;
                      case "Write to Folder":
                        return 1;
                    }
                  }),
                };
              }),
            ],
            userPermissions: [
              ...data.permissions.userPermissions.map(up => {
                return {
                  ...up,
                  permissions: up.permissions.sort(a => {
                    switch (a.permissionName) {
                      case "View Folder":
                        return -1;
                      case "Browse Folder":
                        return 0;
                      case "Write to Folder":
                        return 1;
                    }
                  }),
                };
              }),
            ],
          },
        };
      })
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

  private handlePermissionsChanged(newPermissions: IPermissions): void {
    newPermissions.rolePermissions.forEach(rolePermission => this.adjustRelatedPermissions(rolePermission));
    // do the same for users
    // apply it to the actual folder
  }
  
  private adjustRelatedPermissions(rolePermission: IRolePermission): void {
    const permissionId =
    {
      view: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'View Folder').permissionId,
      browse: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Browse Folder').permissionId,
      write: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Write to Folder').permissionId,
    };

    const viewPermission = rolePermission.permissions.find(p => p.permissionId == permissionId.view);
    // If view permission is denied, then deny all other permissions
    if (viewPermission && viewPermission.allowAccess == false){
      // Deny all permissions
      rolePermission.permissions = [
        {
          allowAccess: false,
          fullControl: false,
          permissionId: permissionId.view,
          permissionCode: null,
          permissionKey: null,
          permissionName: "View Folder",
          view: false,
        },
        {
          allowAccess: false,
          fullControl: false,
          permissionId: permissionId.browse,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Browse Folder",
          view: false,
        },
        {
          allowAccess: false,
          fullControl: false,
          permissionId: permissionId.write,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Write to Folder",
          view: false,
        },
      ]
    }

    // If browse was denied, then deny write
    const browsePermission = rolePermission.permissions.find(p => p.permissionId == permissionId.browse);
    if (browsePermission && browsePermission.allowAccess == false){
      // Deny write
      rolePermission.permissions = [
        ...rolePermission.permissions.filter(p => p.permissionId != permissionId.write),
        {
          allowAccess: false,
          fullControl: false,
          permissionId: permissionId.write,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Write to Folder",
          view: false,
        }
      ]
    }

    // If browse was allowed, then allow view
    if (browsePermission && browsePermission.allowAccess == true){
      // Allow browse
      rolePermission.permissions = [
        {
          allowAccess: true,
          fullControl: false,
          permissionId: permissionId.view,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Browse Folder",
          view: false,
        },
        ...rolePermission.permissions.filter(p => p.permissionId != permissionId.view),
      ];
    }

    // If write was allowed, then allow all other permissions
    const writePermission = rolePermission.permissions.find(p => p.permissionId == permissionId.write);
    if (writePermission && writePermission.allowAccess == true){
      // Allow all permissions
      rolePermission.permissions = [
        {
          allowAccess: true,
          fullControl: false,
          permissionId: permissionId.view,
          permissionCode: null,
          permissionKey: null,
          permissionName: "View Folder",
          view: false,
        },
        {
          allowAccess: true,
          fullControl: false,
          permissionId: permissionId.browse,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Browse Folder",
          view: false,
        },
        {
          allowAccess: true,
          fullControl: false,
          permissionId: permissionId.write,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Write to Folder",
          view: false,
        },
      ]
    }
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
                roles={this.roles}
                onPermissionsChanged={e => this.handlePermissionsChanged(e.detail)}
              />
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
