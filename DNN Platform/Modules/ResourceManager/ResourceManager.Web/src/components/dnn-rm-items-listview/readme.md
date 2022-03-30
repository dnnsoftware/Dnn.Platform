# dnn-rm-items-listview



<!-- Auto Generated Below -->


## Properties

| Property                    | Attribute | Description                | Type                       | Default     |
| --------------------------- | --------- | -------------------------- | -------------------------- | ----------- |
| `currentItems` _(required)_ | --        | The list of current items. | `GetFolderContentResponse` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-files-pane](../dnn-rm-files-pane)

### Depends on

- dnn-collapsible
- [dnn-rm-folder-context-menu](../context-menus/dnn-rm-folder-context-menu)

### Graph
```mermaid
graph TD;
  dnn-rm-items-listview --> dnn-collapsible
  dnn-rm-items-listview --> dnn-rm-folder-context-menu
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-edit-folder
  dnn-rm-edit-folder --> dnn-tabs
  dnn-rm-edit-folder --> dnn-tab
  dnn-rm-edit-folder --> dnn-permissions-grid
  dnn-rm-edit-folder --> dnn-button
  dnn-permissions-grid --> dnn-checkbox
  dnn-permissions-grid --> dnn-button
  dnn-permissions-grid --> dnn-searchbox
  dnn-permissions-grid --> dnn-collapsible
  dnn-rm-files-pane --> dnn-rm-items-listview
  style dnn-rm-items-listview fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
