# dnn-rm-folder-context-menu



<!-- Auto Generated Below -->


## Properties

| Property                       | Attribute           | Description                                                        | Type     | Default     |
| ------------------------------ | ------------------- | ------------------------------------------------------------------ | -------- | ----------- |
| `clickedFolderId` _(required)_ | `clicked-folder-id` | The ID of the folder onto which the context menu was triggered on. | `number` | `undefined` |
| `clickedItem`                  | --                  |                                                                    | `Item`   | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-folder-list-item](../../dnn-rm-folder-list-item)
 - [dnn-rm-items-cardview](../../dnn-rm-items-cardview)
 - [dnn-rm-items-listview](../../dnn-rm-items-listview)

### Depends on

- [dnn-action-create-folder](../../actions/dnn-action-create-folder)
- [dnn-action-edit-item](../../actions/dnn-action-edit-item)

### Graph
```mermaid
graph TD;
  dnn-rm-folder-context-menu --> dnn-action-create-folder
  dnn-rm-folder-context-menu --> dnn-action-edit-item
  dnn-action-create-folder --> dnn-modal
  dnn-action-create-folder --> dnn-rm-create-folder
  dnn-rm-create-folder --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-action-edit-item --> dnn-modal
  dnn-action-edit-item --> dnn-rm-create-folder
  dnn-rm-folder-list-item --> dnn-rm-folder-context-menu
  dnn-rm-items-cardview --> dnn-rm-folder-context-menu
  dnn-rm-items-listview --> dnn-rm-folder-context-menu
  style dnn-rm-folder-context-menu fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
