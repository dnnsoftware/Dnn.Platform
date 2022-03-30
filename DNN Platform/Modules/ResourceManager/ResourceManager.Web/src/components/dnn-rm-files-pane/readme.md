# dnn-rm-files-pane



<!-- Auto Generated Below -->


## Properties

| Property        | Attribute        | Description                                          | Type     | Default |
| --------------- | ---------------- | ---------------------------------------------------- | -------- | ------- |
| `preloadOffset` | `preload-offset` | Defines how much more pixels to load under the fold. | `number` | `5000`  |


## Dependencies

### Used by

 - [dnn-rm-right-pane](../dnn-rm-right-pane)

### Depends on

- [dnn-rm-items-listview](../dnn-rm-items-listview)
- [dnn-rm-items-cardview](../dnn-rm-items-cardview)

### Graph
```mermaid
graph TD;
  dnn-rm-files-pane --> dnn-rm-items-listview
  dnn-rm-files-pane --> dnn-rm-items-cardview
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
  dnn-rm-items-cardview --> dnn-collapsible
  dnn-rm-items-cardview --> dnn-rm-folder-context-menu
  dnn-rm-right-pane --> dnn-rm-files-pane
  style dnn-rm-files-pane fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
