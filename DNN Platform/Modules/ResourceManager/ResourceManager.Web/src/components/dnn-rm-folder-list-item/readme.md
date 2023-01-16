# dnn-rm-folder-list-item



<!-- Auto Generated Below -->


## Properties

| Property                      | Attribute          | Description                                           | Type             | Default     |
| ----------------------------- | ------------------ | ----------------------------------------------------- | ---------------- | ----------- |
| `expanded`                    | `expanded`         | If true, this node will be expanded on load.          | `boolean`        | `false`     |
| `folder` _(required)_         | --                 | The basic information about the folder                | `FolderTreeItem` | `undefined` |
| `parentFolderId` _(required)_ | `parent-folder-id` | The ID of the parent folder.                          | `number`         | `undefined` |
| `selectedFolder`              | --                 | Indicates if this item is the currently selected one. | `FolderTreeItem` | `undefined` |


## Events

| Event                        | Description                                                             | Type                          |
| ---------------------------- | ----------------------------------------------------------------------- | ----------------------------- |
| `dnnRmcontextMenuOpened`     | Fires when a context menu is opened for this item. Emits the folder ID. | `CustomEvent<number>`         |
| `dnnRmFolderListItemClicked` | Fires when a folder is clicked.                                         | `CustomEvent<FolderTreeItem>` |


## Dependencies

### Used by

 - [dnn-rm-folder-list](../dnn-rm-folder-list)
 - [dnn-rm-folder-list-item](.)

### Depends on

- dnn-collapsible
- [dnn-rm-folder-context-menu](../context-menus/dnn-rm-folder-context-menu)
- dnn-treeview-item
- [dnn-rm-folder-list-item](.)

### Graph
```mermaid
graph TD;
  dnn-rm-folder-list-item --> dnn-rm-folder-list-item
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  dnn-rm-folder-context-menu --> dnn-action-move-items
  dnn-rm-folder-context-menu --> dnn-action-delete-items
  dnn-rm-folder-context-menu --> dnn-action-unlink-items
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-edit-folder
  dnn-action-edit-item --> dnn-rm-edit-file
  dnn-rm-edit-folder --> dnn-tabs
  dnn-rm-edit-folder --> dnn-tab
  dnn-rm-edit-folder --> dnn-permissions-grid
  dnn-rm-edit-folder --> dnn-button
  dnn-permissions-grid --> dnn-checkbox
  dnn-permissions-grid --> dnn-button
  dnn-permissions-grid --> dnn-searchbox
  dnn-permissions-grid --> dnn-collapsible
  dnn-rm-edit-file --> dnn-tabs
  dnn-rm-edit-file --> dnn-tab
  dnn-rm-edit-file --> dnn-button
  dnn-action-move-items --> dnn-modal
  dnn-action-move-items --> dnn-rm-move-items
  dnn-rm-move-items --> dnn-rm-folder-list
  dnn-rm-move-items --> dnn-rm-progress-bar
  dnn-rm-move-items --> dnn-button
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  dnn-action-delete-items --> dnn-modal
  dnn-action-delete-items --> dnn-rm-delete-items
  dnn-rm-delete-items --> dnn-rm-progress-bar
  dnn-rm-delete-items --> dnn-button
  dnn-action-unlink-items --> dnn-modal
  dnn-action-unlink-items --> dnn-rm-unlink-items
  dnn-rm-unlink-items --> dnn-rm-progress-bar
  dnn-rm-unlink-items --> dnn-button
  dnn-treeview-item --> dnn-collapsible
  style dnn-rm-folder-list-item fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
