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
  dnn-rm-files-pane --> dnn-rm-items-listview
  dnn-rm-files-pane --> dnn-rm-items-cardview
  dnn-resource-manager --> dnn-rm-right-pane
  style dnn-rm-right-pane fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
