# dnn-rm-right-pane



<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-resource-manager](../dnn-resource-manager)

### Depends on

- [dnn-rm-actions-bar](../dnn-rm-actions-bar)
- [dnn-rm-files-pane](../dnn-rm-files-pane)
- [dnn-rm-status-bar](../dnn-rm-status-bar)

### Graph
```mermaid
graph TD;
  dnn-rm-right-pane --> dnn-rm-actions-bar
  dnn-rm-right-pane --> dnn-rm-files-pane
  dnn-rm-right-pane --> dnn-rm-status-bar
  dnn-rm-actions-bar --> dnn-vertical-overflow-menu
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-actions-bar --> dnn-collapsible
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-edit-folder
  dnn-rm-edit-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-files-pane --> dnn-rm-items-listview
  dnn-rm-files-pane --> dnn-rm-items-cardview
  dnn-rm-items-listview --> dnn-collapsible
  dnn-rm-items-listview --> dnn-rm-folder-context-menu
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-items-cardview --> dnn-collapsible
  dnn-rm-items-cardview --> dnn-rm-folder-context-menu
  dnn-resource-manager --> dnn-rm-right-pane
  style dnn-rm-right-pane fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
