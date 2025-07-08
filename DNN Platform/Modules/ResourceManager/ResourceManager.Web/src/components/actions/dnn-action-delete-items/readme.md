# dnn-action-move-items



<!-- Auto Generated Below -->


## Properties

| Property             | Attribute | Description                  | Type     | Default     |
| -------------------- | --------- | ---------------------------- | -------- | ----------- |
| `items` _(required)_ | `items`   | The list of items to delete. | `Item[]` | `undefined` |


## Dependencies

### Used by

 - [dnn-rm-actions-bar](../../dnn-rm-actions-bar)
 - [dnn-rm-file-context-menu](../../context-menus/dnn-rm-file-context-menu)
 - [dnn-rm-folder-context-menu](../../context-menus/dnn-rm-folder-context-menu)

### Depends on

- dnn-modal
- [dnn-rm-delete-items](../../dnn-rm-delete-items)

### Graph
```mermaid
graph TD;
  dnn-action-delete-items --> dnn-modal
  dnn-action-delete-items --> dnn-rm-delete-items
  dnn-rm-delete-items --> dnn-rm-progress-bar
  dnn-rm-delete-items --> dnn-button
  dnn-button --> dnn-modal
  dnn-button --> dnn-button
  dnn-rm-actions-bar --> dnn-action-delete-items
  dnn-rm-file-context-menu --> dnn-action-delete-items
  dnn-rm-folder-context-menu --> dnn-action-delete-items
  style dnn-action-delete-items fill:#f9f,stroke:#333,stroke-width:4px
```

----------------------------------------------

*Built with [StencilJS](https://stenciljs.com/)*
