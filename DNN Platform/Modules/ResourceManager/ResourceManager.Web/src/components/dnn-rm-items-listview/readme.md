# dnn-rm-items-listview



<!-- Auto Generated Below -->


## Properties

| Property                    | Attribute | Description | Type                       | Default     |
| --------------------------- | --------- | ----------- | -------------------------- | ----------- |
| `currentItems` _(required)_ | --        |             | `GetFolderContentResponse` | `undefined` |


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
  dnn-rm-files-pane --> dnn-rm-items-listview
  style dnn-rm-items-listview fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
