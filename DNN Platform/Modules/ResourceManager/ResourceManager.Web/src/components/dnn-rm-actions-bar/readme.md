# dnn-rm-actions-bar



<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-rm-right-pane](../dnn-rm-right-pane)

### Depends on

- dnn-vertical-overflow-menu
- [dnn-action-create-folder](../actions/dnn-action-create-folder)
- [dnn-action-edit-item](../actions/dnn-action-edit-item)
- dnn-collapsible

### Graph
```mermaid
graph TD;
  dnn-rm-actions-bar --> dnn-vertical-overflow-menu
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-actions-bar --> dnn-action-edit-item
  dnn-rm-actions-bar --> dnn-collapsible
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
  dnn-rm-right-pane --> dnn-rm-actions-bar
  style dnn-rm-actions-bar fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
