# dnn-rm-actions-bar



<!-- Auto Generated Below -->


## Dependencies

### Used by

 - [dnn-rm-right-pane](../dnn-rm-right-pane)

### Depends on

- dnn-vertical-overflow-menu
- [dnn-action-create-folder](../actions/dnn-action-create-folder)
- dnn-collapsible

### Graph
```mermaid
graph TD;
  dnn-rm-actions-bar --> dnn-vertical-overflow-menu
  dnn-rm-actions-bar --> dnn-action-create-folder
  dnn-rm-actions-bar --> dnn-collapsible
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-edit-folder
  dnn-rm-edit-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-right-pane --> dnn-rm-actions-bar
  style dnn-rm-actions-bar fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
