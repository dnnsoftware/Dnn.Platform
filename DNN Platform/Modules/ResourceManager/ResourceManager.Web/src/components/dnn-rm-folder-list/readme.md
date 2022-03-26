# dnn-rm-folder-list



<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-rm-left-pane](../dnn-rm-left-pane)

### Depends on

- [dnn-rm-folder-list-item](../dnn-rm-folder-list-item)

### Graph
```mermaid
graph TD;
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
  dnn-action-edit-item --> dnn-rm-create-folder
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-left-pane --> dnn-rm-folder-list
  style dnn-rm-folder-list fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
