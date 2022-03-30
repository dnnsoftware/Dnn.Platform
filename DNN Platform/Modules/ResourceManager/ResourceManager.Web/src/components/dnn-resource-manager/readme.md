# dnn-resource-manager



<!-- Auto Generated Below -->


## Properties

| Property                | Attribute   | Description           | Type     | Default     |
| ----------------------- | ----------- | --------------------- | -------- | ----------- |
| `moduleId` _(required)_ | `module-id` | The ID of the module. | `number` | `undefined` |


## Dependencies

### Depends on

- [dnn-rm-top-bar](../dnn-rm-top-bar)
- dnn-vertical-splitview
- [dnn-rm-left-pane](../dnn-rm-left-pane)
- [dnn-rm-right-pane](../dnn-rm-right-pane)

### Graph
```mermaid
graph TD;
  dnn-resource-manager --> dnn-rm-top-bar
  dnn-resource-manager --> dnn-vertical-splitview
  dnn-resource-manager --> dnn-rm-left-pane
  dnn-resource-manager --> dnn-rm-right-pane
  dnn-rm-top-bar --> dnn-searchbox
  dnn-rm-left-pane --> dnn-rm-folder-list
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  dnn-rm-folder-list-item --> dnn-collapsible
  dnn-rm-folder-list-item --> dnn-rm-folder-context-menu
  dnn-rm-folder-list-item --> dnn-treeview-item
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
  dnn-rm-edit-folder --> dnn-tabs
  dnn-rm-edit-folder --> dnn-tab
  dnn-rm-edit-folder --> dnn-permissions-grid
  dnn-rm-edit-folder --> dnn-button
  dnn-permissions-grid --> dnn-checkbox
  dnn-permissions-grid --> dnn-button
  dnn-permissions-grid --> dnn-searchbox
  dnn-permissions-grid --> dnn-collapsible
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-right-pane --> dnn-rm-actions-bar
  dnn-rm-right-pane --> dnn-rm-files-pane
  dnn-rm-right-pane --> dnn-rm-status-bar
  dnn-rm-actions-bar --> dnn-vertical-overflow-menu
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-actions-bar --> dnn-action-edit-item
  dnn-rm-actions-bar --> dnn-collapsible
  dnn-rm-files-pane --> dnn-rm-items-listview
  dnn-rm-files-pane --> dnn-rm-items-cardview
  dnn-rm-items-listview --> dnn-collapsible
  dnn-rm-items-listview --> dnn-rm-folder-context-menu
  dnn-rm-items-cardview --> dnn-collapsible
  dnn-rm-items-cardview --> dnn-rm-folder-context-menu
  style dnn-resource-manager fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
