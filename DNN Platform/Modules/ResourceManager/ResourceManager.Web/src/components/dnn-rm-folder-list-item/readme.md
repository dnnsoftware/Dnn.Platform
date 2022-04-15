# dnn-rm-folder-list-item



<!-- Auto Generated Below -->


## Properties

| Property                      | Attribute          | Description                                  | Type             | Default     |
| ----------------------------- | ------------------ | -------------------------------------------- | ---------------- | ----------- |
| `expanded`                    | `expanded`         | If true, this node will be expanded on load. | `boolean`        | `false`     |
| `folder` _(required)_         | --                 | The basic information about the folder       | `FolderTreeItem` | `undefined` |
| `parentFolderId` _(required)_ | `parent-folder-id` | The ID of the parent folder.                 | `number`         | `undefined` |


## Events

| Event                    | Description                                                             | Type                  |
| ------------------------ | ----------------------------------------------------------------------- | --------------------- |
| `dnnRmcontextMenuOpened` | Fires when a context menu is opened for this item. Emits the folder ID. | `CustomEvent<number>` |


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
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  style dnn-rm-folder-list-item fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
