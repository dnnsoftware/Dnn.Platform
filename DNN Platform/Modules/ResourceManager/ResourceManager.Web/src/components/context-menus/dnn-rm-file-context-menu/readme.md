# dnn-rm-file-context-menu



<!-- Auto Generated Below -->


## Properties

| Property            | Attribute | Description                        | Type   | Default     |
| ------------------- | --------- | ---------------------------------- | ------ | ----------- |
| `item` _(required)_ | --        | The item that triggered this menu. | `Item` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-items-cardview](../../dnn-rm-items-cardview)
 - [dnn-rm-items-listview](../../dnn-rm-items-listview)

### Depends on

- [dnn-action-edit-item](../../actions/dnn-action-edit-item)
- [dnn-action-move-items](../../actions/dnn-action-move-items)
- [dnn-action-delete-items](../../actions/dnn-action-delete-items)
- [dnn-action-download-item](../../actions/dnn-action-download-item)
- [dnn-action-copy-url](../../actions/dnn-action-copy-url)

### Graph
```mermaid
graph TD;
  dnn-rm-file-context-menu --> dnn-action-edit-item
  dnn-rm-file-context-menu --> dnn-action-move-items
  dnn-rm-file-context-menu --> dnn-action-delete-items
  dnn-rm-file-context-menu --> dnn-action-download-item
  dnn-rm-file-context-menu --> dnn-action-copy-url
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
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-edit-file --> dnn-tabs
  dnn-rm-edit-file --> dnn-tab
  dnn-rm-edit-file --> dnn-button
  dnn-action-move-items --> dnn-modal
  dnn-action-move-items --> dnn-rm-move-items
  dnn-rm-move-items --> dnn-rm-folder-list
  dnn-rm-move-items --> dnn-rm-progress-bar
  dnn-rm-move-items --> dnn-button
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  dnn-rm-folder-list-item --> dnn-collapsible
  dnn-rm-folder-list-item --> dnn-rm-folder-context-menu
  dnn-rm-folder-list-item --> dnn-treeview-item
  dnn-rm-folder-list-item --> dnn-rm-folder-list-item
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  dnn-rm-folder-context-menu --> dnn-action-move-items
  dnn-rm-folder-context-menu --> dnn-action-delete-items
  dnn-rm-folder-context-menu --> dnn-action-unlink-items
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-button
  dnn-action-delete-items --> dnn-modal
  dnn-action-delete-items --> dnn-rm-delete-items
  dnn-rm-delete-items --> dnn-rm-progress-bar
  dnn-rm-delete-items --> dnn-button
  dnn-action-unlink-items --> dnn-modal
  dnn-action-unlink-items --> dnn-rm-unlink-items
  dnn-rm-unlink-items --> dnn-rm-progress-bar
  dnn-rm-unlink-items --> dnn-button
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-items-cardview --> dnn-rm-file-context-menu
  dnn-rm-items-listview --> dnn-rm-file-context-menu
  style dnn-rm-file-context-menu fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
