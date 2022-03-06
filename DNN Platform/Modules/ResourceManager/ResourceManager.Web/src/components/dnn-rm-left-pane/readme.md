# dnn-rm-left-pane



<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-resource-manager](../dnn-resource-manager)

### Depends on

- [dnn-rm-folder-list](../dnn-rm-folder-list)

### Graph
```mermaid
graph TD;
  dnn-rm-left-pane --> dnn-rm-folder-list
  dnn-rm-folder-list --> dnn-rm-folder-list-item
  dnn-rm-folder-list-item --> dnn-treeview-item
  dnn-rm-folder-list-item --> dnn-rm-folder-list-item
  dnn-rm-folder-list-item --> dnn-collapsible
  dnn-rm-folder-list-item --> dnn-rm-folder-context-menu
  dnn-treeview-item --> dnn-collapsible
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-resource-manager --> dnn-rm-left-pane
  style dnn-rm-left-pane fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
