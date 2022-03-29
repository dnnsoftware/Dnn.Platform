import { IRoleGroup } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface';
import { IRole } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface';
import { Component, Element, Event, EventEmitter, Host, h, State, Prop } from '@stencil/core';
import state from '../../store/store';
import { FolderDetails, ItemsClient, SaveFolderDetailsRequest } from '../../services/ItemsClient';
import { IPermissions, IRolePermission, IUserPermission } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface';
import { ISearchedUser } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/searched-user-interface';
@Component({
  tag: 'dnn-rm-edit-folder',
  styleUrl: 'dnn-rm-edit-folder.scss',
  shadow: true,
})
export class DnnRmEditFolder {
  
  /** The ID of the folder to edit. */
  @Prop() folderId!: number;

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
   @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @Element() el: HTMLDnnRmEditFolderElement;

  @State() roleGroups: IRoleGroup[] = [];
  @State() roles: IRole[] = [];
  @State() folderIconUrl: string;
  @State() folderDetails: FolderDetails;
  @State() foundUsers: ISearchedUser[];

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

    this.itemsClient.getRoleGroups()
      .then(data => this.roleGroups = data)
      .catch(error => alert(error));
    
    this.itemsClient.getRoles()
      .then(data => this.roles = data)
      .catch(error => alert(error));
  }

  private closeModal(): void {
    const modal = this.el.parentElement as HTMLDnnModalElement;
    modal.hide().then(() => {
      setTimeout(() => {
        document.body.removeChild(modal);
      }, 300);
    });
  }

  private handleSave(): void {
    const folderDetails: SaveFolderDetailsRequest = {
      folderId: this.folderId,
      folderName: this.folderDetails.folderName,
      permissions: this.folderDetails.permissions,
    };
    this.itemsClient.saveFolderDetails(folderDetails)
      .then(() => {
        this.dnnRmFoldersChanged.emit();
        this.closeModal();
      })
      .catch(error => alert(error));
  }

  private handlePermissionsChanged(newPermissions: IPermissions): void {
    newPermissions.rolePermissions.forEach(rolePermission => this.adjustRelatedPermissions(rolePermission));
    newPermissions.userPermissions.forEach(userPermission => this.adjustRelatedPermissions(userPermission));
    this.folderDetails = {
      ...this.folderDetails,
      permissions: newPermissions,
    };
  }
  
  private adjustRelatedPermissions(permission: IRolePermission | IUserPermission): void {
    const permissionId =
    {
      view: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'View Folder').permissionId,
      browse: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Browse Folder').permissionId,
      write: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Write to Folder').permissionId,
    };

    const viewPermission = permission.permissions.find(p => p.permissionId == permissionId.view);
    // If view permission is denied, then deny all other permissions
    if (viewPermission && viewPermission.allowAccess == false){
      // Deny all permissions
      permission.permissions = [
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
    const browsePermission = permission.permissions.find(p => p.permissionId == permissionId.browse);
    if (browsePermission && browsePermission.allowAccess == false){
      // Deny write
      permission.permissions = [
        ...permission.permissions.filter(p => p.permissionId != permissionId.write),
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
      permission.permissions = [
        {
          allowAccess: true,
          fullControl: false,
          permissionId: permissionId.view,
          permissionCode: null,
          permissionKey: null,
          permissionName: "Browse Folder",
          view: false,
        },
        ...permission.permissions.filter(p => p.permissionId != permissionId.view),
      ];
    }

    // If write was allowed, then allow all other permissions
    const writePermission = permission.permissions.find(p => p.permissionId == permissionId.write);
    if (writePermission && writePermission.allowAccess == true){
      // Allow all permissions
      permission.permissions = [
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

  private handleUserSearchQueryChanged(detail: string): void {
    this.itemsClient.searchUsers(detail)
      .then(data => this.foundUsers = data)
      .catch(error => alert(error));
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
                foundUsers={this.foundUsers}
                onPermissionsChanged={e => this.handlePermissionsChanged(e.detail)}
                onUserSearchQueryChanged={e => this.handleUserSearchQueryChanged(e.detail)}
              />
            }
          </dnn-tab>
        </dnn-tabs>
        <div class="controls">
          <dnn-button
            type="primary"
            reversed
            onClick={() => this.closeModal()}
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
