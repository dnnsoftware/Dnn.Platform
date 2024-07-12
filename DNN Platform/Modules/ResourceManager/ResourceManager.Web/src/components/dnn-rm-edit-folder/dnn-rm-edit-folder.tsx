import { IRoleGroup } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-group-interface';
import { IRole } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/role-interface';
import { Component, Element, Event, EventEmitter, Host, h, State, Prop } from '@stencil/core';
import state from '../../store/store';
import { FolderDetails, ItemsClient, SaveFolderDetailsRequest } from '../../services/ItemsClient';
import { IPermissionDefinition, IPermissions,  IRolePermission, IUserPermission } from '@dnncommunity/dnn-elements/dist/types/components/dnn-permissions-grid/permissions-interface';
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
  @State() lastPermissions: IPermissions;

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
        this.lastPermissions = {...this.folderDetails.permissions};
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
    // Get previous role permissions and adjust related permissions
    newPermissions.rolePermissions.forEach(rolePermission => {
      const previousPermissions = this.lastPermissions?.rolePermissions?.find(p => p.roleId === rolePermission.roleId).permissions ?? [];
      this.adjustRelatedPermissions(rolePermission, previousPermissions);
    });
  
    // Get previous user permissions and adjust related permissions
    newPermissions.userPermissions.forEach(userPermission => {
      const previousPermissions = this.lastPermissions?.userPermissions?.find(p => p.userId === userPermission.userId).permissions ?? [];
      this.adjustRelatedPermissions(userPermission, previousPermissions);
    });
  
    // Update the folder details with the new permissions
    this.folderDetails = {
      ...this.folderDetails,
      permissions: newPermissions,
    };
  
    // Update the last known permissions state
    this.lastPermissions = newPermissions;
  }
  
  private adjustRelatedPermissions(permission: IRolePermission | IUserPermission, previousPermissions: IPermissionDefinition[]): void {
    const permissionIds = {
      view: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'View Folder').permissionId,
      browse: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Browse Folder').permissionId,
      write: this.folderDetails.permissions.permissionDefinitions.find(p => p.permissionName === 'Write to Folder').permissionId,
    };
  
    const viewPermission = permission.permissions.find(p => p.permissionId === permissionIds.view);
    const browsePermission = permission.permissions.find(p => p.permissionId === permissionIds.browse);
    const writePermission = permission.permissions.find(p => p.permissionId === permissionIds.write);

    // Check if specific permissions have changed from the last known state
    const viewChanged = viewPermission && this.hasPermissionChanged(previousPermissions, viewPermission, permissionIds.view);
    const browseChanged = browsePermission && this.hasPermissionChanged(previousPermissions, browsePermission, permissionIds.browse);
    const writeChanged = writePermission && this.hasPermissionChanged(previousPermissions, writePermission, permissionIds.write);

    // If view permission is denied, then deny all other permissions
    if (viewChanged && !viewPermission.allowAccess) {
      if (browsePermission) {
        browsePermission.allowAccess = false;
      }
      if (writePermission) {
        writePermission.allowAccess = false;
      }
    }
  
    // If browse was denied, then deny write
    if (browseChanged && !browsePermission.allowAccess && writePermission) {
      writePermission.allowAccess = false;
    }
  
    // If browse was allowed, then allow view
    if (browseChanged && browsePermission.allowAccess) {
      if (!viewPermission) {
        // Create a new list with all existing permissions plus the new view permission
        permission.permissions = [...permission.permissions, {
          permissionId: permissionIds.view,
          allowAccess: true,
          fullControl: false,
          permissionCode: null,
          permissionKey: null,
          permissionName: "View Folder",
          view: false,
        }];
      } else {
        viewPermission.allowAccess = true;
      }
    }
  
    // If write was allowed, then allow all other permissions
    if (writeChanged && writePermission.allowAccess) {
      permission.permissions = [
        ...permission.permissions.filter(p => ![permissionIds.view, permissionIds.browse].includes(p.permissionId)), 
        {
          permissionId: permissionIds.view,
          allowAccess: true,
          fullControl: false,
          permissionCode: null,
          permissionKey: null,
          permissionName: "View Folder",
          view: false,
        },
        {
            permissionId: permissionIds.browse,
            allowAccess: true,
            fullControl: false,
            permissionCode: null,
            permissionKey: null,
            permissionName: "Browse Folder",
            view: false,
        }];
    }
  }

  private hasPermissionChanged(lastPermissions: IPermissionDefinition[], currentPermission: IPermissionDefinition, permissionId: number): boolean {
    const lastPermission = lastPermissions.find(p => p.permissionId === permissionId)
    return !lastPermission || JSON.stringify(lastPermission) !== JSON.stringify(currentPermission);
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
