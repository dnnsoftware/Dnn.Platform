# dnn-action-edit-item



<!-- Auto Generated Below -->


## Properties

| Property            | Attribute | Description | Type   | Default     |
| ------------------- | --------- | ----------- | ------ | ----------- |
| `item` _(required)_ | --        |             | `Item` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-actions-bar](../../dnn-rm-actions-bar)
 - [dnn-rm-folder-context-menu](../../context-menus/dnn-rm-folder-context-menu)

### Depends on

- dnn-modal
- [dnn-rm-create-folder](../../dnn-rm-create-folder)

### Graph
```mermaid
graph TD;
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-actions-bar --> dnn-action-edit-item
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  style dnn-action-edit-item fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
